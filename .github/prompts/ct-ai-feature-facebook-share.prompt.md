Prompt instructions file:
---
agent: Create comprehensive Facebook sharing functionality with analytics
always: Use CTDesignSystem components, implement proper error handling, analytics tracking
description: "Template for implementing Facebook sharing with ShareDialog, multiple content types, error handling, analytics tracking, and UI feedback following Cho Tot iOS architecture standards"
---

## Instructions
Follow instructions in [ct-ai-feature-video-player.prompt.md](file:///Users/hai.phan/Desktop/haiphan/ct-ios-app--v3/AppFeatures/CTCorePayment/CTCorePayment/Features/ct-ai-feature-video-player.prompt.md).

# Facebook Share Implementation Prompt

## Task
Create comprehensive Facebook sharing functionality supporting multiple content types (link, photo, video), error handling, fallback mechanisms, delegate pattern, and analytics tracking.

## Requirements
- Must follow MVVM + Clean Architecture patterns
- Use CTDesignSystem components only (no UIKit components)
- Support multiple content types (link, photo, video)
- Include proper error handling and fallback to system share
- Implement delegate pattern for callbacks
- Add analytics tracking for all share events
- Follow Cho Tot iOS coding standards

## Important Note: Facebook Ref Parameter
**What is `ref: "chotot_ios_app"`?**
The `ref` parameter in Facebook sharing serves as a tracking identifier with these purposes:
- **Attribution Tracking**: Identifies shares coming from Cho Tot iOS app vs other platforms (web, Android)
- **Analytics Segmentation**: Helps Facebook Analytics distinguish traffic sources
- **Campaign Tracking**: Enables tracking of share performance by platform
- **User Journey Mapping**: Allows tracking how users interact with shared content across platforms
- **Business Intelligence**: Provides insights into which platform generates more engagement

**Usage Examples**:
- `"chotot_ios_app"` - for iOS app shares
- `"chotot_android_app"` - for Android app shares  
- `"chotot_web"` - for website shares
- `"chotot_ios_payment"` - for specific feature/module shares

## Implementation Instructions

### Step 1: Import Required Dependencies
Include all necessary imports at the top of your file:

```swift
import UIKit
import Foundation
import CTCommon
import CTDesignSystem
import CTComponent
import CTAsset
import CTTracking
import FBSDKShareKit
import RxSwift
import RxRelay
```

### Step 2: Define Core Enums and Models
Add these data structures to support various sharing scenarios:

```swift
// MARK: - Facebook Share Content Types
enum FacebookShareContentType {
    case link
    case photo
    case video
    case story
}

// MARK: - Facebook Share Result
enum FacebookShareResult {
    case success([String: Any])
    case failure(Error)
    case cancelled
}

// MARK: - Facebook Share Configuration
struct FacebookShareConfig {
    let contentType: FacebookShareContentType
    let url: String?
    let image: UIImage?
    let videoURL: URL?
    let title: String
    let description: String?
    let hashtag: String?
    let peopleIDs: [String]?
    let placeID: String?
    let ref: String?
    
    static func linkShare(url: String, title: String, description: String? = nil, hashtag: String? = nil) -> FacebookShareConfig {
        return FacebookShareConfig(
            contentType: .link,
            url: url,
            image: nil,
            videoURL: nil,
            title: title,
            description: description,
            hashtag: hashtag,
            peopleIDs: nil,
            placeID: nil,
            ref: "chotot_ios_app"
        )
    }
    
    static func photoShare(image: UIImage, title: String, hashtag: String? = nil) -> FacebookShareConfig {
        return FacebookShareConfig(
            contentType: .photo,
            url: nil,
            image: image,
            videoURL: nil,
            title: title,
            description: nil,
            hashtag: hashtag,
            peopleIDs: nil,
            placeID: nil,
            ref: "chotot_ios_app"
        )
    }
    
    static func videoShare(videoURL: URL, title: String, hashtag: String? = nil) -> FacebookShareConfig {
        return FacebookShareConfig(
            contentType: .video,
            url: nil,
            image: nil,
            videoURL: videoURL,
            title: title,
            description: nil,
            hashtag: hashtag,
            peopleIDs: nil,
            placeID: nil,
            ref: "chotot_ios_app"
        )
    }
}

// MARK: - Facebook Share Error
enum FacebookShareError: LocalizedError {
    case invalidURL
    case invalidImage
    case invalidVideoURL
    case cannotShow
    case networkUnavailable
    case facebookNotInstalled
    case unknownError(String)
    
    var errorDescription: String? {
        switch self {
        case .invalidURL:
            return "URL không hợp lệ"
        case .invalidImage:
            return "Hình ảnh không hợp lệ"
        case .invalidVideoURL:
            return "Video URL không hợp lệ"
        case .cannotShow:
            return "Không thể hiển thị Facebook share dialog"
        case .networkUnavailable:
            return "Không có kết nối mạng"
        case .facebookNotInstalled:
            return "Chưa cài đặt ứng dụng Facebook"
        case .unknownError(let message):
            return message
        }
    }
}
```

### Step 3: Create Facebook Share Manager Protocol
Define the main protocol for Facebook sharing functionality:

```swift
// MARK: - Facebook Share Manager Protocol
protocol FacebookShareManagerType: AnyObject {
    var shareResult: PublishRelay<FacebookShareResult> { get }
    
    func shareToFacebook(config: FacebookShareConfig, from viewController: UIViewController)
    func canShowFacebookShare() -> Bool
    func validateShareConfig(_ config: FacebookShareConfig) -> Result<Void, FacebookShareError>
}

// MARK: - Facebook Share Delegate Protocol
protocol FacebookShareDelegate: AnyObject {
    func facebookShareDidComplete(with result: FacebookShareResult)
    func facebookShareWillShow()
    func facebookShareDidShow()
}
```

### Step 4: Implement Core Facebook Share Manager
Create the main manager class with full functionality:

```swift
// MARK: - Facebook Share Manager Implementation
final class FacebookShareManager: NSObject {
    
    // MARK: - Properties
    private let theme = CMStaticThemeLoader.defaultTheme
    private let disposeBag = DisposeBag()
    
    // RxSwift Relays
    let shareResult = PublishRelay<FacebookShareResult>()
    
    // Delegate
    weak var delegate: FacebookShareDelegate?
    
    // Current sharing context
    private var currentViewController: UIViewController?
    private var currentConfig: FacebookShareConfig?
    
    // MARK: - Initializer
    override init() {
        super.init()
        setupObservers()
    }
    
    deinit {
        NotificationCenter.default.removeObserver(self)
    }
    
    // MARK: - Setup
    private func setupObservers() {
        // Observe network changes
        NotificationCenter.default.addObserver(
            self,
            selector: #selector(networkStatusChanged),
            name: .networkStatusChanged,
            object: nil
        )
    }
    
    @objc private func networkStatusChanged() {
        // Handle network status changes if needed
        Logger.print("Network status changed for Facebook sharing", level: .info)
    }
}
```

### Step 5: Implement FacebookShareManagerType Protocol
Add the main sharing functionality:

```swift
// MARK: - FacebookShareManagerType Implementation
extension FacebookShareManager: FacebookShareManagerType {
    
    func shareToFacebook(config: FacebookShareConfig, from viewController: UIViewController) {
        // Store current context
        currentViewController = viewController
        currentConfig = config
        
        // Pre-validation
        let validationResult = validateShareConfig(config)
        switch validationResult {
        case .success:
            performShare(config: config, from: viewController)
        case .failure(let error):
            handleShareError(error)
        }
    }
    
    func canShowFacebookShare() -> Bool {
        return ShareDialog.canShow
    }
    
    func validateShareConfig(_ config: FacebookShareConfig) -> Result<Void, FacebookShareError> {
        // Check network connectivity
        guard NetworkReachability.shared.isConnected else {
            return .failure(.networkUnavailable)
        }
        
        // Check if Facebook share dialog can be shown
        guard canShowFacebookShare() else {
            return .failure(.cannotShow)
        }
        
        // Validate based on content type
        switch config.contentType {
        case .link:
            guard let urlString = config.url, 
                  !urlString.isEmpty,
                  URL(string: urlString) != nil else {
                return .failure(.invalidURL)
            }
            
        case .photo:
            guard config.image != nil else {
                return .failure(.invalidImage)
            }
            
        case .video:
            guard config.videoURL != nil else {
                return .failure(.invalidVideoURL)
            }
            
        case .story:
            // Add story validation if needed
            break
        }
        
        return .success(())
    }
    
    private func performShare(config: FacebookShareConfig, from viewController: UIViewController) {
        Logger.print("Starting Facebook share with type: \(config.contentType)", level: .info)
        
        // Notify delegate
        delegate?.facebookShareWillShow()
        
        // Track analytics
        trackShareAttempt(config: config)
        
        // Create share content based on type
        let shareContent = createShareContent(from: config)
        
        // Create and configure share dialog
        let shareDialog = ShareDialog(viewController: viewController, content: shareContent, delegate: self)
        shareDialog.mode = .automatic
        
        // Show dialog
        guard shareDialog.canShow else {
            handleShareError(.cannotShow)
            return
        }
        
        shareDialog.show()
        delegate?.facebookShareDidShow()
    }
    
    private func createShareContent(from config: FacebookShareConfig) -> SharingContent {
        switch config.contentType {
        case .link:
            return createLinkContent(from: config)
        case .photo:
            return createPhotoContent(from: config)
        case .video:
            return createVideoContent(from: config)
        case .story:
            return createStoryContent(from: config)
        }
    }
    
    private func createLinkContent(from config: FacebookShareConfig) -> ShareLinkContent {
        let content = ShareLinkContent()
        
        if let urlString = config.url, let url = URL(string: urlString) {
            content.contentURL = url
        }
        
        content.quote = config.title
        
        if let hashtag = config.hashtag {
            content.hashtag = Hashtag(hashtag)
        }
        
        if let peopleIDs = config.peopleIDs {
            content.peopleIDs = peopleIDs
        }
        
        if let placeID = config.placeID {
            content.placeID = placeID
        }
        
        if let ref = config.ref {
            content.ref = ref
        }
        
        return content
    }
    
    private func createPhotoContent(from config: FacebookShareConfig) -> SharePhotoContent {
        let content = SharePhotoContent()
        
        if let image = config.image {
            let photo = SharePhoto(image: image, isUserGenerated: true)
            content.photos = [photo]
        }
        
        content.quote = config.title
        
        if let hashtag = config.hashtag {
            content.hashtag = Hashtag(hashtag)
        }
        
        if let peopleIDs = config.peopleIDs {
            content.peopleIDs = peopleIDs
        }
        
        if let placeID = config.placeID {
            content.placeID = placeID
        }
        
        if let ref = config.ref {
            content.ref = ref
        }
        
        return content
    }
    
    private func createVideoContent(from config: FacebookShareConfig) -> ShareVideoContent {
        let content = ShareVideoContent()
        
        if let videoURL = config.videoURL {
            let video = ShareVideo(videoURL: videoURL)
            content.video = video
        }
        
        content.quote = config.title
        
        if let hashtag = config.hashtag {
            content.hashtag = Hashtag(hashtag)
        }
        
        if let peopleIDs = config.peopleIDs {
            content.peopleIDs = peopleIDs
        }
        
        if let placeID = config.placeID {
            content.placeID = placeID
        }
        
        if let ref = config.ref {
            content.ref = ref
        }
        
        return content
    }
    
    private func createStoryContent(from config: FacebookShareConfig) -> ShareStoryContent {
        // Implement story content creation if needed
        let content = ShareStoryContent()
        
        if let image = config.image {
            let photo = SharePhoto(image: image, isUserGenerated: true)
            content.backgroundAsset = photo
        }
        
        return content
    }
}
```

### Step 6: Implement SharingDelegate Protocol
Add delegate handling for Facebook SDK callbacks:

```swift
// MARK: - SharingDelegate Implementation
extension FacebookShareManager: SharingDelegate {
    
    func sharer(_ sharer: Sharing, didCompleteWithResults results: [String : Any]) {
        Logger.print("Facebook share completed successfully: \(results)", level: .info)
        
        // Track success
        trackShareSuccess(results: results)
        
        // Notify via RxSwift
        shareResult.accept(.success(results))
        
        // Notify delegate
        delegate?.facebookShareDidComplete(with: .success(results))
        
        // Show success message
        showSuccessMessage()
        
        // Cleanup
        cleanup()
    }
    
    func sharer(_ sharer: Sharing, didFailWithError error: Error) {
        Logger.print("Facebook share failed: \(error.localizedDescription)", level: .error)
        
        // Track failure
        trackShareFailure(error: error)
        
        // Try fallback
        handleShareError(.unknownError(error.localizedDescription))
    }
    
    func sharerDidCancel(_ sharer: Sharing) {
        Logger.print("Facebook share cancelled by user", level: .info)
        
        // Track cancellation
        trackShareCancelled()
        
        // Notify via RxSwift
        shareResult.accept(.cancelled)
        
        // Notify delegate
        delegate?.facebookShareDidComplete(with: .cancelled)
        
        // Cleanup
        cleanup()
    }
    
    private func handleShareError(_ error: FacebookShareError) {
        Logger.print("Handling Facebook share error: \(error.localizedDescription ?? "Unknown")", level: .error)
        
        // Notify via RxSwift
        shareResult.accept(.failure(error))
        
        // Notify delegate
        delegate?.facebookShareDidComplete(with: .failure(error))
        
        // Show error with fallback option
        showErrorWithFallback(error: error)
        
        // Cleanup
        cleanup()
    }
    
    private func cleanup() {
        currentViewController = nil
        currentConfig = nil
    }
}
```

### Step 7: Implement UI Feedback and Fallback Mechanisms
Add user feedback and system share fallback:

```swift
// MARK: - UI Feedback and Fallback
extension FacebookShareManager {
    
    private func showSuccessMessage() {
        guard let topVC = UIApplication.topViewController() else { return }
        
        // Show success toast using CTDesignSystem
        let successView = DSToastView()
        successView.configure(
            message: "Đã chia sẻ lên Facebook thành công!",
            type: .success,
            duration: 3.0
        )
        successView.show(in: topVC.view)
    }
    
    private func showErrorWithFallback(error: FacebookShareError) {
        guard let topVC = UIApplication.topViewController(),
              let config = currentConfig else { return }
        
        // Create alert with fallback option
        let alertController = UIAlertController(
            title: "Không thể chia sẻ lên Facebook",
            message: error.errorDescription,
            preferredStyle: .alert
        )
        
        // Retry action
        let retryAction = UIAlertAction(title: "Thử lại", style: .default) { [weak self] _ in
            self?.shareToFacebook(config: config, from: topVC)
        }
        
        // Fallback to system share
        let systemShareAction = UIAlertAction(title: "Chia sẻ khác", style: .default) { [weak self] _ in
            self?.fallbackToSystemShare(config: config, from: topVC)
        }
        
        // Cancel action
        let cancelAction = UIAlertAction(title: "Hủy", style: .cancel)
        
        alertController.addAction(retryAction)
        alertController.addAction(systemShareAction)
        alertController.addAction(cancelAction)
        
        topVC.present(alertController, animated: true)
    }
    
    private func fallbackToSystemShare(config: FacebookShareConfig, from viewController: UIViewController) {
        var items: [Any] = [config.title]
        
        if let urlString = config.url, let url = URL(string: urlString) {
            items.append(url)
        }
        
        if let image = config.image {
            items.append(image)
        }
        
        if let videoURL = config.videoURL {
            items.append(videoURL)
        }
        
        let activityViewController = UIActivityViewController(
            activityItems: items,
            applicationActivities: nil
        )
        
        // Exclude some activities if needed
        activityViewController.excludedActivityTypes = [
            .print,
            .assignToContact,
            .postToWeibo
        ]
        
        // Present system share
        viewController.present(activityViewController, animated: true)
        
        // Track fallback usage
        trackFallbackShare()
    }
}
```

### Step 8: Implement Analytics Tracking
Add comprehensive analytics tracking:

```swift
// MARK: - Analytics Tracking
extension FacebookShareManager {
    
    private func trackShareAttempt(config: FacebookShareConfig) {
        CTTracking.track(event: "facebook_share_attempt", parameters: [
            "content_type": "\(config.contentType)",
            "has_hashtag": config.hashtag != nil,
            "has_people_tags": config.peopleIDs?.isEmpty == false,
            "has_place_tag": config.placeID != nil,
            "ref": config.ref ?? ""
        ])
    }
    
    private func trackShareSuccess(results: [String: Any]) {
        CTTracking.track(event: "facebook_share_success", parameters: [
            "results": results,
            "content_type": currentConfig?.contentType.description ?? "unknown"
        ])
    }
    
    private func trackShareFailure(error: Error) {
        CTTracking.track(event: "facebook_share_failed", parameters: [
            "error_description": error.localizedDescription,
            "error_domain": (error as NSError).domain,
            "error_code": (error as NSError).code,
            "content_type": currentConfig?.contentType.description ?? "unknown"
        ])
    }
    
    private func trackShareCancelled() {
        CTTracking.track(event: "facebook_share_cancelled", parameters: [
            "content_type": currentConfig?.contentType.description ?? "unknown"
        ])
    }
    
    private func trackFallbackShare() {
        CTTracking.track(event: "facebook_share_fallback_used", parameters: [
            "content_type": currentConfig?.contentType.description ?? "unknown"
        ])
    }
}

// MARK: - FacebookShareContentType Description
extension FacebookShareContentType: CustomStringConvertible {
    var description: String {
        switch self {
        case .link: return "link"
        case .photo: return "photo"
        case .video: return "video"
        case .story: return "story"
        }
    }
}
```

### Step 9: Create Convenience Extensions and Helper Methods
Add utility methods for easier usage:

```swift
// MARK: - Convenience Methods
extension FacebookShareManager {
    
    // Quick share methods for common use cases
    func shareLink(
        url: String,
        title: String,
        description: String? = nil,
        from viewController: UIViewController
    ) {
        let config = FacebookShareConfig.linkShare(
            url: url,
            title: title,
            description: description,
            hashtag: "#ChoTot"
        )
        shareToFacebook(config: config, from: viewController)
    }
    
    func sharePhoto(
        image: UIImage,
        caption: String,
        from viewController: UIViewController
    ) {
        let config = FacebookShareConfig.photoShare(
            image: image,
            title: caption,
            hashtag: "#ChoTot"
        )
        shareToFacebook(config: config, from: viewController)
    }
    
    func shareVideo(
        videoURL: URL,
        title: String,
        from viewController: UIViewController
    ) {
        let config = FacebookShareConfig.videoShare(
            videoURL: videoURL,
            title: title,
            hashtag: "#ChoTot"
        )
        shareToFacebook(config: config, from: viewController)
    }
    
    // Reactive sharing with RxSwift
    func shareToFacebookRx(config: FacebookShareConfig, from viewController: UIViewController) -> Observable<FacebookShareResult> {
        return Observable.create { [weak self] observer in
            guard let self = self else {
                observer.onError(FacebookShareError.unknownError("FacebookShareManager deallocated"))
                return Disposables.create()
            }
            
            let subscription = self.shareResult
                .take(1)
                .subscribe(onNext: { result in
                    observer.onNext(result)
                    observer.onCompleted()
                })
            
            self.shareToFacebook(config: config, from: viewController)
            
            return subscription
        }
    }
}
```

### Step 10: Create Integration Example for ViewModels
Provide example of how to integrate with MVVM:

```swift
// MARK: - ViewModel Integration Example
class ShareViewModel {
    
    // MARK: - Properties
    private let facebookShareManager: FacebookShareManagerType
    private let disposeBag = DisposeBag()
    
    // Output Relays
    let shareResult = PublishRelay<FacebookShareResult>()
    let isSharing = BehaviorRelay<Bool>(value: false)
    let errorMessage = PublishRelay<String>()
    
    // MARK: - Initializer
    init(facebookShareManager: FacebookShareManagerType = FacebookShareManager()) {
        self.facebookShareManager = facebookShareManager
        bindShareManager()
    }
    
    // MARK: - Public Methods
    func shareToFacebook(url: String, title: String, description: String?, from viewController: UIViewController) {
        isSharing.accept(true)
        
        let config = FacebookShareConfig.linkShare(
            url: url,
            title: title,
            description: description,
            hashtag: "#ChoTot"
        )
        
        facebookShareManager.shareToFacebook(config: config, from: viewController)
    }
    
    // MARK: - Private Methods
    private func bindShareManager() {
        facebookShareManager.shareResult
            .subscribe(onNext: { [weak self] result in
                self?.isSharing.accept(false)
                self?.shareResult.accept(result)
                
                if case .failure(let error) = result {
                    self?.errorMessage.accept(error.localizedDescription)
                }
            })
            .disposed(by: disposeBag)
    }
}
```

## Critical Implementation Notes

### Facebook SDK Configuration
**MUST DO**: Ensure Facebook SDK is properly configured in your app:
- Add Facebook App ID to Info.plist
- Configure URL schemes in Info.plist
- Initialize Facebook SDK in AppDelegate
- Add required permissions and capabilities

### Error Handling Strategy
**MUST DO**: Implement comprehensive error handling:
- Validate all inputs before attempting to share
- Provide meaningful error messages to users
- Implement fallback to system share when Facebook sharing fails
- Log all errors using `Logger.print()` from CTCommon

### Memory Management
**MUST DO**: Proper resource cleanup:
- Use weak references to prevent retain cycles
- Clean up observers and subscriptions
- Handle view controller deallocation gracefully

### Analytics Requirements
**MUST DO**: Track all sharing events:
- Share attempts (success/failure/cancellation)
- Fallback usage when Facebook sharing fails
- Content type and configuration details
- Error types and frequencies

### UI/UX Guidelines
**MUST DO**: Follow these UX patterns:
- Show loading states during share operations
- Provide clear error messages with retry options
- Offer fallback to system share when Facebook fails
- Use CTDesignSystem components for all UI feedback

## Complete Integration Example

```swift
// In your ViewController or Module
class PaymentViewController: UIViewController {
    
    private let facebookShareManager = FacebookShareManager()
    private let disposeBag = DisposeBag()
    
    override func viewDidLoad() {
        super.viewDidLoad()
        setupFacebookSharing()
    }
    
    private func setupFacebookSharing() {
        facebookShareManager.delegate = self
        
        facebookShareManager.shareResult
            .subscribe(onNext: { [weak self] result in
                self?.handleShareResult(result)
            })
            .disposed(by: disposeBag)
    }
    
    @IBAction private func shareToFacebookTapped() {
        facebookShareManager.shareLink(
            url: "https://chotot.com/ad/123456",
            title: "Xem tin đăng tuyệt vời này!",
            description: "Sản phẩm chất lượng với giá tốt nhất",
            from: self
        )
    }
    
    private func handleShareResult(_ result: FacebookShareResult) {
        switch result {
        case .success(let results):
            Logger.print("Share successful: \(results)")
        case .failure(let error):
            Logger.print("Share failed: \(error)")
        case .cancelled:
            Logger.print("Share cancelled")
        }
    }
}

extension PaymentViewController: FacebookShareDelegate {
    func facebookShareDidComplete(with result: FacebookShareResult) {
        // Handle completion if needed
    }
    
    func facebookShareWillShow() {
        // Handle will show if needed
    }
    
    func facebookShareDidShow() {
        // Handle did show if needed
    }
}
```

## Expected Outcome
You should have a comprehensive Facebook sharing system that:
- ✅ Supports multiple content types (link, photo, video, story)
- ✅ Includes robust error handling and validation
- ✅ Provides fallback to system share when Facebook fails
- ✅ Tracks detailed analytics for all share events
- ✅ Follows MVVM + Clean Architecture patterns
- ✅ Uses only CTDesignSystem components
- ✅ Implements proper memory management and cleanup
- ✅ Provides both imperative and reactive (RxSwift) APIs
- ✅ Offers convenient methods for common use cases
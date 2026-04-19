Generate and implement a complete UseCase following CTCorePayment 6-layer Clean Architecture:

UseCase: FetchDongtotProfile
Input: String
Output: String 
Endpoint: "v1/dongtot/profile"
Method: get

Implement all 6 layers by adding code directly to project files:

1. Add Api.FetchDongtotProfile = "v1/dongtot/profile" to CRNetworkHelper.swift
2. Add FetchDongtotProfileTarget struct to CRCheckoutTargets.swift
3. Add FetchDongtotProfile method to CRCheckoutService.swift (protocol + implementation)
4. Add FetchDongtotProfile method to CRCheckoutCartRepository.swift (protocol + implementation)
5. Add CRFetchDongtotProfileUseCase class to CRCheckoutUseCase.swift
6. Add executeFetchDongtotProfile method to CRCheckoutPageViewModel.swift (with RxSwift bindings + error handling)

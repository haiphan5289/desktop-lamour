Generate and implement a complete UseCase following CTCorePayment 6-layer Clean Architecture:

UseCase: FetchDongtotProfile
Input: String
Output: String 
Endpoint: "v1/dongtot/profile"
Method: get

Implement all 6 layers by adding code directly to project files:

1. Add Api.FetchDongtotProfile = "v1/dongtot/profile" to CRNetworkHelper.cs
2. Add FetchDongtotProfileTarget struct to CRCheckoutTargets.cs
3. Add FetchDongtotProfile method to CRCheckoutService.cs (protocol + implementation)
4. Add FetchDongtotProfile method to CRCheckoutCartRepository.cs (protocol + implementation)
5. Add CRFetchDongtotProfileUseCase class to CRCheckoutUseCase.cs
6. Add executeFetchDongtotProfile method to CRCheckoutPageViewModel.cs (with CommunityToolkit.Mvvm bindings + error handling)

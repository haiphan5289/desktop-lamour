// RegisterViewModelTests.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.Authentication.Domain.Models;
using DesktopLamour.Features.Authentication.Domain.UseCases;
using DesktopLamour.Features.Authentication.ViewModels;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DesktopLamour.Tests.Features.Authentication;

public class RegisterViewModelTests
{
    // ── Mocks ────────────────────────────────────────────────────────────────
    private readonly Mock<ICheckPhoneExistUseCase> _checkPhoneMock   = new();
    private readonly Mock<ISignUpWithPhoneUseCase> _signUpMock       = new();
    private readonly Mock<INavigationService>      _navMock          = new();
    private readonly Mock<IAuthTokenStorage>       _tokenStorageMock = new();

    // ── Helpers ───────────────────────────────────────────────────────────────
    private RegisterViewModel CreateVm() => new(
        _checkPhoneMock.Object,
        _signUpMock.Object,
        _navMock.Object,
        _tokenStorageMock.Object,
        NullLogger<RegisterViewModel>.Instance);

    // ── Continue command (Phone step) ─────────────────────────────────────────

    [Fact]
    public async Task ContinueCommand_WhenPhoneIsFree_AdvancesToPasswordStep()
    {
        // Arrange
        _checkPhoneMock
            .Setup(x => x.ExecuteAsync("0912345678", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var vm = CreateVm();
        vm.PhoneNumber = "0912345678";

        // Act
        await vm.ContinueCommand.ExecuteAsync(null);

        // Assert
        vm.CurrentStep.Should().Be(RegisterStep.Password);
        vm.PhoneError.Should().BeNullOrEmpty();
        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task ContinueCommand_WhenPhoneAlreadyRegistered_SetsPhoneError()
    {
        // Arrange
        _checkPhoneMock
            .Setup(x => x.ExecuteAsync("0912345678", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var vm = CreateVm();
        vm.PhoneNumber = "0912345678";

        // Act
        await vm.ContinueCommand.ExecuteAsync(null);

        // Assert
        vm.CurrentStep.Should().Be(RegisterStep.Phone);
        vm.PhoneError.Should().NotBeNullOrEmpty();
        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task ContinueCommand_WhenNetworkFails_SetsErrorMessage()
    {
        // Arrange
        _checkPhoneMock
            .Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        var vm = CreateVm();
        vm.PhoneNumber = "0912345678";

        // Act
        await vm.ContinueCommand.ExecuteAsync(null);

        // Assert
        vm.ErrorMessage.Should().NotBeNullOrEmpty();
        vm.CurrentStep.Should().Be(RegisterStep.Phone);
        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task ContinueCommand_SetsIsLoading_TrueThenFalse()
    {
        // Arrange
        var tcs = new TaskCompletionSource<bool>();
        _checkPhoneMock
            .Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        var vm = CreateVm();
        vm.PhoneNumber = "0912345678";

        // Act — fire without awaiting to observe mid-flight state
        var task = vm.ContinueCommand.ExecuteAsync(null);
        vm.IsLoading.Should().BeTrue();

        tcs.SetResult(false);
        await task;

        vm.IsLoading.Should().BeFalse();
    }

    // ── Register command (Password step) ─────────────────────────────────────

    [Fact]
    public async Task RegisterCommand_OnSuccess_NavigatesToMain()
    {
        // Arrange
        _signUpMock
            .Setup(x => x.ExecuteAsync(It.IsAny<RegisterInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserInfo { UserId = 1, AccessToken = "tok123" });

        var vm = CreateVm();
        vm.PhoneNumber      = "0912345678";
        vm.Password         = "Password1";
        vm.ConfirmPassword  = "Password1";

        // Act
        await vm.RegisterCommand.ExecuteAsync(null);

        // Assert
        _navMock.Verify(x => x.NavigateTo(NavigationRoutes.Main), Times.Once);
        _tokenStorageMock.Verify(x => x.SaveToken("tok123"), Times.Once);
        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterCommand_OnNetworkError_SetsErrorMessage()
    {
        // Arrange
        _signUpMock
            .Setup(x => x.ExecuteAsync(It.IsAny<RegisterInput>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Server error"));

        var vm = CreateVm();
        vm.PhoneNumber     = "0912345678";
        vm.Password        = "Password1";
        vm.ConfirmPassword = "Password1";

        // Act
        await vm.RegisterCommand.ExecuteAsync(null);

        // Assert
        vm.ErrorMessage.Should().NotBeNullOrEmpty();
        _navMock.Verify(x => x.NavigateTo(It.IsAny<string>()), Times.Never);
        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterCommand_WhenValidationExceptionForPassword_SetsPasswordError()
    {
        // Arrange
        _signUpMock
            .Setup(x => x.ExecuteAsync(It.IsAny<RegisterInput>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Core.Exceptions.ValidationException(
                nameof(RegisterInput.Password),
                "Password too weak."));

        var vm = CreateVm();
        vm.PhoneNumber     = "0912345678";
        vm.Password        = "Password1";
        vm.ConfirmPassword = "Password1";

        // Act
        await vm.RegisterCommand.ExecuteAsync(null);

        // Assert
        vm.PasswordError.Should().Be("Password too weak.");
        vm.ErrorMessage.Should().BeNullOrEmpty();
    }

    // ── Real-time validation ──────────────────────────────────────────────────

    [Theory]
    [InlineData("0912345678", null)]          // valid Vietnamese phone — no error
    [InlineData("123",        "valid")]       // too short — error shown
    [InlineData("1234567890", "valid")]       // wrong prefix — error shown
    public void PhoneNumber_RealtimeValidation_SetsPhoneErrorCorrectly(
        string phone, string? expectedErrorState)
    {
        var vm = CreateVm();
        vm.PhoneNumber = phone;

        if (expectedErrorState is null)
            vm.PhoneError.Should().BeNullOrEmpty();
        else
            vm.PhoneError.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ConfirmPassword_WhenMismatch_SetsConfirmPasswordError()
    {
        var vm = CreateVm();
        vm.Password        = "Password1";
        vm.ConfirmPassword = "Different1";

        vm.ConfirmPasswordError.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ConfirmPassword_WhenMatch_ClearsConfirmPasswordError()
    {
        var vm = CreateVm();
        vm.Password        = "Password1";
        vm.ConfirmPassword = "Password1";

        vm.ConfirmPasswordError.Should().BeNullOrEmpty();
    }

    // ── Step navigation ───────────────────────────────────────────────────────

    [Fact]
    public void GoBackCommand_FromPasswordStep_ReturnsToPhonesStep()
    {
        var vm = CreateVm();
        vm.PhoneNumber = "0912345678";

        // Simulate advancing to password step
        typeof(RegisterViewModel)
            .GetProperty(nameof(RegisterViewModel.CurrentStep))!
            .SetValue(vm, RegisterStep.Password);

        // Act
        vm.GoBackCommand.Execute(null);

        // Assert
        vm.CurrentStep.Should().Be(RegisterStep.Phone);
        vm.Password.Should().BeEmpty();
        vm.ConfirmPassword.Should().BeEmpty();
    }

    [Fact]
    public void IsPhoneStep_IsTrueInitially()
    {
        var vm = CreateVm();
        vm.IsPhoneStep.Should().BeTrue();
        vm.IsPasswordStep.Should().BeFalse();
    }
}

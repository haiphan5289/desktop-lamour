// SignUpWithPhoneUseCaseTests.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Core.Exceptions;
using DesktopLamour.Features.Authentication.Data.Repositories;
using DesktopLamour.Features.Authentication.Domain.Models;
using DesktopLamour.Features.Authentication.Domain.UseCases;
using FluentAssertions;
using Moq;

namespace DesktopLamour.Tests.Features.Authentication;

public class SignUpWithPhoneUseCaseTests
{
    private readonly Mock<IAuthenticationRepository> _repoMock = new();

    private SignUpWithPhoneUseCase CreateUseCase() => new(_repoMock.Object);

    // ── Validation: phone ─────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("1234567890")]    // invalid prefix
    [InlineData("abcdefghij")]   // non-numeric
    public async Task ExecuteAsync_WithInvalidPhone_ThrowsValidationException(string phone)
    {
        var input = new RegisterInput(phone, "Password1", null);

        var act = async () => await CreateUseCase().ExecuteAsync(input);

        await act.Should()
            .ThrowAsync<ValidationException>()
            .Where(e => e.Field == nameof(RegisterInput.PhoneNumber));
    }

    [Fact]
    public async Task ExecuteAsync_WithValidVietnamesePhone_DoesNotThrowForPhone()
    {
        _repoMock
            .Setup(x => x.SignUpAsync(It.IsAny<RegisterInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserInfo { UserId = 1 });

        var input = new RegisterInput("0912345678", "Password1", null);
        var act   = async () => await CreateUseCase().ExecuteAsync(input);

        await act.Should().NotThrowAsync<ValidationException>();
    }

    // ── Validation: password ──────────────────────────────────────────────────

    [Theory]
    [InlineData("short")]       // < 8 chars
    [InlineData("alllower1")]   // no uppercase
    [InlineData("ALLUPPER")]    // no digit
    public async Task ExecuteAsync_WithWeakPassword_ThrowsValidationException(string password)
    {
        var input = new RegisterInput("0912345678", password, null);

        var act = async () => await CreateUseCase().ExecuteAsync(input);

        await act.Should()
            .ThrowAsync<ValidationException>()
            .Where(e => e.Field == nameof(RegisterInput.Password));
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WithValidInput_ReturnsUserFromRepository()
    {
        var expected = new UserInfo { UserId = 42, Phone = "0912345678", AccessToken = "token" };
        _repoMock
            .Setup(x => x.SignUpAsync(It.IsAny<RegisterInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var input  = new RegisterInput("0912345678", "StrongPass1", "Alice");
        var result = await CreateUseCase().ExecuteAsync(input);

        result.Should().BeEquivalentTo(expected);
        _repoMock.Verify(x => x.SignUpAsync(
            It.Is<RegisterInput>(i =>
                i.PhoneNumber  == "0912345678" &&
                i.Password     == "StrongPass1" &&
                i.DisplayName  == "Alice"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryThrows_PropagatesException()
    {
        _repoMock
            .Setup(x => x.SignUpAsync(It.IsAny<RegisterInput>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Server unavailable"));

        var input = new RegisterInput("0912345678", "Password1", null);
        var act   = async () => await CreateUseCase().ExecuteAsync(input);

        await act.Should().ThrowAsync<HttpRequestException>();
    }
}

// IUseCase.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

namespace DesktopLamour.Core.UseCases;

public interface IUseCase<TInput, TOutput>
{
    Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken = default);
}

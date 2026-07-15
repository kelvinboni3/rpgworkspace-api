using Microsoft.Extensions.Options;
using RpgWorkspace.Application.DTOs.AiUsage;
using RpgWorkspace.Application.Exceptions;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Configuration;

namespace RpgWorkspace.Infrastructure.Services;

public sealed class AiUsageService : IAiUsageService
{
    private readonly IAiUsageRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly int _monthlyLimit;

    public AiUsageService(
        IAiUsageRepository repository,
        IUnitOfWork unitOfWork,
        IOptions<AnthropicSettings> settings)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _monthlyLimit = settings.Value.MonthlyCallLimitPerUser;
    }

    private static string CurrentPeriod() => DateTime.UtcNow.ToString("yyyy-MM");

    private static DateTime PeriodEndUtc()
    {
        var now = DateTime.UtcNow;
        return new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);
    }

    public async Task EnsureWithinQuotaAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var usage = await _repository.GetByUserAndPeriodAsync(userId, CurrentPeriod(), cancellationToken);
        if ((usage?.CallCount ?? 0) >= _monthlyLimit)
            throw new AiQuotaExceededException(
                $"Você atingiu o limite de {_monthlyLimit} usos de IA neste mês. O limite renova no início do próximo mês.");
    }

    public async Task TrackAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var period = CurrentPeriod();
        var usage = await _repository.GetByUserAndPeriodAsync(userId, period, cancellationToken);
        if (usage is null)
        {
            usage = AiUsage.Create(userId, period);
            usage.Increment();
            await _repository.AddAsync(usage, cancellationToken);
        }
        else
        {
            usage.Increment();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<AiUsageStatus> GetStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var usage = await _repository.GetByUserAndPeriodAsync(userId, CurrentPeriod(), cancellationToken);
        var used = usage?.CallCount ?? 0;
        var remaining = Math.Max(0, _monthlyLimit - used);
        return new AiUsageStatus(used, _monthlyLimit, remaining, PeriodEndUtc());
    }
}

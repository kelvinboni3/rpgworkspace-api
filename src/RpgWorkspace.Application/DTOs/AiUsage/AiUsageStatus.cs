namespace RpgWorkspace.Application.DTOs.AiUsage;

/// <summary>Quanto de IA o usuário já usou no mês atual e quanto resta, para exibir no app.</summary>
public sealed record AiUsageStatus(int Used, int Limit, int Remaining, DateTime PeriodEndUtc);

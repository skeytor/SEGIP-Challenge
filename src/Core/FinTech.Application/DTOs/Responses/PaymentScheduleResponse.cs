using FinTech.Domain.Entities;

namespace FinTech.Application.DTOs.Responses;

public sealed record PaymentScheduleResponse(
    int PaymentNumber,
    DateTime DueDate,
    decimal TotalPayment,
    decimal Principal,
    decimal Interest,
    decimal RemainingBalance,
    PaymentScheduleStatus Status);

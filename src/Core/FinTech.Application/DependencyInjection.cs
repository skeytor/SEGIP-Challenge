using FinTech.Application.Abstractions;
using FinTech.Application.DTOs.Responses;
using FinTech.Application.UseCases.Loans.Approve;
using FinTech.Application.UseCases.Loans.Create;
using FinTech.Application.UseCases.Loans.GetAll;
using FinTech.Application.UseCases.Loans.GetById;
using FinTech.Application.UseCases.Loans.GetSchedule;
using FinTech.Application.UseCases.Loans.Reject;
using FinTech.Application.UseCases.Loans.Simulate;
using FinTech.Application.UseCases.Transactions.Create;
using FinTech.Application.UseCases.Transactions.GetAll;
using FinTech.Application.UseCases.Transactions.GetById;
using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services) =>
        services
            .AddScoped<ICommandHandler<ApplyForLoanCommand, LoanResponse>, ApplyForLoanCommandHandler>()
            .AddScoped<ICommandHandler<SimulateLoanCommand, SimulateLoanResponse>, SimulateLoanCommandHandler>()
            .AddScoped<ICommandHandler<ApproveLoanCommand, LoanResponse>, ApproveLoanCommandHandler>()
            .AddScoped<ICommandHandler<RejectLoanCommand, LoanResponse>, RejectLoanCommandHandler>()
            .AddScoped<IQueryHandler<GetLoanByIdQuery, LoanResponse>, GetLoanByIdQueryHandler>()
            .AddScoped<IQueryHandler<GetLoansQuery, IReadOnlyCollection<LoanResponse>>, GetLoansQueryHandler>()
            .AddScoped<IQueryHandler<GetLoanScheduleQuery, IReadOnlyCollection<PaymentScheduleResponse>>, GetLoanScheduleQueryHandler>()
            .AddScoped<ICommandHandler<CreateTransactionCommand, TransactionResponse>, CreateTransactionCommandHandler>()
            .AddScoped<IQueryHandler<GetTransactionByIdQuery, TransactionResponse>, GetTransactionByIdQueryHandler>()
            .AddScoped<IQueryHandler<GetTransactionsQuery, IReadOnlyCollection<TransactionResponse>>, GetTransactionsQueryHandler>();
}

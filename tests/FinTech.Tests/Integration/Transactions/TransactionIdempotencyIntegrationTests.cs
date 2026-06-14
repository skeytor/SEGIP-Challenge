using System.Net;
using System.Net.Http.Json;
using FinTech.Application.DTOs.Requests;
using FinTech.Application.DTOs.Responses;
using FinTech.Persistence;
using FinTech.Tests.Integration.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Tests.Integration.Transactions;

public class TransactionIdempotencyIntegrationTests(WebApplicationFactoryFixture<Program> factory) 
    : IClassFixture<WebApplicationFactoryFixture<Program>>
{
    private readonly HttpClient _httpClient = factory.CreateClient();
    private readonly WebApplicationFactoryFixture<Program> _factory = factory;

    [Theory]
    [InlineData("integration-key-001")]
    public async Task PostTransaction_SameIdempotencyKey_ReturnsOriginalTransactionWithoutCreatingDuplicate(string idempotencyKey)
    {
        CreateTransactionRequest body = new(
            IdempotencyKey: idempotencyKey,
            Type: Domain.Entities.TransactionType.Payment,
            Amount: 850.50m,
            LoanId: null,
            Description: "A short description test"
        );

        HttpResponseMessage firstResponse = await _httpClient.PostAsJsonAsync("/api/transactions", body);
        HttpResponseMessage secondResponse = await _httpClient.PostAsJsonAsync("/api/transactions", body);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        TransactionResponse? firstTransaction = await firstResponse.Content.ReadFromJsonAsync<TransactionResponse>();
        TransactionResponse? secondTransaction = await secondResponse.Content.ReadFromJsonAsync<TransactionResponse>();

        Assert.Equal(firstTransaction!.Id, secondTransaction!.Id);
        Assert.Equal(firstTransaction.Amount, secondTransaction.Amount);

        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        int count = await db.Transactions.CountAsync(t => t.IdempotencyKey == idempotencyKey);
        Assert.Equal(1, count);
    }

    [Theory]
    [InlineData("integration-key-001-A", "integration-key-001-B")]
    public async Task PostTransaction_DifferentIdempotencyKeys_CreatesSeparateTransactions(string keyA, string keyB)
    {
        CreateTransactionRequest firstBody = new(
            IdempotencyKey: keyA,
            Type: Domain.Entities.TransactionType.Payment,
            Amount: 850.50m,
            LoanId: null,
            Description: "A short description test A"
        );

        CreateTransactionRequest secondBody = new(
            IdempotencyKey: keyB,
            Type: Domain.Entities.TransactionType.Payment,
            Amount: 900.50m,
            LoanId: null,
            Description: "A short description test B"
        );

        HttpResponseMessage firstResponse = await _httpClient.PostAsJsonAsync("/api/transactions", firstBody);
        HttpResponseMessage secondResponse = await _httpClient.PostAsJsonAsync("/api/transactions", secondBody);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        TransactionResponse? firstTransaction = await firstResponse.Content.ReadFromJsonAsync<TransactionResponse>();
        TransactionResponse? secondTransaction = await secondResponse.Content.ReadFromJsonAsync<TransactionResponse>();

        Assert.NotEqual(firstTransaction!.Id, secondTransaction!.Id);

        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        int count = await db.Transactions.CountAsync(t =>
            t.IdempotencyKey == keyA ||
            t.IdempotencyKey == keyB);
        
        Assert.Equal(2, count);
    }

    [Theory]
    [InlineData("integration-key-amount-test")]
    public async Task PostTransaction_SameKeyDifferentAmount_IgnoresNewAmountAndReturnsOriginal(string idempotencyKey)
    {
        CreateTransactionRequest originalBody = new(
            IdempotencyKey: idempotencyKey,
            Type: Domain.Entities.TransactionType.Payment,
            Amount: 500.00m,
            LoanId: null,
            Description: "Original Payment"
        );

        CreateTransactionRequest duplicateBody = new(
            IdempotencyKey: idempotencyKey,
            Type: Domain.Entities.TransactionType.Payment,
            Amount: 9999.99m,
            LoanId: null,
            Description: "Try with different amount"
        );

        await _httpClient.PostAsJsonAsync("/api/transactions", originalBody);
        HttpResponseMessage duplicateResponse = await _httpClient.PostAsJsonAsync("/api/transactions", duplicateBody);

        TransactionResponse? duplicate = await duplicateResponse.Content.ReadFromJsonAsync<TransactionResponse>();

        Assert.Equal(HttpStatusCode.OK, duplicateResponse.StatusCode);
        Assert.Equal(500.00m, duplicate!.Amount);
    }
}

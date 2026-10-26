using System;
using System.Collections.Generic;
using System.Linq;

internal class TransactionEventArgs : EventArgs
{
    public Transaction Transaction { get; }

    public TransactionEventArgs(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        Transaction = transaction;
    }
}

internal delegate void ThresholdExceededEventHandler(
    int count,
    decimal sum);

internal class TransactionProcessor
{
    private decimal dayLimitTransactions = -1m;

    public List<Transaction> Transactions { get; } = new();

    public decimal DayLimitTransactions => dayLimitTransactions;

    public event EventHandler<TransactionEventArgs>? TransactionAdded;

    public event ThresholdExceededEventHandler? ThresholdExceeded;

    public TransactionProcessor()
    {
    }

    public TransactionProcessor(decimal limit)
    {
        SetDailyThreshold(limit);
    }

    public void AddTransaction(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        // Проверяем уникальность идентификатора.
        foreach (Transaction existing in Transactions)
        {
            if (existing.Id == transaction.Id)
            {
                throw new ArgumentException(
                    $"Транзакция с Id = {transaction.Id} уже существует.",
                    nameof(transaction));
            }
        }

        Transactions.Add(transaction);

        // Сообщаем о добавлении транзакции.
        TransactionAdded?.Invoke(
            this,
            new TransactionEventArgs(transaction));

        // Если лимит не задан, превышение не проверяем.
        if (dayLimitTransactions == -1m)
        {
            return;
        }

        // Считаем сумму транзакций за текущий день.
        decimal todayTotal = Transactions
            .Where(t => t.Date.Date == DateTime.Today)
            .Sum(t => t.Amount);

        if (todayTotal > dayLimitTransactions)
        {
            int todayCount = Transactions
                .Count(t => t.Date.Date == DateTime.Today);

            ThresholdExceeded?.Invoke(todayCount, todayTotal);
        }
    }

    public void SetDailyThreshold(decimal limit)
    {
        if (limit < 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(limit),
                "Дневной лимит не может быть отрицательным.");
        }

        dayLimitTransactions = limit;

        // Проверяем уже добавленные транзакции за сегодняшний день.
        decimal todayTotal = Transactions
            .Where(t => t.Date.Date == DateTime.Today)
            .Sum(t => t.Amount);

        if (todayTotal > dayLimitTransactions)
        {
            int todayCount = Transactions
                .Count(t => t.Date.Date == DateTime.Today);

            ThresholdExceeded?.Invoke(todayCount, todayTotal);
        }
    }

    public List<Transaction> FilterTransactions(
        Predicate<Transaction> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        List<Transaction> result = new();

        foreach (Transaction transaction in Transactions)
        {
            if (predicate(transaction))
            {
                result.Add(transaction);
            }
        }

        return result;
    }

    public decimal ApplyOperation(
        Func<IEnumerable<Transaction>, decimal> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        return operation(Transactions);
    }

    public void ProcessTransactions(Action<Transaction> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        foreach (Transaction transaction in Transactions)
        {
            operation(transaction);
        }
    }

    public Dictionary<string, decimal> GetTotalByCategory()
    {
        return Transactions
            .GroupBy(t => t.Category)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(t => t.Amount));
    }

    public IEnumerable<Transaction> GetTopExpenses(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                "Количество транзакций не может быть отрицательным.");
        }

        return Transactions
            .OrderByDescending(t => t.Amount)
            .Take(count);
    }

    public decimal GetAverageTransactionAmount(string category)
    {
        ArgumentNullException.ThrowIfNull(category);

        return Transactions
            .Where(t => t.Category == category)
            .Select(t => t.Amount)
            .DefaultIfEmpty(0m)
            .Average();
    }

    public IEnumerable<IGrouping<DateTime, Transaction>> GroupByDate()
    {
        return Transactions
            .GroupBy(t => t.Date.Date);
    }
}
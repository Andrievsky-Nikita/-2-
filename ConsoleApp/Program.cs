using System;
using System.Collections.Generic;
using System.Linq;

TransactionProcessor processor = new();

// Подписка на событие добавления транзакции.
processor.TransactionAdded += (sender, e) =>
{
    Console.WriteLine(
        $"Добавлена транзакция #{e.Transaction.Id}: " +
        $"{e.Transaction.Amount:0.00} {e.Transaction.Currency}, " +
        $"{e.Transaction.Category}");
};

// Подписка на событие превышения дневного лимита.
processor.ThresholdExceeded += (count, sum) =>
{
    Console.WriteLine(
        $"ВНИМАНИЕ! Дневной лимит превышен. " +
        $"Количество транзакций: {count}, сумма: {sum:0.00} USD.");
};

// Устанавливаем лимит до добавления транзакций.
if (!ExecuteSafely(() => processor.SetDailyThreshold(200m)))
{
    return;
}

// Даты нужны для демонстрации группировки по дням.
DateTime today = DateTime.Today;

// Создаём 10 различных транзакций.
List<Transaction> sampleTransactions = new()
{
    new Transaction(1, 25m, "USD", today.AddHours(8),
        "Food", "Breakfast"),

    new Transaction(2, 12m, "USD", today.AddHours(9),
        "Transport", "Bus ticket"),

    new Transaction(3, 70m, "USD", today.AddHours(10),
        "Food", "Groceries"),

    new Transaction(4, 150m, "USD", today.AddHours(11),
        "Shopping", "Shoes"),

    new Transaction(5, 40m, "USD", today.AddDays(-1).AddHours(18),
        "Entertainment", "Cinema"),

    new Transaction(6, 85m, "USD", today.AddDays(-1).AddHours(19),
        "Food", "Dinner"),

    new Transaction(7, 9m, "USD", today.AddDays(-1).AddHours(20),
        "Transport", "Subway"),

    new Transaction(8, 120m, "USD", today.AddDays(-2).AddHours(12),
        "Shopping", "Backpack"),

    new Transaction(9, 35m, "USD", today.AddDays(-2).AddHours(13),
        "Food", "Lunch"),

    new Transaction(10, 40m, "USD", today.AddDays(-2).AddHours(14),
        "Bills", "Internet payment")
};

// Добавляем транзакции через метод процессора.
foreach (Transaction transaction in sampleTransactions)
{
    if (!ExecuteSafely(() => processor.AddTransaction(transaction)))
    {
        return;
    }
}

// Фильтрация транзакций по категории.
if (!ExecuteSafely(() =>
{
    List<Transaction> foodTransactions =
        processor.FilterTransactions(t => t.Category == "Food");

    PrintTransactions(
        "Транзакции категории Food:",
        foodTransactions);
}))
{
    return;
}

// Фильтрация транзакций по сумме.
if (!ExecuteSafely(() =>
{
    List<Transaction> expensiveTransactions =
        processor.FilterTransactions(t => t.Amount > 50m);

    PrintTransactions(
        "Транзакции с суммой больше 50 USD:",
        expensiveTransactions);
}))
{
    return;
}

// Применение различных операций через ApplyOperation.
if (!ExecuteSafely(() =>
{
    decimal total = processor.ApplyOperation(
        transactions => transactions.Sum(t => t.Amount));

    Console.WriteLine(
        $"\nОбщая сумма всех транзакций: {total:0.00} USD");

    decimal maximum = processor.ApplyOperation(
        transactions => transactions.Max(t => t.Amount));

    Console.WriteLine(
        $"Максимальная сумма транзакции: {maximum:0.00} USD");
}))
{
    return;
}

// Демонстрация ProcessTransactions.
if (!ExecuteSafely(() =>
{
    Console.WriteLine("\nОбработка всех транзакций:");

    processor.ProcessTransactions(t =>
        Console.WriteLine($"#{t.Id}: {t.Description}"));
}))
{
    return;
}

// LINQ: общая сумма по каждой категории.
if (!ExecuteSafely(() =>
{
    Console.WriteLine("\nСуммы по категориям:");

    Dictionary<string, decimal> totals =
        processor.GetTotalByCategory();

    foreach (var item in totals)
    {
        Console.WriteLine($"{item.Key}: {item.Value:0.00} USD");
    }
}))
{
    return;
}

// LINQ: три самые крупные транзакции.
if (!ExecuteSafely(() =>
{
    Console.WriteLine("\nТри самые крупные транзакции:");

    PrintTransactions(
        "Топ-3:",
        processor.GetTopExpenses(3));
}))
{
    return;
}

// LINQ: средняя сумма транзакции для категории Food.
if (!ExecuteSafely(() =>
{
    decimal average =
        processor.GetAverageTransactionAmount("Food");

    Console.WriteLine(
        $"\nСредняя сумма транзакции категории Food: " +
        $"{average:0.00} USD");
}))
{
    return;
}

// LINQ: группировка транзакций по дате без учёта времени.
if (!ExecuteSafely(() =>
{
    Console.WriteLine("\nГруппировка транзакций по дням:");

    foreach (var group in processor
        .GroupByDate()
        .OrderBy(group => group.Key))
    {
        Console.WriteLine($"\nДата: {group.Key:yyyy-MM-dd}");

        foreach (Transaction transaction in group)
        {
            Console.WriteLine(
                $"  #{transaction.Id}: " +
                $"{transaction.Amount:0.00} USD, " +
                $"{transaction.Category}");
        }
    }
}))
{
    return;
}

Console.WriteLine("\nДемонстрация завершена.");

// Выполняет действие и обрабатывает возможную ошибку.
// Возвращает false, если пользователь решил завершить программу.
static bool ExecuteSafely(Action action)
{
    try
    {
        action();
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"\nОшибка ({ex.GetType().Name}): {ex.Message}");

        Console.Write(
            "Продолжить выполнение, проигнорировав ошибку? (д/н): ");

        string? answer = Console.ReadLine()?.Trim();

        return string.Equals(
                   answer, "д", StringComparison.OrdinalIgnoreCase)
            || string.Equals(
                   answer, "y", StringComparison.OrdinalIgnoreCase);
    }
}

// Выводит список транзакций.
static void PrintTransactions(
    string title,
    IEnumerable<Transaction> transactions)
{
    Console.WriteLine($"\n{title}");

    foreach (Transaction transaction in transactions)
    {
        Console.WriteLine(
            $"  #{transaction.Id}: " +
            $"{transaction.Amount:0.00} {transaction.Currency}, " +
            $"{transaction.Category}, " +
            $"{transaction.Date:g}, " +
            $"{transaction.Description}");
    }
}
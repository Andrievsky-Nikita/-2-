internal class TransactionEventArgs : EventArgs
{
   public Transaction Transaction { get; }

   public TransactionEventArgs(Transaction transaction)
   {
      Transaction = transaction;
   }
}

internal delegate void excessing(int count, decimal sum);

internal class TransactionProcessor
{
   private decimal dayLimitTransactions;
   public List<Transaction> Transactions { get; set; }
   public decimal DayLimitTransactions { get; }
   public event EventHandler<TransactionEventArgs>? TransactionAdded;
   public event excessing? ThresholdExceeded;
   public TransactionProcessor() { dayLimitTransactions = -1; }
   public TransactionProcessor(decimal limit) { dayLimitTransactions = limit; }
   public TransactionProcessor(Transaction transaction)
   {
      dayLimitTransactions = -1;
      Transactions = new List<Transaction>() {transaction};
   }

   public TransactionProcessor(Transaction transaction, decimal limit)
   {
      dayLimitTransactions = limit;
      Transactions = new List<Transaction>() {transaction};
   }

   public TransactionProcessor(List<Transaction> transactions)
   {
      dayLimitTransactions = -1;
      Transactions = new List<Transaction>(transactions);
   }

   public TransactionProcessor(List<Transaction> transactions, decimal limit)
   {
      dayLimitTransactions = limit;
      Transactions = new List<Transaction>(transactions);
   }

   public void AddTransaction(Transaction transaction)
   {
      Transactions.Add(transaction);

      decimal todayTotal = Transactions
         .Where(t => t.Date.Date == DateTime.Today)
         .Sum(t => t.Amount);

      if (todayTotal > dayLimitTransactions)
      {
         int todayCount = Transactions
            .Count(t => t.Date.Date == DateTime.Today);

         ThresholdExceeded?.Invoke(todayCount, todayTotal);
      }

      TransactionAdded?.Invoke(this, new TransactionEventArgs(transaction));
   }

   public void SetDailyThreshold(decimal limit)
   {
      dayLimitTransactions = limit;

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
}
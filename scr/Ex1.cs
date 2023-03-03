using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

class Ex1
{
    public static void Run()
    {
        string csvPath = "transactions.csv";
        string outputDirectory = "output";
        string dateFormat = "yyyy-MM-dd";
        int batchSize = 10;

        Func<string, DateTime> getDate = (line) =>
        {
            var values = line.Split(',');
            return DateTime.ParseExact(values[0], dateFormat, CultureInfo.InvariantCulture);
        };

        Func<string, double> getAmount = (line) =>
        {
            var values = line.Split(',');
            return double.Parse(values[1], CultureInfo.InvariantCulture);
        };

        Action<DateTime, double> displayTotalAmount = (date, total) =>
        {
            Console.WriteLine($"Загальна витрачена сума за {date.ToString(dateFormat)}: {total}");
        };

        var transactions = new List<Transaction>();
        using (var reader = new StreamReader(csvPath))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var transaction = new Transaction(getDate(line), getAmount(line));
                transactions.Add(transaction);
            }
        }

        var groups = GroupByDay(transactions);

        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        int batchIndex = 0;
        var batch = new List<Transaction>();
        foreach (var group in groups)
        {
            batch.Add(group);
            if (batch.Count == batchSize)
            {
                WriteBatchToFile(outputDirectory, batchIndex, batch);
                batch.Clear();
                batchIndex++;
            }
        }

        if (batch.Count > 0)
        {
            WriteBatchToFile(outputDirectory, batchIndex, batch);
        }
    }

    static IEnumerable<Transaction> GroupByDay(IEnumerable<Transaction> transactions)
    {
        var groups = new Dictionary<DateTime, Transaction>();
        foreach (var transaction in transactions)
        {
            if (groups.TryGetValue(transaction.Date, out var existing))
            {
                groups[transaction.Date] = new Transaction(transaction.Date, existing.Amount + transaction.Amount);
            }
            else
            {
                groups[transaction.Date] = transaction;
            }
        }
        return groups.Values;
    }

    static void WriteBatchToFile(string outputDirectory, int batchIndex, IEnumerable<Transaction> transactions)
    {
        string filePath = Path.Combine(outputDirectory, $"output-{batchIndex}.csv");
        using (var writer = new StreamWriter(filePath))
        {
            foreach (var transaction in transactions)
            {
                writer.WriteLine($"{transaction.Date.ToString("yyyy-MM-dd")},{transaction.Amount.ToString(CultureInfo.InvariantCulture)}");
            }
        }
    }

    class Transaction
    {
        public DateTime Date { get; }
        public double Amount { get; }

        public Transaction(DateTime date, double amount)
        {
            Date = date;
            Amount = amount;
        }
    }
}

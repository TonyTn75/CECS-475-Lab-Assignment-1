using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stock
{
    public class StockBroker
    {
        public string BrokerName { get; set; }

        // List of stocks controlled by this broker.
        public List<Stock> stocks =
            new List<Stock>();

        private readonly string destPath =
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Lab1_Output.txt");

        public string titles =
            "Broker".PadRight(10) +
            "Stock".PadRight(15) +
            "Value".PadRight(10) +
            "Changes".PadRight(10) +
            "Date and Time";

        // Prevents multiple writes at the same time.
        private static readonly SemaphoreSlim semaphore =
            new SemaphoreSlim(1, 1);

        // Ensures the header prints only once.
        private static int count = 0;

        // Stores asynchronous file-writing tasks.
        private static readonly List<Task> pendingWrites =
            new List<Task>();

        private static readonly object taskLock =
            new object();

        public StockBroker(string brokerName)
        {
            BrokerName = brokerName;
        }

        public void AddStock(Stock stock)
        {
            stocks.Add(stock);

            // Subscribe to the Rx.NET observable.
            stock.StockObservable.Subscribe(
                notification =>
                {
                    Task writeTask =
                        EventHandlerAsync(notification);

                    lock (taskLock)
                    {
                        pendingWrites.Add(writeTask);
                    }
                });
        }

        // Handles the Rx.NET notification asynchronously.
        private async Task EventHandlerAsync(
            StockNotification notification)
        {
            await WriteAsync(notification);
        }

        // Writes the notification to the console and file.
        private async Task WriteAsync(
            StockNotification notification)
        {
            string line =
                BrokerName.PadRight(16) +
                notification.StockName.PadRight(16) +
                notification.CurrentValue
                    .ToString()
                    .PadRight(16) +
                notification.NumChanges
                    .ToString()
                    .PadRight(16) +
                DateTime.Now;

            await semaphore.WaitAsync();

            try
            {
                if (Interlocked.CompareExchange(
                        ref count,
                        1,
                        0) == 0)
                {
                    Console.WriteLine(titles);

                    using StreamWriter outputFile =
                        new StreamWriter(
                            destPath,
                            false);

                    await outputFile.WriteLineAsync(
                        titles);
                }

                using StreamWriter appendFile =
                    new StreamWriter(
                        destPath,
                        true);

                await appendFile.WriteLineAsync(line);

                Console.WriteLine(line);
            }
            catch (IOException ex)
            {
                Console.WriteLine(
                    $"Error writing to file: {ex.Message}");
            }
            finally
            {
                semaphore.Release();
            }
        }

        // Waits for all asynchronous writes to finish.
        public static async Task WaitForWritesAsync()
        {
            Task[] writes;

            lock (taskLock)
            {
                writes = pendingWrites.ToArray();
            }

            await Task.WhenAll(writes);
        }
    }
}

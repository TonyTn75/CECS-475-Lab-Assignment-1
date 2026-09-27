using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace Stock
{
    public class StockBroker
    {
        public string BrokerName { get; set; }

        public List<Stock> stocks =
            new List<Stock>();

        readonly string destPath =
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Lab1_Output.txt");

        public string titles =
            "Broker".PadRight(10) +
            "Stock".PadRight(15) +
            "Value".PadRight(10) +
            "Changes".PadRight(10) +
            "Date and Time";

        private static readonly SemaphoreSlim semaphore =
            new SemaphoreSlim(1, 1);

        private static int count = 0;

        public StockBroker(string brokerName)
        {
            BrokerName = brokerName;
        }

        public void AddStock(Stock stock)
        {
            stocks.Add(stock);

            stock.StockObservable.Subscribe(
                e => EventHandler(stock, e));
        }

        public async void EventHandler(
            object sender,
            StockNotification e)
        {
            Stock newStock = (Stock)sender;

            await write(sender, e);

            return;
        }

        public async Task write(
            object sender,
            StockNotification e)
        {
            string line =
                BrokerName.PadRight(16) +
                e.StockName.PadRight(16) +
                Convert.ToString(e.CurrentValue).PadRight(16) +
                Convert.ToString(e.NumChanges).PadRight(16) +
                DateTime.Now;

            await semaphore.WaitAsync();

            try
            {
                if (count == 0)
                {
                    Console.WriteLine(titles);

                    using (StreamWriter outputFile =
                           new StreamWriter(destPath, false))
                    {
                        await outputFile.WriteLineAsync(titles);
                    }

                    count++;
                }

                using (StreamWriter outputFile =
                       new StreamWriter(destPath, true))
                {
                    await outputFile.WriteLineAsync(line);
                }

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
    }
}
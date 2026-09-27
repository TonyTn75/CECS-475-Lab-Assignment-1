using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Stock
{
    public class StockBroker
    {
        public string BrokerName { get; set; }
        public List<Stock> stocks = new List<Stock>();
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
        public StockBroker(string brokerName)
        {
            BrokerName = brokerName;

            Console.WriteLine(titles);

            // false means overwrite the old file contents.
            using (StreamWriter outputFile =
                   new StreamWriter(destPath, false))
            {
                outputFile.WriteLine(titles);
            }
        }

        public void AddStock(Stock stock)
        {
            stocks.Add(stock);

            // Subscribe this broker to the stock event.
            stock.StockEvent += EventHandler;
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
            string message =
                $"{BrokerName.PadRight(10)}" +
                $"{e.StockName.PadRight(15)}" +
                $"{e.CurrentValue.ToString().PadRight(10)}" +
                $"{e.NumChanges.ToString().PadRight(10)}" +
                $"{DateTime.Now}";

            try
            {
                using (StreamWriter outputFile =
                       new StreamWriter(destPath, true))
                {
                    await outputFile.WriteLineAsync(message);
                }

                Console.WriteLine(message);
            }
            catch (IOException ex)
            {
                Console.WriteLine(
                    $"Error writing to file: {ex.Message}");
            }

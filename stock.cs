using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Stock
{
    public class Stock
    {
        // Rx.NET Subject used to send stock notifications.
        private readonly Subject<StockNotification>
            stockSubject =
                new Subject<StockNotification>();

        // Public observable that brokers subscribe to.
        public IObservable<StockNotification>
            StockObservable
        {
            get
            {
                return stockSubject.AsObservable();
            }
        }

        // Name of our stock.
        private readonly string _name;

        // Starting value of the stock.
        private readonly int _initialValue;

        // Maximum possible stock change.
        private readonly int _maxChange;

        // Notification threshold.
        private readonly int _threshold;

        // Number of stock changes.
        private int _numChanges;

        // Current stock value.
        private int _currentValue;

        private readonly Random _random =
            new Random();

        public string StockName
        {
            get { return _name; }
        }

        public int InitialValue
        {
            get { return _initialValue; }
        }

        public int CurrentValue
        {
            get { return _currentValue; }
        }

        public int MaxChange
        {
            get { return _maxChange; }
        }

        public int Threshold
        {
            get { return _threshold; }
        }

        public int NumChanges
        {
            get { return _numChanges; }
        }

        public Stock(
            string name,
            int startingValue,
            int maxChange,
            int threshold)
        {
            _name = name;
            _initialValue = startingValue;
            _currentValue = startingValue;
            _maxChange = maxChange;
            _threshold = threshold;
            _numChanges = 0;
        }

        // Asynchronously changes the stock every 500 milliseconds.
        public async Task ActivateAsync()
        {
            for (int i = 0; i < 25; i++)
            {
                // Does not block the thread.
                await Task.Delay(500);

                ChangeStockValue();
            }

            // Tell Rx.NET that the stock is finished.
            stockSubject.OnCompleted();
        }

        // Changes the stock value and sends an Rx notification.
        public void ChangeStockValue()
        {
            // Allows the value to increase or decrease.
            int change = _random.Next(
                -MaxChange,
                MaxChange + 1);

            _currentValue += change;
            _numChanges++;

            int difference = Math.Abs(
                CurrentValue - InitialValue);

            if (difference > Threshold)
            {
                StockNotification notification =
                    new StockNotification(
                        StockName,
                        CurrentValue,
                        NumChanges);

                // Send notification to all subscribers.
                stockSubject.OnNext(notification);
            }
        }
    }
}

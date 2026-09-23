using System;
using System.Threading;

namespace Stock
{
    public class Stock
    {
        public event EventHandler<StockNotification>? StockEvent;

        private string _name;
        private int _initialValue;
        private int _maxChange;
        private int _threshold;
        private int _numChanges;
        private int _currentValue;

        private readonly Thread _thread;
        private readonly Random _random = new Random();

        public string StockName
        {
            get { return _name; }
            set { _name = value; }
        }

        public int InitialValue
        {
            get { return _initialValue; }
        }

        public int CurrentValue
        {
            get { return _currentValue; }
            set { _currentValue = value; }
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
            set { _numChanges = value; }
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

            _thread = new Thread(Activate);
            _thread.Start();
        }

        public void Activate()
        {
            for (int i = 0; i < 25; i++)
            {
                Thread.Sleep(500);
                ChangeStockValue();
            }
        }

        public void ChangeStockValue()
        {
            // Allows the stock to increase or decrease.
            int change = _random.Next(1, MaxChange + 1);

            CurrentValue += change;
            NumChanges++;

            int difference = Math.Abs(
                CurrentValue - InitialValue);

            if (difference > Threshold)
            {
                StockEvent?.Invoke(
                    this,
                    new StockNotification(
                        StockName,
                        CurrentValue,
                        NumChanges));
            }
        }
    }
}
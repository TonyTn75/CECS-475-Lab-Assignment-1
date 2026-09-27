using System;
using System.Threading;
using System.Reactive.Subjects;

namespace Stock
{
    public class Stock
    {
        private Subject<StockNotification> stockSubject =
            new Subject<StockNotification>();

        public IObservable<StockNotification> StockObservable
        {
            get { return stockSubject; }
        }

        private string _name;
        private int _initialValue;
        private int _maxChange;
        private int _threshold;
        private int _numChanges;
        private int _currentValue;

        private readonly Thread _thread;

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
            _currentValue = InitialValue;
            _maxChange = maxChange;
            _threshold = threshold;
            _numChanges = 0;

            _thread =
                new Thread(new ThreadStart(Activate));

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
            var rand = new Random();

            CurrentValue += rand.Next(1, MaxChange);
            NumChanges++;

            if ((CurrentValue - InitialValue) > Threshold)
            {
                stockSubject.OnNext(
                    new StockNotification(
                        StockName,
                        CurrentValue,
                        NumChanges));
            }
        }
    }
}
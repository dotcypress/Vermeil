#region

using System;
using System.Windows.Threading;

#endregion

namespace Vermeil.Common
{
    internal class OneShotDispatcherTimer
    {
        public event EventHandler Fired;

        private TimeSpan _duration = TimeSpan.Zero;
        private DispatcherTimer _timer;

        public static OneShotDispatcherTimer CreateAndStart(TimeSpan duration, EventHandler callback)
        {
            var timer = new OneShotDispatcherTimer {Duration = duration};
            timer.Fired += callback;
            timer.Start();
            return timer;
        }

        public TimeSpan Duration
        {
            get { return _duration; }
            set
            {
                if (value.TotalMilliseconds < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _duration = value;
            }
        }

        public bool IsStarted
        {
            get { return (_timer != null); }
        }

        private void RaiseFired()
        {
            if (Fired == null)
            {
                return;
            }
            try
            {
                Fired(this, EventArgs.Empty);
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
            {
            }
            // ReSharper restore EmptyGeneralCatchClause
        }

        public void Start()
        {
            if (_timer != null)
            {
                return;
            }

            _timer = new DispatcherTimer {Interval = _duration};
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        public void Stop()
        {
            if (_timer == null)
            {
                return;
            }
            try
            {
                _timer.Stop();
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
            {
            }
            // ReSharper restore EmptyGeneralCatchClause
            _timer = null;
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (sender != _timer)
            {
                return;
            }
            Stop();
            RaiseFired();
        }
    }
}
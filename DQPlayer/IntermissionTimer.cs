using System;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DQPlayer
{
    public class IntermissionTimer : DispatcherTimer
    {
        protected MulticastDelegate _tickEventHandlers;
        protected MulticastDelegate TickEventHandlers =>
            _tickEventHandlers ?? (_tickEventHandlers = (MulticastDelegate)typeof(DispatcherTimer)
                .GetField("Tick", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(this));

        protected CancellationTokenSource _cancellationToken;

        protected readonly Stopwatch _internalTickCounter;
        protected Stopwatch _intervalTickedTime;

        public TimeSpan Elapsed => _internalTickCounter.Elapsed;

        public new TimeSpan Interval
        {
            get => base.Interval;
            set
            {
                base.Interval = value;
                _cancellationToken.Cancel();
                _cancellationToken = new CancellationTokenSource();
                _intervalTickedTime.Reset();
                if (_internalTickCounter.IsRunning)
                {
                    Start();
                }
            }
        }

        public IntermissionTimer()
        {
            _cancellationToken = new CancellationTokenSource();
            _internalTickCounter = new Stopwatch();
            _intervalTickedTime = new Stopwatch();

            Tick += IntermissionTimer_Tick;
        }

        protected virtual void IntermissionTimer_Tick(object sender, EventArgs e)
        {
            if (sender == this)
            {
                _intervalTickedTime = Stopwatch.StartNew();
            }
        }

        public new virtual void Start()
        {
            base.Start();
            _internalTickCounter.Start();
            _intervalTickedTime = Stopwatch.StartNew();
        }

        public new virtual void Stop()
        {
            base.Stop();
            _internalTickCounter.Reset();
            _intervalTickedTime.Reset();
            _cancellationToken.Cancel();
        }

        public virtual void Pause()
        {
            _cancellationToken.Cancel();
            _cancellationToken = new CancellationTokenSource();
            base.Stop();
            _internalTickCounter.Stop();
            _intervalTickedTime.Stop();
        }

        public virtual void Resume()
        {
            if (!_internalTickCounter.IsRunning)
            {
                Task.Factory.StartNew(async () =>
                {
                    _internalTickCounter.Start();
                    _intervalTickedTime.Start();
                    var scheduledIntermissionTime =
                        Math.Abs((int) (Interval - _intervalTickedTime.Elapsed).TotalMilliseconds);
                    await Task.Delay(scheduledIntermissionTime, _cancellationToken.Token)
                        .ContinueWith(task =>
                        {
                            if (task.Status == TaskStatus.RanToCompletion)
                            {
                                foreach (var handler in TickEventHandlers.GetInvocationList())
                                {
                                    handler.Method.Invoke(handler.Target, new object[] {this, EventArgs.Empty});
                                }
                                Start();
                            }
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                    _cancellationToken = new CancellationTokenSource();
                }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }
    }
}
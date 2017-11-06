using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DQPlayer
{
    public class IntermissionTimer : DispatcherTimer
    {
        private readonly Stopwatch _sw;

        private MulticastDelegate _tickEventHandlers;
        private MulticastDelegate tickEventHandlers =>
            _tickEventHandlers ?? (_tickEventHandlers = (MulticastDelegate)typeof(DispatcherTimer)
                .GetField("Tick", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(this));

        private CancellationTokenSource _cancelationToken;
        private Task _task;

        public TimeSpan Elapsed => _sw.Elapsed;

        private bool _hasScheduledIntermission;

        public IntermissionTimer()
        {
            _sw = new Stopwatch();
            Cancelation();
        }

        public new void Start()
        {
            base.Start();
            _sw.Restart();
        }

        public new void Stop()
        {
            base.Stop();
            _sw.Reset();
        }

        private void Cancelation()
        {
            _cancelationToken = new CancellationTokenSource();
            _cancelationToken.Token.Register(() =>
            {
                _hasScheduledIntermission = false;
                _sw.Stop();
            });
        }

        public void Pause()
        {
            if (_task != null && !_task.IsCompleted)
            {
                _cancelationToken.Cancel();
            }
            else
            {
                Cancelation();
            }
            base.Stop();
            _sw.Stop();
            _hasScheduledIntermission = true;
        }

        public void Resume()
        {
            if ((_task == null || _task.IsCompleted) && (_hasScheduledIntermission || _sw.ElapsedTicks == 0))
            {
                _task = Task.Run(async () =>
                {
                    _sw.Start();
                    var value = (int)(_sw.Elapsed.TotalMilliseconds / Interval.TotalMilliseconds);
                    var scheduledIntermissionTime = Math.Abs((int)((value + 1) * Interval.TotalMilliseconds -
                                                                   _sw.Elapsed.TotalMilliseconds));
                    if (_cancelationToken.IsCancellationRequested)
                    {
                        Cancelation();
                        return;
                    }
                    await Task.Delay(scheduledIntermissionTime, _cancelationToken.Token).ContinueWith(t => { });
                    if (_cancelationToken.IsCancellationRequested)
                    {
                        Cancelation();
                        return;
                    }
                    foreach (var handler in tickEventHandlers.GetInvocationList())
                    {
                        Dispatcher.Invoke(() => handler.Method.Invoke(handler.Target,
                            new object[] { this, EventArgs.Empty }));
                    }
                    Start();
                    _hasScheduledIntermission = false;
                }, _cancelationToken.Token);
                Cancelation();
            }
        }
    }
}
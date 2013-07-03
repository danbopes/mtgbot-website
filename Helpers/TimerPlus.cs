using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace MTGBotWebsite.Helpers
{
    public class TimerPlus : IDisposable
    {
        private readonly TimerCallback _realCallback;
        private readonly Timer _timer;
        private DateTime _next;

        public TimerPlus(TimerCallback callback, object state, TimeSpan dueTime)
        {
            _timer = new Timer(Callback, state, dueTime, TimeSpan.MaxValue);
            _realCallback = callback;
            _next = DateTime.Now.Add(dueTime);
        }

        private void Callback(object state)
        {
            _realCallback(state);
            Dispose();
        }

        public DateTime Next
        {
            get
            {
                return _next;
            }
        }

        public TimeSpan DueTime
        {
            get
            {
                return _next - DateTime.Now;
            }
        }

        public bool Change(TimeSpan dueTime)
        {
            _next = DateTime.Now.Add(dueTime);
            return _timer.Change(dueTime, TimeSpan.MaxValue);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}

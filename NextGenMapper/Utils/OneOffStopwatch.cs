using System.Diagnostics;

namespace NextGenMapper.Utils
{
    public class OneOffStopwatch
    {
        private readonly Stopwatch _stopwatch;

        public OneOffStopwatch()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public string Stop()
        {
            _stopwatch.Stop();
            var ts = _stopwatch.Elapsed;
            return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
        }
    }
}

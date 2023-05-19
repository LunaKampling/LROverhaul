//  Original class author: Alex from Jitbit
//  URL: https://stackoverflow.com/a/64867741/10895487

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace linerider.Utils
{
    public static class Debouncer
    {
        static ConcurrentDictionary<string, CancellationTokenSource> _tokens = new ConcurrentDictionary<string, CancellationTokenSource>();

        /// <summary>
        /// Throttle action if it's already been called during specified amount of time.
        /// </summary>
        /// <param name="uniqueKey">An unique key</param>
        /// <param name="action">Lambda action to call</param>
        /// <param name="ms">Milliseconds to wait</param>
        public static void Debounce(string uniqueKey, Action action, int ms)
        {
            var token = _tokens.AddOrUpdate(
                uniqueKey,
                (key) => new CancellationTokenSource(), //key not found - create new
                (key, existingToken) => { existingToken.Cancel(); return new CancellationTokenSource(); } //key found - cancel task and recreate
            );

            Task.Delay(ms, token.Token).ContinueWith(task =>
            {
                if (!task.IsCanceled)
                {
                    action();
                    _tokens.TryRemove(uniqueKey, out _);
                }
            }, token.Token);
        }
    }
}

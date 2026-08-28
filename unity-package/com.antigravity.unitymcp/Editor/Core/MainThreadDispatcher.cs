using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEditor;

namespace Antigravity.UnityMCP.Editor.Core
{
    [InitializeOnLoad]
    public static class MainThreadDispatcher
    {
        private static readonly ConcurrentQueue<Action> ExecutionQueue = new ConcurrentQueue<Action>();

        static MainThreadDispatcher()
        {
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            while (ExecutionQueue.TryDequeue(out var action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[UnityMCP] Error executing main thread task: {ex}");
                }
            }
        }

        public static Task<T> EnqueueAsync<T>(Func<T> function)
        {
            var tcs = new TaskCompletionSource<T>();
            ExecutionQueue.Enqueue(() =>
            {
                try
                {
                    var result = function();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        public static Task EnqueueAsync(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            ExecutionQueue.Enqueue(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }
    }
}

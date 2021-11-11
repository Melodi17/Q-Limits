using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace q_limits
{
    public static class ParallelExecutor
    {
        /// <summary>
        /// Executes asynchronously given function on all elements of given enumerable with task count restriction.
        /// Executor will continue starting new tasks even if one of the tasks throws. If at least one of the tasks threw an exception then <see cref="AggregateException"/> is thrown at the end of the method run.
        /// </summary>
        /// <typeparam name="T">Type of elements in enumerable</typeparam>
        /// <param name="maxTaskCount">The maximum task count.</param>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="asyncFunc">asynchronous function that will be executed on every element of the enumerable. MUST be thread safe.</param>
        /// <param name="onException">Acton that will be executed on every exception that would be thrown by asyncFunc. CAN be thread unsafe.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static async Task ForEachAsync<T>(int maxTaskCount, IEnumerable<T> enumerable, Func<T, Task> asyncFunc, Action<Exception> onException = null, CancellationToken cancellationToken = default)
        {
            using var semaphore = new SemaphoreSlim(initialCount: maxTaskCount, maxCount: maxTaskCount);

            // This `lockObject` is used only in `catch { }` block.
            object lockObject = new object();
            var exceptions = new List<Exception>();
            var tasks = new Task[enumerable.Count()];
            int i = -1;

            try
            {
                foreach (var t in enumerable)
                {
                    await semaphore.WaitAsync(cancellationToken);
                    int myIdx = Interlocked.Increment(ref i);
                    tasks[myIdx] = Task.Run(
                        async () =>
                        {
                            try
                            {
                                await asyncFunc(t);
                            }
                            catch (Exception e)
                            {
                                if (onException != null)
                                {
                                    lock (lockObject)
                                    {
                                        onException.Invoke(e);
                                    }
                                }

                                // This exception will be swallowed here but it will be collected at the end of ForEachAsync method in order to generate AggregateException.
                                throw;
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }, cancellationToken);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException e)
            {
                exceptions.Add(e);
            }


            foreach (var t in tasks)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                // Exception handling in this case is actually pretty fast.
                // https://gist.github.com/shoter/d943500eda37c7d99461ce3dace42141
                try
                {
                    await t;
                }
#pragma warning disable CA1031 // Do not catch general exception types - we want to throw that exception later as aggregate exception. Nothing wrong here.
                catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    exceptions.Add(e);
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }
    }

}

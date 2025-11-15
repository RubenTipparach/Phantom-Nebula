using System;
using System.Threading;
using BepuUtilities;
using BepuUtilities.Memory;

namespace PhantomNebula.Core;

/// <summary>
/// Simple thread dispatcher for BepuPhysics
/// Uses thread pool for multi-threaded physics simulation
/// </summary>
public class SimpleThreadDispatcher : IThreadDispatcher
{
    private int _threadCount;
    private BufferPool _bufferPool;

    public int ThreadCount => _threadCount;

    public SimpleThreadDispatcher(int threadCount)
    {
        _threadCount = Math.Max(1, threadCount);
        _bufferPool = new BufferPool();
    }

    public void DispatchWorkers(Action<int> action, int jobCount)
    {
        if (jobCount <= 0)
            return;

        if (jobCount == 1)
        {
            action(0);
            return;
        }

        // Use ThreadPool to distribute work
        int completedCount = 0;
        object lockObj = new object();
        ManualResetEvent doneEvent = new ManualResetEvent(false);

        for (int i = 0; i < jobCount; i++)
        {
            int jobIndex = i;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                action(jobIndex);

                lock (lockObj)
                {
                    completedCount++;
                    if (completedCount == jobCount)
                    {
                        doneEvent.Set();
                    }
                }
            });
        }

        doneEvent.WaitOne();
        doneEvent.Dispose();
    }

    public BufferPool GetThreadMemoryPool(int threadIndex)
    {
        return _bufferPool;
    }

    public void Dispose()
    {
        // BufferPool doesn't have a Dispose method, just clean up
    }
}

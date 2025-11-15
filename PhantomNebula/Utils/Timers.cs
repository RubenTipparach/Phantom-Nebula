using System;

namespace PhantomNebula.Utils;

/// <summary>
/// Timer using system time
/// Tracks elapsed time using DateTime
/// </summary>
[System.Serializable]
public class Timing
{
    public float duration;
    private float startTime;
    private float pausedAt = 0;
    private bool initialized = false;

    public float Remaining
    {
        get
        {
            float elapsed = (float)(DateTime.UtcNow.Ticks / 10000000.0) - startTime;
            return Math.Max(0, duration - elapsed);
        }
    }

    public void Init()
    {
        StartTimerAt(0);
    }

    public bool Completed()
    {
        float elapsed = (float)(DateTime.UtcNow.Ticks / 10000000.0) - startTime;
        return elapsed >= duration;
    }

    public void FinishTimer()
    {
        startTime = (float)(DateTime.UtcNow.Ticks / 10000000.0) - duration;
    }

    public void StartTimerAt(float offset)
    {
        initialized = true;
        startTime = (float)(DateTime.UtcNow.Ticks / 10000000.0) - offset;
    }

    public float GetProgress
    {
        get
        {
            float elapsed = (float)(DateTime.UtcNow.Ticks / 10000000.0) - startTime;
            return elapsed / duration;
        }
    }

    public float GetProgressClamped
    {
        get
        {
            float elapsed = (float)(DateTime.UtcNow.Ticks / 10000000.0) - startTime;
            return Math.Clamp(elapsed / duration, 0, 1);
        }
    }

    public void Pause()
    {
        pausedAt = Remaining;
    }

    public void Resume()
    {
        if (initialized)
        {
            duration = pausedAt;
        }
        Init();
    }
}

/// <summary>
/// Simulated timer - manually updated with delta time
/// Used for game logic that needs frame-by-frame control
/// </summary>
[System.Serializable]
public class TimingSimulated
{
    public float duration;
    private float time = 0;
    private float timeDelta = 0;

    public bool Completed
    {
        get => (timeDelta - time) > duration;
    }

    public void FinishTimer()
    {
        time = -duration;
    }

    public void StartTimerAt(float offset)
    {
        time = 0 - offset;
        timeDelta = 0;
    }

    public void StartPercentageAt(float offset)
    {
        time = 0;
        timeDelta = offset * duration;
    }

    public void UpdateTime(float deltaTime)
    {
        timeDelta += deltaTime;
    }

    public float GetProgress
    {
        get => (timeDelta - time) / duration;
    }

    public float GetProgressClamped
    {
        get => Math.Clamp((timeDelta - time) / duration, 0, 1);
    }
}

/// <summary>
/// Unscaled timer - for UI and other unscaled time operations
/// Uses DateTime for real elapsed time
/// </summary>
[System.Serializable]
public class TimingUnscaled
{
    public float duration = 1;
    private float startTime;

    public bool Completed
    {
        get
        {
            float elapsed = (float)(DateTime.UtcNow.Ticks / 10000000.0) - startTime;
            return elapsed > duration;
        }
    }

    public void Reset()
    {
        startTime = (float)(DateTime.UtcNow.Ticks / 10000000.0);
    }

    public void SetTime(float offset)
    {
        startTime = (float)(DateTime.UtcNow.Ticks / 10000000.0) - offset;
    }

    public float GetProgress
    {
        get
        {
            float elapsed = (float)(DateTime.UtcNow.Ticks / 10000000.0) - startTime;
            return elapsed / duration;
        }
    }
}

using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;

namespace LCSC.App.Helpers;

public class DispatcherHelper
{
    private readonly List<TimeSpan> _tickTimes;
    private readonly DispatcherTimer _timer;
    private TimeSpan _cooldown;
    private TimeSpan _lastExecution = TimeSpan.Zero;

    public DispatcherHelper()
    {
        _timer = new DispatcherTimer();
        CheckInterval = TimeSpan.FromSeconds(50);
        _cooldown = TimeSpan.FromMinutes(30);
        _tickTimes = [];
        Start();
    }

    public event EventHandler? Tick;

    public TimeSpan CheckInterval
    {
        get => _timer.Interval;
        set => _timer.Interval = value;
    }

    public TimeSpan Cooldown
    {
        get => _cooldown;
        set => _cooldown = value;
    }

    public TimeSpan Interval
    {
        get => _timer.Interval;
        set => _timer.Interval = value;
    }

    public bool IsRunning => _timer.IsEnabled;

    public void AddTickTime(TimeSpan time)
    {
        if (!_tickTimes.Contains(time))
        {
            _tickTimes.Add(time);
        }
    }

    public void ClearTickTimes() => _tickTimes.Clear();

    public void Start()
    {
        if (IsRunning)
        {
            return;
        }
        _timer.Tick -= InternalTick;
        _timer.Tick += InternalTick;
        _timer.Start();
    }

    public void Stop()
    {
        if (!IsRunning)
        {
            return;
        }
        _timer.Stop();
        _timer.Tick -= InternalTick;
    }

    private void InternalTick(object? sender, object e)
    {
        if (Tick is null)
        {
            return;
        }
        DateTime now = DateTime.Now;

        TimeSpan currentTime = now.TimeOfDay;
        if (currentTime - _lastExecution < _cooldown)
        {
            return;
        }
        foreach (TimeSpan tickTime in _tickTimes)
        {
            if (currentTime.Hours == tickTime.Hours && currentTime.Minutes == tickTime.Minutes)
            {
                _lastExecution = currentTime;
                Tick(this, EventArgs.Empty);
                break;
            }
        }
    }
}
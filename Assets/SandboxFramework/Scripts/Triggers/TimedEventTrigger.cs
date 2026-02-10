using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimerTrigger : MonoBehaviour
{
    /// <summary>
    /// Represents a time interval with a start and end boundary.
    /// Start can be greater than end to indicate backward playback.
    /// </summary>
    struct TimeFrame
    {
        public float start;
        public float end;

        public TimeFrame(float a, float b)
        {
            start = a;
            end = b;
        }
    }

    [Serializable]
    public class TimedEvent
    {
        public float delay;           // Time in seconds when this event should fire
        public UnityEvent unityEvent; // The UnityEvent to invoke at the specified delay
    }

    public enum PlaybackMode
    {
        Once,       // Play events once from start to end
        Loop,       // Loop playback continuously from end back to start
        PingPong    // Play forward then backward repeatedly (bouncing)
    }

    [SerializeField] private List<TimedEvent> events = new List<TimedEvent>();
    [SerializeField] private PlaybackMode mode = PlaybackMode.Once;
    [SerializeField] private float playbackSpeed = 1f;
    [SerializeField] private bool autoPlay = true;
    [SerializeField] private float duration = 10f;
    [SerializeField] private float startTime = 0f;

    private bool isPlaying = false;
    private float currentTime = 0f;
    private float previousTime = 0f;

    // Cached and sorted list of events for quick access
    private List<(float time, UnityEvent unityEvent)> processedEvents = new();

    private void OnValidate()
    {
        ProcessEvents();
    }

    private void Awake()
    {
        ProcessEvents();
    }

    private void Start()
    {
        currentTime = startTime;
        if (autoPlay) Play();
    }

    private void Update()
    {
        if (!isPlaying || processedEvents.Count == 0) return;
        HandlePlayback();
    }

    /// <summary>
    /// Advances playback time, handles loop/pingpong/once, and fires events in the current time window.
    /// </summary>
    private void HandlePlayback()
    {
        if (!enabled) return;

        previousTime = currentTime;
        currentTime += Time.deltaTime * playbackSpeed;

        TimeFrame timeWindow = new TimeFrame(previousTime, currentTime);
        bool handledInSwitch = false;

        switch (mode)
        {
            case PlaybackMode.Once:
                if ((playbackSpeed >= 0 && currentTime > duration) || (playbackSpeed < 0 && currentTime < 0))
                {
                    currentTime = Mathf.Clamp(currentTime, 0f, duration);
                    timeWindow = new TimeFrame(previousTime, currentTime);
                    FireEventsInWindow(timeWindow);
                    Stop();
                    handledInSwitch = true;
                }
                break;

            case PlaybackMode.Loop:
                if (playbackSpeed >= 0 && currentTime > duration)
                {
                    float overshoot = currentTime - duration;
                    FireEventsInWindow(new TimeFrame(previousTime, duration));
                    currentTime = overshoot;
                    FireEventsInWindow(new TimeFrame(0f, currentTime));
                    handledInSwitch = true;
                }
                else if (playbackSpeed < 0 && currentTime < 0)
                {
                    float overshoot = -currentTime;
                    FireEventsInWindow(new TimeFrame(previousTime, 0f));
                    currentTime = duration - overshoot;
                    FireEventsInWindow(new TimeFrame(currentTime, duration));
                    handledInSwitch = true;
                }
                break;

            case PlaybackMode.PingPong:
                if (currentTime > duration)
                {
                    float overshoot = currentTime - duration;
                    currentTime = duration - overshoot;
                    playbackSpeed *= -1;
                    FireEventsInWindow(new TimeFrame(previousTime, duration));
                    FireEventsInWindow(new TimeFrame(duration, currentTime));
                    handledInSwitch = true;
                }
                else if (currentTime < 0)
                {
                    float overshoot = -currentTime;
                    currentTime = overshoot;
                    playbackSpeed *= -1;
                    FireEventsInWindow(new TimeFrame(previousTime, 0f));
                    FireEventsInWindow(new TimeFrame(0f, currentTime));
                    handledInSwitch = true;
                }
                break;
        }

        if (!handledInSwitch)
        {
            FireEventsInWindow(timeWindow);
        }
    }

    /// <summary>
    /// Invokes all events whose scheduled times fall within the given timeframe.
    /// Handles forward and backward playback.
    /// </summary>
    private void FireEventsInWindow(TimeFrame time)
    {
        if (!enabled) return;

        bool forward = time.end >= time.start;

        foreach (var (eventTime, unityEvent) in processedEvents)
        {
            float evt = eventTime;

            if (forward)
            {
                // Trigger events strictly after previous time and up to current time
                if (evt >= time.start && evt < time.end)
                {
                    unityEvent?.Invoke();
                }
            }
            else
            {
                // Trigger events strictly before previous time and down to current time
                if (evt <= time.start && evt > time.end)
                {
                    unityEvent?.Invoke();
                }
            }
        }
    }

    public void JumpTo(float time)
    {
        currentTime = Mathf.Clamp(time, 0f, duration);
        previousTime = currentTime;
        FireEventsInWindow(new TimeFrame(currentTime, currentTime));
    }

    public void Play() => isPlaying = true;
    public void Pause() => isPlaying = false;

    public void Stop()
    {
        isPlaying = false;
        JumpTo(0f);
    }

    /// <summary>
    /// Processes and caches all events, sorting them by delay time.
    /// </summary>
    private void ProcessEvents()
    {
        processedEvents.Clear();
        foreach (var e in events)
        {
            if (e == null || e.unityEvent == null) continue;
            processedEvents.Add((e.delay, e.unityEvent));
        }
        processedEvents.Sort((a, b) => a.time.CompareTo(b.time));
    }
}

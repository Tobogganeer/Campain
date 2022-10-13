using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Old_Timer// : IDisposable
{
    public class Old_TimerManager : MonoBehaviour
    {
        public static Old_TimerManager instance;

        private List<Old_Timer> timers = new List<Old_Timer>();
        private Queue<Old_Timer> toAdd = new Queue<Old_Timer>();
        private Queue<Old_Timer> toRemove = new Queue<Old_Timer>();

        public void AddTimer(Old_Timer timer)
        {
            toAdd.Enqueue(timer);
        }

        public void RemoveTimer(Old_Timer timer)
        {
            toRemove.Enqueue(timer);
        }

        private void Update()
        {
            while (toAdd.Count > 0)
            {
                Old_Timer timer = toAdd.Dequeue();
                if (!timers.Contains(timer)) timers.Add(timer);
            }

            while (toRemove.Count > 0)
            {
                Old_Timer timer = toRemove.Dequeue();
                if (timers.Contains(timer)) timers.Remove(timer);
            }

            toAdd.Clear();

            toRemove.Clear();

            foreach (Old_Timer timer in timers)
            {
                timer.Update();
            }
        }

        public void ClearTimers(int id)
        {
            if (id == -1) return;

            foreach (Old_Timer timer in timers)
            {
                if (timer.id == id) RemoveTimer(timer);
            }
        }
    }

    Action action;
    float time;
    bool isDestroyed;
    int id;

    private Old_Timer(Action action, float afterTime)
    {
        CheckManagerInstance();

        this.action = action;
        time = afterTime;
        isDestroyed = false;
        id = -1;

        Old_TimerManager.instance.AddTimer(this);
    }

    private Old_Timer(Action action, float afterTime, int id)
    {
        CheckManagerInstance();

        this.action = action;
        time = afterTime;
        isDestroyed = false;
        this.id = id;

        Old_TimerManager.instance.AddTimer(this);
    }

    /// <summary>
    /// Runs the specified action <paramref name="action"/> after time <paramref name="time"/>.
    /// </summary>
    /// <param name="action">The action to run on completion</param>
    /// <param name="time">The time to run the action after</param>
    public static void Run(Action action, float time)
    {
        new Old_Timer(action, time);
    }

    /// <summary>
    /// Runs the specified action <paramref name="action"/> after time <paramref name="time"/>.
    /// </summary>
    /// <param name="action">The action to run on completion</param>
    /// <param name="time">The time to run the action after</param>
    /// <param name="id">The id of the timer (for referencing later)</param>
    public static void Run(Action action, float time, int id)
    {
        new Old_Timer(action, time, id);
    }

    public void Update()
    {
        if (!isDestroyed)
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                action?.Invoke();
                Dispose();
            }
        }
    }

    public void Dispose()
    {
        //Dispose(true);
        isDestroyed = true;
        Old_TimerManager.instance.RemoveTimer(this);

        GC.SuppressFinalize(this);
    }

    public static void ClearTimers(int id)
    {
        if (id == -1) return;

        CheckManagerInstance();

        Old_TimerManager.instance.ClearTimers(id);
    }

    private static void CheckManagerInstance()
    {
        if (Old_TimerManager.instance == null)
        {
            Old_TimerManager.instance = new GameObject("TimerManager", typeof(Old_TimerManager)).GetComponent<Old_TimerManager>();
        }
    }

    //protected virtual void Dispose(bool disposing)
    //{
    //    if (!isDestroyed)
    //    {
    //        if (!disposing)
    //        {
    //
    //        }
    //
    //        isDestroyed = true;
    //    }
    //}
}

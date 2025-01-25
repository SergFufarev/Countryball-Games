using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace TheSTAR.Utility
{
    public static class TimeUtility
    {
        public static WaitStopper WaitAsync(float timeSeconds, Action action)
        {
            WaitStopper stopper = new();
            WaitAsync(stopper, timeSeconds, action);
            return stopper;
        }

        private static void WaitAsync(WaitStopper stopper, float timeSeconds, Action action)
        {
            if (timeSeconds < 0)
            {
                Debug.LogError($"Невозможно запустить ожидание на время: {timeSeconds} сек");
            }
            else WaitAsync(stopper, (int)(timeSeconds * 1000), action);
        }

        public static void WaitAsync(int timeMilliseconds, Action action)
        {
            WaitStopper stopper = new();
            WaitAsync(stopper, timeMilliseconds, action);
        }

        private async static void WaitAsync(WaitStopper stopper, int timeMilliseconds, Action action)
        {
            //try
            //{
#if UNITY_EDITOR
                if (!Application.isPlaying) return;
#endif
                await Task.Run(() => Task.Delay(timeMilliseconds));
                if (stopper.IsBreak) return;
                action?.Invoke();
            //}
            //catch
            //{
            //    Debug.LogError($"Не удалось запустить ожидание! (продолжительность {timeMilliseconds} млсек)");
            //}
        }

        public static WaitStopper DoWhileAsync(WaitWhileCondition condition, float timeSeconds, Action action) => DoWhileAsync(condition, (int)(timeSeconds * 1000), action);

        public static WaitStopper DoWhileAsync(WaitWhileCondition condition, int timeMilliseconds, Action action)
        {
            action?.Invoke();
            return WhileAsync(condition, timeMilliseconds, action);
        }

        public static WaitStopper WhileAsync(WaitWhileCondition condition, float timeSeconds, Action action) => WhileAsync(condition, (int)(timeSeconds * 1000), action);

        public static WaitStopper WhileAsync(WaitWhileCondition condition, int timeMilliseconds, Action action)
        {
            WaitStopper control = new ();
            WaitWhileAsync(condition, timeMilliseconds, action, control);
            return control;
        }

        private static void WaitWhileAsync(WaitWhileCondition condition, int timeMilliseconds, Action action, WaitStopper control)
        {
            if (control.IsBreak) return;

            WaitAsync(timeMilliseconds, () =>
            {
                if (control.IsBreak) return;

                action?.Invoke();

                if (!condition.Invoke()) return;

                WaitWhileAsync(condition, timeMilliseconds, action, control);
            });
        }

        private enum CycleStatus
        {
            Alive,
            Breaked
        }

        public delegate bool WaitWhileCondition();
    }

    public class WaitStopper
    {
        public bool IsBreak
        {
            get;
            private set;
        }

        public void Stop() => IsBreak = true;
    }

    [Serializable]
    public struct GameTimeSpan
    {
        #region Fields

        [Range(0, 23)]
        [SerializeField] private ushort hours;
        [Range(0, 59)]
        [SerializeField] private byte minutes;
        [Range(0, 59)]
        [SerializeField] private byte seconds;

        public ushort Hours => hours;
        public byte Minutes => minutes;
        public byte Seconds => seconds;

        private const byte SIXTY = 60;

        #endregion Fields

        #region Constructors

        public GameTimeSpan(TimeSpan timeSpan)
        {
            hours = (ushort)timeSpan.Hours;
            minutes = (byte)timeSpan.Minutes;
            seconds = (byte)timeSpan.Seconds;
        }

        public GameTimeSpan(int totalSeconds)
        {
            hours = 0;
            minutes = 0;
            seconds = 0;

            TotalSecondsToFormatValues(totalSeconds, out hours, out minutes, out seconds);
            FormatValuesCounts();
        }

        public GameTimeSpan(ushort h, byte m, byte s)
        {
            hours = h;
            minutes = m;
            seconds = s;

            FormatValuesCounts();
        }

        public TimeSpan ToTimeSpan() => new (hours, minutes, seconds);

        #endregion Constructors

        #region Operators

        public static GameTimeSpan operator +(GameTimeSpan a, GameTimeSpan b)
        {
            GameTimeSpan value = new(
                (ushort)(a.hours + b.hours),
                (byte)(a.minutes + b.minutes),
                (byte)(a.seconds + b.seconds));

            value.FormatValuesCounts();

            return value;
        }

        public static implicit operator GameTimeSpan(TimeSpan timeSpan)
        {
            return new GameTimeSpan(timeSpan);
        }

        #endregion Operators

        #region Overrides

        public override string ToString()
        {
            return $"{hours}:{minutes}:{seconds}";
        }

        #endregion Overrides

        #region Format

        private void FormatValuesCounts()
        {
            while (seconds >= SIXTY)
            {
                seconds -= SIXTY;
                minutes++;
            }

            while (minutes >= SIXTY)
            {
                minutes -= SIXTY;
                hours++;
            }
        }

        private void TotalSecondsToFormatValues(int totalSeconds, out ushort h, out byte m, out byte s)
        {
            h = 0;
            m = 0;
            s = 0;

            while (totalSeconds >= SIXTY)
            {
                totalSeconds -= SIXTY;
                m++;
            }

            s = (byte)totalSeconds;

            while (m >= SIXTY)
            {
                m -= SIXTY;
                h++;
            }
        }

        public string FormatForText()
        {
            if (hours > 0) return $"{hours}h";
            return $"{minutes}m";
        }

        #endregion Format

        public int TotalSeconds
        {
            get
            {
                int totalSeconds = 0;

                totalSeconds += seconds;
                totalSeconds += minutes * SIXTY;
                totalSeconds += hours * SIXTY * SIXTY;

                return totalSeconds;
            }
        }
    }
}
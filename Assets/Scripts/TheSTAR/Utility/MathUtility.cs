using System;
using UnityEngine;

namespace TheSTAR.Utility
{
    public static class MathUtility
    {
        public static int Round(float value)
        {
            var result = -1;
            var residue = value % 1;
            result = (int)value;
            if (residue >= 0.5f) result++;

            return result;
        }

        public static int Limit(int value, int min, int max) => (int)Limit((float)value, min, max);
        public static float Limit(float value, float min, float max)
        {
            if (max < min)
            {
                float temp = max;
                max = min;
                min = temp;
            }

            return MathF.Max(MathF.Min(value, max), min);
        }

        public static bool InBounds(int value, IntRange range) => InBounds(value, range.min, range.max);
        public static bool InBounds(int value, int min, int max) => InBounds((float)value, min, max);
        public static bool InBounds(float value, float min, float max) => (min <= value && value <= max);

        public static bool InBounds(IntVector2 value, IntVector2 min, IntVector2 max) => 
            InBounds(value.X, min.X, max.X) && InBounds(value.Y, min.Y, max.Y);
        
        public static bool InBounds(Vector2 value, Vector2 min, Vector2 max) => 
            InBounds(value.x, min.x, max.x) && InBounds(value.y, min.y, max.y);

        public static bool IsIntValue(string s, out int value)
        {
            s = s.Replace(" ", "");
            value = -1;

            var isMinus = false;
            if (s[0] == '-')
            {
                isMinus = true;
                s = s.Remove(0, 1);
            }

            foreach (char symbol in s)
            {
                if (!char.IsNumber(symbol)) return false;
            }

            if (string.IsNullOrEmpty(s))
            {
                value = 0;
                return true;
            }

            value = Convert.ToInt32(s) * ((isMinus ? -1 : 1));

            return true;
        }

        public static float Difference(float a, float b) => Math.Abs(a - b);

        public static Vector3 Direction(Vector3 from, Vector3 to)
        {
            return to - from;
        }

        public static GameTimeSpan RandomRange(GameTimeSpanRange range) => RandomRange(range.min, range.max);

        public static GameTimeSpan RandomRange(GameTimeSpan min, GameTimeSpan max)
        {
            int secondsMin = min.TotalSeconds;
            int secondsMax = max.TotalSeconds;
            int resultSeconds = UnityEngine.Random.Range(secondsMin, secondsMax);
            return new GameTimeSpan(resultSeconds);
        }

        /// <summary>
        /// Возвращает прогресс от 0 до 1 для value между min и max
        /// </summary>
        public static float GetProgress(float value, float min, float max) => (value - min) / (max - min);

        /// <summary>
        /// Конвертирует прогресс от 0 до 1 в значение от min до max
        /// </summary>
        public static float ProgressToValue(float progress, float min, float max) => (max - min) * progress + min;

        /// <summary>
        /// Конвертирует прогресс от 0 до 1 в значение между a и b
        /// </summary>
        public static Vector3 ProgressToValue(float progress, Vector3 a, Vector3 b)
        {
            float x = ProgressToValue(progress, a.x, b.x);
            float y = ProgressToValue(progress, a.y, b.y);
            float z = ProgressToValue(progress, a.z, b.z);
            return new(x, y, z);
        }


        /// <summary>
        /// Возвращает положение среднее между точками A и B
        /// </summary>
        public static Vector2 MiddlePosition(Vector2 a, Vector2 b)
        {
            float x = ProgressToValue(0.5f, Math.Min(a.x, b.x), Math.Max(a.x, b.x));
            float y = ProgressToValue(0.5f, Math.Min(a.y, b.y), Math.Max(a.y, b.y));
            return new(x, y);
        }

        /// <summary>
        /// Возвращает положение среднее между точками A и B
        /// </summary>
        public static Vector3 MiddlePosition(Vector3 a, Vector3 b) => ProgressToValue(0.5f, a, b);

        public static Vector3 Limit(Vector3 value, float directionLimit)
        {
            return new Vector3(
                Limit(value.x, -directionLimit, directionLimit),
                Limit(value.y, -directionLimit, directionLimit),
                Limit(value.z, -directionLimit, directionLimit));
        }

        /// <summary>
        /// Доворачивает baseRotation к toRotation, но каждый угол не может быть изменён больше чем на maxStep
        /// </summary>
        public static Vector3 ShoothRotateTo(Vector3 baseRotation, Vector3 toRotation, float maxStep)
        {
            var result =
            new Vector3(
                SmoothMoveAngleTo(baseRotation.x, toRotation.x, maxStep),
                SmoothMoveAngleTo(baseRotation.y, toRotation.y, maxStep),
                SmoothMoveAngleTo(baseRotation.z, toRotation.z, maxStep));

            return result;
        }

        /// <summary>
        /// Угол baseValue стремится к toValue, но смещение не может быть больше maxStep.
        /// При этом ToValue может "прыгать" на 360 градусов, чтобы вышел короткий "маршрут" 
        /// </summary>
        public static float SmoothMoveAngleTo(float baseValue, float toValue, float maxStep)
        {
            if (baseValue == toValue) return baseValue;

            var differenceFull = Difference(baseValue, toValue);
            var differenceJumpToMinus = Difference(baseValue, toValue - 360);
            var differenceJumpToPlus = Difference(baseValue, toValue + 360);

            float difference;
            if (differenceJumpToMinus < differenceFull)
            {
                if (differenceJumpToPlus < differenceJumpToMinus)
                {
                    toValue += 360;
                    difference = differenceJumpToPlus;
                }
                else
                {
                    toValue -= 360;
                    difference = differenceJumpToMinus;
                }
            }
            else if (differenceJumpToPlus < differenceFull)
            {
                toValue += 360;
                difference = differenceJumpToPlus;
            }
            else difference = differenceFull;

            var limitedDifference = Math.Min(difference, maxStep);

            if (toValue > baseValue) return baseValue + limitedDifference;
            else return baseValue - limitedDifference;
        }

        /// <summary>
        /// Если значение выходит за максимум, оно принимает минимальное значение. Если выходит за минимум - принимает максимальное
        /// </summary>
        public static int LimitRound(int value, int min, int max) => (int)LimitRound((float)value, min, max);

        /// <summary>
        /// Если значение выходит за максимум, оно принимает минимальное значение. Если выходит за минимум - принимает максимальное
        /// </summary>
        public static float LimitRound(float value, float min, float max)
        {
            if (value > max) value = min;
            else if (value < min) value = max;

            return value;
        }

        public static Vector3 MergeVector3(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.x + b.x,
                a.y + b.y,
                a.z + b.z);
        }
    }

    public struct IntVector2
    {
        private int x, y;

        public int X => x;
        public int Y => y;

        public IntVector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public IntVector2(Vector2 value)
        {
            x = (int)value.x;
            y = (int)value.y;
        }
        
        public static explicit operator IntVector2(Vector2 value)
        {
            return new IntVector2(value);
        }
        
        public static IntVector2 operator + (IntVector2 a, IntVector2 b)
        {
            return new IntVector2(a.x + b.x, a.y + b.y);
        }
    }

    [Serializable]
    public struct IntRange
    {
        public int min;
        public int max;

        public IntRange(int min, int max)
        {
            this.min = min;
            this.max = max;
        }
    }

    [Serializable]
    public struct GameTimeSpanRange
    {
        public GameTimeSpan min;
        public GameTimeSpan max;

        public GameTimeSpanRange(GameTimeSpan min, GameTimeSpan max)
        {
            this.min = min;
            this.max = max;
        }
    }

    [Serializable]
    public struct FloatRange
    {
        public float min;
        public float max;

        public FloatRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
}
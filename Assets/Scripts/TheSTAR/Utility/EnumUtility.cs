using System;
using System.Collections;
using System.Collections.Generic;

namespace TheSTAR.Utility
{
    static class EnumUtility
    {
        public static TEnum[] GetValues<TEnum>() where TEnum : Enum
        {
            return (TEnum[])Enum.GetValues(typeof(TEnum));
        }

        public static bool IsDefined<TEnum>(int i) where TEnum : Enum
        {
            return EnumNamedValues<TEnum>().ContainsKey(i);
        }

        public static Dictionary<int, string> EnumNamedValues<T>() where T : Enum
        {
            var result = new Dictionary<int, string>();
            var values = Enum.GetValues(typeof(T));

            foreach (int item in values) result.Add(item, Enum.GetName(typeof(T), item)!);

            return result;
        }

        public static TEnum GetNextValue<TEnum>(TEnum currentValue) where TEnum : Enum
        {
            int maxNumberValue = GetValues<TEnum>().Length - 1;
            int nextNumverValue = MathUtility.LimitRound(GetNumberValue(currentValue) + 1, 0, maxNumberValue);
            return (TEnum)Enum.Parse(typeof(TEnum), nextNumverValue.ToString());
        }

        public static TEnum GetPreviousValue<TEnum>(TEnum currentValue) where TEnum : Enum
        {
            int maxNumberValue = GetValues<TEnum>().Length - 1;
            int previousNumverValue = MathUtility.LimitRound(GetNumberValue(currentValue) - 1, 0, maxNumberValue);
            return (TEnum)Enum.Parse(typeof(TEnum), previousNumverValue.ToString());
        }

        public static int GetNumberValue<TEnum>(TEnum value)
        {
            return Convert.ToInt32(value);
        }

        public static TEnum GetRandomValue<TEnum>() where TEnum : Enum
        {
            var allValues = GetValues<TEnum>();
            return ArrayUtility.GetRandomValue(allValues);
        }
    }
}
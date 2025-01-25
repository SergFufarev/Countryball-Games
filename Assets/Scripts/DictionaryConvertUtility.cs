using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DictionaryConvertUtility
{
    public static int GetTotalValue(Dictionary<int, int> dictionary)
    {
        int totalValue = 0;

        if (dictionary != null && dictionary.Count > 0)
        {
            foreach (var value in dictionary.Values) totalValue += value;
        }

        return totalValue;
    }

    public static Dictionary<int, int> ConvertTotalLevelsIntoDictionary(int totalLevels, int maxLevelInOneUpgrade)
    {
        Dictionary<int, int> result = new();

        int i = 0;

        while (totalLevels > 0)
        {
            result.Add(i, Mathf.Min(totalLevels, maxLevelInOneUpgrade));
            totalLevels -= maxLevelInOneUpgrade;
            i++;
        }

        return result;
    }

    public static void ReduceFromEnd(Dictionary<int, int> dictionary, int maxKey, int reduceCount)
    {
        for (int i = maxKey; i >= 0; i--)
        {
            if (dictionary.ContainsKey(i))
            {
                if (reduceCount > dictionary[i])
                {
                    reduceCount -= dictionary[i];
                    dictionary[i] = 0;
                }
                else
                {
                    dictionary[i] -= reduceCount;
                    reduceCount = 0;
                    return;
                }
            }
            else continue;
        }
    }
}
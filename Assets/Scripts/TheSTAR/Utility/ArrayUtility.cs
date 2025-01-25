using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheSTAR.Utility
{
    public static class ArrayUtility
    {
        // Fast find an element with type TNeeded among elements with base type T
        public static int FastFindElement<T, TNeeded>(T[] array)
            where T: IComparableType<T> // T - base type for all elements
            where TNeeded : T // TNeeded - current type for needed element
        {
            int 
            minBorder = -1, // exclusive
            maxBorder = array.Length, // exclusive
            index = -1,
            maxIterationCount = 100,
            iterationIndex = 0;

            bool 
            toBigger = true; // from 0 to n

            T element;
            
            while (iterationIndex < maxIterationCount)
            {
                iterationIndex++;
                
                index = (maxBorder + minBorder) / 2;
                element = array[index];

                if (element is TNeeded) return index;
                else
                {
                    toBigger = element.CompareToType<TNeeded>() < 0;

                    if (toBigger) minBorder = index;
                    else maxBorder = index;
                }
            }

            return -1;
        }

        public static int FastFindElement<T>(T[] array, T neededElement) where T : IComparable<T>
        {
            int 
            minBorder = -1, // exclusive
            maxBorder = array.Length, // exclusive
            index = -1,
            maxIterationCount = 100,
            iterationIndex = 0;

            bool 
            toBigger = true; // from 0 to n

            T element;
            
            while (iterationIndex < maxIterationCount)
            {
                iterationIndex++;
                
                index = (maxBorder + minBorder) / 2;
                element = array[index];

                if (element.Equals(neededElement)) return index;
                else
                {
                    toBigger = element.CompareTo(neededElement) < 0;

                    if (toBigger) minBorder = index;
                    else maxBorder = index;
                }
            }

            return -1;
        }

        public static string GetStringFromEnumerable(IEnumerable enumerable)
        {
            StringBuilder sb = new();
            sb.AppendLine("Values:");
            sb.AppendLine();
            
            foreach (var element in enumerable)
            {
                if (element == null) sb.AppendLine("null");
                else sb.AppendLine(element.ToString());
            }
            
            return sb.ToString();
        }

        public static void PrintEnumerable(IEnumerable enumerable)
        {
            Debug.Log(GetStringFromEnumerable(enumerable));
        }

        public static bool IsNullOfEmpty<T>(T[] array) => array == null || array.Length == 0;
    
        public static T[] UpdateArraySize<T>(T[] array, int size)
        {
            var tempClone = new T[size]; // create a new array with new size

            for (int x = 0; x < size; x++)
            {
                if (x >= array.GetLength(0)) return tempClone;

                tempClone[x] = array[x];
            }

            return tempClone;
        }

        public static T[,] UpdateArraySize<T>(T[,] array, int width, int height)
        {
            array ??= new T[0, 0];

            var tempClone = new T[width, height]; // create a new array with new size

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    if (x >= array.GetLength(0) && y >= array.GetLength(1)) return tempClone;
                    if (x >= array.GetLength(0) || y >= array.GetLength(1)) break;

                    tempClone[x, y] = array[x, y];
                }
            }
            
            return tempClone;
        }

        public static T[,,] UpdateArraySize<T>(T[,,] array, int width, int height, int depth)
        {
            array ??= new T[0, 0, 0];

            var tempClone = new T[width, height, depth]; // create a new array with new size

            Parallel.For(0, array.GetLength(2), z =>
            {
                for (var y = 0; y < array.GetLength(1); y++)
                {
                    for (var x = 0; x < array.GetLength(0); x++)
                    {
                        tempClone[x, y, z] = array[x, y, z];
                    }
                }
            });
            
            return tempClone;
        }

        /// <summary>
        /// Returns array with size equal or less than limit
        /// </summary>
        public static T[] LimitArraySize<T>(T[] array, int limit)
        {
            if (array.Length <= limit) return array;
            else
            {
                T[] newArray = new T[limit];
                for (int i = 0; i < limit; i++) newArray[i] = array[i];
                return newArray;
            }
        }

        public static List<T> Exclude<T>(List<T> baseArray, List<T> excludeElements)
        {
            for (int i = 0; i < excludeElements.Count; i++)
            {
                if (baseArray.Contains(excludeElements[i])) baseArray.Remove(excludeElements[i]);
            }

            return baseArray;
        }

        public static T GetRandomValue<T>(T[] baseArray, T[] without = null) => GetRandomValues(baseArray, without, 1)[0];

        /// <summary>
        /// Возвращает рандомные неповторяющиеся значения из массива
        /// </summary>
        public static T[] GetRandomValues<T>(T[] baseArray, int count) => GetRandomValues(baseArray, null, count);

        public static T[] GetRandomValues<T>(T[] baseArray, T[] without, int count)
        {
            if (baseArray.Length < count)
            {
                Debug.LogError($"Невозможно получить {count} уникальных элементов у массива с размером {baseArray.Length}!");
                return null;
            }

            T[] result = new T[count];
            List<T> availableElements = new(baseArray);

            if (without != null && without.Length > 0)
            {
                for (int i = 0; i < without.Length; i++)
                {
                    if (availableElements.Contains(without[i])) availableElements.Remove(without[i]);
                }
            }

            for (int i = 0; i < count; i++)
            {
                var element = TakeOutRandomElement(availableElements);
                result[i] = element;
            }

            return result;
        }

        /// <summary>
        /// Вытащить рандомный элемент из листа
        /// </summary>
        public static T TakeOutRandomElement<T>(List<T> list) => TakeOutElement(list, Random.Range(0, list.Count));

        /// <summary>
        /// Вытащить элемент из листа
        /// </summary>
        public static T TakeOutElement<T>(List<T> list, int index)
        {
            T result = list[index];
            list.Remove(result);
            return result;
        }

        public static List<T> Merge<T>(List<T> a, List<T> b)
        {
            List<T> result = new(a);
            foreach (var e in b) result.Add(e);

            return result;
        }

        /// <summary>
        /// Возвращает следующий по кругу элемент
        /// </summary>
        public static T GetNextRound<T>(List<T> list, int currentIndex)
        {
            if (currentIndex == list.Count - 1) return list[0];
            else return list[currentIndex + 1];
        }

        public static void Randomize<T>(T[] items)
        {
            System.Random rand = new System.Random();

            // For each spot in the array, pick
            // a random item to swap into that spot.
            for (int i = 0; i < items.Length - 1; i++)
            {
                int j = rand.Next(i, items.Length);
                T temp = items[i];
                items[i] = items[j];
                items[j] = temp;
            }
        }
    }

    public interface IComparableType<in T>
    {
        int CompareToType<T1>() where T1 : T;
    }
}
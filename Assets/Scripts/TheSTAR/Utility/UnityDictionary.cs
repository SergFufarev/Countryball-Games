using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Представляет упрощенный функционал Dictionary, доступный для Unity Inspector
/// </summary>
/// Примечание: на данный момент UnityDictionary не использует HashCode для записи и поиска элементов, в отличии от Dictionary
[Serializable]
public class UnityDictionary<TKey, TValue>
{
    [SerializeField] private List<UnityDictionaryKeyValue> keyValues;

    public int Length => keyValues.Count;
    public TValue[] Values => GetAllValues();
    public List<UnityDictionaryKeyValue> KeyValues => keyValues;

    public UnityDictionary()
    {
        keyValues = new List<UnityDictionaryKeyValue>();
    }

    public void Add(TKey key, TValue value)
    {
        keyValues.Add(new UnityDictionaryKeyValue(key, value));
    }

    public bool Contains(TKey key)
    {
        for (int i = 0; i < keyValues.Count; i++)
        {
            if (keyValues[i].Key.Equals(key)) return true;
        }

        return false;
    }

    public TValue Get(TKey key)
    {
        for (int i = 0; i < keyValues.Count; i++)
        {
            if (keyValues[i].Key.Equals(key)) return keyValues[i].Value;
        }

        Debug.LogError("Попытка получить значение по несуществующему ключу");

        return keyValues[-1].Value;
    }

    public void Set(TKey key, TValue value)
    {
        for (int i = 0; i < keyValues.Count; i++)
        {
            if (keyValues[i].Key.Equals(key)) keyValues[i].Set(value);
        }
    }

    public TValue[] GetAllValues()
    {
        int size = keyValues.Count;
        TValue[] result = new TValue[size];

        for (int i = 0; i < size; i++) result[i] = keyValues[i].Value;

        return result;
    }

    [Serializable]
    public class UnityDictionaryKeyValue
    {
        [SerializeField] private TKey key;
        [SerializeField] private TValue value;

        public TKey Key => key;
        public TValue Value => value;

        public UnityDictionaryKeyValue()
        {
            key = default;
            value = default;
        }

        public UnityDictionaryKeyValue(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }

        public void Set(TValue value)
        {
            this.value = value;
        }
    }
}
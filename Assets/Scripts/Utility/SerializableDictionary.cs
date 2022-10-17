using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableDictionary<TKey, TValue>
{
    [SerializeField]
    public SerializablePair<TKey, TValue>[] values;
    private Dictionary<TKey, TValue> dictionary;

    public Dictionary<TKey, TValue> Dict
    {
        get
        {
            if (dictionary == null)
                Init();
            return dictionary;
        }
    }

    void Init()
    {
        dictionary = new Dictionary<TKey, TValue>();
        if (values != null)
            foreach (var pair in values)
                if (dictionary.ContainsKey(pair.key))
                    Debug.LogWarning(GetErrorLog(pair.key, pair.value));
                else
                    dictionary.Add(pair.key, pair.value);
    }

    private string GetErrorLog(TKey key, TValue value)
    {
        return $"Tried to insert {key} ({key.GetType()})" +
                $"into SerializableDictionary, but a value with that key is already present!" +
                $"(Inserting: {value}- Inserted: {dictionary[key]}) -- VALUE LOG:\n{GetValueLog()}";

        string GetValueLog()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var item in values)
                sb.AppendLine($"{item.name}: {item.key} - {item.value}");

            return sb.ToString();
        }
    }

    [System.Serializable]
    public class SerializablePair<T1, T2>
    {
        [SerializeField, HideInInspector]
        public string name;
        [SerializeField]
        public T1 key;
        [SerializeField]
        public T2 value;
    }
}

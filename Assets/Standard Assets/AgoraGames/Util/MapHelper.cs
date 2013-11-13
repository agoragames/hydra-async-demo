using System;
using System.Collections;
using System.Collections.Generic;

using AgoraGames.Hydra;
using System.Text;

namespace AgoraGames.Hydra.Util
{
    public delegate T Converter<T>(object value);

    public class MapHelper
    {
        public IDictionary Map { get; protected set; }

        public MapHelper(IDictionary map)
        {
            Map = map;
        }

        public void Merge(IDictionary merge)
        {
            foreach (DictionaryEntry entry in merge)
            {
                object key = entry.Key;
                object newVal = entry.Value;

                object currVal = Map.Contains(key) ? Map[key] : null;
                if (currVal != null && currVal is IDictionary && newVal is IDictionary)
                    new MapHelper(currVal as IDictionary).Merge(newVal as IDictionary);
                else
                    Map[entry.Key] = entry.Value;
            }
        }

        public void SetValue(string key, object value)
        {
            string[] tokens = key.Split('.');

            IDictionary localMap = Map;
            for (int i = 0; i < tokens.Length; i++)
            {
                bool isLast = i == tokens.Length - 1;
                string token = tokens[i];

                if (isLast)
                    localMap[token] = value;
                else
                {
                    if (!localMap.Contains(token))
                    {
                        Dictionary<object, object> tmp = new Dictionary<object, object>();
                        localMap[token] = tmp;
                        localMap = tmp;
                    }
                    else
                    {
                        object tmp = localMap[token];
                        if (tmp is IDictionary)
                            localMap = (IDictionary)tmp;
                        else
                            return;
                    }
                }
            }
        }

        public object GetValue(string key)
        {
            return GetValue(key, (object)null);
        }

        public object this[string key]
        {
            get { return GetValue(key); }
            set { SetValue(key, value); }
        }

        public T GetValue<T>(string key, T defaultValue, Converter<T> converter)
        {
            string[] tokens = key.Split('.');

            IDictionary localMap = Map;
            for (int i = 0; i < tokens.Length; i++)
            {
                bool isLast = i == tokens.Length - 1;
                object val = localMap.Contains(tokens[i]) ? localMap[tokens[i]] : null;

                if (val == null)
                    return defaultValue;
                else if (isLast)
                {
                    if (converter != null)
                        return converter(val);
                    else
                        return (T)val;
                }
                else if (val is IDictionary)
                    localMap = (IDictionary)val;
                else
                    break;
            }
            return defaultValue;
        }

        public T GetValue<T>(string key, T defaultValue)
        {
            return GetValue(key, defaultValue, null);
        }
    }
}
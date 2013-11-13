using System;
using System.Collections.Generic;

using AgoraGames.Hydra;
using AgoraGames.Hydra.Models;
using System.Collections;

namespace AgoraGames.Hydra.Util
{
    public abstract class ObjectMapBase<T> where T : DataStore
    {
        protected Dictionary<string, T> Objects { get; set; }

        public ObjectMapBase()
        {
            Objects = new Dictionary<string, T>();
        }

        public T FindObject(string id)
        {
            T obj = null;
            Objects.TryGetValue(id, out obj);

            return obj;
        }

        public T GetObject(IDictionary data)
        {
            if (data.Contains(GetIdString()))
            {
                string keyId = data[GetIdString()] as string;
                T obj = FindObject(keyId);

                if (obj == null)
                {
                    obj = ConstructObject(keyId); ;
                    Objects[(string)keyId] = obj;
                }

                obj.Merge(data);
                return obj;
            }
            return null;
        }

        public T GetObject(Request r)
        {
            if (!r.HasError())
            {
                Dictionary<object, object> dataPayload = (Dictionary<object, object>)r.Data;

                return GetObject(dataPayload);
            }
            return default(T);
        }

        public List<T> GetObjectList()
        {
            List<T> objects = new List<T>();
            foreach (var entry in Objects)
            {
                objects.Add(entry.Value);
            }

            return objects;
        }

        public void Remove(string id)
        {
            Objects.Remove(id);
        }

        public void Clear()
        {
            Objects.Clear();
        }

        protected abstract string GetIdString();
        protected abstract T ConstructObject(string id);
    }

    public class ObjectMap<T> : ObjectMapBase<T> where T : DataStore
    {
        public delegate T ConstructFunc(Client client, string id);

        protected Client Client { get; set; }
        protected ConstructFunc Construct { get; set; }

        public ObjectMap(Client client, ConstructFunc construct)
            : base()
        {
            Client = client;
            Construct = construct;
        }

        protected override string GetIdString()
        {
            const string idString = "id";
            return idString;
        }

        protected override T ConstructObject(string id)
        {
            return Construct(Client, id);
        }
    }
}


using System;
using System.Collections.Generic;

using AgoraGames.Hydra.Util;
using System.Collections;

namespace AgoraGames.Hydra.Models
{
    public class DataStore
    {
        public Dictionary<object, object> Data { get; protected set; }

        public DataStore()
        {
            Data = new Dictionary<object, object>();
        }

        public object this[string key]
        {
            get { return new MapHelper(Data).GetValue(key, (object)null); }
        }

        public virtual void Merge(IDictionary map)
        {
            new MapHelper(Data).Merge(map);
        }
    }

    public abstract class Model : DataStore
    {
        protected string id;

        protected Client client;
        public Client Client { get { return client; } }

        public Model(Client client, string id)
        {
            this.client = client;
            this.id = id;
        }

        public string Id
        {
            get { return id; }
        }

        public abstract string Endpoint
        {
            get;
        }

        public virtual string EndpointId
        {
            get { return Id; }
        }

        public void ObjectResponse(Request request)
        {
            if (!request.HasError())
            {
                Merge((Dictionary<object, object>)request.Data);
            }
        }

        public override void Merge(IDictionary map)
        {
            base.Merge(map);

            // TODO: real merge!!
            id = (string)map["id"];
        }
    }

    public abstract class EditableModel : Model
    {
        public EditableModel(Client client, string id)
            : base(client, id)
        {
        }

        public virtual void Load(AgoraGames.Hydra.Client.HydraRequestHandler response)
        {
            Load(null, response);
        }

        public virtual void Load(List<String> fields, AgoraGames.Hydra.Client.HydraRequestHandler response)
        {
            HelperUtil.DualDelegate dual = new HelperUtil.DualDelegate(ObjectResponse, response);

            client.DoRequest(new UrlGenerator(Endpoint + "/" + EndpointId).Append("fields", fields).ToString(), "get", null, dual.Response);
        }

        public virtual void Update(Commands commands, AgoraGames.Hydra.Client.HydraRequestHandler response)
        {
            Update(commands, null, response);
        }

        public virtual void Update(Commands commands, List<String> fields, AgoraGames.Hydra.Client.HydraRequestHandler response)
        {
            HelperUtil.DualDelegate dual = new HelperUtil.DualDelegate(ObjectResponse, response);

            client.DoRequest(new UrlGenerator(Endpoint + "/" + EndpointId).Append("fields", fields).ToString(), "put", commands.ConvertToRequest(), dual.Response);
        }

        public virtual void Update(Dictionary<object, object> data, AgoraGames.Hydra.Client.HydraRequestHandler response)
        {
            Update(data, null, response);
        }

        public virtual void Update(Dictionary<object, object> data, List<String> fields, AgoraGames.Hydra.Client.HydraRequestHandler response)
        {
            HelperUtil.DualDelegate dual = new HelperUtil.DualDelegate(ObjectResponse, response);

            client.DoRequest(new UrlGenerator(Endpoint + "/" + EndpointId).Append("fields", fields).ToString(), "put", data, dual.Response);
        }
    }
}
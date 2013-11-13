using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Util;

namespace AgoraGames.Hydra.Models
{
    public class BroadcastMessage
    {
        public delegate void BroadcastMessageHandler(BroadcastMessage message, Request request);
        public delegate void BroadcastMessageListHandler(List<BroadcastMessage> messages, Request request);

        protected string id;
        public string Id
        {
            get { return id; }
        }

        protected string channelId;
        public string ChannelId
        {
            get { return channelId; }
        }

        protected string message;
        public string Message
        {
            get { return message; }
        }

        protected object data;
        public object Data
        {
            get { return data; }
        }

        protected DateTime startAt;
        public DateTime StartAt
        {
            get { return startAt; }
        }

        protected DateTime endAt;
        public DateTime EndAt
        {
            get { return endAt; }
        }

        public BroadcastMessage(Dictionary<object, object> data)
        {
            MapHelper mapHelper = new MapHelper(data);
            this.id = mapHelper.GetValue("id", (string)null);
            this.channelId = mapHelper.GetValue("broadcast_channel_id", (string)null);
            this.message = mapHelper.GetValue("content", (string)null);
            this.data = mapHelper.GetValue("data", (object)null);

            this.startAt = mapHelper.GetValue("start_at", DateTime.UtcNow);
            this.endAt = mapHelper.GetValue("end_at", DateTime.UtcNow);
        }

        public static List<BroadcastMessage> ResolveList(Request request)
        {
            List<BroadcastMessage> ret = new List<BroadcastMessage>();

            if (!request.HasError())
            {
                List<object> data = (List<object>)request.Data;

                foreach (Dictionary<object, object> iter in data)
                {
                    ret.Add(new BroadcastMessage(iter));
                }
            }
            return ret;
        }

        public static BroadcastMessage Resolve(Request request)
        {
            if (!request.HasError())
            {
                Dictionary<object, object> data = (Dictionary<object, object>)request.Data;

                return new BroadcastMessage(data);
            }
            return null;
        }
    }
}

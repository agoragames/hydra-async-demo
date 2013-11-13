using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Models;
using AgoraGames.Hydra.Util;

namespace AgoraGames.Hydra.Services
{
    public class BroadcastNotificationsService
    {
        protected ObjectMap<BroadcastChannel> map = null;
        protected Client client = null;

        public BroadcastNotificationsService(Client client)
        {
            this.client = client;
            this.map = new ObjectMap<BroadcastChannel>(client, (c, i) => { return new BroadcastChannel(c, i); });
        }

        public void Get(string id, AgoraGames.Hydra.Models.BroadcastChannel.BroadcastChannelHandler handler)
        {
            client.DoRequest("broadcast_channels/" + id, "get", null, delegate(Request req)
            {
                if (!req.HasError())
                {
                    handler(map.GetObject(req), req);
                }
                else
                {
                    handler(null, req);
                }
            });
        }

        public void All(AgoraGames.Hydra.Models.BroadcastChannel.BroadcastChannelListHandler handler)
        {
            client.DoRequest("broadcast_channels", "get", null, delegate(Request req)
            {
                if (!req.HasError())
                {
                    handler(ResolveList(req), req);
                }
                else
                {
                    handler(null, req);
                }
            });
        }

        public void CurrentMessages(string channelId, AgoraGames.Hydra.Models.BroadcastMessage.BroadcastMessageListHandler handler)
        {
            client.DoRequest("broadcast_channels/" + channelId + "/broadcast_messages/current", "get", null, delegate(Request req)
            {
                if (!req.HasError())
                {
                    handler(BroadcastMessage.ResolveList(req), req);
                }
                else
                {
                    handler(null, req);
                }
            });
        }

        public void AllMessages(string channelId, AgoraGames.Hydra.Models.BroadcastMessage.BroadcastMessageListHandler handler)
        {
            client.DoRequest("broadcast_channels/" + channelId + "/broadcast_messages", "get", null, delegate(Request req)
            {
                if (!req.HasError())
                {
                    handler(BroadcastMessage.ResolveList(req), req);
                }
                else
                {
                    handler(null, req);
                }
            });
        }

        public List<BroadcastChannel> ResolveList(Request request)
        {
            List<BroadcastChannel> ret = new List<BroadcastChannel>();

            if (!request.HasError())
            {
                List<object> data = (List<object>)request.Data;

                foreach (Dictionary<object, object> iter in data)
                {
                    ret.Add(map.GetObject(iter));
                }
            }
            return ret;
        }
    }
}

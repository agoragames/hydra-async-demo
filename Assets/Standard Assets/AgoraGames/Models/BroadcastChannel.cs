using AgoraGames.Hydra.Util;
using System.Collections;
using System.Collections.Generic;

namespace AgoraGames.Hydra.Models
{
    public class BroadcastChannel : Model
    {
        public delegate void BroadcastChannelHandler(BroadcastChannel channel, Request request);
        public delegate void BroadcastChannelListHandler(List<BroadcastChannel> list, Request request);
        
        protected string name;
        public string Name { get { return name; } }

        protected string slug;
        public string Slug { get { return slug; } }

        public override string Endpoint
        {
            get { return "broadcast_channels"; }
        }

        public BroadcastChannel(Client client, string id) : base(client, id)
        {
        }

        public void CurrentMessages(AgoraGames.Hydra.Models.BroadcastMessage.BroadcastMessageListHandler handler)
        {
            client.DoRequest("broadcast_channels/" + Id + "/broadcast_messages/current", "get", null, delegate(Request req)
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

        public void AllMessages(AgoraGames.Hydra.Models.BroadcastMessage.BroadcastMessageListHandler handler)
        {
            client.DoRequest("broadcast_channels/" + Id + "/broadcast_messages", "get", null, delegate(Request req)
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

        public override void Merge(IDictionary map)
        {
            base.Merge(map);

            MapHelper mapHelper = new MapHelper(map);
            name = mapHelper.GetValue("name", (string)null);
            slug = mapHelper.GetValue("slug", (string)null);

            // TODO: this data may contain messages
        }
    }
}

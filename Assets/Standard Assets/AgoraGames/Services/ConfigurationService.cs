using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Models;

namespace AgoraGames.Hydra.Services
{
    public class ConfigurationService
    {
        protected Client client = null;

        public delegate void ConfigurationHandler(Configuration obj, Request request);

        public ConfigurationService(Client client)
        {
            this.client = client;
        }

        public void Load(ConfigurationHandler handler)
        {
            client.DoRequest("configuration/sdk?sdk=unity&version=" + Client.VERSION, "get", null, delegate(Request request)
            {
                if (!request.HasError())
                {
                    handler(new Configuration((Dictionary<object, object>)request.Data), request);
                }
            });
        }
    }
}

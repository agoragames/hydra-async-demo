using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgoraGames.Hydra
{
    public class Request
    {
        public object Data;
        public Dictionary<string, string> Headers;
        
        public int Status;

        public string Message;

        // delegate
        protected Client.HydraRequestHandler response;
        public Client.HydraRequestHandler Response { get { return response; } }

        protected Client client;
        public Client Client { get { return client; } }
        
        protected string service;
        public string Service { get { return service; } }

        protected string command;
        public string Command { get { return command; } }

        protected object param;
        public object Param { get { return param; } }

        public Request(Client client, Client.HydraRequestHandler response, string service, string command, object param)
        {
            this.client = client;
            this.response = response;
            this.service = service;
            this.command = command;
            this.param = param;
        }

        public bool HasError()
        {
            // TODO: this is a shitty hack
            return !(Status == 200 || Status == 201);
        }

        // should be invoked in the same thread as the caller
        public void NotifyComplete()
        {
            if(Response != null) 
            {
                Response(this);
            }
        }
    }
}

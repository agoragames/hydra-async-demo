using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Util;
using System.Collections;

namespace AgoraGames.Hydra
{
    public class MatchLogic : IRealtimeLogic
    {
        public delegate void MatchUpdatedHandler(MatchLogic data);

        public event MatchUpdatedHandler MatchUpdated;

        RealtimeSession session;
        public Dictionary<string, object> Data { get; protected set; }

        public MatchLogic(RealtimeSession session)
        {
            this.session = session;
            Data = new Dictionary<string, object>();
        }

        public RealtimeSession Session {
            get { return session; }
            set { session = value; } 
        }

        public void Update(Commands commands)
        {
            Dictionary<string, object> msg = new Dictionary<string, object>();

            MapHelper mapHelper = new MapHelper(msg);
            mapHelper["cmd"] = "update";
            mapHelper["data"] = commands.ConvertToRequest();

            session.LogicSend(msg);
        }

        public void MessageRecieved(byte[] data)
        {
        }

        public void MessageStringRecieved(string data)
        {
        }

        public void MessageObjectRecieved(object obj)
        {
            if (obj is IDictionary)
            {
                IDictionary msgData = obj as IDictionary;
                string cmd = (string)msgData["cmd"];

                object updatedDataVal = msgData["data"];
                if (updatedDataVal is IDictionary)
                {
                    IDictionary updatedData = updatedDataVal as IDictionary;
                    if (cmd == "init")
                    {
                        Data = new Dictionary<string, object>();
                        new MapHelper(Data).Merge(updatedData);

                        if (MatchUpdated != null)
                        {
                            MatchUpdated(this);
                        }
                    }
                    else if (cmd == "update")
                    {
                        new MapHelper(Data).Merge(updatedData);
                        if (MatchUpdated != null)
                        {
                            MatchUpdated(this);
                        }
                    }
                }
            }
        }
    }
}

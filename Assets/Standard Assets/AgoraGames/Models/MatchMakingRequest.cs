using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace AgoraGames.Hydra.Models
{
    public class MatchMakingRequest : Model
    {
        public delegate void MatchMakingHandler(MatchMakingRequest request);
        public event MatchMakingHandler Complete;
        public event MatchMakingHandler Tick;
        public event MatchMakingHandler Timeout;

        protected DateTime completeTime;
        public bool IsCompleted { get; protected set; }
        public bool Found { get; protected set; }

        public int TickCount { get; protected set; }

        public Match Match { get; protected set; }

        public DateTime Started { get; protected set; }
        public TimeSpan Wait { 
            get {
                if (IsCompleted)
                {
                    return completeTime.Subtract(Started);
                }
                else
                {
                    return DateTime.Now.Subtract(Started);
                }
            } 
        }

        public override string Endpoint
        {
            get { return "matches/matchmaking/request"; }
        }

        public MatchMakingRequest(Client client, string id)
            : base(client, id)
        {
            Started = DateTime.Now;
            IsCompleted = false;
            TickCount = 0;
        }

        public void Dispatch(string command, Dictionary<object, object> message)
        {
            if (command == "matchmaking-tick")
            {
                TickCount++;

                if (Tick != null)
                {
                    Tick(this);
                }
            }
            else if (command == "matchmaking-complete")
            {
                Match = client.Match.ResolveMatchFromMessage(message);
                InternalComplete(true);

                if (Complete != null)
                {
                    Complete(this);
                }
            }
            else if (command == "matchmaking-timeout")
            {
                InternalComplete(false);

                if (Timeout != null)
                {
                    Timeout(this);
                }
            }
        }

        protected void InternalComplete(bool found)
        {
            completeTime = DateTime.Now;
            IsCompleted = true;
            Found = found;
        }

        public override void Merge(IDictionary map)
        {
            // TODO: get out data....
            base.Merge(map);
        }
    }
}

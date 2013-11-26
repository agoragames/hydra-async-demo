using AgoraGames.Hydra.Util;
using System.Collections;
using System.Collections.Generic;

namespace AgoraGames.Hydra.Models
{
    public class LeaderboardEntry : DataStore
    {
        protected Client Client { get; set; }
        public long Rank { get; protected set; }
        public double Score { get; protected set; }
        public Profile Profile { get; protected set; }

        public LeaderboardEntry(Client client)
        {
            Client = client;
        }


        public override void Merge(IDictionary map)
        {
            base.Merge(map);

            MapHelper mapHelper = new MapHelper(map);
            Rank = mapHelper.GetValue("rank", Rank);
            Score = mapHelper.GetValue("score", Score);

            // Even though we don't use the account, we need to resolve it so it gets cached and our profile can find it
            this.Client.Account.Resolve((Dictionary<object, object>)mapHelper["account"]);
            Profile = this.Client.Profile.ResolveProfile((Dictionary<object, object>)mapHelper["profile"]);
        }
    }
}

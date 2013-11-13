using System.Collections;
using System.Collections.Generic;

using AgoraGames.Hydra.Util;
using System;

namespace AgoraGames.Hydra.Models
{
	public class Profile : EditableModel
	{
        protected string accountId;
        public string AccountId { get { return accountId; } }

        public Account Account { get; protected set; }

        protected DateTime updatedAt;
        public DateTime UpdatedAt { get { return updatedAt; } }

        protected DateTime createdAt;
        public DateTime CreatedAt { get { return createdAt; } }

        public override string Endpoint
        {
            get { return "profiles"; }
        }

        public override string EndpointId
        {
            get { return AccountId; }
        }

        public Profile(Client client, string id)
            : base(client, id)
        {
        }

        public override void Merge(IDictionary map)
        {
            base.Merge(map);

            MapHelper mapHelper = new MapHelper(map);
            accountId = mapHelper.GetValue("account_id", accountId);
            createdAt = mapHelper.GetValue("created_at", createdAt);
            updatedAt = mapHelper.GetValue("updated_at", updatedAt);

            Account = Client.Account.Resolve(accountId);
        }
	}
}
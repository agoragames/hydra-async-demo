
using AgoraGames.Hydra.Models;
using AgoraGames.Hydra.Util;
using System.Collections.Generic;

namespace AgoraGames.Hydra.Services
{
	public class AccountsService
	{
        protected Client client;
        protected ObjectMap<Account> map;

        public delegate void AccountHandler(Account obj, Request request);
        public delegate void FriendHandler(string accountId);

        public event FriendHandler Friended;
        public event FriendHandler FriendOnline;
        public event FriendHandler FriendOffline;

		public AccountsService (Client client)
		{
            this.map = new ObjectMap<Account>(client, (c, i) => { return new Account(c, i); });
            this.client = client;
		}

        public void Load(AccountHandler handler)
        {
            Load("me", handler);
		}
		
		public void Load(string id, AccountHandler handler) 
        {
            client.DoRequest("accounts/" + id, "get", null, delegate(Request request) {
                handler(map.GetObject(request), request);
            });
		}

        public Account FindAccount(string accountId)
        {
            return map.FindObject(accountId);
        }

        public Account Resolve(string accountId)
        {
            Dictionary<object, object> data = new Dictionary<object, object>();
            data["id"] = accountId;
            return Resolve(data);
        }

        public Account Resolve(Dictionary<object, object> data)
        {
            return map.GetObject(data);
        }

        public void Dispatch(string cmd, Dictionary<object, object> data)
        {
            Dictionary<object, object> payload = (Dictionary<object, object>)data["payload"];

            if (cmd == "friended")
            {
                string accountId = payload["friender_id"] as string;
                if (Friended != null)
                    Friended(accountId);
            }
            else if (cmd == "friend-online")
            {
                string accountId = payload["friend_id"] as string;
                client.MyAccount.Dispatch(cmd, payload);
                if (FriendOnline != null)
                    FriendOnline(accountId);
            }
            else if (cmd == "friend-offline")
            {
                string accountId = payload["friend_id"] as string;
                client.MyAccount.Dispatch(cmd, payload);
                if (FriendOffline != null)
                    FriendOffline(accountId);
            }
        }
	}
}


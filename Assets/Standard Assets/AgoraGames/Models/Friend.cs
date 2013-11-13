using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Util;
using System.Collections;

namespace AgoraGames.Hydra.Models
{
    public enum Presence
    {
        Offline,
        Online,
        Unknown
    }

    static class PresenceMethods
    {
        const string offlineString = "offline";
        const string onlineString = "online";
        const string unknownString = "unknown";

        public static Presence FromString(string str)
        {
            if (str == offlineString)
                return Presence.Offline;
            else if (str == onlineString)
                return Presence.Online;
            else
                return Presence.Unknown;
        }

        public static Presence FromObject(object obj)
        {
            return FromString(obj as string);
        }
    }

    public enum Visibility
    {
        Private = 0,
        Public,
        Unknown
    }

    static class VisibilityMethods
    {
        const string publicString = "public";
        const string privateString = "private";
        const string unknownString = "unknown";

        public static Visibility FromString(string str)
        {
            if (str == publicString)
                return Visibility.Public;
            else if (str == privateString)
                return Visibility.Private;
            else
                return Visibility.Unknown;
        }

        public static Visibility FromObject(object obj)
        {
            return FromString(obj as string);
        }

        public static string ToString(Visibility visibility)
        {
            if (visibility == Visibility.Public)
                return publicString;
            else if (visibility == Visibility.Private)
                return privateString;
            else
                return unknownString;
        }
    }

    public class Friend : DataStore
    {
        protected Client Client { get; set; }
        protected Identity _identity;

        public string AccountId { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public Identity Identity
        {
            get
            {
                Account acct = GetAccount();
                return acct == null || acct.Identity == null ? _identity : acct.Identity;
            }
            protected set { _identity = value; }
        }
        public bool IsMutualFriend { get; protected set; }
        public Presence Presence { get; set; }
        public Visibility Visibility { get; protected set; }

        public Friend(Client client, string accountId)
            : base()
        {
            Client = client;
            AccountId = accountId;
            IsMutualFriend = false;
            Presence = Presence.Unknown;
            Visibility = Visibility.Unknown;
        }

        public override void Merge(IDictionary map)
        {
            base.Merge(map);

            MapHelper mapHelper = new MapHelper(map);
            CreatedAt = mapHelper.GetValue("created_at", CreatedAt);
            object identityVal = mapHelper.GetValue("identity");
            if(identityVal != null && identityVal is Dictionary<object, object>)
                Identity = new Identity(identityVal as Dictionary<object, object>);
            IsMutualFriend = mapHelper.GetValue("mutual", IsMutualFriend);
            Presence = mapHelper.GetValue("presence", Presence, PresenceMethods.FromObject);
            Visibility = mapHelper.GetValue("visibility", Visibility, VisibilityMethods.FromObject);
        }

        public Account GetAccount()
        {
            return Client.Account.FindAccount(AccountId);
        }
    }

    public class FriendMap : ObjectMapBase<Friend>
    {
        protected Client Client { get; set; }

        public FriendMap(Client client)
        {
            Client = client;
        }

        protected override string GetIdString()
        {
            const string idString = "account_id";
            return idString;
        }

        protected override Friend ConstructObject(string id)
        {
            return new Friend(Client, id);
        }
    }
}

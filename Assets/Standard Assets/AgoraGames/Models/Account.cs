using System;
using System.Collections;
using System.Collections.Generic;
using AgoraGames.Hydra.Util;
namespace AgoraGames.Hydra.Models
{
    public class Account : Model
    {
        public Identity Identity { get; protected set; }

        protected FriendMap Friends { get; set; }
        public bool FriendsLoaded { get; protected set; }

        public DateTime CreatedAt { get; protected set; }
        public DateTime UpdatedAt { get; protected set; }
        public Presence Presence { get; protected set; }
        public Visibility Visibility { get; protected set; }

        public delegate void BlockedListHandler(List<Friend> blocked, Request request);

        public override string Endpoint
        {
            get { return "accounts"; }
        }

        public Account(Client client, string id) : base(client, id)
        {
            Friends = new FriendMap(client);
            FriendsLoaded = false;

            Presence = Models.Presence.Unknown;
            Visibility = Models.Visibility.Unknown;
        }

        public void LoadFriends(AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            HelperUtil.DualDelegate dual = new HelperUtil.DualDelegate(HandleLoadFriendsResponse, handler);
            client.DoRequest("accounts/" + Id + "/friends", "get", null, dual.Response);
        }

        public void AddFriendByUsername(String friendName, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            Dictionary<string, object> request = new Dictionary<string, object>();

            request["friend_id"] = friendName;
            request["friend_type"] = "name";

            client.DoRequest("accounts/" + Id + "/friends", "post", request, 
                delegate(Request r)
                {
                    HandleAddFriendResponse(r);
                    handler(r);
                }
            );
        }

        public void AddFriend(String friendId, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            Dictionary<string, object> request = new Dictionary<string, object>();

            request["friend_id"] = friendId;

            client.DoRequest("accounts/" + Id + "/friends", "post", request,
                delegate(Request r)
                {
                    HandleAddFriendResponse(r);
                    handler(r);
                }
            );
        }

        public void RemoveFriend(String friendId, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            client.DoRequest("accounts/" + Id + "/friends/" + friendId, "delete", null,
                delegate(Request r)
                {
                    HandleRemoveFriendResponse(friendId, r);
                    handler(r);
                }
            );
        }

        public void UpdateIdentity(Identity identity, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            HelperUtil.DualDelegate dual = new HelperUtil.DualDelegate(ObjectResponse, handler);
            client.DoRequest("accounts/" + Id + "/identity", "put", identity.ConvertToRequest(), dual.Response);
        }

        public void Link(AuthType type, string token, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            Link(type, token, new Dictionary<string, string>(), handler);
        }

        public void Link(AuthType type, string token, Dictionary<string, string> data, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            client.DoRequest("accounts/me/link", "put", Auth.GenerateAuthRequest(type, token, data), delegate(Request request)
            {
                // TODO: we need to handle the response here... the server will return a status which
                //  will tell us that the link account already exists and we can then login and relink in the other directoin
                //  we can handle that automatically here...

                handler(request);
            });
        }

        public void LoadBlocked(BlockedListHandler handler)
        {
            client.DoRequest("accounts/" + Id + "/friends/blocked", "get", null, delegate(Request request)
            {
                List<Friend> blocked = HandleLoadBlockedResponse(request);
                handler(blocked, request);
            });
        }

        public void Block(string accountId, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            BlockInternal(accountId, false, handler);
        }

        public void BlockByUsername(string username, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            BlockInternal(username, true, handler);
        }

        protected void BlockInternal(string blockId, bool idIsName, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            Dictionary<string, object> request = new Dictionary<string, object>();
            request["friend_id"] = blockId;
            if (idIsName)
                request["friend_type"] = "name";

            string url = "accounts/" + Id + "/friends/block";
            client.DoRequest(url, "post", request,
                delegate(Request r)
                {
                    handler(r);
                }
            );
        }

        public void Unblock(string accountId, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            string url = "accounts/" + Id + "/friends/unblock/" + accountId;
            client.DoRequest(url, "delete", new Dictionary<object, object>(),
                delegate(Request r)
                {
                    handler(r);
                }
            );
        }

        public void UpdateVisibility(Visibility visibility, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            Dictionary<string, object> request = new Dictionary<string, object>();
            request["visibility"] = (int)visibility;

            string url = "accounts/me/update_visibility";
            client.DoRequest(url, "put", request,
                delegate(Request r)
                {
                    HandleUpdateVisibilityResponse(visibility, r);
                    handler(r);
                }
            );
        }

        public List<Friend> GetFriends()
        {
            return Friends.GetObjectList();
        }

        protected void HandleLoadFriendsResponse(Request request)
        {
            if (!request.HasError())
            {
                HandleLoadFriendMapResponse(Friends, request);
                FriendsLoaded = true;
            }
        }

        protected List<Friend> HandleLoadBlockedResponse(Request request)
        {
            List<Friend> blockedList = null;
            if (!request.HasError())
            {
                FriendMap blocked = new FriendMap(Client);
                HandleLoadFriendMapResponse(blocked, request);
                blockedList = blocked.GetObjectList();
            }

            return blockedList;
        }

        protected void HandleLoadFriendMapResponse(FriendMap friendMap, Request request)
        {
            if (!request.HasError())
            {
                friendMap.Clear();

                object value = request.Data;
                if (value is IList)
                {
                    IList list = value as IList;
                    foreach (var item in list)
                    {
                        IDictionary data = item as IDictionary;
                        friendMap.GetObject(data);
                    }
                }
            }
        }

        protected void HandleAddFriendResponse(Request request)
        {
            if (!request.HasError())
            {
                Friends.GetObject(request.Data as IDictionary);
            }
        }

        protected void HandleRemoveFriendResponse(string friendId, Request request)
        {
            if (!request.HasError())
            {
                Friends.Remove(friendId);
            }
        }

        protected void HandleUpdateVisibilityResponse(Visibility visibility, Request request)
        {
            if (!request.HasError())
            {
                Visibility = visibility;
            }
        }

        public override void Merge(IDictionary map)
        {
            base.Merge(map);

            MapHelper mapHelper = new MapHelper(map);
            Dictionary<object, object> identityVal = mapHelper.GetValue("identity", (Dictionary<object, object>)null);
            if(identityVal != null)
                Identity = new Identity(identityVal);
            CreatedAt = mapHelper.GetValue("created_at", CreatedAt);
            UpdatedAt = mapHelper.GetValue("updated_at", UpdatedAt);
            Presence = mapHelper.GetValue("presence", Presence, PresenceMethods.FromObject);
            Visibility = mapHelper.GetValue("visibility", Visibility, VisibilityMethods.FromObject);
        }

        public void Dispatch(string cmd, Dictionary<object, object> data)
        {
            if (cmd == "friend-online")
                DispatchPresenceChange(data, Presence.Online);
            else if (cmd == "friend-offline")
                DispatchPresenceChange(data, Presence.Offline);
        }

        protected void DispatchPresenceChange(Dictionary<object, object> data, Presence presence)
        {
            string accountId = data["friend_id"] as string;

            Friend friend = Friends.FindObject(accountId);
            if (friend != null)
            {
                // TODO: should we update the 'data' part of the friend object too?
                friend.Presence = presence;
            }
        }

    }
}

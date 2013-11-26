using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Models;

namespace AgoraGames.Hydra.Test
{
    public class AccountMenu : BaseMenu
    {
        string text;
        protected string NotificationText { get; set; }

        List<Friend> friends = new List<Friend>();
        string[] friendNames = new string[0];

        List<Friend> blocked = new List<Friend>();
        string[] blockedNames = new string[0];

        int selectedFriend = -1;
        int selectedBlocked = -1;

        public AccountMenu(Main main) : base(main)
        {
            NotificationText = "";
        }

        public override void Deactivate()
        {
#if UNITY_WEBPLAYER
            Main.FacebookTokenReceived -= Main_FacebookTokenReceived;
#endif
        }

        public override void Render()
        {
            TestUtil.RenderHeader(this, "Account");
            int y = 0;

            if (GUI.Button(new Rect(0, y+=60, 130, 30), "Change Username"))
            {
                Main.PopupDialog("Change Username", delegate(string text)
                {
                    Identity newIdentity = new Identity(text);

                    Client.Instance.MyAccount.UpdateIdentity(newIdentity, delegate(Request req)
                    {
                        UpdateAccount();
                        // TODO: handle error
                    });
                });
            }

            if (GUI.Button(new Rect(0, y+30, 130, 30), "Change Password"))
            {
                Main.PopupDialog("Change Password", delegate(string text)
                {
                    Client.Instance.MyAccount.SetPassword(text, delegate(Request req)
                    {
                    });
                });
            }

            if (GUI.Button(new Rect(140, y, 130, 30), "Change Email"))
            {
                Main.PopupDialog("Change Email", delegate(string text)
                {
                    Identity identity = Client.Instance.MyAccount.Identity;
                    identity.Email = text;

                    Client.Instance.MyAccount.UpdateIdentity(identity, delegate(Request req)
                    {
                    });
                });
            }

            if (GUI.Button(new Rect(140, y+30, 130, 30), "Add Friend"))
            {
                Main.PopupDialog("Username", delegate(string text)
                {
                    Client.Instance.MyAccount.AddFriendByUsername(text, delegate(Request request)
                    {
                        RefreshFriends();
                    });
                });
            }

#if UNITY_WEBPLAYER
            if (GUI.Button(new Rect(280, y, 130, 60), "Link facebook"))
            {
                Main.FacebookLogin();
            }
#endif

            if (GUI.Button(new Rect(420, y, 130, 60), "Toggle Visibility"))
            {
                Visibility newVis = Client.Instance.MyAccount.Visibility == Visibility.Public ? Visibility.Private : Visibility.Public;
                Client.Instance.MyAccount.UpdateVisibility(newVis, delegate(Request request)
                {
                });
            }

            string fieldText = "Visibility: " + VisibilityMethods.ToString(Client.Instance.MyAccount.Visibility);
            fieldText += " -- Created at: " + Client.Instance.MyAccount.CreatedAt.ToString();
            fieldText += " -- Updated at: " + Client.Instance.MyAccount.UpdatedAt.ToString();
            GUI.TextArea(new Rect(0, y += 70, 900, 40), fieldText);

            GUI.TextArea(new Rect(0, y += 50, 900, 360), this.text);

            GUI.TextArea(new Rect(0, y+=370, 130, 32), "Friends");
            GUI.TextArea(new Rect(400, y, 130, 32), "Blocked");

            int yList = y;

            selectedFriend = GUI.SelectionGrid(new Rect(0, y+=42, 260, friendNames.Length * 32), selectedFriend, friendNames, 1);
            for (int i = 0; i < friends.Count; i++)
            {
                if (GUI.Button(new Rect(270, y, 60, 32), "Remove"))
                {
                    RemoveFriend(i);
                }

                if (GUI.Button(new Rect(340, y, 60, 32), "Block"))
                {
                    Block(i);
                }

                y += 32;
            }

            int yFriendEnd = y;

            y = yList;
            selectedBlocked = GUI.SelectionGrid(new Rect(400, y += 42, 260, blockedNames.Length * 32), selectedBlocked
                , blockedNames, 1);
            for (int i = 0; i < blocked.Count; i++)
            {
                if (GUI.Button(new Rect(670, y, 60, 32), "Unblock"))
                {
                    Unblock(i);
                }

                y += 32;
            }

            if (y < yFriendEnd)
                y = yFriendEnd;

            GUI.TextArea(new Rect(0, y += 10, 360, 32), this.NotificationText);

            if (selectedFriend != -1)
            {
                Main.SetState(global::Main.State.FRIEND, friends[selectedFriend]);
            }

            if (selectedBlocked != -1)
            {
                Main.SetState(global::Main.State.FRIEND, blocked[selectedBlocked]);
            }
        }

        protected void RemoveFriend(int index)
        {
            Friend friend = friends[index];
            Client.Instance.MyAccount.RemoveFriend(friend.AccountId, delegate(Request request)
            {
                RefreshFriends();
            });
        }

        protected void Block(int index)
        {
            Friend blockedAccount = friends[index];
            Client.Instance.MyAccount.Block(blockedAccount.AccountId, delegate(Request request)
            {
                RefreshBlocked();
            });
        }

        protected void Unblock(int index)
        {
            Friend blockedAccount = blocked[index];
            Client.Instance.MyAccount.Unblock(blockedAccount.AccountId, delegate(Request request)
            {
                RefreshBlocked();
            });
        }

        protected void RefreshFriends()
        {
            if (Client.Instance.MyAccount.FriendsLoaded)
            {
                ProcessFriends();
            }
            else
            {
                Client.Instance.MyAccount.LoadFriends(delegate(Request request)
                {
                    ProcessFriends();
                });
            }
        }

        protected void ProcessFriends()
        {
            this.friends = Client.Instance.MyAccount.GetFriends();
            GenerateFriendNames();
            UpdateAccount();
        }

        protected void GenerateFriendNames()
        {
            friendNames = new string[friends.Count];

            for (int i = 0; i < friends.Count; i++)
            {
                friendNames[i] = friends[i].Identity.UserName;
            }
        }

        protected void RefreshBlocked()
        {
            Client.Instance.MyAccount.LoadBlocked(delegate(List<Friend> blocked, Request request)
            {
                this.blocked = blocked;
                GenerateBlockedNames();
            });
        }

        protected void GenerateBlockedNames()
        {
            blockedNames = new string[blocked.Count];

            for (int i = 0; i < blocked.Count; i++)
            {
                blockedNames[i] = blocked[i].Identity.UserName;
            }
        }

        protected void UpdateAccount()
        {
            text = MiniJSON.Json.Serialize(Client.Instance.MyAccount.Data, true);
        }

        public override void SetParam(object param)
        {
            selectedFriend = -1;
            UpdateAccount();
            RefreshFriends();
            RefreshBlocked();

            Client.Instance.Account.Friended += (friendId) => { this.NotificationText = friendId + " friended me!"; };
            Client.Instance.Account.FriendOnline += (friendId) => { this.NotificationText = friendId + " is online!"; };
            Client.Instance.Account.FriendOffline += (friendId) => { this.NotificationText = friendId + " is offline!"; };

#if UNITY_WEBPLAYER
            Main.FacebookTokenReceived += Main_FacebookTokenReceived;
#endif
        }

#if UNITY_WEBPLAYER
        void Main_FacebookTokenReceived(string fbtoken)
        {
            Client.Instance.MyAccount.Link(new FacebookAuth(fbtoken), delegate(Request request)
            {
            });
        }
#endif
    }
}

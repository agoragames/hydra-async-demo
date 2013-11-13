using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Models;
using UnityEngine;

namespace AgoraGames.Hydra.Test
{
    public class FriendMenu: BaseMenu
    {
        Friend friend;
        Profile profile = null;

        public FriendMenu(Main main)
            : base(main)
        {
        }

        public override void Deactivate()
        {
        }

        public override void Render()
        {
            TestUtil.RenderHeader(this, "Friend");
            int y = 0;

            string friendData = "Parsed Friend Data\n";
            friendData += "\n Account ID: " + friend.AccountId;
            friendData += "\n Created At: " + friend.CreatedAt.ToString();
            friendData += "\n Username: " + friend.Identity.UserName;
            friendData += "\n Avatar: " + friend.Identity.Avatar;
            friendData += "\n Mutual: " + friend.IsMutualFriend;
            friendData += "\n Presence: " + friend.Presence;
            friendData += "\n Visibility: " + friend.Visibility;
            if (profile != null)
            {
                friendData += "\n Kills: " + profile["data.kills"];
            }
            GUI.TextArea(new Rect(0, y += 60, 360, 360), friendData);

            string friendResponse = MiniJSON.Json.Serialize(friend.Data, true); ;
            GUI.TextArea(new Rect(370, y, 360, 360), friendResponse);
        }

        public override void SetParam(object param)
        {
            friend = (Friend)param;

            List<string> fields = new List<string>();
            fields.Add("data.kills");
            Client.Instance.Profile.Load(friend.AccountId, fields, delegate(Profile profile, Request request)
            {
                this.profile = profile;
            });

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AgoraGames.Hydra.Test
{
    public class ProfileMenu : BaseMenu
    {
        string text;

        public ProfileMenu(Main main)
            : base(main)
        {
        }

        public override void Deactivate()
        {
        }

        public override void Render()
        {
            int y = 0;

            TestUtil.RenderHeader(this, "Profile");

            if (GUI.Button(new Rect(0, y += 60, 70, 60), "Kills"))
            {
                UpdateStat("data.kills");
            }

            if (GUI.Button(new Rect(80, y, 70, 60), "Deaths"))
            {
                UpdateStat("data.deaths");
            }

            GUI.TextArea(new Rect(0, y += 70, 360, 300), this.text);
        }

        protected void UpdateStat(string stat)
        {
            Commands commands = new Commands();

            commands.IncValue(stat, 1);
            commands.SetValue("data.date", DateTime.Now);
            commands.IncValue("data.float", 1.10000);

            Client.Instance.MyProfile.Update(commands, delegate(Request req)
            {
                // TODO: handle error
                UpdateProfile();
            });
        }

        protected void UpdateProfile()
        {
            text = MiniJSON.Json.Serialize(Client.Instance.MyProfile.Data, true);
        }

        public override void SetParam(object param)
        {
            UpdateProfile();
        }
    }
}

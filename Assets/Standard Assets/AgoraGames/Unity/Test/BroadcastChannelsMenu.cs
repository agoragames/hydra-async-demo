using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using AgoraGames.Hydra.Models;

namespace AgoraGames.Hydra.Test
{
    public class BroadcastChannelsMenu : BaseMenu
    {
        List<BroadcastChannel> channels;

        string[] channelNames = new string[0];
        int selectedChannel;

        public BroadcastChannelsMenu(Main main)
            : base(main)
        {
        }

        public override void Deactivate()
        {
        }

        public override void Render()
        {
            TestUtil.RenderHeader(this, "Broadcast Channels");
            int y = 0;

            selectedChannel = GUI.SelectionGrid(new Rect(0, y += 60, 360, channelNames.Length * 32), selectedChannel, channelNames, 1);
            
            if (selectedChannel != -1)
            {
                Main.SetState(Main.State.BROADCAST_MESSAGES, channels[selectedChannel]);
            }
        }

        protected void LoadChannels()
        {
            Client.Instance.BroadcastNotifications.All(delegate(List<BroadcastChannel> list, Request request)
            {
                channels = list;

                GenerateNames();
            });
        }

        protected void GenerateNames()
        {
            channelNames = new string[channels.Count];
            for (int i = 0; i < channels.Count; i++)
            {
                channelNames[i] = channels[i].Name;
            }
        }

        public override void SetParam(object param)
        {
            selectedChannel = -1;
            LoadChannels();
        }
    }
}

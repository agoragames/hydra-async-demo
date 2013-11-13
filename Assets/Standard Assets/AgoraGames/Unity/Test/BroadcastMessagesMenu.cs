using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Models;
using UnityEngine;

namespace AgoraGames.Hydra.Test
{
    class BroadcastMessagesMenu : BaseMenu
    {
        BroadcastChannel channel;

        List<BroadcastMessage> messages;
        string[] messageText = new string[0];

        public BroadcastMessagesMenu(Main main)
            : base(main)
        {
        }

        public override void Deactivate()
        {
        }

        public override void Render()
        {
            TestUtil.RenderHeader(this, "Broadcast Messages");
            int y = 0;

            GUI.SelectionGrid(new Rect(0, y += 60, 360, 50), -1, messageText, 1);
        }

        protected void LoadMessages()
        {
            channel.CurrentMessages(delegate(List<BroadcastMessage> messages, Request request)
            {
                this.messages = messages;
                LoadMessageText();
            });
        }

        protected void LoadMessageText() 
        {
            messageText = new string[messages.Count];
            
            for (int i = 0; i < messages.Count; i++)
            {
                messageText[i] = messages[i].Message + " -- " + messages[i].Data;
            }
        }

        public override void SetParam(object param)
        {
            channel = (BroadcastChannel)param;

            LoadMessages();
        }
    }
}

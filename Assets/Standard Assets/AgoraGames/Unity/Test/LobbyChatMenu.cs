using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using AgoraGames.Hydra;

namespace AgoraGames.Hydra.Test
{
    public class LobbyChatMenu : BaseMenu
    {
        string text = "";
        string allText = "";
        ChatLobby lobby;

        public LobbyChatMenu(Main main) : base(main)
        {
        }

        public override void Deactivate()
        {
        }

        public override void Render()
        {
            int y = 0;

            TestUtil.RenderHeader(this, "Chat");

            GUI.TextArea(new Rect(0, y += 70, 360, 300), allText);
            this.text = GUI.TextField(new Rect(0, y += 310, 280, 60), this.text);

            if (GUI.Button(new Rect(290, y, 70, 60), "Send"))
            {
                lobby.Logic.SendMessage(this.text);
                this.text = "";
            }
        }

        public override void SetParam(object param)
        {
            lobby = new ChatLobby();

            allText = "";
            lobby.Join();
            lobby.Logic.ChatMessageRecieved += new ChatLobbyLogic.ChatMessageHandler(lobby_MessageRecieved);
            //UpdateProfile();
        }

        void lobby_MessageRecieved(ChatMessage message)
        {
            allText += message.identity.UserName + " : " + message.message + "\n";
        }
    }
}

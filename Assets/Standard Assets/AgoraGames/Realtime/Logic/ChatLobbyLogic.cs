using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Models;
using AgoraGames.Hydra.Util;

namespace AgoraGames.Hydra
{
    public class ChatMessage
    {
        public Identity identity;
        public string message;

        public ChatMessage(Dictionary<object, object> values)
        {
            MapHelper mapHelper = new MapHelper(values);
            identity = new Identity((Dictionary<object, object>)values["identity"]);
            message = mapHelper.GetValue("msg", (string)null);
        }
    }

    public class ChatLobby
    {
        public ChatLobbyLogic Logic;
        public RealtimeSession Session;

        public ChatLobby()
        {
        }

        public void Join()
        {
            Session = Client.Instance.Message.JoinSession(RealtimeLogicFactory.LogicType.ChatLobby, "121212121212121212121212", null);
            Logic = (ChatLobbyLogic)Session.Logic;

            //session.MessageObjectRecieved += session_MessageObjectRecieved;
        }
    }

    public class ChatLobbyLogic : IRealtimeLogic
    {
        RealtimeSession session;
        List<ChatMessage> messages;

        public delegate void ChatMessageHandler(ChatMessage message);
        public event ChatMessageHandler ChatMessageRecieved;

        public ChatLobbyLogic(RealtimeSession session)
        {
            this.session = session;
        }

        public RealtimeSession Session {
            get { return session; }
            set { session = value; } 
        }

        public void SendMessage(string message)
        {
            session.LogicSend(message);
        }

        public void MessageRecieved(byte[] data)
        {
        }

        public void MessageStringRecieved(string data)
        {
        }

        public void MessageObjectRecieved(object data)
        {
            // this should be a map
            Dictionary<object, object> message = (Dictionary<object, object>)data;

            string cmd = (string)message["cmd"];
            if (cmd == "init")
            {
                popupateMessages(message);
            }
            else if (cmd == "msg")
            {
                Dictionary<object, object> msg = (Dictionary<object, object>)message["msg"];
                ChatMessage chatMessage = new ChatMessage((Dictionary<object, object>)msg);

                this.messages.Add(chatMessage);
                if (ChatMessageRecieved != null)
                {
                    ChatMessageRecieved(chatMessage);
                }
            }
        }

        protected void popupateMessages(Dictionary<object, object> message)
        {
            this.messages = new List<ChatMessage>();
            List<object> list = (List<object>)message["messages"];

            foreach(object m in list) {
                ChatMessage chatMessage = new ChatMessage((Dictionary<object, object>)m);

                this.messages.Add(chatMessage);
                if (ChatMessageRecieved != null)
                {
                    ChatMessageRecieved(chatMessage);
                }
            }
        }
    }
}

using System;

using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace AgoraGames.Hydra
{
    public interface IRealtimeLogic
    {
        RealtimeSession Session { get; set; }

        void MessageRecieved(byte [] data);
        void MessageStringRecieved(string data);
        void MessageObjectRecieved(object data);
    }

    public class RealtimeLogicFactory
    {
        public enum LogicType
        {
            ChatLobby,
            Match,
            Object
        };
        
        public static string GetLogicTypeString(LogicType type)
        {
            switch(type) {
                case LogicType.ChatLobby:
                    return "chat-lobby";
                case LogicType.Match:
                    return "match";
            }
            return null;
        }

        public static IRealtimeLogic Create(RealtimeSession session, LogicType type)
        {
            switch(type) {
                case LogicType.Match:
                    return new MatchLogic(session);
                case LogicType.ChatLobby:
                    return new ChatLobbyLogic(session);
            }
            return null;
        }
    }
}

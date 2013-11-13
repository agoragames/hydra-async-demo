using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.IO;

namespace AgoraGames.Hydra
{
    public class OutgoingSerializerRegistry : MessageSerializerRegistry<OutgoingMessage>
    {
        public OutgoingSerializerRegistry()
        {
            RegisterWriter(OutgoingMessage.Auth, new AuthSerializer());
            RegisterWriter(OutgoingMessage.Disconnect, new DisconnectSerializer());
            RegisterWriter(OutgoingMessage.SendTo, new SendToSerializer());
            RegisterWriter(OutgoingMessage.SendAll, new SendBaseSerializer());
            RegisterWriter(OutgoingMessage.SendOther, new SendBaseSerializer());
            RegisterWriter(OutgoingMessage.LogicSend, new SendBaseSerializer());
            RegisterWriter(OutgoingMessage.Join, new JoinSerializer());
            RegisterWriter(OutgoingMessage.Leave, new LeaveSerializer());
            RegisterWriter(OutgoingMessage.Time, new TimeRequestSerializer());
        }
    }

    public class IncomingSerializerRegistry : MessageSerializerRegistry<IncomingMessage>
    {
        public IncomingSerializerRegistry()
        {
            RegisterReader(IncomingMessage.Recieve, new RecieveSerializer());
            RegisterReader(IncomingMessage.RecieveLogic, new RecieveSerializer());
            RegisterReader(IncomingMessage.Joined, new JoinedSerializer());
            RegisterReader(IncomingMessage.Notification, new NotificationSerializer());
            RegisterReader(IncomingMessage.PlayerDisconnect, new PlayerSerializer());
            RegisterReader(IncomingMessage.PlayerJoin, new PlayerSerializer());
            RegisterReader(IncomingMessage.PlayerLeave, new PlayerSerializer());
            RegisterReader(IncomingMessage.PlayerReconnect, new PlayerSerializer());
            RegisterReader(IncomingMessage.Time, new TimeResponseSerializer());
        }
    }
}

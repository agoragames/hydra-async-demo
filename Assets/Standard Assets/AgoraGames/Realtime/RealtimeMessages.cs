using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AgoraGames.Hydra.IO;
using AgoraGames.Hydra.Util;
using AgoraGames.Hydra.Models;

namespace AgoraGames.Hydra
{
    // we could keep incoming and outgoing messages in the same enum
    //  i just want them separate just if we ever need to version a bunch
    public enum OutgoingMessage
    {
        Auth = 0,
        Disconnect = 1,
        SendAll = 2,
        SendTo = 3,
        SendOther = 4,
        LogicSend = 5,
        Join = 6,
        Leave = 7,
        Time = 8
    }

    public enum IncomingMessage
    {
        Recieve = 0,
        Joined = 1,
        PlayerJoin = 2,
        PlayerLeave = 3,
        PlayerDisconnect = 4,
        PlayerReconnect = 5,
        Notification = 6,
        Time = 7,
        RecieveLogic = 8
    };

    public enum RealtimeEvents
    {
        Connected = 0,
        Disconnected = 1
    };

    // Incoming
    public class RecieveSerializer : MessageReader<IncomingMessage>
    {
        public Message<IncomingMessage> Read(MessageSerializerRegistry<IncomingMessage> r, int type, Stream s)
        {
            uint sessionAlias = StreamUtils.ReadUInt32(s);
            int payloadType = s.ReadByte();
            byte[] payload = StreamUtils.ReadBinary16(s);
            MessageType t = (MessageType)Enum.ToObject(typeof(MessageType), payloadType);

            switch ((IncomingMessage)type)
            {
                case IncomingMessage.Recieve:
                    return new RecieveMessage(sessionAlias, t, payload);
                case IncomingMessage.RecieveLogic:
                    return new RecieveLogicMessage(sessionAlias, t, payload);
            }
            return null;
        }
    }

    public class NotificationSerializer : MessageReader<IncomingMessage>
    {
        public Message<IncomingMessage> Read(MessageSerializerRegistry<IncomingMessage> r, int type, Stream s)
        {
            byte[] payload = StreamUtils.ReadBinary16(s);
            Dictionary<object, object> data = (Dictionary<object, object>)BinaryPacker2.decode(payload);

            return new NotificationMessage(data);
        }
    }

    public class JoinedSerializer : MessageReader<IncomingMessage>
    {
        public Message<IncomingMessage> Read(MessageSerializerRegistry<IncomingMessage> r, int type, Stream s)
        {
            uint sessionAlias = StreamUtils.ReadUInt32(s);
            string sessionid = StreamUtils.ReadString16(s);
            bool success = StreamUtils.ReadByte(s) == 1 ? true : false;
            object data = BinaryPacker2.decode(s);

            return new JoinedSessionMessage(sessionid, sessionAlias, success, data);
        }
    }

    public class TimeResponseSerializer : MessageReader<IncomingMessage>
    {
        public Message<IncomingMessage> Read(MessageSerializerRegistry<IncomingMessage> r, int type, Stream s)
        {
            uint sessionAlias = StreamUtils.ReadUInt32(s);
            DateTime requestTime = HelperUtil.FromMilisecondsSinceEpoch(StreamUtils.ReadInt64(s));
            DateTime serverTime = HelperUtil.FromMilisecondsSinceEpoch(StreamUtils.ReadInt64(s));
            object data = BinaryPacker2.decode(s);

            return new TimeResponseMessage(sessionAlias, requestTime, serverTime, data);
        }
    }

    public class PlayerSerializer : MessageReader<IncomingMessage>
    {
        public Message<IncomingMessage> Read(MessageSerializerRegistry<IncomingMessage> r, int type, Stream s)
        {
            uint sessionAlias = StreamUtils.ReadUInt32(s);
            string playerId = StreamUtils.ReadHexBinaryFixed(s, 12);
            object data = BinaryPacker2.decode(s);

            switch ((IncomingMessage)type)
            {
                case IncomingMessage.PlayerJoin:
                    return new PlayerJoinedMessage(sessionAlias, playerId, data);
                case IncomingMessage.PlayerLeave:
                    return new PlayerLeftMessage(sessionAlias, playerId, data);
                case IncomingMessage.PlayerDisconnect:
                    return new PlayerDisconnectedMessage(sessionAlias, playerId, data);
                case IncomingMessage.PlayerReconnect:
                    return new PlayerReconnectedMessage(sessionAlias, playerId, data);
            }
            return null;
        }
    }

    public abstract class SessionMessage : Message<IncomingMessage>
    {
        public uint Alias;

        public SessionMessage(uint alias)
        {
            Alias = alias;
        }
    }

    public class JoinedSessionMessage : SessionMessage
    {
        public string SessionId;
        public bool Success;
        public object Data;

        public JoinedSessionMessage(string sessionId, uint alias, bool success, object data)
            : base(alias)
        {
            SessionId = sessionId;
            Success = success;
            Data = data;
        }

        public override IncomingMessage GetMessageType()
        {
            return IncomingMessage.Joined;
        }
    }

    public abstract class RecieveMessageBase : SessionMessage
    {
        public MessageType DataType;
        public byte[] Payload;

        public object ObjectPayload;
        public string StringPayload;

        // TODO: support source? ie player or logic

        public RecieveMessageBase(uint alias, MessageType type, byte[] payload)
            : base(alias)
        {
            DataType = type;
            Payload = payload;

            if (type == MessageType.Object)
            {
                ObjectPayload = BinaryPacker2.decode(payload);
            }
            else if (type == MessageType.String)
            {
                StringPayload = System.Text.Encoding.UTF8.GetString(payload);
            }
        }
    }

    public class RecieveMessage : RecieveMessageBase
    {
        public RecieveMessage(uint alias, MessageType type, byte[] payload)
            : base(alias, type, payload)
        {
        }

        public override IncomingMessage GetMessageType()
        {
            return IncomingMessage.Recieve;
        }
    }

    public class RecieveLogicMessage : RecieveMessageBase
    {
        public RecieveLogicMessage(uint alias, MessageType type, byte[] payload)
            : base(alias, type, payload)
        {
        }

        public override IncomingMessage GetMessageType()
        {
            return IncomingMessage.RecieveLogic;
        }
    }

    public class NotificationMessage : Message<IncomingMessage>
    {
        public Dictionary<object, object> Data;

        public NotificationMessage(Dictionary<object, object> data)
        {
            Data = data;
        }

        public override IncomingMessage GetMessageType()
        {
            return IncomingMessage.Notification;
        }
    }

    public abstract class PlayerMessage : SessionMessage
    {
        public string PlayerId;
        public object Data;

        public PlayerMessage(uint session, string playerId, object data)
            : base(session)
        {
            PlayerId = playerId;
            Data = data;
        }
    }

    public class PlayerJoinedMessage : PlayerMessage
    {
        public PlayerJoinedMessage(uint session, string player, object data)
            : base(session, player, data)
        {
        }

        public override IncomingMessage GetMessageType()
        {
            return IncomingMessage.PlayerJoin;
        }
    }

    public class PlayerLeftMessage : PlayerMessage
    {
        public PlayerLeftMessage(uint session, string player, object data)
            : base(session, player, data)
        {
        }

        public override IncomingMessage GetMessageType()
        {
            return IncomingMessage.PlayerLeave;
        }
    }

    public class PlayerDisconnectedMessage : PlayerMessage
    {
        public PlayerDisconnectedMessage(uint session, string player, object data)
            : base(session, player, data)
        {
        }

        public override IncomingMessage GetMessageType()
        {
            return IncomingMessage.PlayerDisconnect;
        }
    }

    public class PlayerReconnectedMessage : PlayerMessage
    {
        public PlayerReconnectedMessage(uint session, string player, object data)
            : base(session, player, data)
        {
        }

        public override IncomingMessage GetMessageType()
        {
            return IncomingMessage.PlayerReconnect;
        }
    }

    public class TimeResponseMessage : SessionMessage
    {
        public DateTime RequestTime;
        public DateTime ServerTime;
        public object Data;

        public TimeResponseMessage(uint session, DateTime requestTime, DateTime serverTime, object data)
            : base(session)
        {
            RequestTime = requestTime;
            ServerTime = serverTime;
            Data = data;
        }

        public override IncomingMessage GetMessageType()
        {
            return IncomingMessage.PlayerReconnect;
        }
    }

    // Outgoing
    public class AuthSerializer : MessageWriter<OutgoingMessage>
    {
        public void Write(MessageSerializerRegistry<OutgoingMessage> r, Stream s, Message<OutgoingMessage> m)
        {
            AuthMessage authMessage = (AuthMessage)m;

            StreamUtils.WriteHexBinary16(s, authMessage.ApiKey);
            StreamUtils.WriteString16(s, authMessage.Token);
            StreamUtils.WriteHexBinaryFixed(s, authMessage.AccountId, 12);

            // send additional info, for now just the identity
            Dictionary<object, object> authData = new Dictionary<object,object>();
            Dictionary<object, object> identityData = new Dictionary<object,object>();
            
            identityData.Add("username", authMessage.Identity.UserName);
            identityData.Add("avatar", authMessage.Identity.Avatar);
            authData.Add("identity", identityData);
            authData.Add("startup", authMessage.Startup);

            StreamUtils.WriteBytes(s, BinaryPacker2.encode(authData));
        }
    }

    public class DisconnectSerializer : MessageWriter<OutgoingMessage>
    {
        public void Write(MessageSerializerRegistry<OutgoingMessage> r, Stream s, Message<OutgoingMessage> m)
        {
        }
    }

    public class SendBaseSerializer : MessageWriter<OutgoingMessage>
    {
        public void Write(MessageSerializerRegistry<OutgoingMessage> r, Stream s, Message<OutgoingMessage> m)
        {
            SessionSendMessage message = (SessionSendMessage)m;

            StreamUtils.WriteUInt32(s, message.Alias);
            // we are currently using this whole byte for payload type, but we should be able to use the rest of the bits
            //  for other flags in the future
            s.WriteByte((byte)Convert.ToInt16(message.PayloadType));
            StreamUtils.WriteBinary16(s, message.Payload);
        }
    }

    public class SendToSerializer : MessageWriter<OutgoingMessage>
    {
        public void Write(MessageSerializerRegistry<OutgoingMessage> r, Stream s, Message<OutgoingMessage> m)
        {
            SendToMessage message = (SendToMessage)m;

            StreamUtils.WriteUInt32(s, message.Alias);
            StreamUtils.WriteHexBinaryFixed(s, message.Dest, 12);
            // we are currently using this whole byte for payload type, but we should be able to use the rest of the bits
            //  for other flags in the future
            s.WriteByte((byte)Convert.ToInt16(message.PayloadType));
            StreamUtils.WriteBinary16(s, message.Payload);
        }
    }

    public class JoinSerializer : MessageWriter<OutgoingMessage>
    {
        public void Write(MessageSerializerRegistry<OutgoingMessage> r, Stream s, Message<OutgoingMessage> m)
        {
            JoinMessage message = (JoinMessage)m;

            StreamUtils.WriteString8(s, message.Type);
            StreamUtils.WriteString16(s, message.Id);

            StreamUtils.WriteBytes(s, BinaryPacker2.encode(message.Data));
        }
    }

    public class LeaveSerializer : MessageWriter<OutgoingMessage>
    {
        public void Write(MessageSerializerRegistry<OutgoingMessage> r, Stream s, Message<OutgoingMessage> m)
        {
            LeaveMessage message = (LeaveMessage)m;

            StreamUtils.WriteUInt32(s, message.Alias);
        }
    }

    public class TimeRequestSerializer : MessageWriter<OutgoingMessage>
    {
        public void Write(MessageSerializerRegistry<OutgoingMessage> r, Stream s, Message<OutgoingMessage> m)
        {
            TimeRequestMessage message = (TimeRequestMessage)m;

            StreamUtils.WriteUInt32(s, message.Alias);
            StreamUtils.WriteInt64(s, HelperUtil.GetMilisecondsSinceEpoch(message.RequestTime));
            StreamUtils.WriteBytes(s, BinaryPacker2.encode(message.Data));
        }
    }

    public class AuthMessage : Message<OutgoingMessage>
    {
        public string Token;
        public string ApiKey;
        public string AccountId;
        public bool Startup;

        public Identity Identity;

        public AuthMessage(string token, string apiKey, string accountId, Identity identity, bool startup) 
        {
            Token = token;
            ApiKey = apiKey;
            AccountId = accountId;
            Identity = identity;
            Startup = startup;
        }

        public override OutgoingMessage GetMessageType()
        {
            return OutgoingMessage.Auth;
        }
    }

    public abstract class SessionOutgoingMessage : Message<OutgoingMessage>
    {
        public uint Alias;

        public SessionOutgoingMessage(uint alias) 
        {
            Alias = alias;
        }
    }

    public abstract class SessionSendMessage : SessionOutgoingMessage
    {
        public MessageType PayloadType;
        public byte[] Payload;

        public SessionSendMessage(uint alias, MessageType type, byte[] payload) : base(alias)
        {
            PayloadType = type;
            Payload = payload;
        }
    }

    public class SendAllMessage : SessionSendMessage
    {
        public SendAllMessage(uint alias, MessageType type, byte[] payload)
            : base(alias, type, payload)
        {
        }

        public override OutgoingMessage GetMessageType()
        {
            return OutgoingMessage.SendAll;
        }
    }

    public class SendOtherMessage : SessionSendMessage
    {
        public SendOtherMessage(uint alias, MessageType type, byte[] payload)
            : base(alias, type, payload)
        {
        }

        public override OutgoingMessage GetMessageType()
        {
            return OutgoingMessage.SendOther;
        }
    }

    public class LogicSendMessage : SessionSendMessage
    {
        public LogicSendMessage(uint alias, MessageType type, byte[] payload)
            : base(alias, type, payload)
        {
        }

        public override OutgoingMessage GetMessageType()
        {
            return OutgoingMessage.LogicSend;
        }
    }

    public class SendToMessage : SessionSendMessage
    {
        public string Dest;

        public SendToMessage(uint alias, string dest, MessageType type, byte[] payload)
            : base(alias, type, payload)
        {
            this.Dest = dest;
        }

        public override OutgoingMessage GetMessageType()
        {
            return OutgoingMessage.SendTo;
        }
    }

    public class JoinMessage : Message<OutgoingMessage>
    {
        public string Type;
        public string Id;
        public object Data;

        public JoinMessage(string type, string id, object data)
        {
            this.Type = type;
            this.Id = id;
            this.Data = data;
        }

        public override OutgoingMessage GetMessageType()
        {
            return OutgoingMessage.Join;
        }
    }

    public class LeaveMessage : SessionOutgoingMessage
    {
        public LeaveMessage(uint alias) : base(alias)
        {
        }

        public override OutgoingMessage GetMessageType()
        {
            return OutgoingMessage.Leave;
        }
    }

    public class DisconnectMessage : Message<OutgoingMessage>
    {
        public DisconnectMessage()
        {
        }

        public override OutgoingMessage GetMessageType()
        {
            return OutgoingMessage.Disconnect;
        }
    }

    public class TimeRequestMessage : SessionOutgoingMessage
    {
        public DateTime RequestTime;
        public object Data;

        public TimeRequestMessage(uint alias, DateTime requestTime, object data)
            : base(alias)
        {
            RequestTime = requestTime;
            Data = data;
        }

        public override OutgoingMessage GetMessageType()
        {
            return OutgoingMessage.Time;
        }
    }
}

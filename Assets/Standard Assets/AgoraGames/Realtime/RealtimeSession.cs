using System;
using System.Collections.Generic;
using AgoraGames.Hydra.Util;
using System.Collections;
using AgoraGames.Hydra.Models;

namespace AgoraGames.Hydra
{
    public class RealtimePlayer
    {
        public enum PlayerState {
            Connected,
            Disconnected,
        };

        protected string id;
        public string Id { get { return id; } }

        protected Identity identity;
        public Identity Identity { get { return identity; } }

        public PlayerState State;

        public RealtimePlayer(string id, Dictionary<object, object> data)
        {
            this.id = id;

            MapHelper mapHelper = new MapHelper(data);
            identity = new Identity(mapHelper.GetValue("identity", new Dictionary<object, object>()));
            State = PlayerState.Disconnected;
        }

        public RealtimePlayer(Dictionary<object, object> data)
        {
            MapHelper mapHelper = new MapHelper(data);
            id = mapHelper.GetValue("id", (string)null);
            identity = new Identity(mapHelper.GetValue("identity", new Dictionary<object, object>()));
            State = getStateFromString(mapHelper.GetValue("state", (string)null));
        }

        protected static PlayerState getStateFromString(string str)
        {
            switch (str)
            {
                case "disconnected":
                    return PlayerState.Disconnected;
                case "connected":
                    return PlayerState.Connected;
            }
            return PlayerState.Disconnected;
        }
    };

	public class RealtimeSession
	{
        protected string sessionId;
        protected Client client;

        public IRealtimeLogic Logic
        {
            get;
            set;
        }

        protected uint alias;
        public bool IsJoined {protected set; get; }
        protected bool joining;

        // server time
        protected long serverTimeOffset = 0;
        public DateTime ServerTime
        {
            get
            {
                return DateTime.UtcNow.AddTicks(serverTimeOffset);
            }
        }
        public long LastLatency = 0;

        // player list
        protected Dictionary<string, RealtimePlayer> players = new Dictionary<string, RealtimePlayer>();
        public Dictionary<string, RealtimePlayer> Players
        {
            get { return players; }
        }

        public delegate void PlayerHandler(RealtimePlayer player);
        public event PlayerHandler PlayerConnected;
        public event PlayerHandler PlayerDisconnected;
        public event PlayerHandler PlayerLeft;
        public event PlayerHandler PlayerReconnected;

        public delegate void MessageHandler(byte[] data);
        public delegate void MessageStringHandler(string data);
        public delegate void MessageObjectHandler(object data);

        public event MessageHandler MessageRecieved;
        public event MessageStringHandler MessageStringRecieved;
        public event MessageObjectHandler MessageObjectRecieved;

        public delegate void JoinHandler(bool success);
        public event JoinHandler Joined;

        public RealtimeSession(Client client, string sessionId)
        {
            this.client = client;
            this.sessionId = sessionId;
        }

        public bool RequestServerTime(object data)
        {
            if (IsJoined)
            {
                TimeRequestMessage m = new TimeRequestMessage(alias, HiResDateTime.UtcNow, data);

                client.Message.Send(Realtime.OutgoingRegistry.ToBytes(m));
                return true;
            }
            return false;
        }

        public bool SendTo(string playerid, object data)
        {
            return InternalSendTo(playerid, MessageType.Object, BinaryPacker2.encode(data));
        }

        public bool SendTo(string playerid, string data)
        {
            return InternalSendTo(playerid, MessageType.String, System.Text.Encoding.UTF8.GetBytes(data));
        }

        public bool SendTo(string playerid, byte[] data)
        {
            return InternalSendTo(playerid, MessageType.Byte, data);
        }

        protected bool InternalSendTo(string playerid, MessageType type, byte[] data)
        {
            if (IsJoined)
            {
                SendToMessage m = new SendToMessage(alias, playerid, type, data);

                client.Message.Send(Realtime.OutgoingRegistry.ToBytes(m));
                return true;
            }
            return false;
        }

        public bool SendAll(object message)
        {
            return InternalSendAll(MessageType.Object, BinaryPacker2.encode(message));
        }

        public bool SendAll(string data)
        {
            return InternalSendAll(MessageType.String, System.Text.Encoding.UTF8.GetBytes(data));
        }

        public bool SendAll(byte[] data)
        {
            return InternalSendAll(MessageType.Byte, data);
        }

        protected bool InternalSendAll(MessageType type, byte[] data) 
        {
            if (IsJoined)
            {
                SendAllMessage m = new SendAllMessage(alias, type, data);

                client.Message.Send(Realtime.OutgoingRegistry.ToBytes(m));
                return true;
            }
            return false;
        }

        public bool SendOther(object message)
        {
            return InternalSendOther(MessageType.Object, BinaryPacker2.encode(message));
        }

        public bool SendOther(string data)
        {
            return InternalSendOther(MessageType.String, System.Text.Encoding.UTF8.GetBytes(data));
        }

        public bool SendOther(byte[] data)
        {
            return InternalSendOther(MessageType.Byte, data);
        }

        protected bool InternalSendOther(MessageType type, byte[] data)
        {
            if (IsJoined)
            {
                SendOtherMessage m = new SendOtherMessage(alias, type, data);

                client.Message.Send(Realtime.OutgoingRegistry.ToBytes(m));
                return true;
            }
            return false;
        }

        public bool LogicSend(object message)
        {
            return InternalLogicSend(MessageType.Object, BinaryPacker2.encode(message));
        }

        public bool LogicSend(string data)
        {
            return InternalLogicSend(MessageType.Byte, System.Text.Encoding.UTF8.GetBytes(data));
        }

        public bool LogicSend(byte[] data)
        {
            return InternalLogicSend(MessageType.Byte, data);
        }

        protected bool InternalLogicSend(MessageType type, byte[] data)
        {
            if (IsJoined)
            {
                LogicSendMessage m = new LogicSendMessage(alias, type, data);

                client.Message.Send(Realtime.OutgoingRegistry.ToBytes(m));
                return true;
            }
            return false;
        }

        public IRealtimeLogic JoinSession(RealtimeLogicFactory.LogicType type, object data)
        {
            if (!joining)
            {
                JoinMessage m = new JoinMessage(RealtimeLogicFactory.GetLogicTypeString(type), sessionId, data);

                client.Message.Send(Realtime.OutgoingRegistry.ToBytes(m));

                Logic = RealtimeLogicFactory.Create(this, type);
                joining = true;
            }
            return Logic;
        }

        public void LeaveSession()
        {
            if (IsJoined)
            {
                LeaveMessage m = new LeaveMessage(alias);

                client.Message.Send(Realtime.OutgoingRegistry.ToBytes(m));
            }
        }

        protected void handleJoinMessage(JoinedSessionMessage joinedMessage)
        {
            joining = false;
            IsJoined = joinedMessage.Success;
            alias = joinedMessage.Alias;
            players.Clear();

            // extra metadata 
            if (joinedMessage.Data != null)
            {
                // inital player list
                MapHelper mapHelper = new MapHelper((Dictionary<object, object>)joinedMessage.Data);
                resolveSesssionPlayers(mapHelper.GetValue("players", (List<object>)null));
            }

            if (Joined != null)
            {
                Joined(IsJoined);
            }
        }

        protected void handleTimeResponseMessage(TimeResponseMessage timeMessage)
        {
            long currentTicks = HiResDateTime.UtcNow.Ticks;
            long latency = currentTicks - timeMessage.RequestTime.Ticks;
            long oneWayLatency = latency / 2;

            LastLatency = latency / TimeSpan.TicksPerMillisecond;
            serverTimeOffset = timeMessage.ServerTime.Ticks - (currentTicks + oneWayLatency);
        }

        protected RealtimePlayer getRealtimePlayer(PlayerMessage data)
        {
            return new RealtimePlayer(data.PlayerId, (Dictionary<object, object>)data.Data);
        }

        protected void addPlayer(RealtimePlayer player)
        {
            players[player.Id] = player;
        }

        protected void removePlayer(RealtimePlayer player)
        {
            players.Remove(player.Id);
        }

        protected void disconnectedPlayer(RealtimePlayer player)
        {
            RealtimePlayer current;

            if (players.TryGetValue(player.Id, out current))
            {
                current.State = RealtimePlayer.PlayerState.Disconnected;
            }
            else
            {
                player.State = RealtimePlayer.PlayerState.Disconnected;
                addPlayer(player);
            }
        }

        protected void reconnectedPlayer(RealtimePlayer player)
        {
            RealtimePlayer current;

            if (players.TryGetValue(player.Id, out current))
            {
                current.State = RealtimePlayer.PlayerState.Connected;
            }
            else
            {
                player.State = RealtimePlayer.PlayerState.Connected;
                addPlayer(player);
            }
        }

        protected void resolveSesssionPlayers(List<object> data)
        {
            if (data != null)
            {
                foreach (object iter in data)
                {
                    Dictionary<object, object> playerData = (Dictionary<object, object>)iter;
                    RealtimePlayer player = new RealtimePlayer(playerData);
                    addPlayer(player);
                }
            }
        }

        public void Dispatch(SessionMessage message)
        {
            if (message is JoinedSessionMessage)
            {
                JoinedSessionMessage joinedMessage = (JoinedSessionMessage)message;

                handleJoinMessage(joinedMessage);
            }
            else if (message is TimeResponseMessage)
            {
                TimeResponseMessage timeResponse = (TimeResponseMessage)message;

                handleTimeResponseMessage(timeResponse);
            }
            else if (message is RecieveLogicMessage)
            {
                if (Logic != null)
                {
                    RecieveLogicMessage recievedMessage = (RecieveLogicMessage)message;

                    if (recievedMessage.DataType == MessageType.Byte)
                    {
                        Logic.MessageRecieved(recievedMessage.Payload);
                    }
                    else if (recievedMessage.DataType == MessageType.String)
                    {
                        Logic.MessageStringRecieved(recievedMessage.StringPayload);
                    }
                    else if (recievedMessage.DataType == MessageType.Object)
                    {
                        Logic.MessageObjectRecieved(recievedMessage.ObjectPayload);
                    }
                }
            }
            else if (message is RecieveMessage)
            {
                RecieveMessage recievedMessage = (RecieveMessage)message;

                if (MessageRecieved != null && recievedMessage.DataType == MessageType.Byte)
                {
                    MessageRecieved(recievedMessage.Payload);
                }
                else if (MessageStringRecieved != null && recievedMessage.DataType == MessageType.String)
                {
                    MessageStringRecieved(recievedMessage.StringPayload);
                }
                else if (MessageObjectRecieved != null && recievedMessage.DataType == MessageType.Object)
                {
                    MessageObjectRecieved(recievedMessage.ObjectPayload);
                }
            }
            else if (message is PlayerMessage)
            {
                PlayerMessage playerMessage = (PlayerMessage)message;
                RealtimePlayer player = getRealtimePlayer(playerMessage);

                if (message is PlayerJoinedMessage)
                {
                    addPlayer(player);
                    if (PlayerConnected != null)
                    {
                        PlayerConnected(player);
                    }
                }
                else if (message is PlayerLeftMessage)
                {
                    removePlayer(player);
                    if (PlayerLeft != null)
                    {
                        PlayerLeft(player);
                    }
                }
                else if (message is PlayerDisconnectedMessage)
                {
                    disconnectedPlayer(player);
                    if (PlayerDisconnected != null)
                    {
                        PlayerDisconnected(player);
                    }
                }
                else if (message is PlayerReconnectedMessage)
                {
                    reconnectedPlayer(player);
                    if (PlayerReconnected != null)
                    {
                        PlayerReconnected(player);
                    }
                }
            }
        }
    }
}


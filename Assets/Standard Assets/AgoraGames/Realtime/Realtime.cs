#define _WEBSOCKETSHARP_

using System;

using System.Collections.Generic;
using System.Threading;

using AgoraGames.Hydra.IO;

#if _WEBSOCKETSHARP_
using WebSocketSharp;
#endif

namespace AgoraGames.Hydra
{
    public enum MessageType
    {
        Byte = 0,
        Object = 1,
        String = 2
    };

    public class Realtime
    {
        public event System.Action Connected;
        public event System.Action Disconnected;

#if _WEBSOCKETSHARP_
        protected WebSocketSharp.WebSocket ws;
#endif
        protected Client client = null;

        protected static OutgoingSerializerRegistry outgoingRegistry = new OutgoingSerializerRegistry();
        public static OutgoingSerializerRegistry OutgoingRegistry { get { return outgoingRegistry; } }

        protected static IncomingSerializerRegistry incomingRegistry = new IncomingSerializerRegistry();
        public static IncomingSerializerRegistry IncomingRegistry { get { return incomingRegistry; } }

        protected Dictionary<string, RealtimeSession> sessions = new Dictionary<string, RealtimeSession>();
        protected Dictionary<uint, RealtimeSession> sessionsAlias = new Dictionary<uint, RealtimeSession>();

        protected bool autoConnect = false;
        protected bool isConnected = false;
        protected bool first = true;

        protected Thread connectThread;
        protected ManualResetEvent connectEvent;

        protected List<object> messages = new List<object>();
        protected object clientLock = new object();

        protected string endpoint;

        public bool IsConnected
        {
            get { return isConnected; }
        }

        public Realtime (Client client)
        {
            this.client = client;
        }

        public void Startup()
        {
#if _WEBSOCKETSHARP_
            connectEvent = new ManualResetEvent(false);
            connectThread = new Thread(StaticRun);
            connectThread.Start(this);
#endif
        }

        public void Connect(string endpoint) 
        {
#if _WEBSOCKETSHARP_
            queueThreadMessage(new ConnectEvent(endpoint));
#endif
        }

        public void Disconnect()
        {
#if _WEBSOCKETSHARP_
            SendDisconnect();
            queueThreadMessage(new DisconnectEvent());
#endif
        }
        
        public void Shutdown()
        {
#if _WEBSOCKETSHARP_
            queueThreadMessage(new ShutdownEvent());

            if (connectThread != null && !connectThread.Join(1000))
            {
                connectThread.Abort();
            }
#endif
        }

        public void Ping()
        {
#if _WEBSOCKETSHARP_
            ws.Ping();
#endif
        }

        public RealtimeSession JoinSession(RealtimeLogicFactory.LogicType type, string id, object data)
        {
#if _WEBSOCKETSHARP_
            RealtimeSession session = new RealtimeSession(client, id);

            session.JoinSession(type, data);
            sessions[id] = session;
            return session;
#else
            return null;
#endif
        }

        protected void SendIdentity()
        {
            AuthMessage m = new AuthMessage(client.AccessToken, client.ApiKey, client.MyAccount.Id, client.MyAccount.Identity, first);

            Send(OutgoingRegistry.ToBytes(m));
            first = false;
        }

        protected void SendDisconnect()
        {
            DisconnectMessage m = new DisconnectMessage();

            Send(OutgoingRegistry.ToBytes(m));
        }

        public void Send(byte [] data, bool reliable = true)
        {
#if _WEBSOCKETSHARP_
            if (ws != null && isConnected)
            {
                ws.Send(data);
            }
            else
            {
                client.Logger.Warn("Trying to send a message when websocket connection is not up.");
            }
#endif
        }

        protected void queueThreadMessage(object msg)
        {
            lock (clientLock)
            {
                if (connectThread != null && connectEvent != null)
                {
                    messages.Add(msg);
                    connectEvent.Set();
                }
            }
        }

        protected void Run()
        {
            bool shutdown = false;
            string currentEndpoint = null;

            while (!shutdown)
            {
                try
                {
                    object currentMessage = null;

                    do
                    {
                        currentMessage = null;

                        lock (clientLock)
                        {
                            if (messages.Count > 0)
                            {
                                currentMessage = messages[0];
                                messages.RemoveAt(0);
                            }
                        }

                        if (currentMessage != null)
                        {
                            if (currentMessage is ConnectEvent)
                            {
                                currentEndpoint = ((ConnectEvent)currentMessage).endpoint;

                                TryConnect(currentEndpoint, -1);
                            }
                            else if (currentMessage is DisconnectEvent)
                            {
                                CloseConnection();
                            }
                            else if (currentMessage is ShutdownEvent)
                            {
                                client.Logger.Info("got shutdown event");
                                CloseConnection();
                                shutdown = true;
                            }
                            else if (currentMessage is ConnectedEvent)
                            {
                                client.Logger.Info("connected to realtime");
                                isConnected = true;

                                SendIdentity();
                            }
                            else if (currentMessage is DisconnectedEvent)
                            {
                                client.Logger.Warn("disconnected from realtime");
                                isConnected = false;

                                TryConnect(currentEndpoint, 5000);
                            }
                            else if (currentMessage is ErrorEvent)
                            {
                                client.Logger.Warn("error from realtime");
                                // TODO: should we disconnect if we have an error??
                                //isConnected = false;
                                //TryConnect(currentEndpoint, 5000);
                            }
                        }
                    } while (currentMessage != null && !shutdown);

                    if (!shutdown)
                    {
                        wait(-1);
                    }
                }
                catch (Exception ex)
                {
                    client.Logger.Error("error in connection thread" + ex.ToString());
                }
            }
            client.Logger.Info("exit thread");
        }

        protected void wait(int delay)
        {
            lock (clientLock)
            {
                connectEvent.Reset();
            }
            connectEvent.WaitOne(delay);
        }

        protected void TryConnect(string endpoint, int delay)
        {
            client.Logger.Info("trying to connect... " + endpoint);

            // we should be safe just blocking this thread, if someone
            //  passes a delay in its in response to a disconnect/error message
            //  and we can just not process until this delay is done.
            if (delay != -1)
            {
                wait(delay);
            }

#if _WEBSOCKETSHARP_
            ws = new WebSocketSharp.WebSocket(endpoint);

            ws.OnOpen += OnOpen;
            ws.OnClose += OnClose;
            ws.OnError += OnError;
            ws.OnMessage += OnMessage;

            ws.Connect();
#endif
        }

        protected static void StaticRun(object instance) 
        {
            Realtime message = (Realtime)instance;

            message.Run();
        }

        protected void CloseConnection()
        {
            isConnected = false;

#if _WEBSOCKETSHARP_
            if (ws != null)
            {
                ws.OnError -= OnError;
                ws.OnOpen -= OnOpen;
                ws.OnClose -= OnClose;
                ws.OnMessage -= OnMessage;
                ws.Close();
                ws = null;
            }
#endif
        }

        public void ProcessMessage(Message<IncomingMessage> message)
        {
            if (message is NotificationMessage)
            {
                NotificationMessage notificationMessage = (NotificationMessage)message;

                client.Notification.Dispatch(notificationMessage.Data);
            }
            else if (message is SessionMessage)
            {
                SessionMessage sessionMessage = (SessionMessage)message;
                RealtimeSession session;

                if (sessionMessage is JoinedSessionMessage)
                {
                    JoinedSessionMessage joinedMessage = (JoinedSessionMessage)sessionMessage;

                    sessions.TryGetValue(joinedMessage.SessionId, out session);
                    sessionsAlias[joinedMessage.Alias] = session;
                }
                else
                {
                    sessionsAlias.TryGetValue(sessionMessage.Alias, out session);
                }

                if (session != null)
                {
                    session.Dispatch(sessionMessage);
                }
                else
                {
                    // TODO: error
                    client.Logger.Warn("unknown session");
                }
            }
        }

        public void ProcessEvent(Message<RealtimeEvents> message)
        {
            RealtimeEvents eventType = message.GetMessageType();
            if (eventType == RealtimeEvents.Connected)
            {
                if (Connected != null)
                    Connected();
            }
            else if (eventType == RealtimeEvents.Disconnected)
            {
                if (Disconnected != null)
                    Disconnected();
            }
        }

        protected void ProcessData(byte[] data)
        {
            try
            {
                // TODO: for now we are going to dispatch all messages to the main thread
                //  we will want to offer developers a way to handle messages inside of the worker
                //  thread so they can process messages with less latency
                Message<IncomingMessage> message = IncomingRegistry.FromBytes(data);

                client.Dispatcher.Add(message);
            }
            catch (Exception ex)
            {
                client.Logger.Warn(ex.ToString());
            }
        }

#if _WEBSOCKETSHARP_
        void OnClose(object sender, CloseEventArgs e)
        {
            DisconnectedEvent ev = new DisconnectedEvent();
            queueThreadMessage(ev);

            if (isConnected)
            {
                client.EventDispatcher.Add(ev);
            }
        }

        void OnOpen(object sender, EventArgs e)
        {
            ConnectedEvent ev = new ConnectedEvent();
            queueThreadMessage(ev);
            client.EventDispatcher.Add(ev);
        }

        void OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Type == Opcode.BINARY)
            {
                ProcessData(e.RawData);
            }
            else if (e.Type == Opcode.TEXT)
            {
            }
        }

        void OnError(object sender, ErrorEventArgs e)
        {
            queueThreadMessage(new ErrorEvent());
        }
#endif

        protected bool IsSessionMessage(String cmd)
        {
            return cmd == "send" || cmd == "session-disc" || cmd == "session-con"
                    || cmd == "session-leave" || cmd == "session-reconnect";
        }

        class ConnectEvent
        {
            public string endpoint;

            public ConnectEvent(string endpoint)
            {
                this.endpoint = endpoint;
            }
        }

        class DisconnectEvent
        {
            public DisconnectEvent()
            {
            }
        }

        class ShutdownEvent
        {
            public ShutdownEvent()
            {
            }
        }

        class ErrorEvent
        {
            public ErrorEvent()
            {
            }
        }

        class ConnectedEvent : Message<RealtimeEvents>
        {
            public ConnectedEvent()
            {
            }

            public override RealtimeEvents GetMessageType()
            {
                return RealtimeEvents.Connected;
            }
        }

        class DisconnectedEvent : Message<RealtimeEvents>
        {
            public DisconnectedEvent()
            {
            }

            public override RealtimeEvents GetMessageType()
            {
                return RealtimeEvents.Disconnected;
            }
        }
    }
}

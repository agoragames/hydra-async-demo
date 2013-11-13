using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Models;
using UnityEngine;
using AgoraGames.Hydra.Util;

namespace AgoraGames.Hydra.Test
{
    public class MatchMenu : BaseMenu
    {
        protected Match match;

        protected string text;
        protected bool inMatch;
        protected bool isOwner;
        protected bool isInvited;

        // realtime
        protected int totalMessages = 0;
        protected string lastMessage = "";
        protected string playerText = "";
        protected string realtimeMatchData = "";

        public MatchMenu(Main main)
            : base(main)
        {
        }

        public override void Deactivate()
        {
            if (this.match != null)
            {
                this.match.Updated -= match_Updated;
                this.match.Completed -= match_Completed;
                this.match.ExpirationWarning -= match_ExpirationWarning;

                if (match.RealtimeSession != null)
                {
                    this.match.RealtimeSession.MessageStringRecieved -= RealtimeSession_MessageStringRecieved;
                    this.match.RealtimeSession.Joined -= RealtimeSession_Joined;
                    this.match.RealtimeSession.PlayerConnected -= RealtimeSession_Player;
                    this.match.RealtimeSession.PlayerDisconnected -= RealtimeSession_Player;
                    this.match.RealtimeSession.PlayerLeft -= RealtimeSession_Player;
                    this.match.RealtimeSession.PlayerReconnected -= RealtimeSession_Player;
                    this.match.RealtimeSession.LeaveSession();
                }
            }
        }

        public override void Render()
        {
            int y = 0;
            TestUtil.RenderHeader(this, "Match");

            GUI.TextArea(new Rect(0, y += 60, 360, 30), match.Name);

            if (isInvited)
            {
                RenderInvitedMatch(ref y);
            }
            else if (inMatch)
            {
                RenderInMatch(ref y);
            }
            else
            {
                RenderOutMatch(ref y);
            }
            GUI.TextArea(new Rect(0, y += 70, 360, 300), this.text);
        }

        protected void RenderInvitedMatch(ref int y)
        {
            int x = 0;

            if (GUI.Button(new Rect(x, y += 40, 70, 60), "Leave"))
            {
                match.Leave(delegate(Request request)
                {
                    // TODO: handle error
                    Main.Back();
                });
            }

            if (GUI.Button(new Rect(x += 80, y, 70, 60), "Join"))
            {
                match.Join(delegate(Request request)
                {
                    UpdateMatch();
                });
            }
        }

        protected void RenderInMatch(ref int y)
        {
            int x = 0;

            // realtime...
            int realtimeColumn = 400;
            int realtimeY = y;

            if (GUI.Button(new Rect(x + realtimeColumn, realtimeY += 35, 70, 20), "counter"))
            {
                Commands commands = new Commands();

                commands.IncValue("player.position", 1);
                commands.SetValue("player.state", "dead");
                commands.SetValue("player.time", DateTime.Now);
                commands.SetValue("player.asdf", HelperUtil.EPOCH);
                
                //commands.PushValue("player.events", 123);

                ((MatchLogic)match.RealtimeSession.Logic).Update(commands);
            }
            
            GUI.TextArea(new Rect(x + realtimeColumn, realtimeY += 35, 250, 20), lastMessage);

            if (GUI.Button(new Rect(x + realtimeColumn + 260, realtimeY, 90, 20), "Send Messages"))
            {
                for (int i = 0; i < 10; i++)
                {
                    match.RealtimeSession.SendAll("hi " + totalMessages);
                    totalMessages++;
                }
            }

            GUI.TextArea(new Rect(x + realtimeColumn, realtimeY += 35, 200, 20), match.RealtimeSession.ServerTime.ToString());
            GUI.TextArea(new Rect(x + realtimeColumn + 210, realtimeY, 40, 20), match.RealtimeSession.LastLatency.ToString());

            if (GUI.Button(new Rect(x + realtimeColumn + 260, realtimeY, 90, 20), "Request Time"))
            {
                match.RealtimeSession.RequestServerTime(null);
            }

            GUI.TextArea(new Rect(x + realtimeColumn, realtimeY += 30, 360, 150), this.realtimeMatchData);
            GUI.TextArea(new Rect(x + realtimeColumn, realtimeY += 150, 360, 150), this.playerText);

            // rest....
            if (GUI.Button(new Rect(x, y += 35, 60, 60), "Kills"))
            {
                UpdateStat("data.kills", MatchNotification.ToEveryoneButMe("kills-update"));
            }

            if (GUI.Button(new Rect(x += 65, y, 60, 60), "Deaths"))
            {
                UpdateStat("data.deaths", MatchNotification.None);
            }

            if (GUI.Button(new Rect(x += 65, y, 60, 60), "Leave"))
            {
                match.Leave(delegate(Request request)
                {
                    // TODO: handle error
                    Main.Back();
                });
            }

            if (!match.IsComplete)
            {
                if (GUI.Button(new Rect(x += 65, y, 60, 60), "Win"))
                {
                    List<MatchPlayer> winningTeam = new List<MatchPlayer>();
                    List<MatchPlayer> losingTeam = new List<MatchPlayer>();
                    foreach (var player in match.Players)
                    {
                        if (player.Id == Client.Instance.MyAccount.Id)
                            winningTeam.Add(player);
                        else
                            losingTeam.Add(player);
                    }

                    match.Complete(new MatchResult(winningTeam, losingTeam, false), delegate(Request request)
                    {
                    });
                }
            }

            if (isOwner && !match.IsComplete)
            {
                if (GUI.Button(new Rect(x += 65, y, 60, 60), "Invite"))
                {
                    Main.PopupDialog("Username", delegate(string text)
                    {
                        match.InviteByUsername(text, delegate(Request requet)
                        {
                            UpdateMatch();
                        });
                    });
                }
            }
        }

        protected void RenderOutMatch(ref int y)
        {
            int x = 0;

            if (GUI.Button(new Rect(x, y += 60, 70, 60), "Join"))
            {
                match.Join(delegate(Request request)
                {
                    UpdateMatch();
                });
            }
        }

        protected void UpdateStat(string stat, MatchNotification notification)
        {
            Commands commands = new Commands();
            commands.IncValue(stat, 1);

            List<MatchPlayer> currentWinners = new List<MatchPlayer>();
            currentWinners.Add(match.GetPlayer(Client.Instance.MyAccount.Id));

            match.Update(commands, null, currentWinners, notification, delegate(Request req)
            {
                // TODO: handle error
                UpdateMatch();
            });
        }

        protected void UpdateMatch()
        {
            text = MiniJSON.Json.Serialize(match.Data, true);
            inMatch = this.match.InMatch(Client.Instance.MyAccount.Id);
            if (inMatch)
            {
                isInvited = match.GetPlayer(Client.Instance.MyAccount.Id).State.Equals("invite");
            }
            isOwner = Client.Instance.MyAccount.Id.Equals(match["account_id"]);
        }

        protected void updatePlayerText()
        {
            this.playerText = "";
            foreach(RealtimePlayer player in match.RealtimeSession.Players.Values) 
            {
                this.playerText += player.Identity.UserName + "\n";
            }
        }

        public override void SetParam(object param)
        {
            Match match = param as Match;

            this.match = match;
            this.playerText = "";
            this.realtimeMatchData = "";

            match.JoinSession();
            match.RealtimeSession.MessageStringRecieved += RealtimeSession_MessageStringRecieved;
            ((MatchLogic)match.RealtimeSession.Logic).MatchUpdated += Logic_MatchUpdated;

            match.RealtimeSession.Joined += RealtimeSession_Joined;
            match.RealtimeSession.PlayerConnected += RealtimeSession_Player;
            match.RealtimeSession.PlayerDisconnected += RealtimeSession_Player;
            match.RealtimeSession.PlayerLeft += RealtimeSession_Player;
            match.RealtimeSession.PlayerReconnected += RealtimeSession_Player;
            match.Updated += match_Updated;
            match.Completed += match_Completed;
            match.ExpirationWarning += match_ExpirationWarning;

            UpdateMatch();
        }

        void Logic_MatchUpdated(MatchLogic data)
        {
            this.realtimeMatchData = MiniJSON.Json.Serialize(data.Data, true);
        }

        void RealtimeSession_Joined(bool success)
        {
            updatePlayerText();
        }

        void RealtimeSession_Player(RealtimePlayer player)
        {
            updatePlayerText();
        }

        void RealtimeSession_MessageStringRecieved(string data)
        {
            lastMessage = data;
        }

        bool match_Updated(Match match, MatchPlayer fromPlayer, string notification)
        {
            AddNotification(notification);
            this.match.Load(delegate(Request req)
            {
                UpdateMatch();
            });
            return true;
        }

        bool match_Completed(Match match, MatchPlayer fromPlayer, string notification)
        {
            AddNotification(notification);
            this.match.Load(delegate(Request req)
            {
                UpdateMatch();
            });
            return true;
        }

        bool match_ExpirationWarning(Match match, string notification)
        {
            AddNotification(notification);
            return true;
        }
    }
}

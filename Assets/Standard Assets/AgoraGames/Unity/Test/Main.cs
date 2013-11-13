using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using AgoraGames.Hydra.Test;
using AgoraGames.Hydra;

public class Notification
{
    public object Receiver { get; set; }
    public string Text { get; set; }

    public Notification(object receiver, string text)
    {
        Receiver = receiver;
        Text = text;
    }
}

// TODO: rename me
public class Main : MonoBehaviour
{
    public enum State
    {
        MAIN_MENU,
        ACCOUNT,
        PROFILE,
        PROFILE_MATCHING,
        LEADERBOARDS,
        LEADERBOARD,
        ACHIEVEMENTS,
        MATCHES,
        MATCHMAKING,
        MATCH,
        BROADCAST_CHANNELS,
        BROADCAST_MESSAGES,
        LOBBY_CHAT,
        FRIEND
    };

    State state = State.MAIN_MENU;
    BaseMenu current = null;
    Dictionary<State, BaseMenu> states = new Dictionary<State, BaseMenu>();
    List<History> history = new List<History>();
    public List<Notification> Notifications { get; protected set; }

    // popup
    protected bool hasPopup;
    protected PopupHandler popupHandler;
    protected string popupTitle;
    protected Rect popupRect = new Rect(10, 50, 330, 200);
    protected string popupTextField = "";
    
    public delegate void PopupHandler(string text);

    public Main()
    {
        Notifications = new List<Notification>();

        states[State.MAIN_MENU] = new MainMenu(this);
        states[State.ACCOUNT] = new AccountMenu(this);
        states[State.PROFILE] = new ProfileMenu(this);
        states[State.PROFILE_MATCHING] = new ProfileMatchingMenu(this);
        states[State.MATCHES] = new MatchesMenu(this);
        states[State.MATCHMAKING] = new MatchMakingMenu(this);
        states[State.MATCH] = new MatchMenu(this);
        states[State.LEADERBOARDS] = new LeaderboardsMenu(this);
        states[State.LEADERBOARD] = new LeaderboardMenu(this);
        states[State.ACHIEVEMENTS] = new AchievementsMenu(this);
        states[State.BROADCAST_CHANNELS] = new BroadcastChannelsMenu(this);
        states[State.BROADCAST_MESSAGES] = new BroadcastMessagesMenu(this);
        states[State.LOBBY_CHAT] = new LobbyChatMenu(this);
        states[State.FRIEND] = new FriendMenu(this);

        Client.Instance.Match.UnhandledInvited += Match_UnhandledInvited;
        Client.Instance.Match.UnhandledUpdated += Match_UnhandledUpdated;
        Client.Instance.Match.UnhandledExpirationWarning += Match_UnhandledExpirationWarning;

        SetState(State.MAIN_MENU, null);
    }

    public void AddNotification(object receiver, string notification)
    {
        Notifications.Add(new Notification(receiver, notification));
    }

    void Match_UnhandledInvited(AgoraGames.Hydra.Models.Match match, AgoraGames.Hydra.Models.MatchPlayer fromPlayer, string notification)
    {
        AddNotification(this, notification);
    }

    void Match_UnhandledUpdated(AgoraGames.Hydra.Models.Match match, AgoraGames.Hydra.Models.MatchPlayer fromPlayer, string notification)
    {
        AddNotification(this, notification);
    }

    void Match_UnhandledExpirationWarning(AgoraGames.Hydra.Models.Match match, string notification)
    {
        AddNotification(this, notification);
    }

    public void Back()
    {
        History prev = this.history[this.history.Count - 2];

        this.history.RemoveAt(this.history.Count - 1);
        SetState(prev.state, prev.param, false);
    }

    public void SetState(State state, object param, bool history = true)
    {
        if (current != null)
        {
            current.Deactivate();
        }

        current = states[state];
        current.SetParam(param);

        this.state = state;

        if(history) 
        {
            this.history.Add(new History(state, param));
        }
    }

    public void PopupDialog(string title, PopupHandler handler)
    {
        popupTextField = "";
        popupHandler = handler;
        hasPopup = true;
        popupTitle = title;
    }
    
    void OnGUI()
    {
        GUI.enabled = Client.Instance.IsInitalized;

        if (hasPopup)
        {
            popupRect = GUI.Window(0, popupRect, DrawPopupDialog, popupTitle);
        }
        else
        {
            states[state].Render();
        }
    }

    // 
    void DrawPopupDialog(int id)
    {
        if (GUI.Button(new Rect(10, 150, 150, 40), "Okay"))
        {
            popupHandler(popupTextField);
            hasPopup = false;
        }

        if (GUI.Button(new Rect(170, 150, 150, 40), "Cancel"))
        {
            hasPopup = false;
        }

        popupTextField = GUI.TextField(new Rect(10, 50, 300, 40), popupTextField, 30);
    }

    class History {
        public State state = State.MAIN_MENU;
        public object param;

        public History(State state, object param) 
        {
            this.state = state;
            this.param = param;
        }
    }
}

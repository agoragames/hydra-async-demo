using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using AgoraGames.Hydra.Test;
using AgoraGames.Hydra;
using AgoraGames.Hydra.Util;
using AgoraGames.Hydra.Models;

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
        FRIEND,
        LOGIN
    };

    public Core Core { get; protected set; }

#if UNITY_WEBPLAYER
    public delegate void FacebookTokenHandler(string fbtoken);
    public event FacebookTokenHandler FacebookTokenReceived;
#endif

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
        states[State.LOGIN] = new LoginMenu(this);

        Client.Instance.Match.UnhandledInvited += Match_UnhandledInvited;
        Client.Instance.Match.UnhandledUpdated += Match_UnhandledUpdated;
        Client.Instance.Match.UnhandledExpirationWarning += Match_UnhandledExpirationWarning;

        SetState(Main.State.MAIN_MENU, null);
    }

    public void Awake()
    {
        Core = GetComponent<Core>();

#if UNITY_WEBPLAYER
        Application.ExternalCall("initHydraSdk",  new object[] { this.name });
#endif
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
        bool loginScreen = state == State.LOGIN;
        if (Client.Instance.Status == Client.ClientState.Shutdown && Client.Instance.AuthToken == null && !loginScreen)
        {
            // If hydra is shut down and it doesn't have an auth token, it must not know how we wan't to login yet.
            // Note: AuthToken may be null while Hydra is starting up until it authenticates.
            LoginScreen();
        }

        GUI.enabled = loginScreen ? true : Client.Instance.IsInitalized;
        if (hasPopup)
        {
            popupRect = GUI.Window(0, popupRect, DrawPopupDialog, popupTitle);
        }
        else
        {
            states[state].Render();
        }
    }

    public void LoginScreen()
    {
        SetState(State.LOGIN, new LoginMenuOptions(true, "Log in", delegate(Auth auth, bool create)
        {
            if (create)
            {
                if (auth.AuthType == AuthType.HYDRA)
                {
                    LoginMenu login = states[State.LOGIN] as LoginMenu;
                    Client.Instance.Account.CreateAccount(login.Username, login.Password, delegate(Account account, Request request)
                    {
                        if (request.HasError())
                        {
                            Client.Instance.Logger.Error("Could not create account");
                        }
                        else
                        {
                            SetState(Main.State.MAIN_MENU, null);
                        }
                    });
                }
            }
            else
            {
                Core.StartupHydra(auth);
                SetState(Main.State.MAIN_MENU, null);
            }
        }));
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

#if UNITY_WEBPLAYER
    // Facebook
    public void FacebookLogin()
    {
        Application.ExternalCall("fbLogin",  new object[] { "FacebookTokenReceiver" });
    }

    public void FacebookTokenReceiver(string facebookToken)
    {
        Debug.Log("FacebookTokenReceiver: " + facebookToken);
        if (FacebookTokenReceived != null)
        {
            FacebookTokenReceived(facebookToken);
        }
    }
#endif

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

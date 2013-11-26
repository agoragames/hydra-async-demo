using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Models;
using AgoraGames.Hydra.Util;

namespace AgoraGames.Hydra.Test
{
    public delegate void AuthSelectedHandler(Auth auth, bool create);

    public class LoginMenuOptions
    {
        public bool AllowCreation { get; set; }
        public string MenuAction { get; set; }
        public AuthSelectedHandler AuthSelectedHandler { get; set; }

        public LoginMenuOptions(bool allowCreation, string menuAction, AuthSelectedHandler handler)
        {
            AllowCreation = allowCreation;
            MenuAction = menuAction;
            AuthSelectedHandler = handler;
        }
    }

    public class LoginMenu : BaseMenu
    {
        int selectedAuthType = -1;
        Auth selectedAuth = null;
        protected LoginMenuOptions options = null;

        public const string UUID_AUTH = "UUID";
        public const string FB_AUTH = "Facebook";
        public const string USER_PASS_AUTH = "Username";
        public const string RECOVERY_AUTH = "Recover";
        public const string CREATE_USER_PASS_AUTH = "Create account";
        string[] authOnlyTypes = new string[] { UUID_AUTH, FB_AUTH, USER_PASS_AUTH, RECOVERY_AUTH };
        string[] authAndCreationTypes = new string[] { UUID_AUTH, FB_AUTH, USER_PASS_AUTH, RECOVERY_AUTH, CREATE_USER_PASS_AUTH };

        // Facebook auth
        protected string fbStatus = "Logged out";
        protected string fbToken = "";
        protected bool fbRequestedLogin = false;

        // Username/Password auth
        public string Username { get; protected set; }
        public string Password { get; protected set; }

        // Recovery auth
        public bool IsRecoveryRequested { get; protected set; }
        public string Email { get; protected set; }
        public string Code { get; protected set; }

        public LoginMenu(Main main)
            : base(main)
        {
            Username = "";
            Password = "";
            Email = "";
            Code = "";
            IsRecoveryRequested = false;
        }

        public override void Deactivate()
        {
#if UNITY_WEBPLAYER
            Main.FacebookTokenReceived -= Main_FacebookTokenReceived;
#endif
        }

        public override void Render()
        {
            TestUtil.RenderHeader(this, "Login");

            bool createAccount = false;

            string[] authTypes = options.AllowCreation ? authAndCreationTypes : authOnlyTypes;

            int y = 0;
            int width = 360;
            selectedAuthType = GUI.SelectionGrid(new Rect(0, y += 60, width, 50), selectedAuthType, authTypes, 3);
            y += 60;

            int x = 10;
            if (selectedAuthType != -1)
            {
                if (authTypes[selectedAuthType] == UUID_AUTH)
                {
                    y = RenderUUIDLogin(x, y, width - x);
                }
                else if (authTypes[selectedAuthType] == FB_AUTH)
                {
                    y = RenderFacebookLogin(x, y, width - x);
                }
                else if (authTypes[selectedAuthType] == USER_PASS_AUTH)
                {
                    y = RenderUserPassLogin(x, y, width - x);
                }
                else if (authTypes[selectedAuthType] == RECOVERY_AUTH)
                {
                    y = RenderRecoveryLogin(x, y, width - x);
                }
                else if (authTypes[selectedAuthType] == CREATE_USER_PASS_AUTH)
                {
                    y = RenderUserPassLogin(x, y, width - x);
                    createAccount = true;
                }
            }

            if (selectedAuth != null)
            {
                string buttonText = createAccount ? "Create account" : options.MenuAction + " with " + Auth.GetAuthString(selectedAuth.AuthType);
                if (GUI.Button(new Rect(0, y, 130, 40), buttonText))
                {
                    options.AuthSelectedHandler(selectedAuth, createAccount);
                }
            }
        }

        protected int RenderUUIDLogin(int x, int y, int width)
        {
            string uuid = SystemInfo.deviceUniqueIdentifier;
            GUI.Label(new Rect(x, y, width, 32), "UUID: " + uuid);

            selectedAuth = new UUIDAuth(uuid);
            y += 40;
            return y;
        }

        protected int RenderFacebookLogin(int x, int y, int width)
        {
#if UNITY_WEBPLAYER
            GUI.Label(new Rect(x, y, width, 32), "Status: " + fbStatus);
            GUI.Label(new Rect(x, y += 40, width, 32), "Facebook Token: " + (fbToken.Length > 0 ? "Valid" : "Invalid"));
            selectedAuth = new FacebookAuth(fbToken);

            if (!fbRequestedLogin)
            {
                Main.FacebookLogin();
                fbStatus = "Logging in...";
                fbRequestedLogin = true;
            }
            y += 40;
#else
            GUI.Label(new Rect(x, y, width, 32), "Only available in Web Player builds");
            selectedAuth = null;
            y += 40;
#endif

            return y;
        }

        protected int RenderUserPassLogin(int x, int y, int width)
        {
            int labelWidth = 100;
            int editWidth = width - labelWidth;

            GUI.Label(new Rect(x, y, labelWidth, 32), "Username:");
            Username = GUI.TextField(new Rect(x + labelWidth, y, editWidth, 32), Username, 30);
            y += 40;

            GUI.Label(new Rect(x, y, labelWidth, 32), "Password:");
            Password = GUI.PasswordField(new Rect(x + labelWidth, y, editWidth, 32), Password, '*', 30);
            y += 40;

            selectedAuth = new HydraAuth(Username, Password);
            return y;
        }

        protected int RenderRecoveryLogin(int x, int y, int width)
        {
            int labelWidth = 100;
            int editWidth = width - labelWidth;

            if (GUI.Button(new Rect(x, y, 200, 32), "Request recovery email."))
            {
                Code = "";
                Client.Instance.Account.RecoverAccount(Email, delegate(Request request)
                {
                    IsRecoveryRequested = !request.HasError();
                });
            }

            if (GUI.Button(new Rect(x+210, y, 200, 32), "Already got my email."))
            {
                IsRecoveryRequested = true;
            }
            y += 40;

            GUI.Label(new Rect(x, y, labelWidth, 32), "Email:");
            Email = GUI.TextField(new Rect(x + labelWidth, y, editWidth, 32), Email, 30);
            y += 40;

            if (IsRecoveryRequested)
            {
                GUI.Label(new Rect(x, y, labelWidth, 32), "Code:");
                Code = GUI.PasswordField(new Rect(x + labelWidth, y, editWidth, 32), Code, '*', 30);
                y += 40;

                selectedAuth = new RecoveryAuth(Email, Code);
            }
            else
            {
                selectedAuth = null;
            }
            return y;
        }

        public override void SetParam(object param)
        {
            selectedAuthType = -1;
            selectedAuth = null;
            options = (LoginMenuOptions)param;

#if UNITY_WEBPLAYER
            Main.FacebookTokenReceived += Main_FacebookTokenReceived;
#endif
        }

#if UNITY_WEBPLAYER
        void Main_FacebookTokenReceived(string fbtoken)
        {
            fbToken = fbtoken;
            fbStatus = "Logged in.";
        }
#endif
    }
}

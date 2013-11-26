using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using AgoraGames.Hydra.Models;
using AgoraGames.Hydra.Services;
using AgoraGames.Hydra.Util;

namespace AgoraGames.Hydra
{
    public class Client
    {
        public static String VERSION = "0.5.0";

        public enum ClientState
        {
            Starting,
            Started,
            Shutdown
        }

        protected string url;
        public string Url { get { return url; } }

        public static string HEADER_APIKEY = "X-Hydra-API-Key";
        public static string HEADER_HTTP_METHOD = "X-Hydra-HTTP-Method";
        public static string HEADER_ACCESS_TOKEN = "x-hydra-access-token";
        public static string HEADER_CONTENT_TYPE = "content-type";

        public AuthTokenManager AuthTokenManager { get; protected set; }
        public AuthToken AuthToken { get; set; }

        public string AccessToken { get; protected set; }
        public string ApiKey { get; protected set; }

        public ClientState Status { get; protected set; }
        public bool IsInitalized { get { return Status == ClientState.Started; } }

        protected Logger logger = new Logger();
        public Logger Logger { get { return logger; } }

        // services/factories
        protected AccountsService account = null;
        public AccountsService Account { get { return account; } }

        protected ProfilesService profile = null;
        public ProfilesService Profile { get { return profile; } }

        protected MatchesService match = null;
        public MatchesService Match { get { return match; } }

        protected ConfigurationService configuration = null;
        public ConfigurationService Configuration { get { return configuration; } }

        protected MatchMakingService matchMaking = null;
        public MatchMakingService MatchMaking { get { return matchMaking; } }

        protected Realtime message = null;
        public Realtime Message { get { return message; } }

        protected NotificationsService notification = null;
        public NotificationsService Notification { get { return notification; } }

        protected LeaderboardsService leaderboardService = null;
        public LeaderboardsService Leaderboards { get { return leaderboardService; } }

        protected AchievementsService achievementService = null;
        public AchievementsService Achievements { get { return achievementService; } }

        protected BroadcastNotificationsService broadcastService = null;
        public BroadcastNotificationsService BroadcastNotifications { get { return broadcastService; } }

        public Dispatcher<IncomingMessage> Dispatcher { get; protected set; }
        public Dispatcher<RealtimeEvents> EventDispatcher { get; protected set; }

        protected Runner runner;
        public Runner Runner { get { return runner; } }

        protected Transport transport;
        public Transport Transport { get { return transport; } }

        public delegate void HydraRequestHandler(Request request);

        protected static Client instance;

        public static Client Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Client();
                }
                return instance;
            }
        }

        // Client properties

        protected Account myAccount = null;
        public Account MyAccount
        {
            get
            {
                return myAccount;
            }
        }

        protected Profile myProfile = null;
        public Profile MyProfile
        {
            get
            {
                return myProfile;
            }
        }

        protected Dictionary<String, Achievement> allAchievements = new Dictionary<String, Achievement>();
        public Dictionary<String, Achievement> AllAchievements
        {
            get
            {
                return allAchievements;
            }
        }

        protected Configuration currentConfiguration = null;
        public Configuration CurrentConfiguration
        {
            get
            {
                return currentConfiguration;
            }
        }

        // Startup properties

        protected HydraRequestHandler InitCallback { get; set; }
        protected bool LoadDataDuringStartup { get; set; }

        public Client()
        {
            account = new AccountsService(this);
            profile = new ProfilesService(this);
            match = new MatchesService(this);
            configuration = new ConfigurationService(this);
            matchMaking = new MatchMakingService(this);

            message = new Realtime(this);
            notification = new NotificationsService(this);

            leaderboardService = new LeaderboardsService(this);
            achievementService = new AchievementsService(this);
            broadcastService = new BroadcastNotificationsService(this);

            Dispatcher = new Dispatcher<IncomingMessage>(this, Message.ProcessMessage);
            EventDispatcher = new Dispatcher<RealtimeEvents>(this, Message.ProcessEvent);
            Status = ClientState.Shutdown;
        }

        protected bool IsValidAuthToken(AuthToken token)
        {
            return token != null && token.Token.Trim() != "" && token.Type != AuthType.UNKNOWN;
        }

        // TODO: we need a better way to pass in the runner and transport
        public void Init(Runner runner, Transport transport, string url, string apiKey, AuthTokenManager authTokenManager)
        {
            this.runner = runner;
            this.transport = transport;

            this.url = url.Trim();
            this.ApiKey = apiKey.Trim();
            this.AuthTokenManager = authTokenManager;
            this.AuthToken = null;

            AuthToken token = AuthTokenManager.LoadAuthToken();
            if (IsValidAuthToken(token))
            {
                AuthToken = token;
            }
        }

        public void Startup(HydraRequestHandler init)
        {
            Startup(true, init);
        }

        public void Startup(bool loadData, HydraRequestHandler init)
        {
            Startup(AuthToken, loadData, init);
        }

        public void Startup(Auth auth, bool loadData, HydraRequestHandler init)
        {
            PreStartup(loadData, init);

            Authenticate(auth);
        }

        public void Startup(AuthToken authToken, bool loadData, HydraRequestHandler init)
        {
            PreStartup(loadData, init);
            AuthToken = authToken;

            Access(AuthToken.Token);
        }

        protected void PreStartup(bool loadData, HydraRequestHandler init)
        {
            InitCallback = init;
            LoadDataDuringStartup = loadData;

            Status = ClientState.Starting;

            Message.Startup();
        }

        public void Shutdown()
        {
            if(Message.IsConnected)
                Message.Disconnect();

            Message.Shutdown();
            Status = ClientState.Shutdown;
            AccessToken = null;

            myAccount = null;
            myProfile = null;
            allAchievements.Clear();
            currentConfiguration = null;

            Logger.Info("Client shut down.");
        }

        public void Logout()
        {
            Shutdown();
            ClearAuthToken();

            Logger.Info("Client logged off.");
        }

        protected void ClearAuthToken()
        {
            AuthTokenManager.DeleteAuthToken();
            AuthToken = null;
        }

        public Request DoRequest(string endpoint, string verb, object param, HydraRequestHandler response)
        {
            Request request = new Request(this, response, endpoint, verb, param);

            Logger.Info(" request " + endpoint);
            runner.DoRequest(this, request);

            return request;
        }

        protected void Authenticate(Auth auth)
        {
            Dictionary<string, object> reqData = auth.GenerateAuthRequest();
            DoRequest("auth", "post", reqData, HandleAuthResponse);
        }

        protected void HandleAuthResponse(Request request)
        {
            if (!request.HasError())
            {
                Dictionary<object, object> resp = (Dictionary<object, object>)request.Data;
                string authToken = (String)resp["token"];
                string authType = (String)resp["type"];
                AuthToken = new AuthToken(authToken, Auth.GetAuthType(authType));

                AuthTokenManager.SaveAuthToken(AuthToken);
                Access(authToken);
            }
            else
            {
                LoadCompleted(request);
            }
        }

        protected void Access(string authToken)
        {
            Dictionary<string, string> accessData = new Dictionary<string, string>();
            accessData["auth_token"] = authToken;
            DoRequest("access", "post", accessData, HandleAccessResponse);
        }

        protected void HandleAccessResponse(Request request)
        {
            if (!request.HasError())
            {
                Dictionary<object, object> resp = (Dictionary<object, object>)request.Data;
                this.AccessToken = (String)resp["token"];

                if (LoadDataDuringStartup)
                    LoadData();
                else
                    LoadCompleted(request);
            }
            else
            {
                // If the access request failed, we must have an invalid auth token. No sense in keeping it around, since we might try to use it again.
                if (request.Status == 400)
                {
                    ClearAuthToken();
                }

                LoadCompleted(request);
            }
        }

        protected void LoadData()
        {
            // only load configuration/achievements when the sdk is
            //   initalized not everytime we authenticate
            if (!IsInitalized)
            {
                loadAllData();
            }
            else
            {
                loadUserData();
            }
        }

        protected void loadAllData()
        {
            Configuration.Load(HandleSdkResponse);
        }

        protected void HandleSdkResponse(Configuration config, Request request)
        {
            if (!request.HasError())
            {
                currentConfiguration = config;

                Achievements.All(HandleAchievementResponse);
            }
            else
            {
                LoadCompleted(request);
            }
        }

        protected void HandleAchievementResponse(List<Achievement> list, Request request)
        {
            if (!request.HasError())
            {
                // index achievements for fast loading
                foreach (Achievement a in list)
                {
                    allAchievements[a.Id] = a;
                }

                loadUserData();
            }
            else
            {
                LoadCompleted(request);
            }
        }

        protected void loadUserData()
        {
            Account.Load(HandleAccountResponse);
        }

        protected void HandleAccountResponse(Account account, Request request)
        {
            if (!request.HasError())
            {
                myAccount = account;

                Profile.Load(HandleProfileResponse);
            }
            else
            {
                LoadCompleted(request);
            }
        }

        protected void HandleProfileResponse(Profile profile, Request request)
        {
            if (!request.HasError())
            {
                myProfile = profile;

                Notification.PollMessages(HandleNotificationResponse);
            }
            else
            {
                LoadCompleted(request);
            }
        }

        protected void HandleNotificationResponse(Request request)
        {
            LoadCompleted(request);
        }

        public void ConnectRealtime() 
        {
            if (currentConfiguration != null && currentConfiguration.RealtimeEnabled)
            {
                Message.Connect(currentConfiguration.RealtimeEndpoint);
            }
        }

        // callbacks
        protected void LoadCompleted(Request request)
        {
            StartupCompleted(request.HasError(), "Please check for latest sdk version");
            if (!request.HasError())
            {
                Status = ClientState.Started;
                ConnectRealtime();
            }

            InitCallback(request);
        }

        protected void StartupCompleted(bool error, string errormessage)
        {
            if (error)
            {
                Status = ClientState.Shutdown;
                Logger.Error("Client Startup failed: " + errormessage);
            }
            else
            {
                Status = ClientState.Started;
                Logger.Info("Client Startup succesful!");
            }
        }

        // utils
        public static string appendHttp(string str)
        {
            if (str.StartsWith("http"))
            {
                return str;
            }
            return "https://" + str;
        }

        public static byte[] encode(object param, Dictionary<string, string> headers)
        {
            return BinaryPacker2.encode(param);
        }
    }
}


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
        public static String VERSION = "0.4.0";

        protected string url;
        public string Url { get { return url; } }

        protected string apiKey;

        public static string HEADER_APIKEY = "X-Hydra-API-Key";
        public static string HEADER_HTTP_METHOD = "X-Hydra-HTTP-Method";
        public static string HEADER_AUTH_TOKEN = "AUTH-TOKEN";
        public static string HEADER_CONTENT_TYPE = "content-type";

        protected string uniqueId = null;
        protected string authToken = null;

        public string AuthToken { get { return authToken; } }
        public string ApiKey { get { return apiKey; } }
        public bool IsInitalized { get; protected set; }

        protected Logger logger = new Logger();
        public Logger Logger { get { return logger; } }

        // services/factories
        protected Auth auth = new Auth();
        public Auth Auth { get { return Auth; } }

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

        protected Configuration currentConfiguration;
        public Configuration CurrentConfiguration
        {
            get
            {
                return currentConfiguration;
            }
        }

        protected HydraRequestHandler InitCallback { get; set; }

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
            IsInitalized = false;
        }

        // TODO: we need a better way to pass in the runner and transport
        public void Init(Runner runner, Transport transport, string url, string apiKey)
        {
            this.runner = runner;
            this.transport = transport;

            this.url = url.Trim();
            this.apiKey = apiKey.Trim();

            this.runner = runner;
        }

        public void Authenticate(AuthType type, string token, HydraRequestHandler init)
		{
            Authenticate(type, token, null, true, init);
		}

        public void Authenticate(AuthType type, string token, Dictionary<string, string> data, bool fetchData, HydraRequestHandler init)
        {
            InitCallback = init;

            Dictionary<string, string> reqData =  Auth.GenerateAuthRequest(type, token, data);
            DoRequest("auth", "post", reqData, HandleAuthResponse);
        }

        protected void HandleAuthResponse(Request request)
		{
            if (!request.HasError())
            {
                Dictionary<object, object> resp = (Dictionary<object, object>)request.Data;
                this.authToken = (String)resp["token"];

                LoadData();
            }
            else
            {
                LoadCompleted(request);
            }
        }

        public void Startup()
        {
            Message.Startup();
        }

        public void Shutdown()
        {
            Message.Disconnect();
            Message.Shutdown();
        }

        public Request DoRequest(string endpoint, string verb, object param, HydraRequestHandler response)
        {
            Request request = new Request(this, response, endpoint, verb, param);

            Logger.Info(" request " + endpoint);
            runner.DoRequest(this, request);

            return request;
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
            if (currentConfiguration.RealtimeEnabled)
            {
                Message.Connect(currentConfiguration.RealtimeEndpoint);
            }
        }

        // callbacks
        protected void LoadCompleted(Request request)
        {
            if (request.HasError())
            {
                // TODO: set api state to something invalid

                IsInitalized = false;
                Logger.Error("sdk initalized failed, please check for latest sdk version");
            }
            else
            {
                IsInitalized = true;
                ConnectRealtime();
            }

            InitCallback(request);
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


using System.Collections.Generic;

namespace AgoraGames.Hydra
{
    public class LiveAuth : Auth
    {
        public override AuthType AuthType { get { return AuthType.LIVE; } }
        public string LiveAuthToken { get; protected set; }

        public LiveAuth(string liveAuthToken, Dictionary<string, string> additionalData)
            : base(additionalData)
        {
            LiveAuthToken = liveAuthToken;
        }

        public LiveAuth(string liveAuthToken)
            : base(null)
        {
            LiveAuthToken = liveAuthToken;
        }

        protected override object GetAuthData()
        {
            return LiveAuthToken;
        }
    }
}

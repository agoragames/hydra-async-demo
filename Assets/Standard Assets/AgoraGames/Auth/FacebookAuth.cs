using System.Collections.Generic;

namespace AgoraGames.Hydra
{
    public class FacebookAuth : Auth
    {
        public override AuthType AuthType { get { return AuthType.FACEBOOK; } }
        public string FacebookAuthToken { get; protected set; }

        public FacebookAuth(string fbAuthToken, Dictionary<string, string> additionalData)
            : base(additionalData)
        {
            FacebookAuthToken = fbAuthToken;
        }

        public FacebookAuth(string fbAuthToken)
            : base(null)
        {
            FacebookAuthToken = fbAuthToken;
        }

        protected override object GetAuthData()
        {
            return FacebookAuthToken;
        }
    }
}

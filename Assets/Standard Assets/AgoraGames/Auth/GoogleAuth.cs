using System.Collections.Generic;

namespace AgoraGames.Hydra
{
    public class GoogleAuth : Auth
    {
        public override AuthType AuthType { get { return AuthType.GOOGLE; } }
        public string GoogleAuthToken { get; protected set; }

        public GoogleAuth(string googleAuthToken, Dictionary<string, string> additionalData)
            : base(additionalData)
        {
            GoogleAuthToken = googleAuthToken;
        }

        public GoogleAuth(string googleAuthToken)
            : base(null)
        {
            GoogleAuthToken = googleAuthToken;
        }

        protected override object GetAuthData()
        {
            return GoogleAuthToken;
        }
    }
}

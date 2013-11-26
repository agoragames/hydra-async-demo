using UnityEngine;

namespace AgoraGames.Hydra.Util
{
    public class UnityAuthTokenManager : AuthTokenManager
    {
        public const string AUTH_TOKEN_PREF = "hydra-auth-token";
        public const string AUTH_TOKEN_TYPE = "hydra-auth-type";
        protected AuthToken AuthToken { get; set; }

        public UnityAuthTokenManager()
        {
            AuthToken = null;
        }

        public AuthToken LoadAuthToken()
        {
            if(AuthToken == null)
            {
                string token = PlayerPrefs.GetString(AUTH_TOKEN_PREF, null);
                AuthType type = (AuthType) PlayerPrefs.GetInt(AUTH_TOKEN_TYPE, 0);

                if (token != null && type != 0)
                    AuthToken = new AuthToken(token, type);
            }

            return AuthToken;
        }

        public void SaveAuthToken(AuthToken authToken)
        {
            PlayerPrefs.SetString(AUTH_TOKEN_PREF, authToken.Token);
            PlayerPrefs.SetInt(AUTH_TOKEN_TYPE, (int)authToken.Type);
        }

        public void DeleteAuthToken()
        {
            PlayerPrefs.DeleteKey(AUTH_TOKEN_PREF);
        }
    }
}
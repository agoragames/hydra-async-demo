using AgoraGames.Hydra.Util;
using System.Collections.Generic;

namespace AgoraGames.Hydra
{
    public class AuthToken
    {
        public string Token { get; protected set; }
        public AuthType Type { get; protected set; }

        public AuthToken(string token, AuthType type)
        {
            Token = token;
            Type = type;
        }
    }

    public interface AuthTokenManager
    {
        AuthToken LoadAuthToken();
        void SaveAuthToken(AuthToken authToken);
        void DeleteAuthToken();
    }

    public enum AuthType
    {
        UNKNOWN = 0,
        UUID = 1,
        ANONYMOUS = 2,
        HYDRA = 3,
        FACEBOOK = 4,
        LIVE = 5,
        GOOGLE = 6,
        RECOVERY = 7,
        PLATFORM = 8 // Deprecated.
    }

    public abstract class Auth
    {
        public abstract AuthType AuthType { get; }
        public Dictionary<string, string> AdditionalData { get; set; }

        public Auth (Dictionary<string, string> additionalData)
        {
            AdditionalData = additionalData;
        }

        public Dictionary<string, object> GenerateAuthRequest()
        {
            Dictionary<string, object> authHash = new Dictionary<string, object>();

            if (AdditionalData != null)
            {
                new MapHelper(authHash).Merge(AdditionalData);
            }

            authHash.Add(Auth.GetAuthString(AuthType), GetAuthData());
            return authHash;
        }

        protected abstract object GetAuthData();

        public static string GetAuthString(AuthType type)
        {
            switch (type)
            {
                case AuthType.UUID:
                    return "UUID";
                case AuthType.FACEBOOK:
                    return "facebook";
                case AuthType.LIVE:
                    return "live";
                case AuthType.GOOGLE:
                    return "google";
                case AuthType.PLATFORM:
                    return "platform";
                case AuthType.HYDRA:
                    return "user";
                case AuthType.ANONYMOUS:
                    return "anonymous";
                case AuthType.RECOVERY:
                    return "recovery";
            }
            return null;
        }

        public static AuthType GetAuthType(string type)
        {
            if (type == "UUID" || type == "uuid")
                return AuthType.UUID;
            else if (type == "anonymous")
                return AuthType.ANONYMOUS;
            else if (type == "user")
                return AuthType.HYDRA;
            else if (type == "facebook")
                return AuthType.FACEBOOK;
            else if (type == "live")
                return AuthType.LIVE;
            else if (type == "google")
                return AuthType.GOOGLE;
            else if (type == "recovery")
                return AuthType.RECOVERY;
            else if (type == "platform")
                return AuthType.PLATFORM;
            else
                return AuthType.UNKNOWN;
        }
    }
}


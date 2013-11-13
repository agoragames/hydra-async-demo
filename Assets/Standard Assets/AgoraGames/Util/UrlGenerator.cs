using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgoraGames.Hydra.Util
{
    public class UrlGenerator
    {
        StringBuilder url = new StringBuilder();
        bool first = false;

        public UrlGenerator(string url) 
        {
            this.url.Append(url);
        }

        public UrlGenerator Append(string path)
        {
            url.Append(path);
            return this;
        }

        public UrlGenerator Append(string field, List<string> values)
        {
            if (values != null)
            {
                for (int j = 0; j < values.Count; j++)
                {
                    addDelimiter();

                    url.Append(field);
                    url.Append("=");
                    url.Append(values[j]);
                }
            }
            return this;
        }

        public UrlGenerator Append(string field, Dictionary<string, object> values)
        {
            List<string> keys = new List<string>(values.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                object value = values[keys[i]];
                addDelimiter();

                if (value is List<string>)
                {
                    Append(field, (List<string>)value);
                }
                else
                {
                    Append(field, value.ToString());
                }
            }

            return this;
        }

        public UrlGenerator Append(string field, object value)
        {
            addDelimiter();

            url.Append(field);
            url.Append("=");
            url.Append(value.ToString());
            return this;
        }

        public override string ToString()
        {
            return url.ToString();
        }

        protected void addDelimiter()
        {
            if (!first)
            {
                url.Append("?");
                first = true;
            }
            else
            {
                url.Append("&");
            }
        }
    }
}

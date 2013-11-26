using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace AgoraGames.Hydra
{
    public class WebRequestHTTPTransport : HTTPTransport
    {
        public override void DoHTTPRequest(string url, string verb, Dictionary<string, string> headers, byte[] postData,
            out int responseCode, out string responseMessage, out Dictionary<string, string> responseHeaders, out byte[] responseData)
        {
            try
            {
                WebRequest webRequest = WebRequest.Create(url);

                // this is stupid, it appears to default to having a proxy
                webRequest.Proxy = null;
                webRequest.Method = headers[Client.HEADER_HTTP_METHOD];

                if (headers.ContainsKey(Client.HEADER_CONTENT_TYPE))
                {
                    webRequest.ContentType = headers[Client.HEADER_CONTENT_TYPE];

                    headers.Remove(Client.HEADER_CONTENT_TYPE);
                }

                foreach (KeyValuePair<string, string> iter in headers)
                {
                    webRequest.Headers.Add(iter.Key, iter.Value);
                }

                if (postData != null)
                {
                    webRequest.ContentLength = postData.Length;

                    Stream dataStream = webRequest.GetRequestStream();

                    // Write the data to the request stream.
                    dataStream.Write(postData, 0, postData.Length);
                    dataStream.Flush();
                    dataStream.Close();
                }

                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

                ProcessWebResponse(response, out responseCode, out responseMessage, out responseHeaders, out responseData);
            }
            catch (WebException ex)
            {
                responseMessage = ex.Message;
                responseData = null;
                responseHeaders = new Dictionary<string, string>();

                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    ProcessWebResponse((HttpWebResponse)ex.Response, out responseCode, out responseMessage, out responseHeaders, out responseData);
                }
                else
                {
                    responseCode = 500;
                }
            }
        }

        protected static void ProcessWebResponse(HttpWebResponse response, out int responseCode, out string responseMessage, out Dictionary<string, string> responseHeaders, out byte[] responseData)
        {
            Stream responseStream = response.GetResponseStream();
            MemoryStream memoryStream = new MemoryStream();
            byte[] responseBuffer = new byte[1024];

            while (true)
            {
                int read = responseStream.Read(responseBuffer, 0, responseBuffer.Length);

                if (read != 0)
                {
                    memoryStream.Write(responseBuffer, 0, read);
                }
                else
                {
                    break;
                }
            }

            responseStream.Close();

            responseCode = Convert.ToInt16(response.StatusCode);
            responseMessage = response.StatusDescription;

            responseHeaders = new Dictionary<string, string>();
            foreach (string key in response.Headers.AllKeys)
            {
                responseHeaders.Add(key, response.Headers[key]);
            }
            responseData = memoryStream.ToArray();
        }

        protected static bool IsSuccess(HttpStatusCode code)
        {
            return code == HttpStatusCode.OK || code == HttpStatusCode.Created;
        }
    }
}
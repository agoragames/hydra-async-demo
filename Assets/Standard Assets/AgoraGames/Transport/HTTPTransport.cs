using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AgoraGames.Hydra.Util;

namespace AgoraGames.Hydra
{
    // small helper for different transports which implement http
    public abstract class HTTPTransport : Transport
    {
        public void DoRequest(Client client, Request request)
        {
            string url;
            Dictionary<string, string> headers;
            byte[] postData = null;
            string verb;

            GetHttpParamsFromRequest(client, request, out url, out verb, out headers, out postData);

            // call generic http request method
            byte[] response;
            int responseCode = 500;
            string responseMessage = null;

            DoHTTPRequest(url, request.Command, headers, postData, out responseCode, out responseMessage, out request.Headers, out response);

            request.Status = responseCode;
            request.Message = responseMessage;

            if (response != null && response.Length > 0)
            {
                request.Data = BinaryPacker2.decode(response);
            }
        }

        public static void GetHttpParamsFromRequest(Client client, Request request, out string url, out string verb, out Dictionary<string, string> headers, out byte[] postData)
        {
            headers = new Dictionary<string, string>();
            headers[Client.HEADER_APIKEY] = client.ApiKey;
            headers[Client.HEADER_HTTP_METHOD] = request.Command;
            headers[Client.HEADER_CONTENT_TYPE] = "application/x-ag-binary";

            if (client.AccessToken != null)
            {
                headers.Add(Client.HEADER_ACCESS_TOKEN, client.AccessToken);
            }

            // 
            postData = null;
            if (request.Param != null)
            {
                postData = Client.encode(request.Param, headers);
            }
            verb = request.Command;
            url = Client.appendHttp(client.Url) + "/" + request.Service;
        }

        public abstract void DoHTTPRequest(string url, string verb, Dictionary<string, string> headers, byte[] postData,
            out int responseCode, out string responseMessage, out Dictionary<string, string> responseHeaders, out byte[] response);
    }
}



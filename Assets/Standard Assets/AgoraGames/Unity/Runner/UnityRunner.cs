using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using AgoraGames.Hydra.Util;

namespace AgoraGames.Hydra
{
    public class UnityRunner : Runner
    {
        MonoBehaviour mono;

        // we're only going to use the unity runner here, having a separate unityrunner
        //  and unitytransport will confuse the way we need to do the request with the corutine.
        public UnityRunner(MonoBehaviour mono)
        {
            this.mono = mono;
        }

        public void DoRequest(Client client, Request request)
        {
            mono.StartCoroutine(_doRequest(client, request));
        }

        public void Shutdown()
        {
        }

        public bool WaitForAll(int timeout) 
        {
            // TODO: find out a way to track requets..
            return false;
        }

        protected IEnumerator _doRequest(Client client, Request request)
        {
            string url;
            string verb;
            Dictionary<string, string> headers;
            byte[] postData;

            // get common headers from the http implemenation
            HTTPTransport.GetHttpParamsFromRequest(client, request, out url, out verb, out headers, out postData);

            // do request using www from unity
            Hashtable wwwHeaders = new Hashtable();

            foreach (KeyValuePair<string, string> iter in headers)
            {
                wwwHeaders.Add(iter.Key, iter.Value);
            }

            // so this is a super hack, the www class doesn't allow you to change the headers
            //  on a get, so we need to send post data everytime
            if (postData == null)
            {
                postData = new byte[4];
            }

            WWW www = new WWW(url, postData, wwwHeaders);

            // wait for response
            yield return www;

            // we need to default to a success here, www only has a error field that has both the text and status code...
            request.Status = 200;

            // TODO: check stauts
            if (www.error != null)
            {
                request.Status = parseError(www.error);
                client.Logger.Error("error: " + www.error);
            }
            else if (www.bytes.Length > 0)
            {
                request.Data = BinaryPacker2.decode(www.bytes);
            }
            request.Headers = www.responseHeaders;

            request.NotifyComplete();
        }

        public static int parseError(string error)
        {
            // Logger.Error(error);
            try
            {
                return Int32.Parse(error.Substring(0, 3));
            }
            catch (FormatException)
            {
                return 500;
            }
        }

        public void SendResponse(Client client, AgoraGames.Hydra.Client.HydraRequestHandler response)
        {
        }
    }
}
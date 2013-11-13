using System;
using System.Collections;
using System.Collections.Generic;

using AgoraGames.Hydra;
using System.Text;

namespace AgoraGames.Hydra.Util
{
	public class HelperUtil
	{
        public static DateTime EPOCH = new DateTime(1970, 1, 1);

        public static long GetMilisecondsSinceEpoch(DateTime dateTime)
        {
            return (long)(dateTime - EPOCH).TotalMilliseconds;
        }

        public static DateTime FromMilisecondsSinceEpoch(long miliseconds)
        {
            return EPOCH.AddMilliseconds(miliseconds);
        }

        public class MultiRequest
        {
			protected List<MultiRequestRequest> responses = new List<MultiRequestRequest>();
			protected MultiRequestComplete complete;

			public delegate void MultiRequestComplete();
			
			public MultiRequest(MultiRequestComplete complete) {
				this.complete = complete;
			}
			
			public Client.HydraRequestHandler addRequest(string id) {
				MultiRequestRequest ret = new MultiRequestRequest(this, id);
			
				responses.Add(ret);
				return ret.Response;
			}
			
			public void responseFinished(MultiRequestRequest multiRequest, Request request) {
				if(isComplete()) {
					complete();
				}
			}
			
			protected bool isComplete() {
				foreach(MultiRequestRequest r in responses) {
					if(!r.complete) {
						return false;
					}
				}
				return true;
			}
		}
		
		public class MultiRequestRequest {
			public string Id;
			public bool complete;
			protected MultiRequest multiRequest;
			
			public MultiRequestRequest(MultiRequest multiRequest, string id) {
				Id = id;
				this.multiRequest = multiRequest;
			}
			
			public void Response(Request request) {
				complete = true;
				this.multiRequest.responseFinished(this, request);
			}		
		}
		
		public class DualDelegate {
            Client.HydraRequestHandler first;
            Client.HydraRequestHandler second;

            public DualDelegate(Client.HydraRequestHandler first, Client.HydraRequestHandler second)
            {
				this.first = first;
				this.second = second;
			}
			
			public void Response(Request request) {
				// process response
				first(request);
				second(request);
			}		
		}
		
	}
}
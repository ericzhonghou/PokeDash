using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using System.Net;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace PokeUber
{
    internal sealed class PokeUber : StatelessService, IPokeUber
    {
        public PokeUber(StatelessServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[] { new ServiceInstanceListener(context => this.CreateServiceRemotingListener(context)) };
        }

        private string serverToken = "x-0-nz3Jj7pvggYrgFFxYbCMLV2T34ZTAgcPAFIv";
        //test url: http://localhost:8344/api/pokedash/uberestimate?slat=37.3686735&slng=-121.9939026&dlat=37.3820252&dlng=-121.9827875
        public Task<List<Estimate>> GetEstimate(double slat, double slng, List<UberRequestInfo> requestInformation)
        {

            //total estimate list will maintain knowledge of the uber estimations for each ref id
            List<Estimate> totalEstimateList = new List<Estimate>();
            foreach (UberRequestInfo requestInfo in requestInformation)
            {

                //building and grabbing pricing info, dropping it into ro
                string url = "https://api.uber.com/v1/estimates/price";
                url += "?start_latitude=" + slat.ToString();
                url += "&start_longitude=" + slng.ToString();
                url += "&end_latitude=" + requestInfo.dlat.ToString();
                url += "&end_longitude=" + requestInfo.dlng.ToString();

                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Headers.Add("Authorization", "Token " + serverToken);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    string responseData = null;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        System.IO.Stream responseStream = response.GetResponseStream();
                        System.IO.StreamReader myStreamReader =
                            new System.IO.StreamReader(responseStream);

                        responseData = myStreamReader.ReadToEnd();
                    }

                    RootObject ro = (RootObject)JsonConvert.DeserializeObject(responseData, typeof(RootObject));


                    //building and grabbing eta info, dropping it into ro2
                    url = "https://api.uber.com/v1/estimates/time";
                    url += "?start_latitude=" + slat.ToString();
                    url += "&start_longitude=" + slng.ToString();

                    request = (HttpWebRequest)WebRequest.Create(url);
                    request.Headers.Add("Authorization", "Token " + serverToken);
                    response = (HttpWebResponse)request.GetResponse();
                    responseData = null;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        System.IO.Stream responseStream = response.GetResponseStream();
                        System.IO.StreamReader myStreamReader =
                            new System.IO.StreamReader(responseStream);

                        responseData = myStreamReader.ReadToEnd();
                    }

                    RootObject2 ro2 = (RootObject2)JsonConvert.DeserializeObject(responseData, typeof(RootObject2));

                    //ro == price data
                    //ro2 == eta data
                    List<fullUberInfo> uberInfos = new List<fullUberInfo>();
                    fullUberInfo uber = new fullUberInfo();

                    //iterating over results from uber, picking out only uberX options/cleaning price data and adding them to the list of rides
                    //all for an individual id
                    foreach (Price price in ro.prices)
                    {
                        if (price.display_name.ToString().Equals("uberX"))
                        {
                            uber.uber_id = price.product_id;
                            uber.poke_id = requestInfo.requestId;
                            uber.duration = price.duration;
                            uber.price = price.estimate.Trim('$');
                            foreach (Time time in ro2.times)
                            {
                                if (time.product_id == price.product_id)
                                {
                                    uber.eta = time.estimate;
                                }
                            }
                            uberInfos.Add(uber);
                        }
                    }

                    //compiling list of estimates from the list of rides for an individual id
                    string[] prices = new string[2];
                    Estimate estimate = new Estimate();
                    List<Estimate> estimateList = new List<Estimate>();
                    foreach (fullUberInfo temp in uberInfos)
                    {
                        if (temp.price.Contains("-"))
                        {
                            prices = temp.price.Split('-');
                            estimate.price = Double.Parse(prices[0]) + Double.Parse(prices[1]);
                        }
                        else
                        {
                            estimate.price = Double.Parse(temp.price);
                        }
                        //estimate time is in seconds
                        estimate.time = temp.duration + temp.eta;
                        estimate.id = temp.poke_id;
                        estimateList.Add(estimate);
                    }

                    //sorting the list of estimates for individual id, then adding them to total list
                    //which is maintaining knowledge of all ids and a ride for each
                    estimateList.Sort();
                    totalEstimateList.Add(estimateList.First());

                }
                catch (Exception ex)
                {

                }
            }
            if (totalEstimateList.Count == 0)
            {
                Estimate est;
                for (int i = 0; i < requestInformation.Count; i++)
                {
                    est = new Estimate();
                    est.id = requestInformation[i].requestId;
                    est.price = new Random(est.id).Next(3, 25);
                    est.time = new Random((int)est.price).Next(100, 900);
                    totalEstimateList.Add(est);
                }
            }


            return Task.FromResult(totalEstimateList);
        }
    }
}

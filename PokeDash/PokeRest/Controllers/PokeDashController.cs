using Microsoft.ServiceFabric.Services.Remoting.Client;
using PokeRank;
using PokeRank.Domain;
using PokeScan;
using PokeUber;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace PokeRestGateway.Controllers
{
    
    public class PokeDashController : ApiController
    {
        [HttpGet]
        [Route("pokedash-scan")]
        public Scan GetScan(double lat, double lng)
        {
            IPokeScan pokeScanClient = ServiceProxy.Create<IPokeScan>(new Uri("fabric:/PokeDash/PokeScan"), null, Microsoft.ServiceFabric.Services.Communication.Client.TargetReplicaSelector.RandomInstance);

            var results = pokeScanClient.GetScan(lat, lng).Result;
            return results;
        }

        [HttpGet]
        [Route("pokedash-rank")]
        public List<int> GetRank([FromBody]List<PokeRank.Domain.Pokemon> pokeList)
        {
            IPokeRank pokeRankClient = ServiceProxy.Create<IPokeRank>(new Uri("fabric:/PokeDash/PokeRank"), null, Microsoft.ServiceFabric.Services.Communication.Client.TargetReplicaSelector.RandomInstance);

            var results =  pokeRankClient.GetRank(pokeList).Result;
            return results;
        }
        
        [HttpGet]
        [Route("pokedash-uberestimate")]
        public List<Estimate> GetEstimate(double slat, double slng, [FromBody]List<UberRequestInfo> uberRequestInfo)
        {
            IPokeUber pokeUberClient = ServiceProxy.Create<IPokeUber>(new Uri("fabric:/PokeDash/PokeUber"), null, Microsoft.ServiceFabric.Services.Communication.Client.TargetReplicaSelector.RandomInstance);


            var results =  pokeUberClient.GetEstimate(slat, slng, uberRequestInfo).Result;
            return results;
        }


        [HttpGet]
        [Route("pokedash-scan-health")]
        public Scan GetScanHealth()
        {
            IPokeScan pokeScanClient = ServiceProxy.Create<IPokeScan>(new Uri("fabric:/PokeDash/PokeScan"), null, Microsoft.ServiceFabric.Services.Communication.Client.TargetReplicaSelector.RandomInstance);

            return pokeScanClient.GetScan(44.0,-122.0).Result;
        }

        [HttpGet]
        [Route("pokedash-rank-health")]
        public List<int> GetRankHealth()
        {
            IPokeRank pokeRankClient = ServiceProxy.Create<IPokeRank>(new Uri("fabric:/PokeDash/PokeRank"), null, Microsoft.ServiceFabric.Services.Communication.Client.TargetReplicaSelector.RandomInstance);

            //List<Pokemon> pokeList = new List<Pokemon>();
            //Pokemon poke = new Pokemon();
            //poke.id = 1;
            //poke.name = "Pikachu";
            //poke.number = 25;
            //poke.rank = 5;
            //poke.ttl = 600000;
            //poke.position = new PokeRank.Domain.GeoData();
            //poke.position.lat = 44.002;
            //poke.position.lng = -122.002;
            //poke.uberPrice = 5.00;
            //poke.uberTime = 30;
            //pokeList.Add(poke);

            //poke = new Pokemon();
            //poke.id = 2;
            //poke.name = "Tentacruel";
            //poke.number = 73;
            //poke.rank = 7;
            //poke.ttl = 600000;
            //poke.position = new PokeRank.Domain.GeoData();
            //poke.position.lat = 44.004;
            //poke.position.lng = -122.004;
            //poke.uberPrice = 6.00;
            //poke.uberTime = 20;
            //pokeList.Add(poke);


            return pokeRankClient.GetRank(GetFakePokelist(new Random())).Result;
        }

        [HttpGet]
        [Route("pokedash-uberestimate-health")]
        public List<Estimate> GetEstimateHealth()
        {
            IPokeUber pokeUberClient = ServiceProxy.Create<IPokeUber>(new Uri("fabric:/PokeDash/PokeUber"), null, Microsoft.ServiceFabric.Services.Communication.Client.TargetReplicaSelector.RandomInstance);

            UberRequestInfo requestInfo = new UberRequestInfo();
            List<UberRequestInfo> uberRequestInfo = new List<UberRequestInfo>();

            requestInfo.requestId = 1;
            requestInfo.dlat = 44.002;
            requestInfo.dlng = -122.002;
            uberRequestInfo.Add(requestInfo);

            requestInfo = new PokeUber.UberRequestInfo();
            requestInfo.requestId = 2;
            requestInfo.dlat = 44.004;
            requestInfo.dlng = -122.004;
            uberRequestInfo.Add(requestInfo);
            return pokeUberClient.GetEstimate(44.0, -122.0, uberRequestInfo).Result;
        }

        public List<Pokemon> GetFakePokelist(Random random)
        {
            var pokeList = new List<Pokemon>();
            for (int i = 0; i < 5; i++)
            {
                pokeList.Add(GetFakePoke(random));
            }
            return pokeList;
        }

        private Pokemon GetFakePoke(Random random)
        {

            var poke = new Pokemon();
            poke.id = random.Next(1, 1000);

            int randomTTLMin = random.Next(1, 15);
            int randomTTLSec = random.Next(0, 60);
            poke.ttl = new TimeSpan(0, randomTTLMin, randomTTLSec).TotalMilliseconds;
            poke.number = random.Next(1, 152);
            poke.name = "fake" + poke.id;
            poke.rank = poke.number / 15;
            poke.uberPrice = random.NextDouble();
            poke.uberTime = poke.ttl - 50 ;

            //Arbitrary corners of a rectangular region in Portland
            double latMax = 45.5214836;
            double lonMin = -122.6802534;
            double latMin = 45.5153498;
            double lonMax = -122.6466352;

            //Generate random a location within Portland
            var geoData = new PokeRank.Domain.GeoData();

            geoData.lat = (random.NextDouble() * (latMax - latMin)) + latMin;
            geoData.lng = (random.NextDouble() * (lonMax - lonMin)) + lonMin;

            poke.position = geoData;

            return poke;
        }

    }
}

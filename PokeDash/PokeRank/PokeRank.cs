using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using PokeRank.Domain;
using System;

namespace PokeRank
{
    internal sealed class PokeRank : StatelessService, IPokeRank
    {
        public PokeRank(StatelessServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[] { new ServiceInstanceListener(context => this.CreateServiceRemotingListener(context)) };
        }

        public Task<List<int>> GetRank(List<Pokemon> pokeList)
        {
            List<int> toReturn = new List<int>();
            toReturn = GetPokeOrder(pokeList);
            return Task.FromResult(toReturn);
        }

        private List<int> GetPokeOrder(List<Pokemon> pokeList)
        {
            List<int> toReturn = new List<int>();
            List<int> unreachable = new List<int>();

            pokeList.Sort();
            foreach (Pokemon p in pokeList)
            {
                if (p.ttl > p.uberTime)
                {
                    toReturn.Add(p.id);
                }
                else
                {
                    unreachable.Add(p.id);
                }
            }

            toReturn.AddRange(unreachable);

            return toReturn;
        }

        public Task<List<int>> GetRankTest() //For Testing
        {
            Random random = new Random();

            var pokeList = GetFakePokelist(random);

            return Task.FromResult(GetPokeOrder(pokeList));
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
            poke.uberTime = new TimeSpan(0, randomTTLMin, randomTTLSec).TotalMilliseconds;

            //Arbitrary corners of a rectangular region in Portland
            double latMax = 45.5214836;
            double lonMin = -122.6802534;
            double latMin = 45.5153498;
            double lonMax = -122.6466352;

            //Generate random a location within Portland
            var geoData = new Domain.GeoData();

            geoData.lat = (random.NextDouble() * (latMax - latMin)) + latMin;
            geoData.lng = (random.NextDouble() * (lonMax - lonMin)) + lonMin;

            poke.position = geoData;

            return poke;
        }

    }
}

using System.Collections.Generic;
using System.Fabric;
using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace PokeScan
{
    internal sealed class PokeScan : StatelessService, IPokeScan
    {
        public PokeScan(StatelessServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[] { new ServiceInstanceListener(context => this.CreateServiceRemotingListener(context)) };
        }
        private class Rank
        {
            public string name;
            public int rank;
        }

        public Task<Scan> GetScan(double lat, double lng)
        {
            //DateTime before = DateTime.Now;
            //ScanLogic thing = new ScanLogic();
            //PokeData scanResults = thing.doScan(lat, lng, auth).Result;
            //Read in the file.
            Rank[] theRanks = new Rank[151];
            string[] lines = System.IO.File.ReadAllLines(@"../PokeScanPkg.MyData.1.0.0/pokeNamedRankings.csv");
            foreach (string line in lines)
            {

                var temp = line.Split(',');
                int num = int.Parse(temp[0]);
                theRanks[num - 1] = new Rank();
                theRanks[num - 1].name = temp[1];
                theRanks[num - 1].rank = int.Parse(temp[2]);

            }

            Random rng = new Random();
            PokeData pokemon = new PokeData();
            PokeEntity amon;
            GeoData geo;
            int count = rng.Next(1, 4);
            for (int i = 0; i < count; i++)
            {
                int id = rng.Next(50);
                bool newId = false;
                while (!newId)
                {
                    bool found = false;
                    foreach (var j in pokemon.pokeList)
                    {

                        if (j.id == id)
                        {
                            found = true;
                        }
                    }

                    if (!found)
                        newId = true;
                    else
                        id = rng.Next(50);
                }
                amon = new PokeEntity();
                amon.id = id;
                amon.ttl = rng.Next(30, 900);
                amon.number = rng.Next(1, 151);
                amon.name = theRanks[amon.number - 1].name;
                amon.rank = theRanks[amon.number - 1].rank;
                geo = new GeoData();
                geo.lat = lat + (rng.Next(-1000, 1000) / 500000.0);
                geo.lng = lng + (rng.Next(-1000, 1000) / 500000.0);
                amon.position = geo;
                pokemon.pokeList.Add(amon);
            }

            return Task.FromResult(
                new Scan
                {
                    Lat = lat,
                    Lng = lng,
                    entities = pokemon
                }
            );
        }
    }
}

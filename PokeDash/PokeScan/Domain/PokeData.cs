using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeScan
{
    public class PokeData
    {
        public List<PokeEntity> pokeList;

        public PokeData()
        {
            pokeList = new List<PokeEntity>();
        }

    }

    public class PokeEntity
    {
        public int id;
        public double ttl;
        public int number;
        public string name;
        public int rank;
        public GeoData position;
    }

}

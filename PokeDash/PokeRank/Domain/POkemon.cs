using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeRank.Domain
{
    public class Pokemon : IComparable
    {
        public int id;
        public double ttl;
        public int number;
        public string name;
        public int rank;
        public GeoData position;
        public double uberPrice;
        public double uberTime;

        //Comparable so List object can sort Pokemon
        //First compare ranks, then compare ttl
        public int CompareTo(object obj)
        {
            Pokemon p = (Pokemon)obj;

            if (this.rank > p.rank)
            {
                return -1;
            }
            else if (this.rank == p.rank)
            {
                if (this.ttl < p.ttl)
                {
                    return -1;
                }
                else if (this.ttl == p.ttl)
                {
                    if (this.uberPrice < p.uberPrice)
                    {
                        return -1;
                    }
                    else if (this.uberPrice > p.uberPrice)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return 1;
            }
        }
    }



}

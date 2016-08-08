using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeUber
{
    public class Estimate : IComparable
    {
        public double price { get; set; }
        public double time { get; set; }
        public int id { get; set; }

        public int CompareTo(object obj)
        {
            Estimate est = (Estimate)obj;
            if (time <= est.time)
            {
                return 0;
            }
            return 1;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeUber
{
    public class Time
    {
        public string localized_display_name { get; set; }
        public int estimate { get; set; }
        public string display_name { get; set; }
        public string product_id { get; set; }
    }

    public class RootObject2
    {
        public List<Time> times { get; set; }
    }
}

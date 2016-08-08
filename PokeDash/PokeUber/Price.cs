using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeUber
{
    public class Price
    {
        public string product_id { get; set; }
        public string currency_code { get; set; }
        public string display_name { get; set; }
        public string estimate { get; set; }
        public int? low_estimate { get; set; }
        public int? high_estimate { get; set; }
        public double surge_multiplier { get; set; }
        public int duration { get; set; }
        public double distance { get; set; }
    }

    public class RootObject
    {
        public List<Price> prices { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeScan
{
    public class GeoData
    {
        public double lat;
        public double lng;

        public GeoData(double lat, double lng)
        {
            this.lat = lat;
            this.lng = lng;
        }
        public GeoData()
        {

        }
    }
}

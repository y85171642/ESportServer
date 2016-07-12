using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Esports.space.domain
{
    public class Venue
    {
        public string id;
        public string name;
        public string time;
        public string address;
        public string latitude;
        public string longitude;
        public Venue(string id, string name, string time, string address, string latitude, string longitude)
        {
            this.id = id;
            this.name = name;
            this.time = time;
            this.address = address;
            this.latitude = latitude;
            this.longitude = longitude;
        }
    }
}
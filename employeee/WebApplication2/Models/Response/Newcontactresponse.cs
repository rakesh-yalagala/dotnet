using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Models.Response
{
    public class Newcontactresponse
    {
         public required string Contact_name { get; set; }
        public required string Numbers { get; set; }
        public required string Relation { get; set; }
        public required string Gender { get; set; }
    }
}
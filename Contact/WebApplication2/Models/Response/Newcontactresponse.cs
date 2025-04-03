using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Models.Response
{
    public class Newcontactresponse
    {
         public string ?ContactName { get; set; }
        
        // public required string Relation { get; set; }
        // public required string Gender { get; set; }
        public List<Alternewcontactresponse>?ListOfNumbers{get; set;}

    }
}
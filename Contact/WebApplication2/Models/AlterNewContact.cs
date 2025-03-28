using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Models
{
    public class AlterNewContact
    {
        [Key]
        [Column("numberid")]
        public int NumberId{get; set;}

        
        [Column("contactid")]

        public int ContactId { get; set; }
        [Column("phonenumber")]
        public string? PhoneNumber { get; set; } 
        [Column("createdon")]
        public DateTime CreatedOn{set; get;}
        [Column("isdeleted")]
        public Boolean IsDeleted{set; get;}
    }
}
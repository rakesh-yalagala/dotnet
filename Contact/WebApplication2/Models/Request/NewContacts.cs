using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Models
{
    public class NewContacts
    {
        // [Column("contact_guid")]  
        
        // public Guid Contact_guid { get; set; } = Guid.NewGuid();

        [Key]

        [Column("contactid")]   
        public int ContactId  { get; set; }
        [Column("contactname")] 
        
        public string? ContactName { get; set; }
       
        [Column("relation")]


        public required string Relation { get; set; }
        [Column("gender")]
        public required string Gender { get; set; }
        [Required(ErrorMessage = "CreatedOn is required.")]
        [Column("createdon")]

        public DateTime CreatedOn { get; set; }
        //[Required(ErrorMessage="boolean type is required")]
        //[RegularExpression(@"(true|false)$",ErrorMessage ="boolean must be the following:true,false.")]
        [Column("isdeleted")]
        public bool IsDeleted { get; set; }
        
         [ForeignKey("ContactId")]
         

        public List<AlterNewContact>Alternewcontact{get; set;}= new List<AlterNewContact>();


    }
}
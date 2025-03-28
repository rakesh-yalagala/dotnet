using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication2.Models
{
    public class Customers
    {
        [Column("customer_guid")]  
        
        public Guid Customer_guid { get; set; } = Guid.NewGuid();

        [Key]

        [Column("customer_id")]   
        public int Customer_id { get; set; }
        [Column("customer_name")] 
        [Required(ErrorMessage = "name is required.")]
        public required string Customer_name { get; set; }
        [Column("numbers")]

        [Required(ErrorMessage = "Number is required.")]
        public required string Numbers { get; set; }
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
        
        


    }
   
          
}   

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using WebApplication2.Context;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContactsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        

        [HttpGet("Get")]
        public IActionResult GetCustomerById(int id)
        {
            
            var customer = _context.NewCustomers.Find(id);
            if (customer == null)
            {
                return NotFound("Customer not found.");
            }
            return Ok(customer);
        }
        [HttpPut("update")]
        public IActionResult UpdateCustomer(int id, Customers UpdatedCustomer_name){
            var existingname=_context.NewCustomers.Find(id);
            if(existingname==null)
            {
                return BadRequest("i can't find ant name from these table");

            }
            if(existingname!=null)
            {
                existingname.Customer_name=UpdatedCustomer_name.Customer_name;
                existingname.Numbers=UpdatedCustomer_name.Numbers;
                existingname.Relation=UpdatedCustomer_name.Relation;
                existingname.Gender=UpdatedCustomer_name.Gender;
               
                _context.SaveChanges();
            }
            return Ok(existingname);

        }
        
        
            [HttpDelete("Delete")]

            public IActionResult deletecustomer(int id)
            {
                var Existingdelete=_context.NewCustomers.Find(id);
                if(Existingdelete==null)
                {
                    return NotFound("the contact is not found");
                }
                //_context.NewCustomers.Remove(Existingdelete);
                Existingdelete.IsDeleted=true;
                _context.SaveChanges();
                return Ok(Existingdelete);


            }     
            [HttpPut("restoring")]
            public IActionResult restoringdelete(int id)
            {
                var existingrestore=_context.NewCustomers.Find(id);
                if(existingrestore==null)
                {
                    return NotFound("the details of the contact is not found");
                }
                if( !existingrestore.IsDeleted)
                {
                    BadRequest("i didn't get any details of the contact");
                }
                existingrestore.IsDeleted=false;
                _context.SaveChanges();
                return Ok(existingrestore);
            }
            [HttpPost("creating")]
            public IActionResult Creating( [FromBody]Customers NewCustomers )
            {
                NewCustomers.Customer_guid = Guid.NewGuid();
                _context.Add(NewCustomers);
                _context.SaveChanges();
                
               
                return Ok("new contact added");
                
            }
            // [HttpPost("creatingContact")]
            // public IActionResult Creatingnewcontact([FromBody]Customers NewContact)
            // {
            //     _context.Add(NewContact);
            //     _context.SaveChanges();
            //     return Ok("new contact were  added successfully");
            // }
    }
}
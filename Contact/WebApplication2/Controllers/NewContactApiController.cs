using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Npgsql.Internal;
using WebApplication2.Context;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewContactApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NewContactApiController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost("Create")]
        public IActionResult CreateContact([FromBody] NewContacts Details)
        {
            if (Details.ContactName == null)
            {
                return BadRequest("The contact details are required.");
            }

            else if (Details.ContactName != null)
            {
                var Information = new NewContacts
                {
                    ContactName = Details.ContactName,
                    Gender = Details.Gender,
                    Relation = Details.Relation,
                    IsDeleted = false,
                    CreatedOn = DateTime.UtcNow
                };

                _context.Contact.Add(Information);
                _context.SaveChanges();


                if (Details.Alternewcontact != null && Details.Alternewcontact.Any())
                {
                    foreach (var numbers in Details.Alternewcontact)
                    {
                        if (!string.IsNullOrEmpty(numbers.PhoneNumber))
                        {
                            var NewAlternatenumber = new AlterNewContact
                            {
                                ContactId = Information.ContactId,
                                PhoneNumber = numbers.PhoneNumber,
                                IsDeleted = false,
                                CreatedOn = DateTime.UtcNow
                            };

                            _context.NewNumber.Add(NewAlternatenumber);
                        }
                    }
                    _context.SaveChanges();
                }


            }
            return Ok("added");
        }
        [HttpPut("Update")]
        public IActionResult Updatecontact(int ContactId, [FromBody] NewContacts Updatedcontact)
        {
            var ExistingUpdate = _context.Contact.Include(nc => nc.Alternewcontact).FirstOrDefault(dbNc => dbNc.ContactId == ContactId);

            if (ExistingUpdate == null)
            {
                Console.WriteLine("uhytfgyh");
                return BadRequest("The contact ID does not exist.");
            }
           
                if (ExistingUpdate != null)
                {
                    ExistingUpdate.ContactName = Updatedcontact.ContactName;

                    ExistingUpdate.Relation = Updatedcontact.Relation;
                    ExistingUpdate.Gender = Updatedcontact.Gender;
                    ExistingUpdate.IsDeleted = Updatedcontact.IsDeleted;


                    foreach (var anc in Updatedcontact.Alternewcontact)
                    {
                        var ExistingnewUpdate = ExistingUpdate.Alternewcontact.FirstOrDefault(dbAnc => dbAnc.NumberId == anc.NumberId);
                        if (ExistingnewUpdate != null)
                        {
                            ExistingnewUpdate.PhoneNumber = anc.PhoneNumber;
                            ExistingnewUpdate.CreatedOn = anc.CreatedOn;
                            ExistingnewUpdate.IsDeleted = anc.IsDeleted;
                        }
                        else if (ExistingnewUpdate == null)
                        {
                            var newAlterContact = new AlterNewContact
                            {
                                ContactId = anc.ContactId,
                                PhoneNumber = anc.PhoneNumber,
                                CreatedOn = anc.CreatedOn,
                                IsDeleted = anc.IsDeleted
                            };

                            ExistingUpdate.Alternewcontact.Add(newAlterContact);
                        }
                    }

                    var updatedContactIds = Updatedcontact.Alternewcontact.Select(anc => anc.ContactId).ToList();


                    var ContactstoDelete = ExistingUpdate.Alternewcontact.Where(dbAnc => !updatedContactIds.Contains(dbAnc.ContactId));


                    foreach (var contact in ContactstoDelete)
                    {
                        ExistingUpdate.Alternewcontact.Remove(contact);
                    }

                }
                _context.SaveChanges();
                return Ok(ExistingUpdate);
               
            }

       
        [HttpGet("GetContact")]
        public IActionResult Getdetail()
        {
            var Gettingdetails = _context.Contact.Include(nc => nc.Alternewcontact);//.FirstOrDefault(nc => nc.Contact_id == contactid);
            if (Gettingdetails == null)
            {
                return NotFound("the contact details was not found");
            }
            return Ok(Gettingdetails);

        }
        [HttpGet("GetNumbersbyId")]
        public IActionResult Getcontactsnumber(int contactId)
        {
            var gettingnumbers = _context.Contact
            .Include(nc => nc.Alternewcontact).Where(nc => nc.ContactId == contactId && nc.IsDeleted == false)
            .Select(nc => new
            {
                nc.ContactName,

                Alternatenumber = nc.Alternewcontact
                .Select(anc => new
                {
                    anc.PhoneNumber,
                    anc.NumberId,
                })
            });
            return Ok(gettingnumbers);
        }


        [HttpDelete("Delete")]
        public IActionResult Deletecustomer(int id)
        {
            var Existingdelete = _context.Contact.Include(nc => nc.Alternewcontact).FirstOrDefault(nc => nc.ContactId == id);
            if (Existingdelete == null)
            {
                return NotFound("the contact is not found");
            }
            Existingdelete.IsDeleted = true;
            foreach (var alterContact in Existingdelete.Alternewcontact)
            {
                alterContact.IsDeleted = true;
            }
            _context.SaveChanges();
            return Ok(Existingdelete);
        }


    }
}
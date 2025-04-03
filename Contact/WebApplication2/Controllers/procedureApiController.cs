using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Npgsql;
using NpgsqlTypes;
using WebApplication2.Models;
using WebApplication2.Models.Response;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class procedureApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public procedureApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("createcall")]
        public IActionResult CreateContact([FromBody] NewContacts Contacts)
        {
            string? pgsqlConnection = _configuration.GetConnectionString("DefaultConnection");
            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(pgsqlConnection))
            {
                npgsqlConnection.Open();
                int ContactId = 0;
                //ConstructorCreatedOn contact = new ConstructorCreatedOn(Contacts.CreatedOn);
                DateTime createdOn = Contacts.CreatedOn == default ? DateTime.UtcNow : Contacts.CreatedOn;

                using (var cmd = new NpgsqlCommand("CALL CreateContact(@p_ContactName, @p_Relation, @p_Gender, @p_IsDeleted, @p_CreatedOn, @rv_ContactId);", npgsqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@p_ContactName", NpgsqlDbType.Varchar).Value = Contacts.ContactName;
                    cmd.Parameters.Add("@p_Gender", NpgsqlDbType.Varchar).Value = Contacts.Gender;
                    cmd.Parameters.Add("@p_Relation", NpgsqlDbType.Varchar).Value = Contacts.Relation;
                    cmd.Parameters.Add("@p_IsDeleted", NpgsqlDbType.Boolean).Value = Contacts.IsDeleted;
                    cmd.Parameters.Add("@p_CreatedOn", NpgsqlDbType.TimestampTz).Value = createdOn;


                    var id = new NpgsqlParameter("@rv_ContactId", NpgsqlDbType.Integer)
                    {
                        Direction = ParameterDirection.InputOutput
                        ,
                        Value = DBNull.Value

                    };
                    cmd.Parameters.Add(id);
                    cmd.ExecuteNonQuery();
                    ContactId = id.Value != DBNull.Value ? Convert.ToInt32(id.Value) : 0;
                    if (ContactId == 0)
                    {
                        return BadRequest("Failed to create contact.");
                    }
                    //  int contactid = (int)(id.Value ?? 0);
                    //     Console.WriteLine($" ContactId: {contactid}");   
                }
                foreach (var t in Contacts.Alternewcontact)
                {
                    //ConstructorCreatedOn Phone = new ConstructorCreatedOn(t.CreatedOn);
                    //DateTime phoneCreatedOn = t.CreatedOn == default ? DateTime.UtcNow : t.CreatedOn;

                    using (NpgsqlCommand cmd = new NpgsqlCommand(@"call CreateNumber(@p_ContactId,@p_PhoneNumber,@p_IsDeleted,@rv_NumberId)", npgsqlConnection))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@p_ContactId", NpgsqlDbType.Integer).Value = ContactId;
                        cmd.Parameters.Add("@p_PhoneNumber", NpgsqlDbType.Varchar).Value = t.PhoneNumber;
                        cmd.Parameters.Add("@p_IsDeleted", NpgsqlDbType.Boolean).Value = t.IsDeleted;
                        //cmd.Parameters.Add("@p_CreatedOn",NpgsqlDbType.TimestampTz).Value=phoneCreatedOn;
                        // cmd.Parameters.Add("@p_CreatedOn", NpgsqlTypes.NpgsqlDbType.Timestamp)
                        // .Value = DateTime.SpecifyKind(t.CreatedOn == default(DateTime) ? DateTime.UtcNow : t.CreatedOn, DateTimeKind.Unspecified);
                        var id = new NpgsqlParameter("@rv_NumberId", NpgsqlDbType.Integer)
                        {
                            Direction = ParameterDirection.InputOutput
                           ,
                            Value = DBNull.Value

                        };
                        cmd.Parameters.Add(id);
                        cmd.ExecuteNonQuery();
                        //NumberId=id.Value != DBNull.Value ? Convert.ToInt32(id.Value) :0;
                        int Id = (int)(id.Value ?? 0);
                        Console.WriteLine($" NumberId: {Id}");
                    }
                }
                npgsqlConnection.Close();


            }

            return Ok("contact added successfully");
        }
        

        [HttpGet("Get")]
        public IActionResult GetDetails(int id)
        {
            string? pgsqlConnection = _configuration.GetConnectionString("DefaultConnection");

            using (var npgsqlConnection = new NpgsqlConnection(pgsqlConnection))
            {
                npgsqlConnection.Open();
                using (var transaction = npgsqlConnection.BeginTransaction())
                {
                    
                    using (var cmd = new NpgsqlCommand("CALL GetNumber(@p_ContactId, @p_PhoneNumber, @p_NumberId)", npgsqlConnection, transaction))
                    {
                        cmd.Parameters.AddWithValue("p_ContactId", id);

                        
                        var phoneNumberParam = new NpgsqlParameter("p_PhoneNumber", NpgsqlDbType.Varchar)
                        {
                            Direction = ParameterDirection.InputOutput,Value = DBNull.Value
                        };
                        cmd.Parameters.Add(phoneNumberParam);

                        var numberIdParam = new NpgsqlParameter("p_NumberId", NpgsqlDbType.Integer)
                        {
                            Direction = ParameterDirection.InputOutput,
                            Value = DBNull.Value
                        };
                        cmd.Parameters.Add(numberIdParam);

                        
                        cmd.ExecuteNonQuery();

                        string phoneNumber = phoneNumberParam.Value?.ToString() ?? "Not Defined";
                        int numberId = numberIdParam.Value != DBNull.Value ? Convert.ToInt32(numberIdParam.Value) : 0;

                        string contactName = "Not Defined";
                        using (var contactCmd = new NpgsqlCommand("CALL GetContact(@p_ContactId, @p_ContactName)", npgsqlConnection, transaction))
                        {
                            contactCmd.CommandType = CommandType.Text;
                            contactCmd.Parameters.Add("p_ContactId", NpgsqlDbType.Integer).Value = id;

                            var contactNameParam = new NpgsqlParameter("p_ContactName", NpgsqlDbType.Varchar)
                            {
                                Direction = ParameterDirection.InputOutput,
                                Value = DBNull.Value
                            };
                            contactCmd.Parameters.Add(contactNameParam);
                            contactCmd.ExecuteNonQuery();

                            contactName = contactNameParam.Value?.ToString() ?? "Not Defined";
                        }

                        transaction.Commit();

                        var contactDetails = new Newcontactresponse
                        {
                            ContactName = contactName,
                            ListOfNumbers = new List<Alternewcontactresponse>
                    {
                        new Alternewcontactresponse
                        {
                            NumberId = numberId,
                            PhoneNumber = phoneNumber
                        }
                    }
                        };

                        return Ok(contactDetails);
                    }
                }
            }
        }



        [HttpDelete("Delete")]
        public IActionResult Deletecontact(int id)
        {
            string? pgsqlConnection = _configuration.GetConnectionString("DefaultConnection");

            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(pgsqlConnection))
            {
                npgsqlConnection.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(@"call DeleteContact(@p_ContactId)", npgsqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("p_ContactId", NpgsqlDbType.Integer).Value = id;
                    cmd.ExecuteNonQuery();
                }

                using (NpgsqlTransaction transaction = npgsqlConnection.BeginTransaction())
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand(@"call DeleteNumber(@p_ContactId)", npgsqlConnection, transaction))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("p_ContactId", NpgsqlDbType.Integer).Value = id;
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                npgsqlConnection.Close();

            }
            return Ok("contact deleted");
        }




    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Npgsql;

using NpgsqlTypes;
using WebApplication2.Models;
using WebApplication2.Models.Response;
using System.Text.Json;
using System.Globalization;
using System.Transactions;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class procedureController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public procedureController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("Create")]
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
                        return BadRequest("Failed ");
                    }
                    
                }
                foreach (var t in Contacts.Alternewcontact)
                {
                    

                    using (NpgsqlCommand cmd = new NpgsqlCommand(@"call CreateNumber(@p_ContactId,@p_PhoneNumber,@p_IsDeleted,@rv_NumberId)", npgsqlConnection))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@p_ContactId", NpgsqlDbType.Integer).Value = ContactId;
                        cmd.Parameters.Add("@p_PhoneNumber", NpgsqlDbType.Varchar).Value = t.PhoneNumber;
                        cmd.Parameters.Add("@p_IsDeleted", NpgsqlDbType.Boolean).Value = t.IsDeleted;

                        var id = new NpgsqlParameter("@rv_NumberId", NpgsqlDbType.Integer)
                        {
                            Direction = ParameterDirection.InputOutput,
                            Value = DBNull.Value
                        };
                        cmd.Parameters.Add(id);
                        cmd.ExecuteNonQuery();
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

                    using (var cmd = new NpgsqlCommand("CALL GetNumbers(@p_ContactId, @p_PhoneNumber, @p_NumberId)", npgsqlConnection, transaction))
                    {
                        cmd.Parameters.AddWithValue("p_ContactId", NpgsqlDbType.Integer, id);


                        var phoneNumberParam = new NpgsqlParameter("p_PhoneNumber", NpgsqlDbType.Varchar)
                        {
                            Direction = ParameterDirection.InputOutput,
                            Value = DBNull.Value
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
                        using (var contactCmd = new NpgsqlCommand("CALL GetContacts(@p_ContactId, @p_ContactName)", npgsqlConnection, transaction))
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

        [HttpPost("Update")]
        public IActionResult UpdateContacts([FromBody] NewContacts Contacts)
        {
            int ContactId;
            string? pgsqlConnection = _configuration.GetConnectionString("DefaultConnection");

            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(pgsqlConnection))
            {
                npgsqlConnection.Open();

                IActionResult result = GetDetails(Contacts.ContactId);
                string json = "";

                if (result is OkObjectResult okResult)
                {
                    json = JsonSerializer.Serialize(okResult.Value);
                }

                List<int> dBContactId = new List<int>();

                if (!string.IsNullOrEmpty(json))
                {
                    var contactDetails = JsonSerializer.Deserialize<Newcontactresponse>(json);

                    if (contactDetails != null && contactDetails.ListOfNumbers != null)
                    {
                        dBContactId = contactDetails.ListOfNumbers.Select(n => n.NumberId).ToList();
                    }
                }
                List<int> swagNumberId = Contacts.Alternewcontact.Select(n => n.NumberId).ToList();
                DateTime createdOn = Contacts.CreatedOn == default ? DateTime.UtcNow : Contacts.CreatedOn;

                using (NpgsqlCommand cmd = new NpgsqlCommand(@"call UpdateContact(@p_ContactId,@p_ContactName,@p_Relation,@p_Gender,@p_IsDeleted,@p_CreatedOn)", npgsqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@p_ContactId", NpgsqlDbType.Integer).Value = Contacts.ContactId;

                    cmd.Parameters.Add("@p_ContactName", NpgsqlDbType.Varchar).Value = Contacts.ContactName;
                    cmd.Parameters.Add("@p_Gender", NpgsqlDbType.Varchar).Value = Contacts.Gender;
                    cmd.Parameters.Add("@p_Relation", NpgsqlDbType.Varchar).Value = Contacts.Relation;
                    cmd.Parameters.Add("@p_IsDeleted", NpgsqlDbType.Boolean).Value = Contacts.IsDeleted;
                    cmd.Parameters.Add("@p_CreatedOn", NpgsqlDbType.TimestampTz).Value = createdOn;



                    cmd.ExecuteNonQuery();

                }



                foreach (var n in Contacts.Alternewcontact)
                {
                    if (dBContactId.Contains(n.NumberId))
                    {

                        using (NpgsqlCommand cmd = new NpgsqlCommand(@"call UpdateNumber(@p_ContactId,@p_PhoneNumber)", npgsqlConnection))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add("@p_ContactId", NpgsqlDbType.Integer).Value = Contacts.ContactId;
                            cmd.Parameters.Add("@p_PhoneNumber", NpgsqlDbType.Varchar).Value = n.PhoneNumber;

                            cmd.ExecuteNonQuery();
                        }
                    }
                    else if (n.NumberId == 0)
                    {
                        using (NpgsqlCommand cmd = new NpgsqlCommand(@"call CreateNumber(@p_ContactId,@p_PhoneNumber)", npgsqlConnection))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add("@p_ContactId", NpgsqlDbType.Integer).Value = Contacts.ContactId;
                            cmd.Parameters.Add("@p_PhoneNumber", NpgsqlDbType.Varchar).Value = n.PhoneNumber;
                            cmd.ExecuteNonQuery();
                        }

                    }

                }


                List<int> DeleteId = dBContactId.Except(swagNumberId).ToList();
                foreach (var id in DeleteId)
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand(@"call DeleteUpdateNumber(@p_NumberId)", npgsqlConnection))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("p_NumberId", NpgsqlDbType.Integer).Value = id;
                        cmd.ExecuteNonQuery();
                    }

                }
                npgsqlConnection.Close();

            }



            return Ok("Updated sucessfully");
        }

        [HttpGet("GetAll")]
        public IActionResult GetAllDetails(int id)
        {
            string? pgsqlConnection = _configuration.GetConnectionString("DefaultConnection");

            using (var npgsqlConnection = new NpgsqlConnection(pgsqlConnection))
            {
                npgsqlConnection.Open();

                string contactName;
                using (var contactCmd = new NpgsqlCommand("CALL GetContacts(@p_ContactId, @p_ContactName)", npgsqlConnection))
                {
                    contactCmd.Parameters.AddWithValue("p_ContactId", id);
                    var contactNameParam = new NpgsqlParameter("p_ContactName", NpgsqlDbType.Varchar)
                    {
                        Direction = ParameterDirection.InputOutput,
                        Value = DBNull.Value
                    };
                    contactCmd.Parameters.Add(contactNameParam);

                    contactCmd.ExecuteNonQuery();
                    contactName = contactNameParam.Value?.ToString() ?? "Not Defined";
                }

                var numberDetails = new List<Alternewcontactresponse>();
                using (var Cmd = new NpgsqlCommand("CALL GetAllNumbers(@p_ContactId, @getCursor)", npgsqlConnection))
                {
                    Cmd.Parameters.AddWithValue("p_ContactId", id);
                    var cursorParam = new NpgsqlParameter("getCursor", NpgsqlDbType.Refcursor)
                    {
                        Direction = ParameterDirection.InputOutput,
                        Value = "getCursor"
                    };
                    Cmd.Parameters.Add(cursorParam);
                    Cmd.ExecuteNonQuery();


                    using (var fetchCmd = new NpgsqlCommand("FETCH ALL FROM getCursor;", npgsqlConnection))
                    using (var reader = fetchCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            numberDetails.Add(new Alternewcontactresponse
                            {
                                NumberId = reader.GetInt32(1),
                                PhoneNumber = reader.GetString(0)
                            });
                        }
                    }


                    using (var closeCmd = new NpgsqlCommand("CLOSE getCursor;", npgsqlConnection))
                    {
                        closeCmd.ExecuteNonQuery();
                    }
                }

                var contactDetails = new Newcontactresponse
                {
                    ContactName = contactName,
                    ListOfNumbers = numberDetails
                };

                return Ok(contactDetails);
            }
        }




      
    }



}


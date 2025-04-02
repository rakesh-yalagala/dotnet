using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
            using (NpgsqlConnection npgsqlConnection=new NpgsqlConnection(pgsqlConnection))
            {
                npgsqlConnection.Open();
                int ContactId =0;
                //ConstructorCreatedOn contact = new ConstructorCreatedOn(Contacts.CreatedOn);
                DateTime createdOn = Contacts.CreatedOn == default ? DateTime.UtcNow : Contacts.CreatedOn;

                using (var cmd = new NpgsqlCommand("CALL CreateContact(@p_ContactName, @p_Relation, @p_Gender, @p_IsDeleted, @p_CreatedOn, @rv_ContactId);", npgsqlConnection))
                {
                    cmd.CommandType = CommandType.Text; 
                    cmd.Parameters.Add("@p_ContactName",NpgsqlDbType.Varchar).Value = Contacts.ContactName;
                    cmd.Parameters.Add("@p_Gender",NpgsqlDbType.Varchar).Value=Contacts.Gender;
                    cmd.Parameters.Add("@p_Relation",NpgsqlDbType.Varchar).Value=Contacts.Relation;
                    cmd.Parameters.Add("@p_IsDeleted",NpgsqlDbType.Boolean).Value=Contacts.IsDeleted;
                    cmd.Parameters.Add("@p_CreatedOn", NpgsqlDbType.TimestampTz).Value = createdOn;


                    var id =new NpgsqlParameter("@rv_ContactId",NpgsqlDbType.Integer)
                    {
                        Direction=ParameterDirection.InputOutput
                        ,Value=DBNull.Value
                        
                    };
                    cmd.Parameters.Add(id);
                    cmd.ExecuteNonQuery();
                    ContactId=id.Value != DBNull.Value ? Convert.ToInt32(id.Value) :0;
                }
                foreach (var t in  Contacts.Alternewcontact )
                {
                    //ConstructorCreatedOn Phone = new ConstructorCreatedOn(t.CreatedOn);
                    DateTime phoneCreatedOn = t.CreatedOn == default ? DateTime.UtcNow : t.CreatedOn;

                    using(NpgsqlCommand cmd=new NpgsqlCommand(@"CreateNumber(@p_ContactId,@p_PhoneNumber,@p_CreatedOn,@p_IsDeleted)",npgsqlConnection))
                    {
                        cmd.CommandType=CommandType.Text;
                        cmd.Parameters.Add("@p_ContactId",NpgsqlDbType.Integer).Value=Contacts.ContactId;
                        cmd.Parameters.Add("@p_PhoneNumber",NpgsqlDbType.Varchar).Value=t.PhoneNumber;
                        cmd.Parameters.Add("@p_IsDeleted",NpgsqlDbType.Boolean).Value=t.IsDeleted;
                        cmd.Parameters.Add("@p_CreatedOn",NpgsqlDbType.TimestampTz).Value=phoneCreatedOn;
                        // cmd.Parameters.Add("@p_CreatedOn", NpgsqlTypes.NpgsqlDbType.Timestamp)
                        // .Value = DateTime.SpecifyKind(t.CreatedOn == default(DateTime) ? DateTime.UtcNow : t.CreatedOn, DateTimeKind.Unspecified);
                         cmd.ExecuteNonQuery();
                    }
                }
                npgsqlConnection.Close();


            }

            return Ok("contact added successfully");
        }
        [HttpGet("Get")]
        public IActionResult Getdetails(int id)
        {

            string? pgsqlConnection = _configuration.GetConnectionString("DefaultConnection");
            using (NpgsqlConnection npgsqlConnection=new NpgsqlConnection(pgsqlConnection)) 
            {
                npgsqlConnection.Open();

                using (NpgsqlCommand cmd =new NpgsqlCommand(@"Call GetContact(:p_ContactId,:p_ContactName)",npgsqlConnection))
                {
                  cmd.CommandType=CommandType.Text;
                  cmd.Parameters.Add("p_ContactId",NpgsqlDbType.Integer).Value=id ; 
                  var Contactname=new NpgsqlParameter("p_ContactName",NpgsqlDbType.Varchar)
                  {
                    Direction=ParameterDirection.InputOutput,
                    Value=DBNull.Value
                  };
                  cmd.Parameters.Add(Contactname);
                  cmd.ExecuteNonQuery();
                  string contactName = Contactname.Value?.ToString() ?? "notdefined";/////////
                }
                using (NpgsqlTransaction transaction=npgsqlConnection.BeginTransaction())
                {
                    using (NpgsqlCommand cmd =new NpgsqlCommand(@"call GetNumber(@p_ContactId,@ref_cursor)",npgsqlConnection,transaction))
                    {
                        cmd.Parameters.Add("p_ContactId",NpgsqlDbType.Integer).Value=id;
                        var Contact=new NpgsqlParameter("ref_cursor",NpgsqlDbType.Refcursor)
                        {
                            Direction=ParameterDirection.InputOutput,
                            Value="getcontact"
                        };
                        cmd.Parameters.Add(Contact);
                        cmd.ExecuteNonQuery();
                        cmd.CommandText="Fetch all in getcontact";
                        cmd.CommandType=CommandType.Text;
                        NpgsqlDataReader Details=cmd.ExecuteReader();
                        var resultcontact=new List<Alternewcontactresponse>();
                        while (Details.Read())
                        {
                             resultcontact.Add(new Alternewcontactresponse
                             {
                                NumberId=Details.GetInt32(0),
                                PhoneNumber=Details.GetString(1)

                             });  

                        }
                        Details.Close();



                        
                    }
                }
            }
            return Ok();
        }
    }
}
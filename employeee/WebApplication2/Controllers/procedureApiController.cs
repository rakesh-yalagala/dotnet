using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebApplication2.Models;

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
        public IActionResult createcontact([FromBody] NewContacts Contacts)
        {
            string? pgsqlConnection = _configuration.GetConnectionString("DefaultConnection");
            using (NpgsqlConnection npgsqlConnection=new NpgsqlConnection(pgsqlConnection))
            {
                npgsqlConnection.Open();
                //int contact_id;
                using (NpgsqlCommand cmd=new NpgsqlCommand(@"call create_contact(:p_contact_name,:p_Number,:p_Relation,:p_Gender,:p_IsDeleted,:p_CreatedOn,v_Contact_id)",npgsqlConnection))
                {
                    cmd.CommandType = CommandType.Text; 
                    cmd.Parameters.Add("p_Contact_name",NpgsqlTypes.NpgsqlDbType.Varchar).Value = Contacts.ContactName;
                    cmd.Parameters.Add("p_Gender",NpgsqlTypes.NpgsqlDbType.Varchar).Value=Contacts.Gender;
                    cmd.Parameters.Add("p_Relation",NpgsqlTypes.NpgsqlDbType.Varchar).Value=Contacts.Relation;
                    cmd.Parameters.Add("p_IsDeleted",NpgsqlTypes.NpgsqlDbType.Boolean).Value=Contacts.IsDeleted;
                    cmd.Parameters.Add("p_CreatedOn",NpgsqlTypes.NpgsqlDbType.Timestamp).Value=Contacts.CreatedOn;
                    var id =new NpgsqlParameter("v_Contact_id",NpgsqlTypes.NpgsqlDbType.Integer)
                    {
                        Direction=ParameterDirection.InputOutput,Value=DBNull.Value
                    };
                    cmd.Parameters.Add(id);
                    cmd.ExecuteNonQuery();
                }


            }

            return Ok();
        }
    }
}
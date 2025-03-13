using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto.Generators;
using BCrypt;
using Microsoft.AspNetCore.Identity.Data;
using WebApplication1;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

// Configurăm CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll"); 
app.UseAuthorization();

app.MapControllers(); 

app.Run(); 

namespace WebApplication2
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        [HttpPost]
        public IActionResult CreateClient([FromBody] Client client)
        {
            MySqlConnection conn = Connection.GetConn();
            conn.Open();
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }
            string query = "INSERT into users(first_name, last_name, email,password) VALUES(@first_name, @last_name, @email, @password)";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@first_name", client.firstName);
            cmd.Parameters.AddWithValue("@last_name", client.lastName);
            cmd.Parameters.AddWithValue("@email", client.email);
            cmd.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.HashPassword(client.password, 13));
            int l = cmd.ExecuteNonQuery();
            conn.Close();
            return Ok(client);
        }

        [HttpPost("login")]
        public IActionResult LoginClient([FromBody] LoginModel login)
        {
            MySqlConnection conn = Connection.GetConn();
            conn.Open();
            string query = "SELECT * FROM users WHERE email = @email";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", login.email);
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    if (!BCrypt.Net.BCrypt.Verify(login.password, reader["password"].ToString()))
                    {
                        return Unauthorized("Incorrect password");
                    }

                    var client = new Client
                    {
                        firstName = reader["first_name"].ToString(),
                        lastName = reader["last_name"].ToString(),
                        email = reader["email"].ToString(),
                        password = reader["password"].ToString()
                    };
                    return Ok(client);
                }
                else
                {
                    return Unauthorized("Incorrect email");
                }
            }
            
        }
    }

    public class Client
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        
    }

    public class LoginModel
    {
        public string email { get; set; }
        public string password { get; set; }
    }
}
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto.Generators;
using BCrypt;
using Microsoft.AspNetCore.Identity.Data;
using Mysqlx;
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

    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetProducts()
        {
            var products = new List<Product>();
            MySqlConnection conn = Connection.GetConn();
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM products", conn);
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string productName = reader["product_name"].ToString();
                    string productDescription = reader["description"].ToString();
                    string price = reader["price"].ToString();
                    string catergory = reader["category"].ToString();
                    byte[] byteImage = (byte[])reader["product_image"];
                    string image = Convert.ToBase64String(byteImage);
                    var product = new Product(productName, productDescription, price, image,catergory);
                    products.Add(product);
                }
            }
            return Ok(products);
        }
        [HttpPost("upload")]
        public async Task<IActionResult> upload([FromForm] ProductUpload productUpload)
        {
            MySqlConnection conn = Connection.GetConn();
            conn.Open();
            using var memoryStream = new MemoryStream();
            await productUpload.image.CopyToAsync(memoryStream);
            byte[] image = memoryStream.ToArray();
            
            string query = "INSERT into products(product_name, description, price,product_image,category) VALUES(@product_name, @description, @price, @product_image,@category)";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@product_name", productUpload.productName);
            cmd.Parameters.AddWithValue("@description", productUpload.productDescription);
            cmd.Parameters.AddWithValue("@price", productUpload.price);
            cmd.Parameters.AddWithValue("@product_image", image);
            cmd.Parameters.AddWithValue("@category", productUpload.category);
            
            int l = cmd.ExecuteNonQuery();
            return Ok(new { message = "Produsul a fost incarcat cu succes!" });
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

    public class Product
    {
        public string productName { get; set; }
        public string productDescription { get; set; }
        public string price { get; set; }
        public string productImage { get; set; }
        public string category { get; set; }

        public Product(string productName, string productDescription, string price, string productImage,string category)
        {
            this.productName = productName;
            this.productDescription = productDescription;
            this.price = price;
            this.productImage = productImage;
            this.category = category;
        }
    }

    public class ProductUpload
    {
        public string productName { get; set; }
        public string productDescription { get; set; }
        public string price { get; set; }
        public IFormFile image { get; set; }
        public string category { get; set; }
    }

}
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using MySql.Data.MySqlClient;

using WebApplication1;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

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
            string query = "SELECT email FROM users";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (client.email == reader["email"].ToString())
                    {
                        return Unauthorized("Exista un utilizator cu aceasta adresa de email!");
                    }
                }
            }
            string query1 = "INSERT into users(first_name, last_name, email,password) VALUES(@first_name, @last_name, @email, @password)";
            MySqlCommand cmd1 = new MySqlCommand(query1, conn);
            cmd1.Parameters.AddWithValue("@first_name", client.firstName);
            cmd1.Parameters.AddWithValue("@last_name", client.lastName);
            cmd1.Parameters.AddWithValue("@email", client.email);
            cmd1.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.HashPassword(client.password, 13));
            int l = cmd1.ExecuteNonQuery();
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
                    string category = reader["category"].ToString();
                    byte[] byteImage = (byte[])reader["product_image"];
                    string stock = reader["stock"].ToString();
                    string rating = reader["rating"].ToString();
                    string image = Convert.ToBase64String(byteImage);
                    var product = new Product(productName, productDescription, price, image, category,rating,stock);
                    products.Add(product);
                }
            }

            return Ok(products);
        }

        [HttpPost("cart")]
        public IActionResult AddToCart([FromForm] ProductAndClient pac)
        {
            int clientId = 0;
            int productId = 0;
            MySqlConnection conn = Connection.GetConn();
            conn.Open();
            string query = "SELECT id FROM users WHERE email = @email";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", pac.clientEmail);
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    clientId = reader.GetInt32("id");
                }
            }

            string query1 = "SELECT id FROM products WHERE product_name = @product_name";
            MySqlCommand cmd1 = new MySqlCommand(query1, conn);
            cmd1.Parameters.AddWithValue("@product_name", pac.productName);
            using (MySqlDataReader reader = cmd1.ExecuteReader())
            {
                if (reader.Read())
                {
                    productId = reader.GetInt32("id");
                }
            }

            string query2 = @"INSERT INTO shopping_cart(user_id, product_id, quantity)
                  VALUES(@user_id, @product_id, @quantity)
                  ON DUPLICATE KEY UPDATE quantity = quantity + @quantity";
            MySqlCommand cmd2 = new MySqlCommand(query2, conn);
            cmd2.Parameters.AddWithValue("@user_id", clientId);
            cmd2.Parameters.AddWithValue("@product_id", productId);
            cmd2.Parameters.AddWithValue("@quantity", pac.quantity);
            int l = cmd2.ExecuteNonQuery();
            conn.Close();
            return Ok(new { message = "Produsul a fost adaugat in cos cu succes!" });
        }

        [HttpPost("cartelements")]
        public IActionResult GetCartElememts([FromForm] string email)
        {
            int clientId = 0;
            MySqlConnection conn = Connection.GetConn();
            conn.Open();
            var products = new List<ProductCart>();
            string query = "SELECT id FROM users WHERE email = @email";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", email);
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    clientId = reader.GetInt32("id");
                }
            }

            if (clientId == 0)
            {
                conn.Close();
                return BadRequest(new { message = "Utilizatorul nu a fost găsit." });
            }

            string query1 =
                "SELECT p.*, sc.quantity FROM shopping_cart sc JOIN products p ON sc.product_id = p.id WHERE user_id = @user_id";
            MySqlCommand cmd1 = new MySqlCommand(query1, conn);
            cmd1.Parameters.AddWithValue("@user_id", clientId);
            using (MySqlDataReader reader = cmd1.ExecuteReader())
            {
                while (reader.Read())
                {
                    string productName = reader["product_name"].ToString();
                    string productDescription = reader["description"].ToString();
                    string price = reader["price"].ToString();
                    string catergory = reader["category"].ToString();
                    int quantity = reader.GetInt32("quantity");
                    byte[] byteImage = (byte[])reader["product_image"];
                    string image = Convert.ToBase64String(byteImage);
                    var product = new ProductCart(productName, productDescription, price, image, catergory, quantity);
                    products.Add(product);
                }
            }

            conn.Close();
            return Ok(products);
        }

        [HttpPost("changequantity")]
        public IActionResult UpdateQuantity([FromForm] ProductAndClient pac)
        {
            MySqlConnection conn = Connection.GetConn();
            conn.Open();
            int clientId = 0;
            int productId = 0;
            string query = "SELECT id FROM users WHERE email = @email";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", pac.clientEmail);
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    clientId = reader.GetInt32("id");
                }
            }
            string query1 = "SElECT id FROM products WHERE product_name = @product_name";
            MySqlCommand cmd1 = new MySqlCommand(query1, conn);
            cmd1.Parameters.AddWithValue("@product_name", pac.productName);
            using (MySqlDataReader reader = cmd1.ExecuteReader())
            {
                if (reader.Read())
                {
                    productId = reader.GetInt32("id");
                }
            }

            if (pac.quantity > 0)
            {
                string query2 = "UPDATE shopping_cart SET quantity = @quantity WHERE user_id = @user_id AND product_id = @product_id";
                MySqlCommand cmd2 = new MySqlCommand(query2, conn);
                cmd2.Parameters.AddWithValue("@user_id", clientId);
                cmd2.Parameters.AddWithValue("@product_id", productId);
                cmd2.Parameters.AddWithValue("@quantity", pac.quantity);
                int l = cmd2.ExecuteNonQuery();
            }

            if (pac.quantity == 0)
            {
                string query3 = "DELETE FROM shopping_cart  WHERE user_id = @user_id AND product_id = @product_id";
                MySqlCommand cmd3 = new MySqlCommand(query3, conn);
                cmd3.Parameters.AddWithValue("@user_id", clientId);
                cmd3.Parameters.AddWithValue("@product_id", productId);
                cmd3.Parameters.AddWithValue("@quantity", pac.quantity);
                int l = cmd3.ExecuteNonQuery();
            }
            conn.Close();
            return Ok(505);
        }
    

    [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] ProductUpload productUpload)
        {
            MySqlConnection conn = Connection.GetConn();
            conn.Open();
            using var memoryStream = new MemoryStream();
            await productUpload.image.CopyToAsync(memoryStream);
            byte[] image = memoryStream.ToArray();
            
            string query = "INSERT into products(product_name, description, price,product_image,,rating,stock) VALUES(@product_name, @description, @price, @product_image,@category)";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@product_name", productUpload.productName);
            cmd.Parameters.AddWithValue("@description", productUpload.productDescription);
            cmd.Parameters.AddWithValue("@price", productUpload.price);
            cmd.Parameters.AddWithValue("@product_image", image);
            cmd.Parameters.AddWithValue("@category", productUpload.category);
            cmd.Parameters.AddWithValue("@rating", 0);
            cmd.Parameters.AddWithValue("@stock", productUpload.stock);
            
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
        
        public string rating { get; set; }
        
        public string stock { get; set; }

        public Product(string productName, string productDescription, string price, string productImage,string category, string rating,string stock)
        {
            this.productName = productName;
            this.productDescription = productDescription;
            this.price = price;
            this.productImage = productImage;
            this.category = category;
            this.rating = rating;
            this.stock = stock;
        }
    }
    public class ProductCart
    {
        public string productName { get; set; }
        public string productDescription { get; set; }
        public string price { get; set; }
        public string productImage { get; set; }
        public string category { get; set; }
        public int quantity { get; set; }

        public ProductCart(string productName, string productDescription, string price, string productImage,string category,int quantity)
        {
            this.productName = productName;
            this.productDescription = productDescription;
            this.price = price;
            this.productImage = productImage;
            this.category = category;
            this.quantity = quantity;
        }
    }

    public class ProductUpload
    {
        public string productName { get; set; }
        public string productDescription { get; set; }
        public string price { get; set; }
        public IFormFile image { get; set; }
        public string category { get; set; }
        
        public string rating { get; set; }
        
        public string stock { get; set; }
    }

    public class ProductAndClient
    {
        public string clientEmail { get; set; }
        public string productName { get; set; }
        public int quantity { get; set; }
    }

}
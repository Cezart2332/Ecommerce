using MySql.Data.MySqlClient;
namespace WebApplication1
{
    public class Connection
    {
        private static MySqlConnection connection;
        private static string server = "127.0.0.1;";
        private static string database = "ecommerce;";
        private static string Uid = "root;";
        private static string password = "Cezarica1@;";

        public static MySqlConnection GetConn()
        {
            connection = new MySqlConnection($"server={server} database={database} Uid={Uid} password={password}");
            return connection;
        }
    }
}
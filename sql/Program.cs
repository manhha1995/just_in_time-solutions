using System;
using System.Data.SqlClient;
namespace sql
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var client = new SqlConnection("ConnectionString"))
            {
                client.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM DATABASE", client);
            }
        }
    }
}

using Backend.Entities;
using System.Data.SqlClient;

namespace Backend.Data
{
    public class DataBaseHelper
    {
        private readonly string connectionstring;
        public DataBaseHelper(IConfiguration _configuration)
        {
            connectionstring = _configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<(bool success, string? errorMessage)> ToRegisterUser(User user)
        {
            using (var con = new SqlConnection(connectionstring))
            {
                await con.OpenAsync();
                int count;
                string checkquery = "Select count(*) from Users Where Username = @Username";
                using (var checkcmd = new SqlCommand(checkquery, con))
                {
                    checkcmd.Parameters.AddWithValue("@Username", user.Username);
                    count = (int)checkcmd.ExecuteScalar(); //check if the user already exsist or not
                }
                if (count > 0) { return (false, "User Already Exsist"); }
                else
                {
                    string query = "Insert into Users (Id, Username, PasswordHash, Role) Values (@Id, @Username, @PasswordHash, @Role)";
                    using (var cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", user.Id);
                        cmd.Parameters.AddWithValue("@Username", user.Username);
                        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                        cmd.Parameters.AddWithValue("@Role", user.Role);
                        int rowAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowAffected > 0) return (true, "User Register Successfully");
                        else { return (false, "Failed to Add User To the DataBase"); }
                    }
                }
            }
        }

        public User? GetUserByUsername(string Username)
        {
            using (var con = new SqlConnection(connectionstring))
            {
                con.Open();
                string query = "Select Id, Username, PasswordHash, Role From Users Where Username = @Username";

                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Username", Username);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = reader.GetGuid(0),
                                Username = reader.GetString(1),
                                PasswordHash = reader.GetString(2),
                                Role = reader.GetString(3)
                            };
                        }
                    }
                }
                return null;
            }
        }
    }
}

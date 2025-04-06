using Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace Backend.Controllers
{
    [Route("api/employee")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly string _connectionString;

        public EmployeeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection"); // Get your connection string
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployee()
        {
            List<EmployeeModel> Employees = new List<EmployeeModel>();
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    await con.OpenAsync();
                    string query = "SELECT * FROM Employees";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Employees.Add(new EmployeeModel
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    FatherName = reader.GetString(2),
                                    ContactNo = reader.GetString(3),
                                    Designation = reader.GetString(4),
                                    DateOfJoining = reader.GetDateTime(5)
                                });
                            }
                        }
                    }

                }
                return Ok(Employees);

            }
            catch (Exception ex)
            {
                return BadRequest(new {message = "Failed to Get Data From API", ex.Message});
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeModel employee)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();
                string query = "UPDATE Employees SET Name = @Name, FatherName = @FatherName, ContactNo = @ContactNo, Designation = @Designation, DateOfJoining = @DateOfJoining WHERE Id = @id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Name", employee.Name);
                    cmd.Parameters.AddWithValue("@FatherName", employee.FatherName);
                    cmd.Parameters.AddWithValue("@ContactNo", employee.ContactNo);
                    cmd.Parameters.AddWithValue("@Designation", employee.Designation);
                    cmd.Parameters.AddWithValue("@DateOfJoining", employee.DateOfJoining);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0 ? Ok("Employee Updated SuccessFully") : NotFound("Employee Not Found");
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    await con.OpenAsync();
                    string query = "DELETE FROM Employees WHERE Id = @id";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0) return Ok(new { message = "Employee Deleted Successfully" });
                        else return BadRequest();
                    }
                }
            }catch(Exception ex)
            {
                return BadRequest(new { message = "Failed To delete Employee" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee([FromBody] EmployeeModel employee)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    await con.OpenAsync();
                    string query = "INSERT INTO Employees (Name, FatherName, ContactNo, Designation, DateOfJoining) VALUES (@name, @fatherName, @contactNo, @designation, @dateOfJoining)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@name", employee.Name);
                        cmd.Parameters.AddWithValue("@fatherName", employee.FatherName);
                        cmd.Parameters.AddWithValue("@contactNo", employee.ContactNo);
                        cmd.Parameters.AddWithValue("@designation", employee.Designation);
                        cmd.Parameters.AddWithValue("@dateOfJoining", employee.DateOfJoining);

                        int rowAffected = cmd.ExecuteNonQuery();
                        if (rowAffected > 0) return Ok(new { message = "Employee added Successfully" });
                        else return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error Adding Employee" });
            }
        }
    }
}

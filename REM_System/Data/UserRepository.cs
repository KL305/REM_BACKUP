using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace REM_System.Data
{
    internal sealed class UserRepository
    {
        public bool UsernameExists(string username)
        {
            using (var connection = new SqlConnection(Database.ConnectionString))
            using (var command = new SqlCommand("SELECT COUNT(1) FROM [dbo].[Users] WHERE [Username] = @Username", connection))
            {
                command.Parameters.Add("@Username", SqlDbType.NVarChar, 100).Value = username;
                connection.Open();
                var count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }

        public bool EmailExists(string email)
        {
            using (var connection = new SqlConnection(Database.ConnectionString))
            using (var command = new SqlCommand("SELECT COUNT(1) FROM [dbo].[Users] WHERE [Email] = @Email", connection))
            {
                command.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = email;
                connection.Open();
                var count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }

        public int CreateUser(string username, string password, string email, string userRole)
        {
            using (var connection = new SqlConnection(Database.ConnectionString))
            using (var command = new SqlCommand(@"INSERT INTO [dbo].[Users] ([Username], [Password], [Email], [UserRole])
VALUES (@Username, @Password, @Email, @UserRole);
SELECT CAST(SCOPE_IDENTITY() as int);", connection))
            {
                command.Parameters.Add("@Username", SqlDbType.NVarChar, 100).Value = username;
                command.Parameters.Add("@Password", SqlDbType.NVarChar, 256).Value = password;
                command.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = email;
                command.Parameters.Add("@UserRole", SqlDbType.NVarChar, 50).Value = string.IsNullOrWhiteSpace(userRole) ? "User" : userRole;

                connection.Open();
                var userId = (int)command.ExecuteScalar();
                return userId;
            }
        }

        public bool ValidateCredentials(string username, string password, out string userRole)
        {
            userRole = string.Empty;
            using (var connection = new SqlConnection(Database.ConnectionString))
            using (var command = new SqlCommand(@"SELECT [UserRole] FROM [dbo].[Users] WHERE [Username] = @Username AND [Password] = @Password", connection))
            {
                command.Parameters.Add("@Username", SqlDbType.NVarChar, 100).Value = username;
                command.Parameters.Add("@Password", SqlDbType.NVarChar, 256).Value = password;
                connection.Open();
                var result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    userRole = Convert.ToString(result);
                    return true;
                }
                return false;
            }
        }

        public bool ValidateCredentials(string username, string password, out string userRole, out int userId)
        {
            userRole = string.Empty;
            userId = 0;
            try
            {
                using (var connection = new SqlConnection(Database.ConnectionString))
                using (var command = new SqlCommand(@"SELECT [UserId], [UserRole] FROM [dbo].[Users] WHERE [Username] = @Username AND [Password] = @Password", connection))
                {
                    command.Parameters.Add("@Username", SqlDbType.NVarChar, 100).Value = username;
                    command.Parameters.Add("@Password", SqlDbType.NVarChar, 256).Value = password;
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Use safer conversion methods with null checks
                            userId = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader[0]);
                            userRole = reader.IsDBNull(1) ? string.Empty : Convert.ToString(reader[1]) ?? string.Empty;
                            return true;
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 208) // Invalid object name
                {
                    return false;
                }
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ValidateCredentials: {ex.Message}");
                throw;
            }
            return false;
        }

        public List<UserViewModel> GetAllUsers()
        {
            var users = new List<UserViewModel>();
            try
            {
                var connectionString = Database.ConnectionString;
                if (string.IsNullOrEmpty(connectionString))
                {
                    System.Diagnostics.Debug.WriteLine("Database connection string is null or empty.");
                    return users;
                }

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(@"SELECT [UserId], [Username], [Email], [UserRole] 
                    FROM [dbo].[Users] 
                    ORDER BY [UserRole], [Username]", connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                users.Add(new UserViewModel
                                {
                                    UserId = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader[0]),
                                    Username = reader.IsDBNull(1) ? string.Empty : Convert.ToString(reader[1]) ?? string.Empty,
                                    Email = reader.IsDBNull(2) ? string.Empty : Convert.ToString(reader[2]) ?? string.Empty,
                                    UserRole = reader.IsDBNull(3) ? string.Empty : Convert.ToString(reader[3]) ?? string.Empty
                                });
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error reading user row: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 208) // Invalid object name
                {
                    return users; // Return empty list
                }
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetAllUsers: {ex.Message}");
                throw;
            }
            return users;
        }

        public bool UpdateUser(int userId, string username, string email, string userRole, string password = null)
        {
            try
            {
                using (var connection = new SqlConnection(Database.ConnectionString))
                {
                    string query;
                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        query = @"UPDATE [dbo].[Users] 
                            SET [Username] = @Username, [Email] = @Email, [UserRole] = @UserRole, [Password] = @Password
                            WHERE [UserId] = @UserId";
                    }
                    else
                    {
                        query = @"UPDATE [dbo].[Users] 
                            SET [Username] = @Username, [Email] = @Email, [UserRole] = @UserRole
                            WHERE [UserId] = @UserId";
                    }

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                        command.Parameters.Add("@Username", SqlDbType.NVarChar, 100).Value = username;
                        command.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = email;
                        command.Parameters.Add("@UserRole", SqlDbType.NVarChar, 50).Value = userRole;
                        if (!string.IsNullOrWhiteSpace(password))
                        {
                            command.Parameters.Add("@Password", SqlDbType.NVarChar, 256).Value = password;
                        }

                        connection.Open();
                        var rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateUser: {ex.Message}");
                throw;
            }
        }

        public bool DeleteUser(int userId)
        {
            try
            {
                using (var connection = new SqlConnection(Database.ConnectionString))
                using (var command = new SqlCommand(@"DELETE FROM [dbo].[Users] WHERE [UserId] = @UserId", connection))
                {
                    command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    connection.Open();
                    var rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DeleteUser: {ex.Message}");
                throw;
            }
        }

        public UserViewModel GetUserById(int userId)
        {
            try
            {
                using (var connection = new SqlConnection(Database.ConnectionString))
                using (var command = new SqlCommand(@"SELECT [UserId], [Username], [Email], [UserRole] 
                    FROM [dbo].[Users] WHERE [UserId] = @UserId", connection))
                {
                    command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserViewModel
                            {
                                UserId = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader[0]),
                                Username = reader.IsDBNull(1) ? string.Empty : Convert.ToString(reader[1]) ?? string.Empty,
                                Email = reader.IsDBNull(2) ? string.Empty : Convert.ToString(reader[2]) ?? string.Empty,
                                UserRole = reader.IsDBNull(3) ? string.Empty : Convert.ToString(reader[3]) ?? string.Empty
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetUserById: {ex.Message}");
                throw;
            }
            return null;
        }
    }
}



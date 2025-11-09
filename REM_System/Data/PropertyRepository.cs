using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace REM_System.Data
{
    internal sealed class PropertyRepository
    {
        public int CreateProperty(Property property)
        {
            using (var connection = new SqlConnection(Database.ConnectionString))
            using (var command = new SqlCommand(@"INSERT INTO [dbo].[Properties] 
                ([SellerId], [Title], [Description], [PropertyType], [Address], [Price], [Bedrooms], [Bathrooms], [Area], [Status], [CreatedDate])
                VALUES (@SellerId, @Title, @Description, @PropertyType, @Address, @Price, @Bedrooms, @Bathrooms, @Area, @Status, @CreatedDate);
                SELECT CAST(SCOPE_IDENTITY() as int);", connection))
            {
                command.Parameters.Add("@SellerId", SqlDbType.Int).Value = property.SellerId;
                command.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = property.Title;
                command.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = property.Description ?? (object)DBNull.Value;
                command.Parameters.Add("@PropertyType", SqlDbType.NVarChar, 50).Value = property.PropertyType;
                command.Parameters.Add("@Address", SqlDbType.NVarChar, 500).Value = property.Address ?? (object)DBNull.Value;
                command.Parameters.Add("@Price", SqlDbType.Decimal).Value = property.Price;
                command.Parameters.Add("@Bedrooms", SqlDbType.Int).Value = property.Bedrooms ?? (object)DBNull.Value;
                command.Parameters.Add("@Bathrooms", SqlDbType.Int).Value = property.Bathrooms ?? (object)DBNull.Value;
                command.Parameters.Add("@Area", SqlDbType.Decimal).Value = property.Area ?? (object)DBNull.Value;
                command.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = property.Status;
                command.Parameters.Add("@CreatedDate", SqlDbType.DateTime).Value = DateTime.Now;

                connection.Open();
                var propertyId = (int)command.ExecuteScalar();
                return propertyId;
            }
        }

        public List<Property> GetPropertiesBySellerId(int sellerId)
        {
            var properties = new List<Property>();
            try
            {
                var connectionString = Database.ConnectionString;
                if (string.IsNullOrEmpty(connectionString))
                {
                    System.Diagnostics.Debug.WriteLine("Database connection string is null or empty.");
                    return properties;
                }

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(@"SELECT [PropertyId], [SellerId], [Title], [Description], [PropertyType], 
                    [Address], [Price], [Bedrooms], [Bathrooms], [Area], [Status], [CreatedDate], [ModifiedDate]
                    FROM [dbo].[Properties] WHERE [SellerId] = @SellerId ORDER BY [CreatedDate] DESC", connection))
                {
                    command.Parameters.Add("@SellerId", SqlDbType.Int).Value = sellerId;
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                properties.Add(new Property
                                {
                                    PropertyId = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader[0]),
                                    SellerId = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader[1]),
                                    Title = reader.IsDBNull(2) ? string.Empty : Convert.ToString(reader[2]) ?? string.Empty,
                                    Description = reader.IsDBNull(3) ? string.Empty : Convert.ToString(reader[3]) ?? string.Empty,
                                    PropertyType = reader.IsDBNull(4) ? string.Empty : Convert.ToString(reader[4]) ?? string.Empty,
                                    Address = reader.IsDBNull(5) ? string.Empty : Convert.ToString(reader[5]) ?? string.Empty,
                                    Price = reader.IsDBNull(6) ? 0 : Convert.ToDecimal(reader[6]),
                                    Bedrooms = reader.IsDBNull(7) ? (int?)null : Convert.ToInt32(reader[7]),
                                    Bathrooms = reader.IsDBNull(8) ? (int?)null : Convert.ToInt32(reader[8]),
                                    Area = reader.IsDBNull(9) ? (decimal?)null : Convert.ToDecimal(reader[9]),
                                    Status = reader.IsDBNull(10) ? "Available" : Convert.ToString(reader[10]) ?? "Available",
                                    CreatedDate = reader.IsDBNull(11) ? DateTime.Now : Convert.ToDateTime(reader[11]),
                                    ModifiedDate = reader.IsDBNull(12) ? (DateTime?)null : Convert.ToDateTime(reader[12])
                                });
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error reading property row: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 208) // Invalid object name
                {
                    return properties; // Return empty list
                }
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetPropertiesBySellerId: {ex.Message}");
                throw;
            }
            return properties;
        }

        public bool UpdateProperty(Property property)
        {
            using (var connection = new SqlConnection(Database.ConnectionString))
            using (var command = new SqlCommand(@"UPDATE [dbo].[Properties] 
                SET [Title] = @Title, [Description] = @Description, [PropertyType] = @PropertyType, 
                [Address] = @Address, [Price] = @Price, [Bedrooms] = @Bedrooms, [Bathrooms] = @Bathrooms, 
                [Area] = @Area, [Status] = @Status, [ModifiedDate] = @ModifiedDate
                WHERE [PropertyId] = @PropertyId AND [SellerId] = @SellerId", connection))
            {
                command.Parameters.Add("@PropertyId", SqlDbType.Int).Value = property.PropertyId;
                command.Parameters.Add("@SellerId", SqlDbType.Int).Value = property.SellerId;
                command.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = property.Title;
                command.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = property.Description ?? (object)DBNull.Value;
                command.Parameters.Add("@PropertyType", SqlDbType.NVarChar, 50).Value = property.PropertyType;
                command.Parameters.Add("@Address", SqlDbType.NVarChar, 500).Value = property.Address ?? (object)DBNull.Value;
                command.Parameters.Add("@Price", SqlDbType.Decimal).Value = property.Price;
                command.Parameters.Add("@Bedrooms", SqlDbType.Int).Value = property.Bedrooms ?? (object)DBNull.Value;
                command.Parameters.Add("@Bathrooms", SqlDbType.Int).Value = property.Bathrooms ?? (object)DBNull.Value;
                command.Parameters.Add("@Area", SqlDbType.Decimal).Value = property.Area ?? (object)DBNull.Value;
                command.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = property.Status;
                command.Parameters.Add("@ModifiedDate", SqlDbType.DateTime).Value = DateTime.Now;

                connection.Open();
                var rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        public bool DeleteProperty(int propertyId, int sellerId)
        {
            using (var connection = new SqlConnection(Database.ConnectionString))
            using (var command = new SqlCommand(@"DELETE FROM [dbo].[Properties] 
                WHERE [PropertyId] = @PropertyId AND [SellerId] = @SellerId", connection))
            {
                command.Parameters.Add("@PropertyId", SqlDbType.Int).Value = propertyId;
                command.Parameters.Add("@SellerId", SqlDbType.Int).Value = sellerId;

                connection.Open();
                var rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        public List<PropertyViewModel> GetAllPropertiesWithSeller()
        {
            // Get all properties (both RealEstate and Product) with seller name for buyer dashboard
            var properties = new List<PropertyViewModel>();
            try
            {
                var connectionString = Database.ConnectionString;
                if (string.IsNullOrEmpty(connectionString))
                {
                    System.Diagnostics.Debug.WriteLine("Database connection string is null or empty.");
                    return properties;
                }

                string query = @"SELECT p.[PropertyId], p.[SellerId], u.[Username] AS SellerName, p.[Title], p.[Description], 
                    p.[PropertyType], p.[Address], p.[Price], p.[Bedrooms], p.[Bathrooms], p.[Area], p.[Status], p.[CreatedDate]
                    FROM [dbo].[Properties] p
                    INNER JOIN [dbo].[Users] u ON p.[SellerId] = u.[UserId]
                    ORDER BY p.[CreatedDate] DESC";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                properties.Add(new PropertyViewModel
                                {
                                    PropertyId = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader[0]),
                                    SellerId = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader[1]),
                                    SellerName = reader.IsDBNull(2) ? string.Empty : Convert.ToString(reader[2]) ?? string.Empty,
                                    Title = reader.IsDBNull(3) ? string.Empty : Convert.ToString(reader[3]) ?? string.Empty,
                                    Description = reader.IsDBNull(4) ? string.Empty : Convert.ToString(reader[4]) ?? string.Empty,
                                    PropertyType = reader.IsDBNull(5) ? string.Empty : Convert.ToString(reader[5]) ?? string.Empty,
                                    Address = reader.IsDBNull(6) ? string.Empty : Convert.ToString(reader[6]) ?? string.Empty,
                                    Price = reader.IsDBNull(7) ? 0 : Convert.ToDecimal(reader[7]),
                                    Bedrooms = reader.IsDBNull(8) ? (int?)null : Convert.ToInt32(reader[8]),
                                    Bathrooms = reader.IsDBNull(9) ? (int?)null : Convert.ToInt32(reader[9]),
                                    Area = reader.IsDBNull(10) ? (decimal?)null : Convert.ToDecimal(reader[10]),
                                    Status = reader.IsDBNull(11) ? "Available" : Convert.ToString(reader[11]) ?? "Available",
                                    CreatedDate = reader.IsDBNull(12) ? DateTime.Now : Convert.ToDateTime(reader[12])
                                });
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error reading property row: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 208) // Invalid object name
                {
                    return properties; // Return empty list
                }
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetAllPropertiesWithSeller: {ex.Message}");
                throw;
            }
            return properties;
        }

        public Property GetPropertyById(int propertyId)
        {
            try
            {
                using (var connection = new SqlConnection(Database.ConnectionString))
                using (var command = new SqlCommand(@"SELECT [PropertyId], [SellerId], [Title], [Description], [PropertyType], 
                    [Address], [Price], [Bedrooms], [Bathrooms], [Area], [Status], [CreatedDate], [ModifiedDate]
                    FROM [dbo].[Properties] WHERE [PropertyId] = @PropertyId", connection))
                {
                    command.Parameters.Add("@PropertyId", SqlDbType.Int).Value = propertyId;
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Property
                            {
                                PropertyId = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader[0]),
                                SellerId = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader[1]),
                                Title = reader.IsDBNull(2) ? string.Empty : Convert.ToString(reader[2]) ?? string.Empty,
                                Description = reader.IsDBNull(3) ? string.Empty : Convert.ToString(reader[3]) ?? string.Empty,
                                PropertyType = reader.IsDBNull(4) ? string.Empty : Convert.ToString(reader[4]) ?? string.Empty,
                                Address = reader.IsDBNull(5) ? string.Empty : Convert.ToString(reader[5]) ?? string.Empty,
                                Price = reader.IsDBNull(6) ? 0 : Convert.ToDecimal(reader[6]),
                                Bedrooms = reader.IsDBNull(7) ? (int?)null : Convert.ToInt32(reader[7]),
                                Bathrooms = reader.IsDBNull(8) ? (int?)null : Convert.ToInt32(reader[8]),
                                Area = reader.IsDBNull(9) ? (decimal?)null : Convert.ToDecimal(reader[9]),
                                Status = reader.IsDBNull(10) ? "Available" : Convert.ToString(reader[10]) ?? "Available",
                                CreatedDate = reader.IsDBNull(11) ? DateTime.Now : Convert.ToDateTime(reader[11]),
                                ModifiedDate = reader.IsDBNull(12) ? (DateTime?)null : Convert.ToDateTime(reader[12])
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetPropertyById: {ex.Message}");
                throw;
            }
            return null;
        }

        public bool DeleteProperty(int propertyId)
        {
            // Admin version - doesn't require sellerId
            try
            {
                using (var connection = new SqlConnection(Database.ConnectionString))
                using (var command = new SqlCommand(@"DELETE FROM [dbo].[Properties] WHERE [PropertyId] = @PropertyId", connection))
                {
                    command.Parameters.Add("@PropertyId", SqlDbType.Int).Value = propertyId;
                    connection.Open();
                    var rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DeleteProperty: {ex.Message}");
                throw;
            }
        }

        public bool UpdatePropertyAdmin(Property property)
        {
            // Admin version - doesn't check sellerId
            try
            {
                using (var connection = new SqlConnection(Database.ConnectionString))
                using (var command = new SqlCommand(@"UPDATE [dbo].[Properties] 
                    SET [SellerId] = @SellerId, [Title] = @Title, [Description] = @Description, [PropertyType] = @PropertyType, 
                    [Address] = @Address, [Price] = @Price, [Bedrooms] = @Bedrooms, [Bathrooms] = @Bathrooms, 
                    [Area] = @Area, [Status] = @Status, [ModifiedDate] = @ModifiedDate
                    WHERE [PropertyId] = @PropertyId", connection))
                {
                    command.Parameters.Add("@PropertyId", SqlDbType.Int).Value = property.PropertyId;
                    command.Parameters.Add("@SellerId", SqlDbType.Int).Value = property.SellerId;
                    command.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = property.Title;
                    command.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = property.Description ?? (object)DBNull.Value;
                    command.Parameters.Add("@PropertyType", SqlDbType.NVarChar, 50).Value = property.PropertyType;
                    command.Parameters.Add("@Address", SqlDbType.NVarChar, 500).Value = property.Address ?? (object)DBNull.Value;
                    command.Parameters.Add("@Price", SqlDbType.Decimal).Value = property.Price;
                    command.Parameters.Add("@Bedrooms", SqlDbType.Int).Value = property.Bedrooms ?? (object)DBNull.Value;
                    command.Parameters.Add("@Bathrooms", SqlDbType.Int).Value = property.Bathrooms ?? (object)DBNull.Value;
                    command.Parameters.Add("@Area", SqlDbType.Decimal).Value = property.Area ?? (object)DBNull.Value;
                    command.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = property.Status;
                    command.Parameters.Add("@ModifiedDate", SqlDbType.DateTime).Value = DateTime.Now;

                    connection.Open();
                    var rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdatePropertyAdmin: {ex.Message}");
                throw;
            }
        }
    }
}


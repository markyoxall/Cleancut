using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace DXDevil.Data
{
    /// <summary>
    /// SQL Server implementation for retrieving order list items.
    /// </summary>
    public class SqlOrderRepository : IOrderRepository
    {
        private readonly string? _defaultConnection;

        /// <summary>
        /// Creates a new instance of <see cref="SqlOrderRepository"/>.
        /// </summary>
        /// <param name="defaultConnection">Optional default connection string to use if environment variable is not set.</param>
        public SqlOrderRepository(string? defaultConnection = null)
        {
            _defaultConnection = defaultConnection;
        }

        /// <inheritdoc />
        public async Task<List<OrderListGridItem>> GetOrdersAsync()
        {
            var conn = GetConnectionString();
            if (string.IsNullOrWhiteSpace(conn))
                throw new InvalidOperationException("Could not locate connection string. Ensure appsettings.json is present or environment variable CLEANCUT_DEFAULT_CONNECTION is set.");

            var list = new List<OrderListGridItem>();

            await using var connection = new SqlConnection(conn);
            await connection.OpenAsync();

            const string sql = @"SELECT o.Id, c.FirstName, c.LastName, o.OrderDate, o.Status, o.TotalAmount, o.CreatedAt, o.UpdatedAt
                                FROM Orders o
                                LEFT JOIN Customers c ON o.CustomerId = c.Id
                                ORDER BY o.OrderDate DESC";

            await using var cmd = new SqlCommand(sql, connection);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var id = reader.GetFieldValue<Guid>(0);
                var first = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                var last = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                var orderDate = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3);
                var statusInt = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                var total = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5);
                var updated = reader.IsDBNull(7) ? (DateTime?) null : reader.GetDateTime(7);

                list.Add(new OrderListGridItem
                {
                    Id = id,
                    CustomerName = string.IsNullOrWhiteSpace(first) && string.IsNullOrWhiteSpace(last) ? string.Empty : $"{first} {last}".Trim(),
                    OrderDate = orderDate,
                    Status = MapStatus(statusInt),
                    TotalAmount = total,
                    CreatedBy = string.Empty,
                    LastModified = updated,
                    IsActive = true
                });
            }

            return list;
        }

        private string? GetConnectionString()
        {
            // Environment override preserved for backward compatibility
            var env = Environment.GetEnvironmentVariable("CLEANCUT_DEFAULT_CONNECTION");
            if (!string.IsNullOrWhiteSpace(env))
                return env;

            return _defaultConnection;
        }

        private static string MapStatus(int s)
        {
            return s switch
            {
                0 => "Pending",
                1 => "Confirmed",
                2 => "Shipped",
                3 => "Delivered",
                4 => "Cancelled",
                _ => s.ToString()
            };
        }
    }
}

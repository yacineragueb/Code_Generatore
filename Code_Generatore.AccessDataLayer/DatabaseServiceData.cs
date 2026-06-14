using Microsoft.Data.SqlClient;
using System.Data;

namespace Code_Generatore.AccessDataLayer
{
    public static class DatabaseServiceData
    {
        public static List<string> GetDatabases(string connectionString)
        {
            List<string> databases = new List<string>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"SELECT name FROM sys.databases
                                 WHERE name NOT IN ('master', 'model', 'msdb', 'tempdb')
                                 ORDER BY name;";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        databases.Add(reader.GetString(0));
                    }
                }
            }

            return databases;
        }

        public static async Task<bool> TestConnectionAsync(string ConnectionString)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();

                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    
        public static List<string> GetAllTables(string connectionString, string databaseName)
        {
            List<string> tables = new List<string>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                conn.ChangeDatabase(databaseName);

                string query = @"SELECT TABLE_NAME
                                FROM INFORMATION_SCHEMA.TABLES
                                WHERE TABLE_TYPE = 'BASE TABLE' 
                                AND TABLE_SCHEMA = 'dbo'
                                AND TABLE_NAME NOT IN ('sysdiagrams')
                                ORDER BY TABLE_NAME;";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tables.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return tables;
        }

        public static List<ColumnInfo> GetTableColumns(string connectionString, string databaseName, string tableName)
        {
            List<ColumnInfo> columns = new List<ColumnInfo>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                            SELECT
                                c.name AS ColumnName,
                                t.name AS DataType,
                                c.is_nullable,
                                CASE 
                                    WHEN pk.column_id IS NOT NULL THEN 1
                                    ELSE 0
                                END AS IsPrimaryKey
                            FROM sys.columns c
                            INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                            INNER JOIN sys.tables tb ON c.object_id = tb.object_id
                            LEFT JOIN (
                                SELECT
                                    ic.object_id,
                                    ic.column_id
                                FROM sys.indexes i
                                INNER JOIN sys.index_columns ic
                                    ON i.object_id = ic.object_id
                                    AND i.index_id = ic.index_id
                                WHERE i.is_primary_key = 1
                            ) pk
                                ON c.object_id = pk.object_id
                                AND c.column_id = pk.column_id
                            WHERE tb.name = @TableName;
                        ";

                using (SqlCommand command = new(query, connection)) {

                    command.Parameters.AddWithValue("@TableName", tableName);

                    try
                    {
                        connection.Open();
                        connection.ChangeDatabase(databaseName);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                columns.Add(new ColumnInfo
                                {
                                    ColumnName = reader.GetString(reader.GetOrdinal("ColumnName")),
                                    DataType = reader.GetString(reader.GetOrdinal("DataType")),
                                    IsNullable = reader.GetBoolean(reader.GetOrdinal("is_nullable")),
                                    IsPrimaryKey = reader.GetInt32(reader.GetOrdinal("IsPrimaryKey")) == 1
                                });
                            }
                        }

                    } catch (Exception ex)
                    {

                    }
                }
            }

            return columns;
        }
    }
}

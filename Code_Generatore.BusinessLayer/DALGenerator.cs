using Code_Generatore.AccessDataLayer;
using System.Text;

namespace Code_Generatore.BusinessLayer
{
    public class DALGenerator : CodeGeneratoreBase
    {
        public DALGenerator(string tableName, List<ColumnInfo> columns) : base(tableName, columns) { }

        public override string Generate(GenerationOptions generationOptions)
        {
            return Generate(generationOptions.Insert, generationOptions.Update,
                generationOptions.Delete, generationOptions.GetById,
                generationOptions.GetAll, generationOptions.FunctionNames);
        }

        private string Generate(bool insert, bool update, bool delete, bool getById, bool getAll, FunctionNames? fnNames = null)
        {
            fnNames ??= new FunctionNames();
            var code = new StringBuilder();

            code.AppendLine(GenerateClassHeader(fnNames));

            if (getById) code.AppendLine(GenerateGetByIdCode(fnNames));
            if (insert) code.AppendLine(GenerateInsertCode(fnNames));
            if (update) code.AppendLine(GenerateUpdateCode(fnNames));
            if (delete) code.AppendLine(GenerateDeleteCode(fnNames));
            if (getAll) code.AppendLine(GenerateGetAllCode(fnNames));

            code.AppendLine(GenerateClassFooter());

            return code.ToString();
        }

        private string GenerateClassHeader(FunctionNames fnNames)
        {
            var templateText = """
                using System;
                using System.Collections.Generic;
                using System.Data;
                using Microsoft.Data.SqlClient;
                using Microsoft.Extensions.Configuration;

                namespace GeneratedDataAccessLayer
                {
                    public partial class {{ DataClass }}
                    {
                        private static readonly string _connectionString = new ConfigurationBuilder()
                            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                            .AddJsonFile("appsettings.json")
                            .Build()
                            .GetConnectionString("DefaultConnection")!;

                """;

            return RenderTemplate(templateText, BuildTemplateModel(fnNames));
        }

        private string GenerateClassFooter() => "    }\n}";

        private string GenerateGetByIdCode(FunctionNames fnNames)
        {
            var templateText = """
                        // ── GetById ──────────────────────────────────
                        {{- if Pk }}
                        public static bool {{ FnGetById }}({{ Pk.CSharpType }} {{ Pk.ColumnName }}{{ for col in NonPkCols }}, ref {{ col.CSharpType }} {{ col.ColumnName }}{{ end }})
                        {
                            bool isFound = false;
                            using (SqlConnection connection = new SqlConnection(_connectionString))
                            {
                                string query = "SELECT * FROM {{ ClassName }} WHERE {{ Pk.ColumnName }} = @{{ Pk.ColumnName }}";
                                using (SqlCommand command = new SqlCommand(query, connection))
                                {
                                    command.Parameters.AddWithValue("@{{ Pk.ColumnName }}", {{ Pk.ColumnName }});
                                    try
                                    {
                                        connection.Open();
                                        using (SqlDataReader reader = command.ExecuteReader())
                                        {
                                            if (reader.Read())
                                            {
                                                isFound = true;
                                                {{- for col in NonPkCols }}
                                                {{- if col.IsNullable }}
                                                {{ col.ColumnName }} = reader["{{ col.ColumnName }}"] != DBNull.Value ? ({{ col.CSharpType }})reader["{{ col.ColumnName }}"] : null;
                                                {{- else if col.CSharpType == "int" }}
                                                {{ col.ColumnName }} = Convert.ToInt32(reader["{{ col.ColumnName }}"]);
                                                {{- else if col.CSharpType == "bool" }}
                                                {{ col.ColumnName }} = Convert.ToBoolean(reader["{{ col.ColumnName }}"]);
                                                {{- else }}
                                                {{ col.ColumnName }} = ({{ col.CSharpType }})reader["{{ col.ColumnName }}"];
                                                {{- end }}
                                                {{- end }}
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                        isFound = false;
                                    }
                                }
                            }
                            return isFound;
                        }
                        {{- else }}
                        // ⚠ No primary key defined
                        {{- end }}
                """;

            return RenderTemplate(templateText, BuildTemplateModel(fnNames));
        }

        private string GenerateInsertCode(FunctionNames fnNames)
        {
            var templateText = """
                        // ── Insert ───────────────────────────────────
                        {{- if Pk }}
                        public static int {{ FnInsert }}({{ for col in NonPkCols }}{{ col.CSharpType }} {{ col.ColumnName }}{{ if !for.last }}, {{ end }}{{ end }})
                        {
                            int newId = -1;
                            using (SqlConnection connection = new SqlConnection(_connectionString))
                            {
                                string query = @"INSERT INTO {{ ClassName }} ({{ for col in NonPkCols }}{{ col.ColumnName }}{{ if !for.last }}, {{ end }}{{ end }})
                                                 VALUES ({{ for col in NonPkCols }}@{{ col.ColumnName }}{{ if !for.last }}, {{ end }}{{ end }});
                                                 SELECT SCOPE_IDENTITY();";

                                using (SqlCommand command = new SqlCommand(query, connection))
                                {
                                    {{- for col in NonPkCols }}
                                    {{- if col.IsNullable }}
                                    command.Parameters.AddWithValue("@{{ col.ColumnName }}", (object?){{ col.ColumnName }} ?? DBNull.Value);
                                    {{- else }}
                                    command.Parameters.AddWithValue("@{{ col.ColumnName }}", {{ col.ColumnName }});
                                    {{- end }}
                                    {{- end }}
                                    try
                                    {
                                        connection.Open();
                                        object? result = command.ExecuteScalar();
                                        if (result != null && int.TryParse(result.ToString(), out int insertedId))
                                            newId = insertedId;
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                        newId = -1;
                                    }
                                }
                            }
                            return newId;
                        }
                        {{- else }}
                        // ⚠ No primary key defined
                        {{- end }}
                """;

            return RenderTemplate(templateText, BuildTemplateModel(fnNames));
        }

        private string GenerateUpdateCode(FunctionNames fnNames)
        {
            var templateText = """
                        // ── Update ───────────────────────────────────
                        {{- if Pk }}
                        public static bool {{ FnUpdate }}({{ for col in Columns }}{{ col.CSharpType }} {{ col.ColumnName }}{{ if !for.last }}, {{ end }}{{ end }})
                        {
                            int rowsAffected = 0;
                            using (SqlConnection connection = new SqlConnection(_connectionString))
                            {
                                string query = @"UPDATE {{ ClassName }} SET
                                                 {{ for col in NonPkCols }}{{ col.ColumnName }} = @{{ col.ColumnName }}{{ if !for.last }},{{ end }}
                                                 {{ end }}WHERE {{ Pk.ColumnName }} = @{{ Pk.ColumnName }}";

                                using (SqlCommand command = new SqlCommand(query, connection))
                                {
                                    command.Parameters.AddWithValue("@{{ Pk.ColumnName }}", {{ Pk.ColumnName }});
                                    {{- for col in NonPkCols }}
                                    {{- if col.IsNullable }}
                                    command.Parameters.AddWithValue("@{{ col.ColumnName }}", (object?){{ col.ColumnName }} ?? DBNull.Value);
                                    {{- else }}
                                    command.Parameters.AddWithValue("@{{ col.ColumnName }}", {{ col.ColumnName }});
                                    {{- end }}
                                    {{- end }}
                                    try
                                    {
                                        connection.Open();
                                        rowsAffected = command.ExecuteNonQuery();
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                        rowsAffected = 0;
                                    }
                                }
                            }
                            return rowsAffected > 0;
                        }
                        {{- else }}
                        // ⚠ No primary key defined
                        {{- end }}
                """;

            return RenderTemplate(templateText, BuildTemplateModel(fnNames));
        }

        private string GenerateDeleteCode(FunctionNames fnNames)
        {
            var templateText = """
                        // ── Delete ───────────────────────────────────
                        {{- if Pk }}
                        public static bool {{ FnDelete }}({{ Pk.CSharpType }} {{ Pk.ColumnName }})
                        {
                            int rowsAffected = 0;
                            using (SqlConnection connection = new SqlConnection(_connectionString))
                            {
                                string query = "DELETE FROM {{ ClassName }} WHERE {{ Pk.ColumnName }} = @{{ Pk.ColumnName }}";
                                using (SqlCommand command = new SqlCommand(query, connection))
                                {
                                    command.Parameters.AddWithValue("@{{ Pk.ColumnName }}", {{ Pk.ColumnName }});
                                    try
                                    {
                                        connection.Open();
                                        rowsAffected = command.ExecuteNonQuery();
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                        rowsAffected = 0;
                                    }
                                }
                            }
                            return rowsAffected > 0;
                        }
                        {{- else }}
                        // ⚠ No primary key defined
                        {{- end }}
                """;

            return RenderTemplate(templateText, BuildTemplateModel(fnNames));
        }

        private string GenerateGetAllCode(FunctionNames fnNames)
        {
            var templateText = """
                        // ── GetAll ───────────────────────────────────
                        public static List<string> {{ FnGetAll }}()
                        {
                            var list = new List<string>();
                            using (SqlConnection connection = new SqlConnection(_connectionString))
                            {
                                string query = "SELECT * FROM {{ ClassName }}";
                                using (SqlCommand command = new SqlCommand(query, connection))
                                {
                                    try
                                    {
                                        connection.Open();
                                        using (SqlDataReader reader = command.ExecuteReader())
                                        {
                                            while (reader.Read())
                                            {
                                                list.Add(reader[0].ToString() ?? string.Empty);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }
                            }
                            return list;
                        }
                """;

            return RenderTemplate(templateText, BuildTemplateModel(fnNames));
        }
    }
}
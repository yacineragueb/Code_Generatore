
namespace Code_Generatore.AccessDataLayer
{
    public class ColumnInfo
    {
        public string ColumnName { get; set; }   
        public string DataType { get; set; }
        public string CSharpType
        {
            get
            {
                string type = DataType switch
                {
                    "int" => "int",
                    "tinyint" => "byte",
                    "bigint" => "long",
                    "smallint" => "short",
                    "bit" => "bool",
                    "decimal" => "decimal",
                    "smallmoney" => "decimal",
                    "numeric" => "decimal",
                    "float" => "double",
                    "real" => "float",
                    "datetime" => "DateTime",
                    "date" => "DateTime",
                    "smalldatetime" => "DateTime",
                    "time" => "TimeSpan",
                    "uniqueidentifier" => "Guid",
                    "nvarchar" => "string",
                    "varchar" => "string",
                    "text" => "string",
                    _ => "object"
                };

                // string is already nullable reference type
                if (type == "string")
                    return type;

                // value types become nullable if SQL allows NULL
                return IsNullable ? $"{type}?" : type;
            }
        }
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
    }
}

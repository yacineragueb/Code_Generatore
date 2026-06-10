
namespace Code_Generatore.BusinessLayer
{
    public class FunctionNames
    {
        public string Insert { get; set; }
        public string Update { get; set; }
        public string Delete { get; set; }
        public string GetById { get; set; }
        public string GetAll { get; set; }

        public FunctionNames()
        {
            Insert = "Insert";
            Update = "Update";
            Delete = "Delete";
            GetById = "GetById";
            GetAll = "GetAll";
        }

        public FunctionNames(string insert, string update, string delete, string getById, string getAll)
        {
            Insert = insert;
            Update = update;
            Delete = delete;
            GetById = getById;
            GetAll = getAll;
        }
    }
}


namespace Code_Generatore.BusinessLayer
{
    public class GenerationOptions
    {
        // Which CRUD operations to emit
        public bool Insert { get; set; } 
        public bool Update { get; set; } 
        public bool Delete { get; set; } 
        public bool GetById { get; set; }
        public bool GetAll { get; set; } 

        // Custom function names (null = use defaults)
        public FunctionNames? FunctionNames { get; set; }
    }
}

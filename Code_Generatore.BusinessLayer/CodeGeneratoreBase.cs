using Code_Generatore.AccessDataLayer;
using Scriban;
using Scriban.Runtime;

namespace Code_Generatore.BusinessLayer
{
    public abstract class CodeGeneratoreBase
    {
        public string TableName { get; set; }
        public List<ColumnInfo> Columns { get; set; }

        protected CodeGeneratoreBase(string tableName, List<ColumnInfo> columns)
        {
            TableName = tableName;
            Columns = columns;
        }

        protected string RenderTemplate(string templateText, object model)
        {
            var template = Template.Parse(templateText);
            var context = new TemplateContext { MemberRenamer = member => member.Name };
            var scriptObj = new ScriptObject();
            scriptObj.Import(model, renamer: member => member.Name);
            context.PushGlobal(scriptObj);
            return template.Render(context);
        }

        protected virtual object BuildTemplateModel(FunctionNames fnNames)
        {
            return new
            {
                ClassName = TableName,
                DataClass = TableName + "Data",
                Pk = Columns.FirstOrDefault(c => c.IsPrimaryKey),
                Columns = Columns,
                NonPkCols = Columns.Where(c => !c.IsPrimaryKey).ToList(),
                FnInsert = fnNames.Insert,
                FnUpdate = fnNames.Update,
                FnDelete = fnNames.Delete,
                FnGetById = fnNames.GetById,
                FnGetAll = fnNames.GetAll,
            };
        }

        public abstract string Generate(GenerationOptions generationOptions);
    }
}

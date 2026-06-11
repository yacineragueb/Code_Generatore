using System.Text;
using Code_Generatore.AccessDataLayer;

namespace Code_Generatore.BusinessLayer
{
    public class BLLGenerator : CodeGeneratoreBase
    {
        public BLLGenerator(string tableName, List<ColumnInfo> columns) : base(tableName, columns) { }

        public override string Generate(GenerationOptions generationOptions)
        {
            return Generate(generationOptions.Insert, generationOptions.Update, generationOptions.Delete, generationOptions.GetById, generationOptions.GetAll, generationOptions.FunctionNames);
        }

        private string Generate(bool insert, bool update, bool delete, bool getById, bool getAll, FunctionNames? fnNames = null)
        {
            fnNames ??= new FunctionNames();
            var code = new StringBuilder();

            code.AppendLine(GenerateClassHeader(fnNames));

            if (insert) code.AppendLine(GenerateInsertCode(fnNames));
            if (update) code.AppendLine(GenerateUpdateCode(fnNames));
            if (delete) code.AppendLine(GenerateDeleteCode(fnNames));
            if (getById) code.AppendLine(GenerateFindCode(fnNames));
            if (getAll) code.AppendLine(GenerateGetAllCode(fnNames));

            code.AppendLine(GenerateSaveCode(insert, update, fnNames));

            code.AppendLine(GenerateClassFooter());

            return code.ToString();
        }

        private string GenerateClassHeader(FunctionNames fnNames)
        {
            var templateText = """
            using System;
            using System.Collections.Generic;
            using GeneratedDataAccessLayer;

            namespace GeneratedBusinessLogicLayer
            {
                public partial class {{ ClassName }}
                {
                    // ── Mode ──────────────────────────────────────
                    enum enMode { AddNew = 0, Update = 1 }
                    enMode _Mode = enMode.AddNew;

                    // ── Properties ───────────────────────────────
                    {{ for col in Columns }}
                    public {{ col.CSharpType }} {{ col.ColumnName }} { get; set; }
                    {{- end }}

                    // ── Default Constructor ──────────────────────
                    public {{ ClassName }}()
                    {
                        {{- for col in Columns }}
                        {{- if col.IsPrimaryKey }}
                        {{ col.ColumnName }} = -1;
                        {{- else if col.CSharpType == "string" }}
                        {{ col.ColumnName }} = string.Empty;
                        {{- else if col.CSharpType == "bool" || col.CSharpType == "bool?" }}
                        {{ col.ColumnName }} = false;
                        {{- else if col.CSharpType == "DateTime" }}
                        {{ col.ColumnName }} = DateTime.Now;
                        {{- else if col.CSharpType == "byte" }}
                        {{ col.ColumnName }} = 0;
                        {{- else if col.IsNullable }}
                        {{ col.ColumnName }} = null;
                        {{- else }}
                        {{ col.ColumnName }} = -1;
                        {{- end }}
                        {{- end }}
                        _Mode = enMode.AddNew;
                    }

                    // ── Private Constructor (used by Find) ───────
                    private {{ ClassName }}({{ for col in Columns }}{{ col.CSharpType }} {{ col.ColumnName }}{{ if !for.last }}, {{ end }}{{ end }})
                    {
                        {{- for col in Columns }}
                        this.{{ col.ColumnName }} = {{ col.ColumnName }};
                        {{- end }}
                        _Mode = enMode.Update;
                    }
            """;

            return RenderTemplate(templateText, BuildTemplateModel(fnNames));
        }

        private string GenerateClassFooter() => "    }\n}";

        private string GenerateInsertCode(FunctionNames fnNames)
        {
            var templateText = """
                    // ── Insert ───────────────────────────────────
                    {{- if Pk }}
                    private bool {{ FnInsert }}()
                    {
                        this.{{ Pk.ColumnName }} = {{ DataClass }}.{{ FnInsert }}({{ for col in NonPkCols }}this.{{ col.ColumnName }}{{ if !for.last }}, {{ end }}{{ end }});
                        return (this.{{ Pk.ColumnName }} != -1);
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
                    private bool {{ FnUpdate }}()
                    {
                        return {{ DataClass }}.{{ FnUpdate }}({{ for col in Columns }}this.{{ col.ColumnName }}{{ if !for.last }}, {{ end }}{{ end }});
                    }
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
                        return {{ DataClass }}.{{ FnDelete }}({{ Pk.ColumnName }});
                    }
                    {{- else }}
                    // ⚠ No primary key defined
                    {{- end }}
            """;

            return RenderTemplate(templateText, BuildTemplateModel(fnNames));
        }

        private string GenerateFindCode(FunctionNames fnNames)
        {
            var templateText = """
                    // ── Find ─────────────────────────────────────
                    {{- if Pk }}
                    public static {{ ClassName }} Find({{ Pk.CSharpType }} {{ Pk.ColumnName }})
                    {
                        {{- for col in NonPkCols }}
                        {{- if col.CSharpType == "string" }}
                        {{ col.CSharpType }} {{ col.ColumnName }} = string.Empty;
                        {{- else if col.CSharpType == "bool" || col.CSharpType == "bool?" }}
                        {{ col.CSharpType }} {{ col.ColumnName }} = false;
                        {{- else if col.CSharpType == "DateTime" }}
                        {{ col.CSharpType }} {{ col.ColumnName }} = DateTime.Now;
                        {{- else if col.IsNullable }}
                        {{ col.CSharpType }} {{ col.ColumnName }} = null;
                        {{- else if col.CSharpType == "byte" }}
                        {{ col.CSharpType }} {{ col.ColumnName }} = 0;
                        {{- else }}
                        {{ col.CSharpType }} {{ col.ColumnName }} = -1;
                        {{- end }}
                        {{- end }}

                        if ({{ DataClass }}.{{ FnGetById }}({{ Pk.ColumnName }}{{ for col in NonPkCols }}, ref {{ col.ColumnName }}{{ end }}))
                            return new {{ ClassName }}({{ for col in Columns }}{{ col.ColumnName }}{{ if !for.last }}, {{ end }}{{ end }});
                        else
                            return null;
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
                    // ── Get All ──────────────────────────────────
                    public static List<string> {{ FnGetAll }}()
                    {
                        return {{ DataClass }}.{{ FnGetAll }}();
                    }
            """;

            return RenderTemplate(templateText, BuildTemplateModel(fnNames));
        }

        private string GenerateSaveCode(bool hasInsert, bool hasUpdate, FunctionNames fnNames)
        {
            if (!hasInsert && !hasUpdate) return string.Empty;

            var templateText = """
                    // ── Save ─────────────────────────────────────
                    public bool Save()
                    {
                        switch (_Mode)
                        {
                            {{- if HasInsert }}
                            case enMode.AddNew:
                                if ({{ FnInsert }}()) { _Mode = enMode.Update; return true; }
                                return false;
                            {{- end }}
                            {{- if HasUpdate }}
                            case enMode.Update:
                                return {{ FnUpdate }}();
                            {{- end }}
                        }
                        return false;
                    }
            """;

            var model = new
            {
                HasInsert = hasInsert,
                HasUpdate = hasUpdate,
                FnInsert = fnNames.Insert,
                FnUpdate = fnNames.Update,
            };

            return RenderTemplate(templateText, model);
        }
    }
}
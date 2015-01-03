using VLang.Runtime;

namespace VLang.AST
{
    public interface IASTElement
    {
        object GetValue(ExecutionContext context);

        bool HasValue(ExecutionContext context);

        string ToJSON();
    }
}
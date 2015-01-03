using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal abstract class ASTElement
    {
        public virtual bool HasValue(ExecutionContext context)
        {
            return true;
        }

        public abstract string ToJSON();
    }
}
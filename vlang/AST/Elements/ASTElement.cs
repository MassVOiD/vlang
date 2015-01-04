using VLang.Runtime;

namespace VLang.AST.Elements
{
    public abstract class ASTElement
    {
        public virtual bool HasValue(ExecutionContext context)
        {
            return true;
        }

        public abstract string ToJSON();
    }
}
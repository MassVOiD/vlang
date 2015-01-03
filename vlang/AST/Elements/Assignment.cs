using System;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class Assignment : ASTElement, IASTElement
    {
        private Name Target;
        private Expression Value;

        public Assignment(Name target, Expression value)
        {
            Target = target;
            Value = value;
        }

        public object GetValue(ExecutionContext context)
        {
            object val = Value.GetValue(context);
            Target.GetReference(context).Value = val;
            return val;
        }

        public override string ToJSON()
        {
            return String.Format("Assignment({0} = {{{1}}})", Target.ToJSON(), Value.ToJSON());
        }
    }
}
using System;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class Assignment : ASTElement, IASTElement
    {
        private Expression Target;
        private Expression Value;

        public Assignment(Expression target, Expression value)
        {
            Target = target;
            Value = value;
        }

        public object GetValue(ExecutionContext context)
        {
            try
            {
                object val = Value.GetValue(context);
                object target = Target.GetValue(context);
                ((Variable)target).Value = val;
                return val;
            }
            catch
            {
                var clrref = Target.GetCLRReference(context);
                dynamic field = clrref.field;
                object val = Value.GetValue(context);
                field.SetValue(clrref.instance, val);
                return field;
               // context.InteropManager.SetValue(instance, name, Value.GetValue(context));
            }
            throw new Exception("Can not assign field");
        }

        public override string ToJSON()
        {
            return String.Format("Assignment({0} = {{{1}}})", Target.ToJSON(), Value.ToJSON());
        }
    }
}
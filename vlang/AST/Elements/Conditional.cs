using System;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class Conditional : ASTElement, IASTElement
    {
        public IASTElement Condition;
        public int IfNode, ElseNode;

        public Conditional(IASTElement condition, int trueBranch, int falseBranch = -1)
        {
            Condition = condition;
            IfNode = trueBranch;
            ElseNode = falseBranch;
        }

        public object GetValue(ExecutionContext context)
        {
            object expraw = Condition.GetValue(context);
            bool? exp = null;
            if (expraw is bool) exp = (bool)expraw;
            else throw new Exception("Not boolean in conditional expression");
            return exp.Value ? context.GetGroup(IfNode).GetValue(context) : (ElseNode != -1 ? context.GetGroup(ElseNode).GetValue(context) : null);
        }

        public override string ToJSON()
        {
            if (ElseNode != -1)
            {
                return String.Format("if({0}){{{1}}}else{{{2}}}", Condition.ToJSON(), Engine.Groups[IfNode].ToJSON(), Engine.Groups[ElseNode].ToJSON());
            }
            else
            {
                return String.Format("if({0}){{{1}}}", Condition.ToJSON(), Engine.Groups[IfNode].ToJSON());
            }
        }
    }
}
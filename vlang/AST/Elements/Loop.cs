using System;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class Loop : ASTElement, IASTElement
    {
        public int Node;

        public Loop(int branch)
        {
            Node = branch;
        }

        public object GetValue(ExecutionContext context)
        {
            return context.GetGroup(Node).GetValue(context);
        }

        public override string ToJSON()
        {
            return String.Format("loop{{{1}}}", Engine.Groups[Node].ToJSON());
        }
    }
}
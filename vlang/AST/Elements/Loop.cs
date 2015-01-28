using System;

namespace VLang.AST.Elements
{
    public class Loop : ASTElement, IASTElement
    {
        public int Node;

        public Loop(int branch)
        {
            Node = branch;
        }

        public override string ToJSON()
        {
            return String.Format("loop{{{1}}}", Engine.Groups[Node].ToJSON());
        }
    }
}
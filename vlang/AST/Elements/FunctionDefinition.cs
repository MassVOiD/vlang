using System;
using System.Linq;

namespace VLang.AST.Elements
{
    public class FunctionDefinition : ASTElement, IASTElement
    {
        public string[] Arguments;
        public int Body;
        public string Name;

        public FunctionDefinition(string name, string[] args, int body)
        {
            Name = name;
            Arguments = args;
            Body = body;
        }

        public override string ToJSON()
        {
            return String.Format("{0}({1}){{{2}}}", Name, String.Join(",", Arguments), Engine.Groups[Body].ToJSON());
        }
    }
}
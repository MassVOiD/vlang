using System;
using System.Linq;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class FunctionDefinition : ASTElement, IASTElement
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

        public object GetValue(ExecutionContext context)
        {
            if (context.Exists(Name))
            {
                Function func = context.GetValue(Name) as Function;
                func.AddOverload(Arguments.ToList(), Body);
                return func;
            }
            else
            {
                var func = new Function(Arguments.ToList(), Body);
                context.SetValue(Name, func);
                return func;
            }
        }

        public override string ToJSON()
        {
            return String.Format("{0}({1}){{{2}}}", Name, String.Join(",", Arguments), Engine.Groups[Body].ToJSON());
        }
    }
}
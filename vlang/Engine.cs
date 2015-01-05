using VLang.AST;
using VLang.Frontends;
using VLang.Runtime;
using System.Collections.Generic;

namespace VLang
{
    public class Engine
    {
        public ExecutionContext Context;
        private Frontend Frontend;
        private InteropManager Interop;

        public Engine()
        {
            Interop = new InteropManager(true);
            Context = new ExecutionContext();
            Context.SetInteropManager(Interop);
        }

        public Engine(string[] imports)
        {
            Interop = new InteropManager(imports);
            Context = new ExecutionContext();
            Context.SetInteropManager(Interop);
        }
        public Engine(System.Reflection.Assembly[] imports)
        {
            Interop = new InteropManager(imports);
            Context = new ExecutionContext();
            Context.SetInteropManager(Interop);
        }

        public static Dictionary<int, ASTNode> Groups;

        public void SetGroups(Dictionary<int, ASTNode> g)
        {
            Groups = g;
            Context.SetGroups(Groups);
        }

        public ASTNode Compile(string script)
        {
            Frontend = new DefaultFrontend(script);
            ASTNode node = Frontend.Parse();
            SetGroups(node.Groups);
            return node;
        }

        public object Execute(string script)
        {
            Frontend = new DefaultFrontend(script);
            ASTNode node = Frontend.Parse();
            return node.GetValue(Context);
        }

        public object Execute(ASTNode node)
        {
            return node.GetValue(Context);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VLang;
using VLang.AST;
using VLang.AST.Elements;

namespace InterpreterBackend
{
    public class InterpreterBackend : VLang.Frontends.IBackend
    {
        InteropManager Interop;
        ExecutionContext RootContext;
        public InterpreterBackend()
        {
            RootContext = new ExecutionContext();
            Interop = new InteropManager(true);
            RootContext.SetInteropManager(Interop);
        }

        public void SetField(string name, object value)
        {
            if(RootContext.Exists(name))
            {
                RootContext.SetValue(name, value);
            }
            else
            {
                RootContext.CreateField(name, value.GetType(), value);
            }
        }

        public object Execute(ASTNode node)
        {
            return RootContext.Execute(node);
        }
    }
}

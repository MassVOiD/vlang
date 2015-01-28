using System;
using VLang;

namespace Tester
{
    public class TestClass
    {
        public int v;

        public TestClass(int value)
        {
            v = value;
        }

        public int V
        {
            get { return v; }
            set { V = v; }
        }

        public int GetV()
        {
            return v;
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            Engine engine = new Engine();
            var ast = engine.Compile(System.IO.File.ReadAllText("test.vs"));
            Console.WriteLine("Raw");
            Console.WriteLine(new JSBeautifyLib.JSBeautify(ast.ToJSON(), new JSBeautifyLib.JSBeautifyOptions()
            {
                preserve_newlines = true
            }).GetResult());
            Console.WriteLine("Optimized");
            ast.Optimize();
            Console.WriteLine(new JSBeautifyLib.JSBeautify(ast.ToJSON(), new JSBeautifyLib.JSBeautifyOptions()
            {
                preserve_newlines = true
            }).GetResult());
            Console.Read();

            InterpreterBackend.InterpreterBackend backend = new InterpreterBackend.InterpreterBackend();

            backend.SetField("print", new InterpreterBackend.Delegation(new Action<string>(a => Console.WriteLine(a))));

            backend.Execute(ast);

            Console.Read();

        }
    }
}
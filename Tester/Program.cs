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
            Engine engine = new Engine(new VLang.Frontends.DefaultFrontend(), new JavascriptBackend.JavascriptBackend());
            //var ast = engine.Compile(System.IO.File.ReadAllText("test.vs"));
            Console.WriteLine(engine.Execute(System.IO.File.ReadAllText("test.vs")));

            Console.Read();

        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using VLang;
using VLang.Runtime;
using VLang.AST;

namespace Tester
{
    public class TestClass
    {
        public int v;
        public TestClass(int value)
        {
            v = value;
        }
        public int GetV(){
            return v;
        }
        public int V
        {
            get { return v; }
            set { V = v; }
        }
    }
    internal class Program
    {
        private static void Main(string[] args)
        {
            Engine engine = new Engine(new System.Reflection.Assembly[] { System.Reflection.Assembly.GetExecutingAssembly() });
            var ast = engine.Compile(System.IO.File.ReadAllText("test.vs"));
            /*Console.WriteLine("Raw");
            Console.WriteLine(new JSBeautifyLib.JSBeautify(ast.ToJSON(), new JSBeautifyLib.JSBeautifyOptions()
            {
                preserve_newlines = true
            }).GetResult());
            Console.WriteLine("Optimized");*/
            ast.Optimize();
            /*Console.WriteLine(new JSBeautifyLib.JSBeautify(ast.ToJSON(), new JSBeautifyLib.JSBeautifyOptions()
            {
                preserve_newlines = true
            }).GetResult());
            Console.Read();*/

            engine.Context.SetValue("print", 
                new Delegation(
                    new Action<object>(
                        (a) => Console.WriteLine(a))));

            engine.Execute(ast);
            //Console.Read();
            //Console.Read();
        }
    }
}
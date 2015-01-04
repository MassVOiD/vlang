using System;
using VLang;
using VLang.Runtime;

namespace Tester
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Engine engine = new Engine();
            var ast = engine.Compile(System.IO.File.ReadAllText("test.vs"));
            Console.WriteLine(new JSBeautifyLib.JSBeautify(ast.ToJSON(), new JSBeautifyLib.JSBeautifyOptions(){
                preserve_newlines = true
            }).GetResult());
            Console.Read();

            engine.Context.SetValue("print", 
                new Delegation(
                    new Action<object>(
                        (a) => Console.WriteLine(a))));

            engine.Execute(ast);
            Console.Read();
            Console.Read();
        }
    }
}
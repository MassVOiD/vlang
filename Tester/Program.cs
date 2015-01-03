﻿using System;
using VLang;
using VLang.Runtime;

namespace Tester
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Engine engine = new Engine();
            var ast = engine.Compile(
@"a = 2;
if(a == 2){
    print('ok');
} else {
    print('nie');
}
print(123123);"
);
            Console.WriteLine(ast.ToJSON());
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
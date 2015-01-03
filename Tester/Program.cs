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
            var ast = engine.Compile(
@"
test(a){
    if(a > 0){
        print(a);
        test(a-1);
    }
}
test(15);
print(if(14 % 2 == 0) {
    'tak';
} else {
    'nie';
});"
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
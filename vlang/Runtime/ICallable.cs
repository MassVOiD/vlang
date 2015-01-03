namespace VLang.Runtime
{
    internal interface ICallable
    {
        object Call(ExecutionContext context);

        object Call(ExecutionContext context, object[] args);

        bool IsVoid();
    }
}
namespace InterpreterBackend
{
    internal interface ICallable
    {
        object Call(ExecutionContext context);

        object Call(ExecutionContext context, object[] args);

        bool IsVoid();
    }
}
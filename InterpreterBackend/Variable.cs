using System;

namespace InterpreterBackend
{
    public class Variable
    {
        private object RealValue;
        private Type DataType;

        public Variable(Type Type, object value)
        {
            DataType = Type;
            Value = value;
        }
        public Variable(Type Type)
        {
            DataType = Type;
            Value = null;
        }

        bool IsAllowedType(Type type)
        {
            do
            {
                if(type == DataType)
                    return true;
                type = type.BaseType;
            } while(type != null);
            return false;
        }

        public object Value
        {
            get { return RealValue; }
            set
            {
                if(!IsAllowedType(value.GetType()))
                {
                    object cast = Convert.ChangeType(value, DataType);
                    if(cast == null)
                    {
                        throw new InvalidCastException("Cannot cast " + value.GetType().ToString() +
                            " to " + DataType.ToString());
                    }
                    else
                    {
                        RealValue = cast;
                    }
                }
                else
                {
                    RealValue = value;
                }
            }
        }
    }
}
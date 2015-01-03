namespace VLang.Runtime
{
    public class Variable
    {
        private object RealValue;

        public Variable(object value)
        {
            Value = value;
        }

        public object Value
        {
            get { return RealValue; }
            set { RealValue = value; }
        }
    }
}
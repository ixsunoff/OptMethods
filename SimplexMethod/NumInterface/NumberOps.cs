namespace SimplexMethod.NumInterface
{
    internal interface INumOps{}
    
    public abstract class NumberOps<T> : INumOps where T: struct
    {
        public abstract T Zero { get; }           // 0
        public abstract T One { get; }            // 1
        public abstract T MinusOne { get; }       // -1
        
        public abstract T Sum(T a, T b);          // a + b
        public abstract T Subst(T a, T b);        // a - b
        public abstract T Invert(T a);            // -a

        public abstract T Mul(T a, T b);          // a * b
        public abstract T Div(T a, T b);          // a / b

        public abstract bool Equal(T a, T b);     // a == b
        public abstract bool NotEqual(T a, T b);  // a != b

        public abstract bool Less(T a, T b);      // a < b
        public abstract bool More(T a, T b);      // a > b
    }
}
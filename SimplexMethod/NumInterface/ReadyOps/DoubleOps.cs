using System;

namespace SimplexMethod.NumInterface.ReadyOps
{
    public sealed class DoubleOps : NumberOps<double>
    {
        public override double Zero => 0d;
        public override double One => 1d;
        public override double MinusOne => -1d;
        
        private readonly int _tollerance;

        private DoubleOps(int tollerance) => _tollerance = tollerance;
        public static DoubleOps Create(int tollerance)
        {
            if (tollerance <= 0)
                throw new ArgumentException(nameof(tollerance), "Порядок округления должен быть строго положительным");
            
            return new DoubleOps(tollerance);
        } 
        
        public override double Sum(double a, double b) => a + b;
        public override double Subst(double a, double b) => a - b;
        public override double Invert(double a) => -a;

        public override double Mul(double a, double b) => a * b;
        public override double Div(double a, double b) => a / b;

        public override bool Equal(double a, double b) => Math.Abs(a - b) < Math.Pow(0.1d, _tollerance);
        public override bool NotEqual(double a, double b) => Math.Abs(a - b) > Math.Pow(0.1d, _tollerance);

        public override bool Less(double a, double b) => Math.Round(a, _tollerance) < Math.Round(b, _tollerance);
        public override bool More(double a, double b) => Math.Round(a, _tollerance) > Math.Round(b, _tollerance);
    }
}
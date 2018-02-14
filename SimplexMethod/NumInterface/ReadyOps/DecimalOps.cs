using System;

namespace SimplexMethod.NumInterface.ReadyOps
{
    public sealed class DecimalOps : NumberOps<decimal>
    {
        public override decimal Zero => 0;
        public override decimal One => 1;
        public override decimal MinusOne => -1;

        private readonly int _tollerance;

        private DecimalOps(int tollerance) => _tollerance = tollerance;
        public static DecimalOps Create(int tollerance)
        {
            if (tollerance <= 0)
                throw new ArgumentException(nameof(tollerance), "Порядок округления должен быть строго положительным");

            return new DecimalOps(tollerance);
        }

        public override decimal Sum(decimal a, decimal b) => a + b;
        public override decimal Subst(decimal a, decimal b) => a - b;
        public override decimal Invert(decimal a) => -a;

        public override decimal Mul(decimal a, decimal b) => a * b;
        public override decimal Div(decimal a, decimal b) => a / b;

        public override bool Equal(decimal a, decimal b) => Math.Round(a, _tollerance) == Math.Round(b, _tollerance);
        public override bool NotEqual(decimal a, decimal b) => Math.Round(a, _tollerance) != Math.Round(b, _tollerance);

        public override bool Less(decimal a, decimal b) => Math.Round(a, _tollerance) < Math.Round(b, _tollerance);
        public override bool More(decimal a, decimal b) => Math.Round(a, _tollerance) > Math.Round(b, _tollerance);
    }
}
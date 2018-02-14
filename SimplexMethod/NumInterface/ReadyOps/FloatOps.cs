using System;

namespace SimplexMethod.NumInterface.ReadyOps
{
    public sealed class FloatOps : NumberOps<float>
    {
        public override float Zero => 0;
        public override float One => 1;
        public override float MinusOne => -1;

        private readonly int _tollerance;

        private FloatOps(int tollerance) => _tollerance = tollerance;
        public static FloatOps Create(int tollerance)
        {
            if (tollerance <= 0)
                throw new ArgumentException(nameof(tollerance), "Порядок округления должен быть строго положительным");

            return new FloatOps(tollerance);
        }

        public override float Sum(float a, float b) => a + b;
        public override float Subst(float a, float b) => a - b;
        public override float Invert(float a) => -a;

        public override float Mul(float a, float b) => a * b;
        public override float Div(float a, float b) => a / b;

        public override bool Equal(float a, float b) => Math.Abs(a - b) < Math.Pow(0.1d, _tollerance);
        public override bool NotEqual(float a, float b) => Math.Abs(a - b) > Math.Pow(0.1d, _tollerance);

        public override bool Less(float a, float b) => Math.Round(a, _tollerance) < Math.Round(b, _tollerance);
        public override bool More(float a, float b) => Math.Round(a, _tollerance) > Math.Round(b, _tollerance);
    }
}
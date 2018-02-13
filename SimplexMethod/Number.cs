using System;
using System.Runtime.InteropServices;

namespace SimplexMethod
{
    internal interface INumOps{}
    
    public abstract class NumberOps<T> : INumOps where T: struct
    {
        public abstract T Zero { get; }
        public abstract T One { get; }
        public abstract T MinusOne { get; }
        
        public abstract T Sum(T a, T b);
        public abstract T Subst(T a, T b);
        public abstract T Invert(T a);

        public abstract T Mul(T a, T b);
        public abstract T Div(T a, T b);

        public abstract bool Equal(T a, T b);
        public abstract bool NotEqual(T a, T b);

        public abstract bool Less(T a, T b);
        public abstract bool More(T a, T b);

        public abstract bool LessEqual(T a, T b);
        public abstract bool MoreEqual(T a, T b);
        
    }

    public sealed class FloatOps : NumberOps<float>
    {
        public override float Zero => 0;
        public override float One => 1;
        public override float MinusOne => -1;

        private readonly int _tollerance;

        private FloatOps() { }
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

        public override bool LessEqual(float a, float b) => Math.Round(a, _tollerance) <= Math.Round(b, _tollerance);
        public override bool MoreEqual(float a, float b) => Math.Round(a, _tollerance) >= Math.Round(b, _tollerance);
    }

    public sealed class DoubleOps : NumberOps<double>
    {
        public override double Zero => 0d;
        public override double One => 1d;
        public override double MinusOne => -1d;
        
        private readonly int _tollerance;

        private DoubleOps(){}
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

        public override bool LessEqual(double a, double b) => Math.Round(a, _tollerance) <= Math.Round(b, _tollerance);
        public override bool MoreEqual(double a, double b) => Math.Round(a, _tollerance) >= Math.Round(b, _tollerance);
    }

    public sealed class DecimalOps : NumberOps<decimal>
    {
        public override decimal Zero => 0;
        public override decimal One => 1;
        public override decimal MinusOne => -1;

        private readonly int _tollerance;

        private DecimalOps() { }
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

        public override bool LessEqual(decimal a, decimal b) => Math.Round(a, _tollerance) <= Math.Round(b, _tollerance);
        public override bool MoreEqual(decimal a, decimal b) => Math.Round(a, _tollerance) >= Math.Round(b, _tollerance);
    }

}
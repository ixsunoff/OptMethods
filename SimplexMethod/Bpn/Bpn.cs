using System;
#pragma warning disable 660,661

namespace SimplexMethod.Bpn
{
    internal struct Bpn<T> where T: struct
    {
        /// <summary>
        /// Коэффициент при составляющей числа, много большей любого исчисляемого числа
        /// </summary>
        internal T Bp;

        /// <summary>
        /// Стандартное значение
        /// </summary>
        internal readonly T Fp;

        private readonly BpnFactory<T> _factory;
        
        internal Bpn(T bp, T fp, BpnFactory<T> factory)
        {
            Bp = bp;
            Fp = fp;
            _factory = factory;
        }

        internal bool ContainsBigPart()
        {
            return !(_factory.Ops.Equal(Bp, _factory.Ops.Zero));
        }
        
        public static Bpn<T> operator +(Bpn<T> a, Bpn<T> b)
        {
            return a._factory.Create(
                a._factory.Ops.Sum(a.Bp, b.Bp),
                a._factory.Ops.Sum(a.Fp, b.Fp)
            );
        }
        public static Bpn<T> operator -(Bpn<T> a, Bpn<T> b)
        {
            return a._factory.Create(
                a._factory.Ops.Subst(a.Bp, b.Bp),
                a._factory.Ops.Subst(a.Fp, b.Fp)
            );
        }
        public static Bpn<T> operator -(Bpn<T> a)
        {
            return a._factory.Create(
                a._factory.Ops.Invert(a.Bp),
                a._factory.Ops.Invert(a.Fp)
            );
        }
        
        public static Bpn<T> operator *(Bpn<T> a, Bpn<T> b)
        {
            if (b._factory.Ops.NotEqual(b.Bp, b._factory.Ops.Zero))
                throw new ArgumentException("Нельзя умножать на сколь угодно большое число");

            return a._factory.Create(
                a._factory.Ops.Mul(a.Bp, b.Fp),
                a._factory.Ops.Mul(a.Fp, b.Fp)
            );
        }
        public static Bpn<T> operator /(Bpn<T> a, Bpn<T> b)
        {
            if (b._factory.Ops.NotEqual(b.Bp, b._factory.Ops.Zero))
                throw new ArgumentException("Нельзя делить на сколь угодно большое число");

            return a._factory.Create(
                a._factory.Ops.Div(a.Bp, b.Fp),
                a._factory.Ops.Div(a.Fp, b.Fp)
            );
        }
        
        public static bool operator >(Bpn<T> a, Bpn<T> b)
        {
            if (a._factory.Ops.More(a.Bp, b.Bp))
                return true;
            else if (a._factory.Ops.Equal(a.Bp, b.Bp))
                if (a._factory.Ops.More(a.Fp, b.Fp))
                    return true;

            return false;
        }
        public static bool operator <(Bpn<T> a, Bpn<T> b)
        {
            if (a._factory.Ops.Less(a.Bp, b.Bp))
                return true;
            else if (a._factory.Ops.Equal(a.Bp, b.Bp))
                if (a._factory.Ops.Less(a.Fp, b.Fp))
                    return true;

            return false;
        }
        public static bool operator ==(Bpn<T> a, Bpn<T> b)
        {
            return (a._factory.Ops.Equal(a.Bp, b.Bp)) &&
                   (a._factory.Ops.Equal(a.Fp, b.Fp));
        }
        public static bool operator !=(Bpn<T> a, Bpn<T> b)
        {
            return !(a == b);
        }
        public static bool operator >=(Bpn<T> a, Bpn<T> b)
        {
            return a == b || a > b;
        }
        public static bool operator <=(Bpn<T> a, Bpn<T> b)
        {
            return a == b || a < b;
        }

        public override string ToString()
        {
            if (_factory.Ops.Equal(Bp, _factory.Ops.Zero))
                return $"{Fp}";
            if (_factory.Ops.Equal(Fp, _factory.Ops.Zero))
                return $"{Bp}M";

            return $"{Bp}M " + ((_factory.Ops.Less(Fp, _factory.Ops.Zero)) ? "+" : "-") + $" {Fp}";
        }
    }
}
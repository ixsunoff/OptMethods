using SimplexMethod.NumInterface;

namespace SimplexMethod.Bpn
{
    internal class BpnFactory<T> where T: struct
    {
        internal readonly NumberOps<T> Ops;

        public BpnFactory(NumberOps<T> ops)
        {
            Ops = ops;
        }

        public Bpn<T> Create(T bp, T fp) => new Bpn<T>(bp, fp, this);

        public Bpn<T> CreateOne() => new Bpn<T>(Ops.Zero, Ops.One, this);
        public Bpn<T> CreateMinusOne() => new Bpn<T>(Ops.Zero, Ops.MinusOne, this);
        public Bpn<T> CreateMinusM() => new Bpn<T>(Ops.MinusOne, Ops.Zero, this);
        public Bpn<T> CreateZero() => new Bpn<T>(Ops.Zero, Ops.Zero, this);

        public Bpn<T>[] CreateArray(int count)
        {
            var array = new Bpn<T>[count];

            for (var i = 0; i < count; i++)
                array[i] = Create(Ops.Zero, Ops.Zero);

            return array;
        }

        public Bpn<T>[,] CreateMatrix(int firstDim, int secondDim)
        {
            var matrix = new Bpn<T>[firstDim, secondDim];

            for (var i = 0; i < firstDim; i++)
            for (var j = 0; j < secondDim; j++)
                matrix[i, j] = Create(Ops.Zero, Ops.Zero);

            return matrix;
        }
    }
}
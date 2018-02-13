using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double.Solvers;

namespace SimplexMethod
{
    public enum RelationType
    {
        Equality,
        MoreEqual,
        LessEqual
    }

    internal class BpnFactory<T> where T: struct
    {
        public NumberOps<T> ops; 

        public BpnFactory(NumberOps<T> ops)
        {
            this.ops = ops;
        }

        public Bpn<T> Create(T bp, T fp)
        {
            return new Bpn<T>(bp, fp, this);
        }

        public Bpn<T>[] CreateArray(int count)
        {
            var array = new Bpn<T>[count];

            for (int i = 0; i < count; i++)
                array[i] = Create(ops.Zero, ops.Zero);

            return array;
        }

        public Bpn<T>[,] CreateMatrix(int firstDim, int secondDim)
        {
            var matrix = new Bpn<T>[firstDim, secondDim];

            for (int i = 0; i < firstDim; i++)
                for (int j = 0; j < secondDim; j++)
                    matrix[i, j] = Create(ops.Zero, ops.Zero);

            return matrix;
        }
    }
    
    internal struct Bpn<T> where T: struct
    {
        /// <summary>
        /// Коэффициент при составляющей числа, много большей любого исчисляемого числа
        /// </summary>
        internal T Bp;

        /// <summary>
        /// Стандартное значение
        /// </summary>
        internal T Fp;
        internal BpnFactory<T> factory;
        
        internal Bpn(T bp, T fp, BpnFactory<T> factory)
        {
            this.Bp = bp;
            this.Fp = fp;
            this.factory = factory;
        }
        
        public bool ContainsBigPart()
        {
            return !(factory.ops.Equal(Bp, factory.ops.Zero));
        }
        
        public static Bpn<T> operator +(Bpn<T> a, Bpn<T> b)
        {
            return a.factory.Create(
                a.factory.ops.Sum(a.Bp, b.Bp),
                a.factory.ops.Sum(a.Fp, b.Fp)
            );
        }
        public static Bpn<T> operator -(Bpn<T> a, Bpn<T> b)
        {
            return a.factory.Create(
                a.factory.ops.Subst(a.Bp, b.Bp),
                a.factory.ops.Subst(a.Fp, b.Fp)
            );
        }
        public static Bpn<T> operator -(Bpn<T> a)
        {
            return a.factory.Create(
                a.factory.ops.Invert(a.Bp),
                a.factory.ops.Invert(a.Fp)
            );
        }
        
        public static Bpn<T> operator *(Bpn<T> a, Bpn<T> b)
        {
            if (b.factory.ops.NotEqual(b.Bp, b.factory.ops.Zero))
                throw new ArgumentException("Нельзя умножать на сколь угодно большое число");

            return a.factory.Create(
                a.factory.ops.Mul(a.Bp, b.Fp),
                a.factory.ops.Mul(a.Fp, b.Fp)
            );
        }
        public static Bpn<T> operator /(Bpn<T> a, Bpn<T> b)
        {
            if (b.factory.ops.NotEqual(b.Bp, b.factory.ops.Zero))
                throw new ArgumentException("Нельзя делить на сколь угодно большое число");

            return a.factory.Create(
                a.factory.ops.Div(a.Bp, b.Fp),
                a.factory.ops.Div(a.Fp, b.Fp)
            );
        }
        
        public static bool operator >(Bpn<T> a, Bpn<T> b)
        {
            if (a.factory.ops.More(a.Bp, b.Bp))
                return true;
            else if (a.factory.ops.Equal(a.Bp, b.Bp))
                if (a.factory.ops.More(a.Fp, b.Fp))
                    return true;

            return false;
        }
        public static bool operator <(Bpn<T> a, Bpn<T> b)
        {
            if (a.factory.ops.Less(a.Bp, b.Bp))
                return true;
            else if (a.factory.ops.Equal(a.Bp, b.Bp))
                if (a.factory.ops.Less(a.Fp, b.Fp))
                    return true;

            return false;
        }
        public static bool operator ==(Bpn<T> a, Bpn<T> b)
        {
            return (a.factory.ops.Equal(a.Bp, b.Bp)) &&
                   (a.factory.ops.Equal(a.Fp, b.Fp));
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
            if (factory.ops.Equal(Bp, factory.ops.Zero))
                return $"{Fp}";
            if (factory.ops.Equal(Fp, factory.ops.Zero))
                return $"{Bp}M";

            return $"{Bp}M " + ((factory.ops.Less(Fp, factory.ops.Zero)) ? "+" : "-") + $" {Fp}";
        }
    }

    internal class Relation<T> : ICloneable where T : struct 
    {
        /// <summary>
        /// Набор переменных с коэффициентами
        /// </summary>
        public Dictionary<string, Bpn<T>> VarCoefs { get; internal set; }
        /// <summary>
        /// независимый член
        /// </summary>
        public Bpn<T> IndCoef { get; internal set; }
        public RelationType Type { get; internal set; }

        private BpnFactory<T> _factory;

        private Relation()
        {
        }

        public Relation(Dictionary<string, T> valCoefs, T indCoef, RelationType type, BpnFactory<T> factory)
        {
            if (valCoefs == null) throw new ArgumentNullException(nameof(valCoefs));
            
            _factory = factory;
            VarCoefs = valCoefs.ToDictionary(c => c.Key, d => factory.Create(_factory.ops.Zero, d.Value));
            IndCoef =  factory.Create(_factory.ops.Zero, indCoef);
            Type = type;
        }

        public object Clone()
        {
            var cloneCoefs = VarCoefs.ToDictionary(c => c.Key, d => d.Value);

            var result = new Relation<T>();
            result.VarCoefs = cloneCoefs;
            result.IndCoef = IndCoef;
            result.Type = Type;
            result._factory = _factory;

            return result;
        }

        /// <summary>
        /// Добавляет в отношение дополнительную переменную,
        /// коэффициент которой отличается от выбранной переменной лишь знаком
        /// </summary>
        /// <param name="varName">Обозначение переменной, для которой необходимо создать отрицательный дубликат</param>
        /// <param name="clone">Если true, то функция вернёт копию отношения "Relation"</param>
        internal Relation<T> AddNegativeVar(string varName, bool clone = true)
        {
            if (!VarCoefs.ContainsKey(varName))
                throw new ArgumentException("Данной переменной не существует в отношении");

            var result = clone ? (Relation<T>) Clone() : this;

            result.VarCoefs.Add($"{varName}'", -VarCoefs[varName]);

            return result;
        }

        /// <summary>
        /// Преобразует отношение к виду равенства
        /// </summary>
        /// <param name="clone">Если true, то функция вернёт копию отношения "Relation"</param>
        internal Relation<T> ConvertToEquality(bool clone = true)
        {
            var result = clone ? (Relation<T>) Clone() : this;

            switch (Type)
            {
                case RelationType.Equality:
                    break;
                case RelationType.MoreEqual:
                    result.VarCoefs.Add("u", _factory.Create(_factory.ops.Zero, _factory.ops.MinusOne));
                    result.Type = RelationType.Equality;
                    break;
                case RelationType.LessEqual:
                    result.VarCoefs.Add("u", _factory.Create(_factory.ops.Zero, _factory.ops.One));
                    result.Type = RelationType.Equality;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }

        /// <summary>
        /// Добавление дополнительной переменной "w", в случае если "u" меньше или равен 0  
        /// </summary>
        /// <param name="clone"></param>
        /// <returns></returns>
        internal Relation<T> TryInsertW(bool clone = true)
        {
            var result = clone ? (Relation<T>) Clone() : this;

            var uCoef = GetCoefU();
            if (uCoef == null || _factory.ops.Less(uCoef.Value.Fp, _factory.ops.Zero))
                result.VarCoefs.Add("w", _factory.Create(_factory.ops.Zero, _factory.ops.One));

            return result;
        }

        /// <summary>
        /// задаёт номер дополнительной переменной "w",
        /// создаваемой при отрицательной или отсутствующей переменной "u" 
        /// </summary>
        /// <param name="number">задаваемый номер переменной "w"</param>
        /// <param name="clone">Если true, то функция вернёт копию отношения "Relation"</param>
        internal Relation<T> NumerizeW(int number, bool clone = true)
        {
            var result = clone ? (Relation<T>) Clone() : this;

            if (result.VarCoefs.ContainsKey("w"))
            {
                var c = result.VarCoefs["w"];
                result.VarCoefs.Remove("w");
                result.VarCoefs.Add($"w{number}", c);
            }

            return result;
        }

        /// <summary>
        /// задаёт номер дополнительной переменной "u", создаваемой при приведении отношения к равенству
        /// </summary>
        /// <param name="number">задаваемый номер переменной "u"</param>
        /// <param name="clone">Если true, то функция вернёт копию отношения "Relation"</param>
        internal Relation<T> NumerizeU(int number, bool clone = true)
        {
            var result = clone ? (Relation<T>) Clone() : this;

            if (result.VarCoefs.ContainsKey("u"))
            {
                var c = result.VarCoefs["u"];
                result.VarCoefs.Remove("u");
                result.VarCoefs.Add($"u{number}", c);
            }

            return result;
        }

        /// <summary>
        /// Проверяет отношение на наличие дополнительной переменной "u"
        /// </summary>
        internal bool ContainsU()
        {
            return VarCoefs.Select(c => c.Key).Any(c => c[0] == 'u');
        }

        /// <summary>
        /// Проверяет отношение на наличие дополнительной переменной "w"
        /// </summary>
        internal bool ContainsW()
        {
            return VarCoefs.Select(c => c.Key).Any(c => c[0] == 'w');
        }

        /// <summary>
        /// Получение значения переменной "u"
        /// </summary>
        internal Bpn<T>? GetCoefU()
        {
            if (!ContainsU())
                return null;

            return VarCoefs.First(c => c.Key[0] == 'u').Value;
        }

        /// <summary>
        /// Получение значения переменной "w"
        /// </summary>
        internal Bpn<T>? GetCoefW()
        {
            if (!ContainsW())
                return null;

            return VarCoefs.First(c => c.Key[0] == 'w').Value;
        }

        internal string GetCoefNameU()
        {
            return !ContainsU() ? null : VarCoefs.First(c => c.Key[0] == 'u').Key;
        }

        internal string GetCoefNameW()
        {
            return !ContainsW() ? null : VarCoefs.First(c => c.Key[0] == 'w').Key;
        }
        
        /// <summary>
        /// Инвертирование коэффициентов в отношении
        /// </summary>
        /// <param name="clone">Если true, то функция вернёт копию отношения "Relation"</param>
        internal Relation<T> Invert(bool clone = true)
        {
            var result = clone ? (Relation<T>) Clone() : this;

            for (int i = 0; i < result.VarCoefs.Count; i++)
            {
                var key = result.VarCoefs.Keys.ToList()[i];
                result.VarCoefs[key] = -result.VarCoefs[key];
            }

            result.IndCoef = -result.IndCoef;

            return result;
        }
    }

    public class SimplexLPT<T> where T : struct 
    {
        /// <summary>
        /// Максимизируемая функция
        /// </summary>
        internal Relation<T> Function { get; set; }
        /// <summary>
        /// Накладываемые условия в виде отношений
        /// </summary>
        internal List<Relation<T>> Conditions { get; set; }
        /// <summary>
        /// Переменные, которые не могу принимать отрицательные значения
        /// </summary>
        public List<string> NotNegativeVars { get; internal set; }
        public bool FindMax { get; }

        private BpnFactory<T> _factory;
        private static Dictionary<Type, INumOps> _readyOps = new Dictionary<Type, INumOps>(){
            [typeof(float)] = FloatOps.Create(10),
            [typeof(decimal)] = DecimalOps.Create(10),
            [typeof(double)] = DoubleOps.Create(10)
        };
        
        public SimplexLPT(bool findMax, List<string> notNegativeVars)
        {
            if (!_readyOps.ContainsKey(typeof(T)))
                throw new Exception("Для данного числового типа отсутствует предопределённый " +
                                    $"экземпляр класса NumberOps<{nameof(T)}>");

            _factory = new BpnFactory<T>((NumberOps<T>) _readyOps[typeof(T)]);
            FindMax = findMax;
            NotNegativeVars = notNegativeVars == null ? new List<string>() : notNegativeVars.ToList();
        }
        
        public SimplexLPT(bool findMax, List<string> notNegativeVars, NumberOps<T> ops)
        {
            if (ops == null) throw new ArgumentNullException(nameof(ops));

            _factory = new BpnFactory<T>(ops);
            FindMax = findMax;
            NotNegativeVars = notNegativeVars == null ? new List<string>() : notNegativeVars.ToList();
        }
        
        //дополнительные методы
        #region SubMethods

        private IEnumerable<Relation<T>> GetAllRelations()
        {
            foreach (var cond in Conditions)
                yield return cond;

            yield return Function;
        }

        #endregion
        
        /// <summary>
        /// Этап приведения ЗЛП к каноническому виду 
        /// </summary>
        private void Canonize()
        {
            //добавление дополнительных переменных в случае,
            //если не на все переменные наложено условие неотрицательности

            #region CatchNegativeVars

            var canBeNegative = new List<string>();

            foreach (var relation in GetAllRelations())
                canBeNegative.AddRange(
                    relation.VarCoefs.Select(c => c.Key).Except(NotNegativeVars)
                );

            canBeNegative = canBeNegative.Distinct().ToList();

            foreach (var vr in canBeNegative)
            foreach (var relation in GetAllRelations())
                if (relation.VarCoefs.ContainsKey(vr))
                    relation.AddNegativeVar(vr, false);

            #endregion

            //приведение всех условий к виду равенств

            #region ConvertConditionsToEquality

            foreach (var condition in Conditions)
                condition.ConvertToEquality(false);

            for (int i = 0; i < Conditions.Count; i++)
                Conditions[i].NumerizeU(i + 1, false);

            #endregion

            //преобразование задачи на минимум в задачу на максимум
            if (!FindMax)
                Function.Invert(false);

            //инвертирование условий с отрицательными свободными членами
            foreach (var condition in Conditions)
                if (condition.IndCoef < _factory.Create(_factory.ops.Zero, _factory.ops.Zero))
                    condition.Invert(false);

            //добавление переменной "w"
            for (int i = 0; i < Conditions.Count; i++)
            {
                Conditions[i].TryInsertW(false);
                Conditions[i].NumerizeW(i + 1, false);

                if (Conditions[i].ContainsW())
                    Function.VarCoefs.Add(Conditions[i].GetCoefNameW(), _factory.Create(_factory.ops.MinusOne, _factory.ops.Zero));
            }
        }

        public SimplexLPT<T> AddFunction(Dictionary<string, T> varsVals)
        {
            if (varsVals == null) throw new ArgumentNullException(nameof(varsVals));
            if (varsVals.Any(c => c.Key.Contains('\'') || c.Key.Contains('w') || c.Key.Contains('u')))
                throw new ArgumentException("Названия переменных не могут содержать символов \" \' \", \" u \" и \" w \"");
            
            Function = new Relation<T>(
                varsVals.ToDictionary(c => c.Key, d => d.Value),
                _factory.ops.Zero,
                RelationType.Equality,
                _factory
            );

            return this;
        }
        
        public SimplexLPT<T> AddCondition(Dictionary<string, T> varsVals, T indCoef, RelationType type)
        {
            if (varsVals == null) throw new ArgumentNullException(nameof(varsVals));
            if (varsVals.Any(c => c.Key.Contains('\'') || c.Key.Contains('w') || c.Key.Contains('u')))
                throw new ArgumentException("Названия переменных не могут содержать символов [ \' ], [ u ] и [ w ]");

            if (Conditions == null) Conditions = new List<Relation<T>>();
            
            Conditions.Add(new Relation<T>(
                varsVals.ToDictionary(c => c.Key, d => d.Value),
                indCoef,
                type,
                _factory
            ));

            return this;
        }
        
        public LptResult<T> Compute()
        {
            if (Function == null)
                throw new Exception("Исследуемая функция не объявлена");
            if (Conditions == null || Conditions.Count == 0)
                throw new Exception("Отсутствуют условия ЗЛП");
            
            Canonize();
            
            var matrix = new LptMatrix<T>(this, _factory);
            return matrix.Compute();
        }
    }

    internal class LptMatrix<T> where T: struct
    {
        internal string[] BasisVars;
        internal string[] AllVars;

        internal Bpn<T>[] FuncCoefs;
        internal Bpn<T>[,] MainMatrix;
        internal bool FindMax;

        private BpnFactory<T> _factory;

        public LptMatrix(SimplexLPT<T> lpt, BpnFactory<T> factory)
        {
            FindMax = lpt.FindMax;
            _factory = factory;
            
            //заполнение вектора имён всех переменных
            var allVars = new List<string>();

            allVars.AddRange(lpt.Function.VarCoefs.Select(c => c.Key));
            foreach (var cond in lpt.Conditions)
                allVars.AddRange(cond.VarCoefs.Select(c => c.Key));

            allVars = allVars.Distinct().ToList();
            SortVarArray(allVars);
            AllVars = allVars.ToArray();


            //заполение вектора коэффициентов переменных функции
            FuncCoefs = _factory.CreateArray(AllVars.Length);
            for (int i = 0; i < AllVars.Length; i++)
                if (lpt.Function.VarCoefs.ContainsKey(AllVars[i]))
                    FuncCoefs[i] = lpt.Function.VarCoefs[AllVars[i]];

            //заполнение вектора имён базисных переменных
            BasisVars = new string[lpt.Conditions.Count];

            for (int i = 0; i < lpt.Conditions.Count; i++)
                if (lpt.Conditions[i].ContainsW())
                    BasisVars[i] = lpt.Conditions[i].GetCoefNameW();
                else BasisVars[i] = lpt.Conditions[i].GetCoefNameU();

            //заполнение основной матрицы
            #region fillMainMatrix
            
            MainMatrix = _factory.CreateMatrix(lpt.Conditions.Count + 1, AllVars.Length + 1);

            //заполнение части со свободными членами и части с коэффициентами при переменных условий
            for (int i = 0; i < lpt.Conditions.Count; i++)
            {
                MainMatrix[i, 0] = lpt.Conditions[i].IndCoef;

                for(int j = 0; j < AllVars.Length; j++)
                    if (lpt.Conditions[i].VarCoefs.ContainsKey(AllVars[j]))
                        MainMatrix[i, 1 + j] = lpt.Conditions[i].VarCoefs[AllVars[j]];
            }

            //заполнение левого нижнего угла матрицы
            var buffCell = _factory.Create(_factory.ops.Zero, _factory.ops.Zero);
            for (var i = 0; i < lpt.Conditions.Count - 1; i++)
            {
                var first = lpt.Conditions[i].ContainsW()
                    ? _factory.Create(_factory.ops.MinusOne, _factory.ops.Zero)
                    : _factory.Create(_factory.ops.Zero, _factory.ops.Zero);

                var add = first * MainMatrix[i, 0];
                buffCell += add;
            }

            MainMatrix[MainMatrix.GetLength(0) - 1, 0] = buffCell;

            //заполнение последней строки
            for (int i = 1; i < MainMatrix.GetLength(1); i++)
            {
                buffCell = _factory.Create(_factory.ops.Zero, _factory.ops.Zero);

                for (int j = 0; j < MainMatrix.GetLength(0) - 1; j++)
                    if (BasisVars[j][0] == 'w')
                        buffCell += _factory.Create(_factory.ops.MinusOne, _factory.ops.Zero) * MainMatrix[j, i];
                    else buffCell += _factory.Create(_factory.ops.Zero, _factory.ops.Zero) * MainMatrix[j, i];

                MainMatrix[MainMatrix.GetLength(0) - 1, i] += buffCell - FuncCoefs[i - 1];
            }
            #endregion
        }

        public LptResult<T> Compute()
        {
            var badNoVals = false;
            
            while (true)
            {
                if (CheckBottomPozitive()) return GetFinalAnswer();

                var minBottomCol = GetMinimalBottom();
                var bestRow = GetBestRow(minBottomCol);

                if (bestRow == null)
                {
                    if (MainMatrix[MainMatrix.GetLength(0) - 1, 0].ContainsBigPart())
                        badNoVals = true;
                    
                    break;
                }
                
                //перерасчёт ячеек матрицы
                var colBuffer = _factory.CreateArray(MainMatrix.GetLength(0));
                
                for (var i = 0; i < MainMatrix.GetLength(1); i++)
                {
                    if (i == minBottomCol) continue;
                    
                    for (var j = 0; j < MainMatrix.GetLength(0); j++)
                        colBuffer[j] = MainMatrix[j, i];

                    for (var j = 0; j < MainMatrix.GetLength(0); j++)
                        if (j == bestRow.Value)
                            MainMatrix[j, i] /= MainMatrix[j, minBottomCol];
                        else
                        {
                            MainMatrix[j, i] = colBuffer[j] * MainMatrix[bestRow.Value, minBottomCol];
                            MainMatrix[j, i] -= MainMatrix[j, minBottomCol] * colBuffer[bestRow.Value];
                            MainMatrix[j, i] /= MainMatrix[bestRow.Value, minBottomCol];
                        }
                }

                for (var i = 0; i < MainMatrix.GetLength(0); i++)
                {
                    if (i == bestRow.Value)
                    {
                        MainMatrix[i, minBottomCol] = _factory.Create(_factory.ops.Zero, _factory.ops.One);
                        continue;
                    }
                    
                    MainMatrix[i, minBottomCol] = _factory.Create(_factory.ops.Zero, _factory.ops.Zero);
                }

                var prevBasis = BasisVars[bestRow.Value];
                var newBasis = AllVars[minBottomCol - 1];

                BasisVars[bestRow.Value] = AllVars[minBottomCol - 1];
            }
            
            //достигается при невозможности нахождения минимума/максимума
            return new LptResult<T>(){
                Type = (badNoVals) ? ResultType.NoVals : ResultType.InfiniteMax
            };
        }
        
        /// <summary>
        /// Проверяет, являются ли все нижние элементы MainMatrix (за исключением левой ячейки) положительными 
        /// </summary>
        private bool CheckBottomPozitive()
        {
            var matrixRow = MainMatrix.GetLength(0) - 1;
            
            for (var i = 1; i < MainMatrix.GetLength(1); i++)
                if (MainMatrix[matrixRow, i] < _factory.Create(_factory.ops.Zero, _factory.ops.Zero))
                    return false;

            return true;
        }

        //получение индекса столбца матрицы MainMatrix с минимальным нижним элементом
        private int GetMinimalBottom()
        {
            var result = 1;
            var matrixRow = MainMatrix.GetLength(0) - 1;
            
            for (var i = 2; i < MainMatrix.GetLength(1); i++)
                if (MainMatrix[matrixRow, i] < MainMatrix[matrixRow, result])
                    result = i;

            return result;
        }

        //получение индекса опорной строки MainMatrix вместе с результатом деления свободного члена на элемент 
        //Если в столбце не будет строго положительных значений, возвращается null
        private int? GetBestRow(int col)
        {
            var colNums = _factory.CreateArray(MainMatrix.GetLength(0) - 1);
            for (var i = 0; i < colNums.Length; i++)
                colNums[i] = MainMatrix[i, col];

            var divs = new Dictionary<int, Bpn<T>>();
            for (var i = 0; i < colNums.Length; i++)
                if (colNums[i] > _factory.Create(_factory.ops.Zero, _factory.ops.Zero))
                    divs.Add(i, MainMatrix[i, 0] / colNums[i]);

            if (divs.Count == 0)
                return null;

            var minDiv = divs.ToList()[0];
            foreach (var div in divs)
                if (div.Value < minDiv.Value)
                    minDiv = div;

            return minDiv.Key;
        }

        private T GetBasisVarValue(string vrName)
        {
            if (!(BasisVars.Any(c => c == vrName))) return _factory.ops.Zero;

            return MainMatrix[BasisVars.ToList().IndexOf(vrName), 0].Fp;
        }
        
        private LptResult<T> GetFinalAnswer()
        {
            if (BasisVars.Any(c => c[0] == 'w'))
                return new LptResult<T>() {Type = ResultType.NoVals};

            var result = new LptResult<T>();
            result.Type = ResultType.Done;
            result.Vars = new Dictionary<string, T>();
            
            var vrVals = AllVars.Where(vr => vr[0] != 'u' && vr[0] != 'w').ToDictionary(vr => vr, GetBasisVarValue);

            foreach (var vrVal in vrVals)
            {
                if (vrVal.Key.Contains('\''))
                    continue;
                else if (vrVals.ContainsKey(vrVal.Key + "\'"))
                    result.Vars.Add(vrVal.Key, _factory.ops.Subst(vrVal.Value, vrVals[vrVal.Key + "\'"]));
                else result.Vars.Add(vrVal.Key, vrVal.Value);
            }

            result.FuncValue = _factory.ops.Mul(
                MainMatrix[MainMatrix.GetLength(0) - 1, 0].Fp, 
                (FindMax ? _factory.ops.One : _factory.ops.MinusOne)
            );

            return result;
        }

        private void SortVarArray(List<string> arr)
        {
            var allVarsW = arr.FindAll(c => c[0] == 'w');
            var allVarsU = arr.FindAll(c => c[0] == 'u');

            arr.RemoveAll(c => allVarsU.Contains(c) || allVarsW.Contains(c));

            var wVarsInds = allVarsW.Select(c => int.Parse(c.Remove(0, 1))).ToList();
            var uVarsInds = allVarsU.Select(c => int.Parse(c.Remove(0, 1))).ToList();

            wVarsInds.Sort();
            uVarsInds.Sort();

            arr.AddRange(uVarsInds.Select(c => "u" + c));
            arr.AddRange(wVarsInds.Select(c => "w" + c));
        }
    }

    public enum ResultType
    {
        Done, //решение ЗЛП найдено
        NoVals, //множество допустимых решений ЗЛП пусто
        InfiniteMax //функция не ограничена
    }
    
    public class LptResult<T> where T : struct 
    {
        public ResultType Type { get; internal set; }
        public Dictionary<string, T> Vars { get; internal set; }
        public T? FuncValue { get; internal set; }
    }
}
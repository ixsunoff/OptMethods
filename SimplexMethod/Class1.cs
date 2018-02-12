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

    public struct BPN
    {
        /// <summary>
        /// Коэффициент при составляющей числа, много большей любого исчисляемого числа
        /// </summary>
        public double Bp;

        /// <summary>
        /// Стандартное значение
        /// </summary>
        public double Fp;

        public static int Tollerance = 10;
        
        
        public BPN(double bp, double fp, int tollerance = 10)
        {
            this.Bp = bp;
            this.Fp = fp;
            Tollerance = tollerance;
        }

        public BPN(double fp, int tollerance = 10)
        {
            Bp = 0;
            this.Fp = fp;
            Tollerance = tollerance;
        }
        
        /// <summary>
        /// проверяет, содержит ли число сколь угодно большой элемент
        /// </summary>
        public bool ContainsBigPart()
        {
            return !(Math.Abs(Bp) < Math.Pow(0.1d, Tollerance));
        }
        

        public static BPN operator +(BPN a, BPN b)
        {
            return new BPN(a.Bp + b.Bp, a.Fp + b.Fp);
        }

        public static BPN operator -(BPN a, BPN b)
        {
            return new BPN(a.Bp - b.Bp, a.Fp - b.Fp);
        }

        public static BPN operator -(BPN a)
        {
            return new BPN(-a.Bp, -a.Fp);
        }
        

        public static BPN operator *(BPN a, BPN b)
        {
            if (Math.Abs(b.Bp) > Math.Pow(0.1d, -Tollerance))
                throw new ArgumentException("Нельзя умножать на сколь угодно большое число");

            return new BPN(a.Bp * b.Fp, a.Fp * b.Fp);
        }

        public static BPN operator /(BPN a, BPN b)
        {
            if (Math.Abs(b.Bp) > Math.Pow(0.1d, -Tollerance))
                throw new ArgumentException("Нельзя делить на сколь угодно большое число");

            return new BPN(a.Bp / b.Fp, a.Fp / b.Fp);
        }
        

        public static bool operator >(BPN a, BPN b)
        {
            if (Math.Round(a.Bp, Tollerance) > Math.Round(b.Bp, Tollerance))
                return true;
            else if (Math.Abs(a.Bp - b.Bp) < Math.Pow(0.1d, Tollerance))
                if (Math.Round(a.Fp, Tollerance) > Math.Round(b.Fp, Tollerance))
                    return true;

            return false;
        }

        public static bool operator <(BPN a, BPN b)
        {
            if (Math.Round(a.Bp, Tollerance) < Math.Round(b.Bp, Tollerance))
                return true;
            else if (Math.Abs(a.Bp - b.Bp) < Math.Pow(0.1d, Tollerance))
                if (Math.Round(a.Fp, Tollerance) < Math.Round(b.Fp, Tollerance))
                    return true;

            return false;
        }

        public static bool operator ==(BPN a, BPN b)
        {
            return (Math.Abs(a.Bp - b.Bp) < Math.Pow(0.1d, -Tollerance)) &&
                   (Math.Abs(a.Fp - b.Fp) < Math.Pow(0.1d, -Tollerance));
        }

        public static bool operator !=(BPN a, BPN b)
        {
            return !(a == b);
        }

        public static bool operator >=(BPN a, BPN b)
        {
            return a == b || a > b;
        }

        public static bool operator <=(BPN a, BPN b)
        {
            return a == b || a < b;
        }

        public override string ToString()
        {
            if (Math.Abs(Bp) < Math.Pow(0.1d, Tollerance))
                return $"{Math.Round(Fp, Tollerance)}";
            if (Math.Abs(Fp) < Math.Pow(0.1d, Tollerance))
                return $"{Math.Round(Bp, Tollerance)}M";

            return $"{Math.Round(Bp, Tollerance)}M + {Math.Round(Fp, Tollerance)}";
        }
    }

    public class Relation : ICloneable
    {
        /// <summary>
        /// Набор переменных с коэффициентами
        /// </summary>
        public Dictionary<string, BPN> VarCoefs { get; internal set; }

        /// <summary>
        /// независимый член
        /// </summary>
        public BPN IndCoef { get; internal set; }

        public RelationType Type { get; internal set; }

        private Relation()
        {
        }

        public Relation(Dictionary<string, double> valCoefs, double indCoef, RelationType type)
        {
            if (valCoefs == null) throw new ArgumentNullException(nameof(valCoefs));

            VarCoefs = valCoefs.ToDictionary(c => c.Key, d => new BPN(0, d.Value));
            IndCoef = new BPN(0d, indCoef);
            Type = type;
        }

        public object Clone()
        {
            var cloneCoefs = VarCoefs.ToDictionary(c => c.Key, d => d.Value);

            var result = new Relation();
            result.VarCoefs = cloneCoefs;
            result.IndCoef = IndCoef;
            result.Type = Type;

            return result;
        }

        /// <summary>
        /// Добавляет в отношение дополнительную переменную,
        /// коэффициент которой отличается от выбранной переменной лишь знаком
        /// </summary>
        /// <param name="varName">Обозначение переменной, для которой необходимо создать отрицательный дубликат</param>
        /// <param name="clone">Если true, то функция вернёт копию отношения "Relation"</param>
        internal Relation AddNegativeVar(string varName, bool clone = true)
        {
            if (!VarCoefs.ContainsKey(varName))
                throw new ArgumentException("Данной переменной не существует в отношении");

            var result = clone ? (Relation) Clone() : this;

            result.VarCoefs.Add($"{varName}'", -VarCoefs[varName]);

            return result;
        }

        /// <summary>
        /// Преобразует отношение к виду равенства
        /// </summary>
        /// <param name="clone">Если true, то функция вернёт копию отношения "Relation"</param>
        internal Relation ConvertToEquality(bool clone = true)
        {
            var result = clone ? (Relation) Clone() : this;

            switch (Type)
            {
                case RelationType.Equality:
                    break;
                case RelationType.MoreEqual:
                    result.VarCoefs.Add("u", new BPN(-1));
                    result.Type = RelationType.Equality;
                    break;
                case RelationType.LessEqual:
                    result.VarCoefs.Add("u", new BPN(1));
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
        internal Relation TryInsertW(bool clone = true)
        {
            var result = clone ? (Relation) Clone() : this;

            var uCoef = GetCoefU();
            if (uCoef == null || uCoef.Value.Fp < 0d)
                result.VarCoefs.Add("w", new BPN(1));

            return result;
        }

        /// <summary>
        /// задаёт номер дополнительной переменной "w",
        /// создаваемой при отрицательной или отсутствующей переменной "u" 
        /// </summary>
        /// <param name="number">задаваемый номер переменной "w"</param>
        /// <param name="clone">Если true, то функция вернёт копию отношения "Relation"</param>
        internal Relation NumerizeW(int number, bool clone = true)
        {
            var result = clone ? (Relation) Clone() : this;

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
        internal Relation NumerizeU(int number, bool clone = true)
        {
            var result = clone ? (Relation) Clone() : this;

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
        internal BPN? GetCoefU()
        {
            if (!ContainsU())
                return null;

            return VarCoefs.First(c => c.Key[0] == 'u').Value;
        }

        /// <summary>
        /// Получение значения переменной "w"
        /// </summary>
        internal BPN? GetCoefW()
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
        internal Relation Invert(bool clone = true)
        {
            var result = clone ? (Relation) Clone() : this;

            for (int i = 0; i < result.VarCoefs.Count; i++)
            {
                var key = result.VarCoefs.Keys.ToList()[i];
                result.VarCoefs[key] = -result.VarCoefs[key];
            }

            result.IndCoef = -result.IndCoef;

            return result;
        }
    }

    public class SimplexLPT
    {
        /// <summary>
        /// Максимизируемая функция
        /// </summary>
        public Relation Function { get; }

        /// <summary>
        /// Накладываемые условия в виде отношений
        /// </summary>
        public List<Relation> Conditions { get; }

        /// <summary>
        /// Переменные, которые не могу принимать отрицательные значения
        /// </summary>
        public List<string> NotNegativeVars { get; }

        public bool FindMax { get; }

        public SimplexLPT(Relation function, List<Relation> conditions, List<string> notNegativeVars, bool findMax)
        {
            //TODO: выполнить проверку на null
            Function = function;
            Function.IndCoef = new BPN(0);
            Conditions = conditions;
            FindMax = findMax;
            NotNegativeVars = notNegativeVars;
        }

        //дополнительные методы

        #region SubMethods

        private IEnumerable<Relation> GetAllRelations()
        {
            foreach (var cond in Conditions)
                yield return cond;

            yield return Function;
        }

        #endregion
        /// <summary>
        /// Этап приведения ЗЛП к каноническому виду 
        /// </summary>
        public void Canonize()
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
                if (condition.IndCoef < new BPN(0))
                    condition.Invert(false);

            //добавление переменной "w"
            for (int i = 0; i < Conditions.Count; i++)
            {
                Conditions[i].TryInsertW(false);
                Conditions[i].NumerizeW(i + 1, false);

                if (Conditions[i].ContainsW())
                    Function.VarCoefs.Add(Conditions[i].GetCoefNameW(), new BPN(-1d, 0d));
            }
        }
    }

    public class LptMatrix
    {
        public string[] BasisVars;
        public string[] AllVars;

        public BPN[] FuncCoefs;
        public BPN[,] MainMatrix;
        public bool FindMax;
        
        private List<LptMatrix> _recursiveAnswers = new List<LptMatrix>();


        public LptMatrix(SimplexLPT lpt)
        {
            FindMax = lpt.FindMax;
            
            //заполнение вектора имён всех переменных
            var allVars = new List<string>();

            allVars.AddRange(lpt.Function.VarCoefs.Select(c => c.Key));
            foreach (var cond in lpt.Conditions)
                allVars.AddRange(cond.VarCoefs.Select(c => c.Key));

            allVars = allVars.Distinct().ToList();
            SortVarArray(allVars);
            AllVars = allVars.ToArray();


            //заполение вектора коэффициентов переменных функции
            FuncCoefs = new BPN[AllVars.Length];
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
            
            MainMatrix = new BPN[lpt.Conditions.Count + 1, AllVars.Length + 1];

            //заполнение части со свободными членами и части с коэффициентами при переменных условий
            for (int i = 0; i < lpt.Conditions.Count; i++)
            {
                MainMatrix[i, 0] = lpt.Conditions[i].IndCoef;

                for(int j = 0; j < AllVars.Length; j++)
                    if (lpt.Conditions[i].VarCoefs.ContainsKey(AllVars[j]))
                        MainMatrix[i, 1 + j] = lpt.Conditions[i].VarCoefs[AllVars[j]];
            }

            //заполнение левого нижнего угла матрицы
            var buffCell = new BPN();
            for (var i = 0; i < lpt.Conditions.Count - 1; i++)
                buffCell += (lpt.Conditions[i].ContainsW() ? new BPN(-1d, 0d) : new BPN()) * MainMatrix[i, 0];
            MainMatrix[MainMatrix.GetLength(0) - 1, 0] = buffCell;

            //заполнение последней строки
            for (int i = 1; i < MainMatrix.GetLength(1); i++)
            {
                buffCell = new BPN();

                for (int j = 0; j < MainMatrix.GetLength(0) - 1; j++)
                    if (BasisVars[j][0] == 'w')
                        buffCell += new BPN(-1d, 0d) * MainMatrix[j, i];
                    else buffCell += new BPN() * MainMatrix[j, i];

                MainMatrix[MainMatrix.GetLength(0) - 1, i] += buffCell - FuncCoefs[i - 1];
            }
            #endregion
        }

        public LptResult Compute()
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
                var colBuffer = new BPN[MainMatrix.GetLength(0)];
                
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
                        MainMatrix[i, minBottomCol] = new BPN(1d);
                        continue;
                    }
                    
                    MainMatrix[i, minBottomCol] = new BPN();
                }

                var prevBasis = BasisVars[bestRow.Value];
                var newBasis = AllVars[minBottomCol - 1];

                BasisVars[bestRow.Value] = AllVars[minBottomCol - 1];
            }
            
            //достигается при невозможности нахождения минимума/максимума
            return new LptResult(){
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
                if (MainMatrix[matrixRow, i] < new BPN(0d))
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
            var colNums = new BPN[MainMatrix.GetLength(0) - 1];
            for (var i = 0; i < colNums.Length; i++)
                colNums[i] = MainMatrix[i, col];

            var divs = new Dictionary<int, BPN>();
            for (var i = 0; i < colNums.Length; i++)
                if (colNums[i] > new BPN())
                    divs.Add(i, MainMatrix[i, 0] / colNums[i]);

            if (divs.Count == 0)
                return null;

            var minDiv = divs.ToList()[0];
            foreach (var div in divs)
                if (div.Value < minDiv.Value)
                    minDiv = div;

            return minDiv.Key;
        }

        private double GetBasisVarValue(string vrName)
        {
            if (!(BasisVars.Any(c => c == vrName))) return 0d;

            return MainMatrix[BasisVars.ToList().IndexOf(vrName), 0].Fp;

        }
        
        private LptResult GetFinalAnswer()
        {
            if (BasisVars.Any(c => c[0] == 'w'))
                return new LptResult() {Type = ResultType.NoVals};

            var result = new LptResult();
            result.Type = ResultType.Done;
            result.Vars = new Dictionary<string, double>();
            
            var vrVals = AllVars.Where(vr => vr[0] != 'u' && vr[0] != 'w').ToDictionary(vr => vr, GetBasisVarValue);

            foreach (var vrVal in vrVals)
            {
                if (vrVal.Key.Contains('\''))
                    continue;
                else if (vrVals.ContainsKey(vrVal.Key + "\'"))
                    result.Vars.Add(vrVal.Key, vrVal.Value - vrVals[vrVal.Key + "\'"]);
                else result.Vars.Add(vrVal.Key, vrVal.Value);
            }

            result.FuncValue = MainMatrix[MainMatrix.GetLength(0) - 1, 0].Fp * (FindMax ? 1d : -1d);

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
    
    public class LptResult
    {
        public ResultType Type { get; internal set; }
        public Dictionary<string, double> Vars { get; internal set; }
        public double? FuncValue { get; internal set; }
    }
}
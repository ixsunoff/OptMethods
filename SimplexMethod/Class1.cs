using System;
using System.Collections.Generic;
using System.Linq;

namespace SimplexMethod
{
    public enum RelationType
    {
        Equality, MoreEqual, LessEqual
    }

    
    
    internal struct BPN
    {
        /// <summary>
        /// Коэффициент при составляющей числа, много большей любого исчисляемого числа
        /// </summary>
        public double Bp;
        /// <summary>
        /// Стандартное значение
        /// </summary>
        public double Fp;

        public static int Tollerance = 20;
        

        public BPN(double bp, double fp, int tollerance = 20)
        {
            this.Bp = bp;
            this.Fp = fp;
            Tollerance = tollerance;
        }

        public BPN(double fp)
        {
            Bp = 0;
            this.Fp = fp;
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
                if (Math.Round(a.Fp, Tollerance) < Math.Round(b.Fp,-Tollerance))
                    return true;

            return false;
        }
        public static bool operator ==(BPN a, BPN b)
        {
            return (Math.Abs(a.Bp - b.Bp) < Math.Pow(0.1d, -Tollerance)) && (Math.Abs(a.Fp - b.Fp) < Math.Pow(0.1d, -Tollerance));
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
    }
    
    public class Relation: ICloneable
    {
        /// <summary>
        /// Набор переменных с коэффициентами
        /// </summary>
        public Dictionary<string, double> VarCoefs { get; }
        /// <summary>
        /// независимый член
        /// </summary>
        public double IndCoef { get; }
        public RelationType Type { get; internal set; }

        public Relation(Dictionary<string, double> valCoefs, double indCoef, RelationType type)
        {
            if (valCoefs != null) VarCoefs = valCoefs;
            IndCoef = indCoef;
            Type = type;
        }
        
        //клонирование отношения
        public object Clone()
        {
            var cloneCoefs = VarCoefs.ToDictionary(c => c.Key, d => d.Value);
            return new Relation(cloneCoefs, IndCoef, Type);
        }

        /// <summary>
        /// Преобразует отношение к виду равенства
        /// </summary>
        /// <param name="clone">Если true, то функция вернёт копию отношения "Relation"</param>
        internal Relation ConvertToEquality(bool clone = true)
        {
            var result = clone ? (Relation)Clone() : this;

            switch (Type)
            {
                case RelationType.Equality:
                    break;
                case RelationType.MoreEqual:
                    result.VarCoefs.Add("u", -1);
                    result.Type = RelationType.Equality;
                    break;
                case RelationType.LessEqual:
                    result.VarCoefs.Add("u", 1);
                    result.Type = RelationType.Equality;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
                result.VarCoefs.Add("u" + number, c);
            }

            return result;
        }

        /// <summary>
        /// Инвертирование коэффициентов в отношении
        /// </summary>
        /// <param name="clone">Если true, то функция вернёт копию отношения "Relation"</param>
        internal Relation Invert(bool clone = true)
        {
            var result = clone ? (Relation) Clone() : this;

            foreach (var pair in result.VarCoefs)
                result.VarCoefs[pair.Key] = - result.VarCoefs[pair.Key];

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

            if (result.ContainsU()) return result;
            
            
        }
        
        /// <summary>
        /// задаёт номер дополнительной переменной "w",
        /// создаваемой при отрицательной или отсутствующей переменной "u" 
        /// </summary>
        /// <param name="number">задаваемый номер переменной "w"</param>
        /// <param name="clone">Если true, то функция вернёт копию отношения "Relation"</param>
        internal Relation NumerizeW(int number, bool clone = true)
        {
            
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
    }

    public class SimplexLPT
    {
        public Relation Function { get; }
        public List<Relation> Conditions { get; }

        public SimplexLPT(Relation function, List<Relation> conditions)
        {
            if (function != null) Function = function;
            if (conditions != null) Conditions = conditions;
        }

        private List<string> allVars;
        private double
        
        public List<SimplexResults> Compute()
        {
            Normalizate();
            
            return null;
        }

        /// <summary>
        /// Этап приведения ЗЛП к каноническому виду 
        /// </summary>
        private void Canonize()
        {
            
        }
    }

    public class SimplexResults
    {
        public double Answer { get; set; }
        public Dictionary<Relation, double> ConditionsIndCoefs { get; set; }
        
        
    }
}

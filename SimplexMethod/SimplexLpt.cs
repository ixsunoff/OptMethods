using System;
using System.Collections.Generic;
using System.Linq;
using SimplexMethod.Bpn;
using SimplexMethod.NumInterface;
using SimplexMethod.NumInterface.ReadyOps;

namespace SimplexMethod
{
    public class LptResult<T> where T : struct 
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ResultType Type { get; internal set; }
        public Dictionary<string, T> Vars { get; internal set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public T? FuncValue { get; internal set; }
    }
    
    public class SimplexLpt<T> where T : struct 
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
        /// Переменные, которые не могут принимать отрицательные значения
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public List<string> NotNegativeVars { get; internal set; }
        public bool FindMax { get; }

        private readonly BpnFactory<T> _factory;
        
        // ReSharper disable once StaticMemberInGenericType
        // ReSharper disable once InconsistentNaming
        private static readonly Dictionary<Type, INumOps> _readyOps = new Dictionary<Type, INumOps>()
        {
            [typeof(float)] = FloatOps.Create(10),
            [typeof(decimal)] = DecimalOps.Create(10),
            [typeof(double)] = DoubleOps.Create(10)
        };
        
        public SimplexLpt(bool findMax, IReadOnlyCollection<string> notNegativeVars)
        {
            if (!_readyOps.ContainsKey(typeof(T)))
                throw new Exception("Для данного числового типа отсутствует предопределённый " +
                                    $"экземпляр класса NumberOps<{nameof(T)}>");

            _factory = new BpnFactory<T>((NumberOps<T>) _readyOps[typeof(T)]);
            FindMax = findMax;
            NotNegativeVars = notNegativeVars == null ? new List<string>() : notNegativeVars.ToList();
        }
        
        public SimplexLpt(bool findMax, IReadOnlyCollection<string> notNegativeVars, NumberOps<T> ops)
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

            for (var i = 0; i < Conditions.Count; i++)
                Conditions[i].NumerizeU(i + 1, false);

            #endregion

            //преобразование задачи на минимум в задачу на максимум
            if (!FindMax)
                Function.Invert(false);

            //инвертирование условий с отрицательными свободными членами
            foreach (var condition in Conditions)
                if (condition.IndCoef < _factory.CreateZero())
                    condition.Invert(false);

            //добавление переменной "w"
            for (var i = 0; i < Conditions.Count; i++)
            {
                Conditions[i].TryInsertW(false);
                Conditions[i].NumerizeW(i + 1, false);

                if (Conditions[i].ContainsW())
                    Function.VarCoefs.Add(Conditions[i].GetCoefNameW(), _factory.CreateMinusM());
            }
        }

        public SimplexLpt<T> AddFunction(Dictionary<string, T> varsVals)
        {
            if (varsVals == null) throw new ArgumentNullException(nameof(varsVals));
            if (varsVals.Any(c => c.Key.Contains('\'') || c.Key.Contains('w') || c.Key.Contains('u')))
                throw new ArgumentException("Названия переменных не могут содержать символов \" \' \", \" u \" и \" w \"");
            
            Function = new Relation<T>(
                varsVals.ToDictionary(c => c.Key, d => d.Value),
                _factory.Ops.Zero,
                RelationType.Equality,
                _factory
            );

            return this;
        }
        
        public SimplexLpt<T> AddCondition(Dictionary<string, T> varsVals, T indCoef, RelationType type)
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
}
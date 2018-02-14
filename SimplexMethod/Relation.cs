using System;
using System.Collections.Generic;
using System.Linq;
using SimplexMethod.Bpn;

namespace SimplexMethod
{
    internal class Relation<T> : ICloneable where T : struct 
    {
        /// <summary>
        /// Набор переменных с коэффициентами
        /// </summary>
        public Dictionary<string, Bpn<T>> VarCoefs { get; private set; }
        /// <summary>
        /// независимый член
        /// </summary>
        public Bpn<T> IndCoef { get; private set; }

        private RelationType Type { get; set; }

        private BpnFactory<T> _factory;

        private Relation()
        {
        }

        public Relation(Dictionary<string, T> valCoefs, T indCoef, RelationType type, BpnFactory<T> factory)
        {
            if (valCoefs == null) throw new ArgumentNullException(nameof(valCoefs));
            
            _factory = factory;
            VarCoefs = valCoefs.ToDictionary(c => c.Key, d => factory.Create(_factory.Ops.Zero, d.Value));
            IndCoef =  factory.Create(_factory.Ops.Zero, indCoef);
            Type = type;
        }

        public object Clone()
        {
            var cloneCoefs = VarCoefs.ToDictionary(c => c.Key, d => d.Value);

            var result = new Relation<T>
            {
                VarCoefs = cloneCoefs,
                IndCoef = IndCoef,
                Type = Type,
                _factory = _factory
            };

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
                    result.VarCoefs.Add("u", _factory.CreateMinusOne());
                    result.Type = RelationType.Equality;
                    break;
                case RelationType.LessEqual:
                    result.VarCoefs.Add("u", _factory.CreateOne());
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
            if (uCoef == null || _factory.Ops.Less(uCoef.Value.Fp, _factory.Ops.Zero))
                result.VarCoefs.Add("w", _factory.CreateOne());

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

            for (var i = 0; i < result.VarCoefs.Count; i++)
            {
                var key = result.VarCoefs.Keys.ToList()[i];
                result.VarCoefs[key] = -result.VarCoefs[key];
            }

            result.IndCoef = -result.IndCoef;

            return result;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using SimplexMethod.Bpn;

namespace SimplexMethod
{
    internal class LptMatrix<T> where T: struct
    {
        private readonly string[] _basisVars;
        private readonly string[] _allVars;

        private readonly Bpn<T>[,] _mainMatrix;
        private readonly bool _findMax;

        private readonly BpnFactory<T> _factory;

        public LptMatrix(SimplexLpt<T> lpt, BpnFactory<T> factory)
        {
            _findMax = lpt.FindMax;
            _factory = factory;
            
            //заполнение вектора имён всех переменных
            var allVars = new List<string>();

            allVars.AddRange(lpt.Function.VarCoefs.Select(c => c.Key));
            foreach (var cond in lpt.Conditions)
                allVars.AddRange(cond.VarCoefs.Select(c => c.Key));

            allVars = allVars.Distinct().ToList();
            SortVarArray(allVars);
            _allVars = allVars.ToArray();


            //заполение вектора коэффициентов переменных функции
            var funcCoefs = _factory.CreateArray(_allVars.Length);
            for (int i = 0; i < _allVars.Length; i++)
                if (lpt.Function.VarCoefs.ContainsKey(_allVars[i]))
                    funcCoefs[i] = lpt.Function.VarCoefs[_allVars[i]];

            //заполнение вектора имён базисных переменных
            _basisVars = new string[lpt.Conditions.Count];

            for (int i = 0; i < lpt.Conditions.Count; i++)
                if (lpt.Conditions[i].ContainsW())
                    _basisVars[i] = lpt.Conditions[i].GetCoefNameW();
                else _basisVars[i] = lpt.Conditions[i].GetCoefNameU();

            //заполнение основной матрицы
            #region fillMainMatrix
            
            _mainMatrix = _factory.CreateMatrix(lpt.Conditions.Count + 1, _allVars.Length + 1);

            //заполнение части со свободными членами и части с коэффициентами при переменных условий
            for (int i = 0; i < lpt.Conditions.Count; i++)
            {
                _mainMatrix[i, 0] = lpt.Conditions[i].IndCoef;

                for(int j = 0; j < _allVars.Length; j++)
                    if (lpt.Conditions[i].VarCoefs.ContainsKey(_allVars[j]))
                        _mainMatrix[i, 1 + j] = lpt.Conditions[i].VarCoefs[_allVars[j]];
            }

            //заполнение левого нижнего угла матрицы
            var buffCell = _factory.Create(_factory.Ops.Zero, _factory.Ops.Zero);
            for (var i = 0; i < lpt.Conditions.Count - 1; i++)
            {
                var first = lpt.Conditions[i].ContainsW()
                    ? _factory.CreateMinusM()
                    : _factory.CreateZero();

                var add = first * _mainMatrix[i, 0];
                buffCell += add;
            }

            _mainMatrix[_mainMatrix.GetLength(0) - 1, 0] = buffCell;

            //заполнение последней строки
            for (int i = 1; i < _mainMatrix.GetLength(1); i++)
            {
                buffCell = _factory.CreateZero();

                for (int j = 0; j < _mainMatrix.GetLength(0) - 1; j++)
                    if (_basisVars[j][0] == 'w')
                        buffCell += _factory.CreateMinusM() * _mainMatrix[j, i];
                    //else buffCell += _factory.Create(_factory.Ops.Zero, _factory.Ops.Zero) * MainMatrix[j, i];

                _mainMatrix[_mainMatrix.GetLength(0) - 1, i] += buffCell - funcCoefs[i - 1];
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
                    if (_mainMatrix[_mainMatrix.GetLength(0) - 1, 0].ContainsBigPart())
                        badNoVals = true;
                    
                    break;
                }
                
                //перерасчёт ячеек матрицы
                var colBuffer = _factory.CreateArray(_mainMatrix.GetLength(0));
                
                for (var i = 0; i < _mainMatrix.GetLength(1); i++)
                {
                    if (i == minBottomCol) continue;
                    
                    for (var j = 0; j < _mainMatrix.GetLength(0); j++)
                        colBuffer[j] = _mainMatrix[j, i];

                    for (var j = 0; j < _mainMatrix.GetLength(0); j++)
                        if (j == bestRow.Value)
                            _mainMatrix[j, i] /= _mainMatrix[j, minBottomCol];
                        else
                        {
                            _mainMatrix[j, i] = colBuffer[j] * _mainMatrix[bestRow.Value, minBottomCol];
                            _mainMatrix[j, i] -= _mainMatrix[j, minBottomCol] * colBuffer[bestRow.Value];
                            _mainMatrix[j, i] /= _mainMatrix[bestRow.Value, minBottomCol];
                        }
                }

                for (var i = 0; i < _mainMatrix.GetLength(0); i++)
                {
                    if (i == bestRow.Value)
                    {
                        _mainMatrix[i, minBottomCol] = _factory.CreateOne();
                        continue;
                    }
                    
                    _mainMatrix[i, minBottomCol] = _factory.CreateZero();
                }

                _basisVars[bestRow.Value] = _allVars[minBottomCol - 1];
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
            var matrixRow = _mainMatrix.GetLength(0) - 1;
            
            for (var i = 1; i < _mainMatrix.GetLength(1); i++)
                if (_mainMatrix[matrixRow, i] < _factory.CreateZero())
                    return false;

            return true;
        }

        //получение индекса столбца матрицы MainMatrix с минимальным нижним элементом
        private int GetMinimalBottom()
        {
            var result = 1;
            var matrixRow = _mainMatrix.GetLength(0) - 1;
            
            for (var i = 2; i < _mainMatrix.GetLength(1); i++)
                if (_mainMatrix[matrixRow, i] < _mainMatrix[matrixRow, result])
                    result = i;

            return result;
        }

        //получение индекса опорной строки MainMatrix вместе с результатом деления свободного члена на элемент 
        //Если в столбце не будет строго положительных значений, возвращается null
        private int? GetBestRow(int col)
        {
            var colNums = _factory.CreateArray(_mainMatrix.GetLength(0) - 1);
            for (var i = 0; i < colNums.Length; i++)
                colNums[i] = _mainMatrix[i, col];

            var divs = new Dictionary<int, Bpn<T>>();
            for (var i = 0; i < colNums.Length; i++)
                if (colNums[i] > _factory.CreateZero())
                    divs.Add(i, _mainMatrix[i, 0] / colNums[i]);

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
            if (!(_basisVars.Any(c => c == vrName))) return _factory.Ops.Zero;

            return _mainMatrix[_basisVars.ToList().IndexOf(vrName), 0].Fp;
        }
        
        private LptResult<T> GetFinalAnswer()
        {
            if (_basisVars.Any(c => c[0] == 'w'))
                return new LptResult<T>() {Type = ResultType.NoVals};

            var result = new LptResult<T>
            {
                Type = ResultType.Done,
                Vars = new Dictionary<string, T>()
            };

            var vrVals = _allVars.Where(vr => vr[0] != 'u' && vr[0] != 'w').ToDictionary(vr => vr, GetBasisVarValue);

            foreach (var vrVal in vrVals)
            {
                if (vrVal.Key.Contains('\'')) continue;

                result.Vars.Add(vrVal.Key,
                    vrVals.ContainsKey(vrVal.Key + "\'")
                        ? _factory.Ops.Subst(vrVal.Value, vrVals[vrVal.Key + "\'"])
                        : vrVal.Value);
            }

            result.FuncValue = _factory.Ops.Mul(
                _mainMatrix[_mainMatrix.GetLength(0) - 1, 0].Fp, 
                (_findMax ? _factory.Ops.One : _factory.Ops.MinusOne)
            );

            return result;
        }

        private static void SortVarArray(List<string> arr)
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
}
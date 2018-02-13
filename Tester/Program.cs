using System;
using System.Collections.Generic;
using SimplexMethod;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new SimplexLPT<decimal>(true, new List<string>() {"x1", "x2", "x3", "x4", "x5"});

            test.AddFunction(new Dictionary<string, decimal>() {["x1"] = -3, ["x2"] = 1, ["x3"] = 4});

            test.AddCondition(
                new Dictionary<string, decimal>() {["x2"] = -1, ["x3"] = 1, ["x4"] = 1},
                1,
                RelationType.Equality
            );
            
            test.AddCondition(
                new Dictionary<string, decimal>() {["x1"] = -5, ["x2"] = 1, ["x3"] = 1},
                2,
                RelationType.Equality
            );
            
            test.AddCondition(
                new Dictionary<string, decimal>() {["x1"] = -8, ["x2"] = 1, ["x3"] = 2, ["x5"] = -1},
                3,
                RelationType.Equality
            );

            var result = test.Compute();

            Console.WriteLine("Hello World!");
        }
    }
}
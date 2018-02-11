using System;
using System.Collections.Generic;
using System.Numerics;
using SimplexMethod;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var func = new Relation(
                new Dictionary<string, double>() {["x1"] = 2, ["x2"] = -3},
                0,
                RelationType.Equality
            );
            
            var cond1 = new Relation(
                new Dictionary<string, double>() {["x1"] = 1, ["x2"] = -5},
                -5,
                RelationType.LessEqual
            );
            
            var cond2 = new Relation(
                new Dictionary<string, double>() {["x1"] = 7, ["x2"] = 4},
                28,
                RelationType.LessEqual
            );

            var cond3 = new Relation(
                new Dictionary<string, double>() { ["x1"] = -7, ["x2"] = 2 },
                0,
                RelationType.MoreEqual
            );

            var test = new SimplexLPT(
                func, 
                new List<Relation>(){cond1, cond2, cond3}, 
                new List<string>() {"x1"},
                true
            );
            test.Canonize();
            
            var matrix = new LptMatrix(test);
            var result = matrix.Compute();

            Console.WriteLine("Hello World!");
        }
    }
}
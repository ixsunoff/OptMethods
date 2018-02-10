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
                new Dictionary<string, double>() {["x"] = 1, ["y"] = 2, ["z"] = 3},
                0,
                RelationType.Equality
            );
            
            var cond1 = new Relation(
                new Dictionary<string, double>() {["y"] = 2, ["z"] = 3},
                15,
                RelationType.Equality
            );
            
            var cond2 = new Relation(
                new Dictionary<string, double>() {["x"] = 1, ["z"] = 3},
                10,
                RelationType.MoreEqual
            );
            
            var cond3 = new Relation(
                new Dictionary<string, double>() {["x"] = 1, ["y"] = 2},
                5,
                RelationType.LessEqual
            );
            
            //SimplexLPT test = new SimplexLPT(func, new List<Relation>(){cond1, cond2, cond3});
            //test.Compute();
            Console.WriteLine("Hello World!");
        }
    }
}
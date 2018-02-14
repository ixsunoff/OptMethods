using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimplexMethod;

namespace UnitTest.SimplexMethod
{
    [TestClass]
    public class SimplexTest
    {
        [TestMethod]
        public void GoodTaskFloatTest()
        {
            var testLpt = new SimplexLpt<float>(true, new[] { "x1", "x2", "x3", "x4", "x5" });

            testLpt.AddFunction(new Dictionary<string, float>() { ["x1"] = -3f, ["x2"] = 1, ["x3"] = 4 });

            testLpt.AddCondition(
                new Dictionary<string, float>() { ["x2"] = -1, ["x3"] = 1, ["x4"] = 1 },
                1,
                RelationType.Equality
            );

            testLpt.AddCondition(
                new Dictionary<string, float>() { ["x1"] = -5, ["x2"] = 1, ["x3"] = 1 },
                2,
                RelationType.Equality
            );

            testLpt.AddCondition(
                new Dictionary<string, float>() { ["x1"] = -8, ["x2"] = 1, ["x3"] = 2, ["x5"] = -1 },
                3,
                RelationType.Equality
            );

            var result = testLpt.Compute();

            if (result.Type != ResultType.Done) Assert.Fail("ЗЛП не имеет решений, хотя должна");
            if (result.Vars.Count != 5) Assert.Fail("Не совпадает состав переменных");

            if (Math.Abs(result.Vars["x1"] - 1d) > 0.001d) Assert.Fail("Переменная x1 не принимает нужного значения");
            if (Math.Abs(result.Vars["x2"] - 3d) > 0.001d) Assert.Fail("Переменная x2 не принимает нужного значения");
            if (Math.Abs(result.Vars["x3"] - 4d) > 0.001d) Assert.Fail("Переменная x3 не принимает нужного значения");
            if (Math.Abs(result.Vars["x4"]) > 0.001d) Assert.Fail("Переменная x4 не принимает нужного значения");
            if (Math.Abs(result.Vars["x5"]) > 0.001d) Assert.Fail("Переменная x5 не принимает нужного значения");
        }

        [TestMethod]
        public void GoodTaskDoubleTest()
        {
            var testLpt = new SimplexLpt<double>(true, new[] { "x1", "x2", "x3", "x4", "x5" });

            testLpt.AddFunction(new Dictionary<string, double>() { ["x1"] = -3f, ["x2"] = 1, ["x3"] = 4 });

            testLpt.AddCondition(
                new Dictionary<string, double>() { ["x2"] = -1, ["x3"] = 1, ["x4"] = 1 },
                1,
                RelationType.Equality
            );

            testLpt.AddCondition(
                new Dictionary<string, double>() { ["x1"] = -5, ["x2"] = 1, ["x3"] = 1 },
                2,
                RelationType.Equality
            );

            testLpt.AddCondition(
                new Dictionary<string, double>() { ["x1"] = -8, ["x2"] = 1, ["x3"] = 2, ["x5"] = -1 },
                3,
                RelationType.Equality
            );

            var result = testLpt.Compute();

            if (result.Type != ResultType.Done) Assert.Fail("ЗЛП не имеет решений, хотя должна");
            if (result.Vars.Count != 5) Assert.Fail("Не совпадает состав переменных");

            if (Math.Abs(result.Vars["x1"] - 1d) > 0.001d) Assert.Fail("Переменная x1 не принимает нужного значения");
            if (Math.Abs(result.Vars["x2"] - 3d) > 0.001d) Assert.Fail("Переменная x2 не принимает нужного значения");
            if (Math.Abs(result.Vars["x3"] - 4d) > 0.001d) Assert.Fail("Переменная x3 не принимает нужного значения");
            if (Math.Abs(result.Vars["x4"]) > 0.001d) Assert.Fail("Переменная x4 не принимает нужного значения");
            if (Math.Abs(result.Vars["x5"]) > 0.001d) Assert.Fail("Переменная x5 не принимает нужного значения");
        }

        [TestMethod]
        public void GoodTaskDecimalTest()
        {
            var testLpt = new SimplexLpt<float>(true, new[] { "x1", "x2", "x3", "x4", "x5" });

            testLpt.AddFunction(new Dictionary<string, float>() { ["x1"] = -3f, ["x2"] = 1, ["x3"] = 4 });

            testLpt.AddCondition(
                new Dictionary<string, float>() { ["x2"] = -1, ["x3"] = 1, ["x4"] = 1 },
                1,
                RelationType.Equality
            );

            testLpt.AddCondition(
                new Dictionary<string, float>() { ["x1"] = -5, ["x2"] = 1, ["x3"] = 1 },
                2,
                RelationType.Equality
            );

            testLpt.AddCondition(
                new Dictionary<string, float>() { ["x1"] = -8, ["x2"] = 1, ["x3"] = 2, ["x5"] = -1 },
                3,
                RelationType.Equality
            );

            var result = testLpt.Compute();

            if (result.Type != ResultType.Done) Assert.Fail("ЗЛП не имеет решений, хотя должна");
            if (result.Vars.Count != 5) Assert.Fail("Не совпадает состав переменных");
            
            if (Math.Abs(Math.Round(result.Vars["x1"], 3) - 1d) > 0.001d) Assert.Fail("Переменная x1 не принимает нужного значения");
            if (Math.Abs(Math.Round(result.Vars["x2"], 3) - 3d) > 0.001d) Assert.Fail("Переменная x2 не принимает нужного значения");
            if (Math.Abs(Math.Round(result.Vars["x3"], 3) - 4d) > 0.001d) Assert.Fail("Переменная x3 не принимает нужного значения");
            if (Math.Abs(Math.Round(result.Vars["x4"], 3)) > 0.001d) Assert.Fail("Переменная x4 не принимает нужного значения");
            if (Math.Abs(Math.Round(result.Vars["x5"], 3)) > 0.001d) Assert.Fail("Переменная x5 не принимает нужного значения");
        }

        [TestMethod]
        public void InfiniteMaxTest()
        {
            var test = new SimplexLpt<double>(false, new[] {"x1", "x2"});

            test.AddFunction(new Dictionary<string, double>() {["x1"] = 1, ["x2"] = -1});

            test.AddCondition(
                new Dictionary<string, double>() {["x1"] = 2, ["x2"] = -5},
                10,
                RelationType.LessEqual
            );

            test.AddCondition(
                new Dictionary<string, double>() { ["x1"] = 1, ["x2"] = 1},
                1,
                RelationType.MoreEqual
            );

            test.AddCondition(
                new Dictionary<string, double>() { ["x1"] = 3, ["x2"] = -2 },
                -6,
                RelationType.MoreEqual
            );

            switch (test.Compute().Type)
            {
                case ResultType.Done:
                    Assert.Fail("Вместо ResultType.InfiniteMax был получен ResultType.Done");
                    break;
                case ResultType.NoVals:
                    Assert.Fail("Вместо ResultType.InfiniteMax был получен ResultType.NoVals");
                    break;
                case ResultType.InfiniteMax:
                    break;
            }
        }
    

        [TestMethod]
        public void NoValsTest()
        {
            var test = new SimplexLpt<double>(true, new List<string>());

            test.AddFunction(new Dictionary<string, double>() {["x1"] = 5, ["x2"] = -2});

            test.AddCondition(
                new Dictionary<string, double>() {["x1"] = 1, ["x2"] = -2},
                -4,
                RelationType.LessEqual
            );

            test.AddCondition(
                new Dictionary<string, double>() { ["x1"] = 2, ["x2"] = 1 },
                6,
                RelationType.MoreEqual
            );

            test.AddCondition(
                new Dictionary<string, double>() { ["x1"] = 1, ["x2"] = -2 },
                4,
                RelationType.MoreEqual
            );

            switch (test.Compute().Type)
            {
                case ResultType.Done:
                    Assert.Fail("Вместо ResultType.NoVals был получен ResultType.Done");
                    break;
                case ResultType.NoVals:
                    break;
                case ResultType.InfiniteMax:
                    Assert.Fail("Вместо ResultType.NoVals был получен ResultType.InfiniteMax");
                    break;
            }
        }

}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CurrencyConverter.Tests
{
    public class AlgorithmTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        private readonly List<Tuple<string, string, double>> _tuples= new()
        {
            new("USD","CAD",1.34),
            new("CAD","GBP",0.58),
            new("USD","EUR",0.86),
            new("GBP","IIR",300000.25),
            new("GBP","JPY",2.5),
            new("CNY", "AUD", 0.2)
        };

        public AlgorithmTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            Algorithms.CurrencyConverter.Instance.UpdateConfiguration(_tuples);
        }

        private void PrintResult(string from, string to, double amount, double result, double testResult)
        {
            var isPassed = Math.Abs(result - testResult) < 1;
            var msg = isPassed ? "Passed" : "Not Passed";
            Console.ForegroundColor = isPassed ? ConsoleColor.Green : ConsoleColor.Red;
            _testOutputHelper.WriteLine(
                $"Conversion from {from} to {to} with amount of {amount} should be {result}. Test result is {testResult} |{msg}|");
            Console.ForegroundColor = ConsoleColor.White;
            //_testOutputHelper.WriteLine("—————————————————————");
        }

        [Theory]
        [InlineData(50, 8700007.25)]
        public void Conversion_From_CAD_IIR(double amount, double result)
        {
            // CAD=>IIR amount (50)-> CAD,GBP 50*0.58= 29 -> GBP,IIR 29*300000.25= 8,700,007.25‬
            string from = "CAD", to = "IIR";
            var res= Algorithms.CurrencyConverter.Instance.Convert(from, to, amount);
            PrintResult(from, to, amount, result, res);
            Assert.True(Math.Abs(res - result) < 1);
        }
        [Theory]
        [InlineData(8700007.25, 50)]
        public void Conversion_From_IIR_CAD(double amount, double result)
        {
            // IIR=>CAD amount (8700007.25‬) => GBP,IIR 8700007.25/300000.25 = 29 -> CAD,GBP 29/0.58=50
            string from = "IIR", to = "CAD";
            var res = Algorithms.CurrencyConverter.Instance.Convert(from, to, amount);
            PrintResult(from, to, amount, result, res);
            Assert.True(Math.Abs(res - result) < 1);
        }
        [Theory]
        [InlineData(50, 32)]
        public void Conversion_From_CAD_EUR(double amount, double result)
        {
            // CAD=>EUR amount (50) => CAD,USD 50/1.34 = 37.31 -> USD,EUR 37.31*0.86=32.08
            string from = "CAD", to = "EUR";
            var res = Algorithms.CurrencyConverter.Instance.Convert(from, to, amount);
            PrintResult(from, to, amount, result, res);
            Assert.True(Math.Abs(res - result) < 1);
        }
        [Theory]
        [InlineData(32, 50)]
        public void Conversion_From_EUR_CAD(double amount, double result)
        {
            // CAD=>EUR amount (50) => CAD,USD 50/1.34 = 37.31 -> USD,EUR 37.31*0.86=32.08
            string from = "EUR", to = "CAD";
            var res = Algorithms.CurrencyConverter.Instance.Convert(from, to, amount);
            PrintResult(from, to, amount, result, res);
            Assert.True(Math.Abs(res - result) < 1);
        }
        [Theory]
        [InlineData(340000, 1.2)]
        public void Conversion_From_IIR_EUR(double amount, double result)
        {
            string from = "IIR", to = "EUR";
            var res = Algorithms.CurrencyConverter.Instance.Convert(from, to, amount);
            PrintResult(from, to, amount, result, res);
            Assert.True(Math.Abs(res - result) < 1);
        }
        [Theory]
        [InlineData(10, 8.6)]
        public void Conversion_From_USD_EUR(double amount, double result)
        {
            string from = "USD", to = "EUR";
            var res = Algorithms.CurrencyConverter.Instance.Convert(from, to, amount);
            PrintResult(from, to, amount, result, res);
            Assert.True(Math.Abs(res - result) < 1);
        }
        [Theory]
        [InlineData(10, 10)]
        public void Conversion_From_USD_USD(double amount, double result)
        {
            string from = "USD", to = "USD";
            var res = Algorithms.CurrencyConverter.Instance.Convert(from, to, amount);
            PrintResult(from, to, amount, result, res);
            Assert.True(Math.Abs(res - result) < 1);
        }
        [Theory]
        [InlineData(10, -1)]
        public void Conversion_From_AUD_USD(double amount, double result)
        {
            string from = "AUD", to = "USD";
            var res = Algorithms.CurrencyConverter.Instance.Convert(from, to, amount);
            PrintResult(from, to, amount, result, res);
            Assert.True(Math.Abs(res - (result)) < 1);
        }
        [Theory]
        [InlineData(1000, 1)]
        //Train_Model
        public void Conversion_Concurrency1(int threadCount, int maxAmount)
        {
            RunThreads(threadCount, maxAmount);
        }
        [Theory]
        [InlineData(1000000, 100)]
        public void Conversion_Concurrency2(int threadCount, int maxAmount)
        {
            RunThreads(threadCount, maxAmount);
        }
        [Theory]
        [InlineData(10000, 10000000)]
        public void Conversion_Concurrency3(int threadCount, int maxAmount)
        {
            RunThreads(threadCount, maxAmount);
        }

        private void RunThreads(int threadCount, int maxAmount)
        {
            var timer = new Stopwatch();
            timer.Start();
            var random = new Random();
            var list = new List<string> { "USD", "CAD", "GBP", "EUR", "IIR", "JPY", "CNY", "AUD" };
            var cnt = list.Count;
            List<Task<TaskResult>> tasks = new List<Task<TaskResult>>();
            for (int i = 0; i < threadCount; i++)
            {
                tasks.Add(
                    Task.Factory.StartNew(() =>
                    {
                        var extimer = new Stopwatch();
                        extimer.Start();
                        var indexFrom = random.Next(cnt);
                        var indexTo = random.Next(cnt);
                        var amount = random.Next(maxAmount);
                        var res = Algorithms.CurrencyConverter.Instance.Convert(list[indexFrom], list[indexTo], amount);
                        extimer.Stop();
                        return new TaskResult
                        {
                            From = list[indexFrom],
                            To = list[indexTo],
                            Amount = amount,
                            Result = res,
                            Duration = extimer.Elapsed
                        };
                    })
                    );
                //batch operation 
                //if (i % 100000 == 0)
                //{
                //    Task.WaitAll(tasks.ToArray());
                //    tasks = new List<Task>();
                //}
            }
            Task.WaitAll(tasks.ToArray());
            timer.Stop();
            _testOutputHelper.WriteLine($"Conversion concurrency for {threadCount} threads and max amount of {maxAmount} took {timer.Elapsed}. " +
                                        $"The max took {tasks.Max(c => c.Result.Duration)}, " +
                                        $"min took {tasks.Min(c => c.Result.Duration)} " +
                                        $"average took {new TimeSpan(Convert.ToInt64(tasks.Select(c => c.Result.Duration).Average(t => t.Ticks)))}");


        }

    }

    public class TaskResult
    {
        public string From { get; set; }
        public string To { get; set; }
        public double Amount { get; set; }
        public double Result { get; set; }
        public  TimeSpan Duration { get; set; }
    }
}

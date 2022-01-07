using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Xunit;
using Xunit.Runners;

namespace CurrencyConverter
{
    internal class Program
    {
        private static readonly object ConsoleLock = new();

        private static readonly ManualResetEvent Finished = new(false);

        private static int _result;

        private static int Main(string[] args)
        {
            using var runner = AssemblyRunner.WithoutAppDomain("CurrencyConverter.Tests.dll");
            runner.OnDiscoveryComplete = OnDiscoveryComplete;
            runner.OnExecutionComplete = OnExecutionComplete;
            runner.OnTestFailed = OnTestFailed;
            runner.OnTestSkipped = OnTestSkipped;
            runner.OnTestOutput = OnTestOutput;

            Console.WriteLine("Discovering...");
            runner.Start();
            Finished.WaitOne();
            Finished.Dispose();

            return _result;
        }

        private static void OnTestOutput(TestOutputInfo obj)
        {
            Console.WriteLine(obj.Output);
        }

        private static void OnDiscoveryComplete(DiscoveryCompleteInfo info)
        {
            lock (ConsoleLock)
                Console.WriteLine($"Running {info.TestCasesToRun} of {info.TestCasesDiscovered} tests...");
        }

        private static void OnExecutionComplete(ExecutionCompleteInfo info)
        {
            lock (ConsoleLock)
                Console.WriteLine($"Finished: {info.TotalTests} tests in {Math.Round(info.ExecutionTime, 3)}s ({info.TestsFailed} failed, {info.TestsSkipped} skipped)");

            Finished.Set();
        }

        private static void OnTestFailed(TestFailedInfo info)
        {
            lock (ConsoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("[FAIL] {0}: {1}", info.TestDisplayName, info.ExceptionMessage);
                if (info.ExceptionStackTrace != null)
                    Console.WriteLine(info.ExceptionStackTrace);

                Console.ResetColor();
            }

            _result = 1;
        }

        private static void OnTestSkipped(TestSkippedInfo info)
        {
            lock (ConsoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[SKIP] {0}: {1}", info.TestDisplayName, info.SkipReason);
                Console.ResetColor();
            }
        }
    }
}

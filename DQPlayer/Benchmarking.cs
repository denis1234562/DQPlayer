using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DQPlayer
{
    public static class Benchmarking
    {
        public static void Benchmark<T>(int cycles, int runs, Func<T> method)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            List<long> benchmarkSpeed = new List<long>();
            for (int i = 0; i < cycles; i++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                for (int j = 0; j < runs; j++)
                {
                    var result = method();
                }
                sw.Stop();
                benchmarkSpeed.Add(sw.ElapsedMilliseconds);
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Slowest: {benchmarkSpeed.Max()}");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Average: {benchmarkSpeed.Average()}");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Fastest: {benchmarkSpeed.Min()}");

            Console.ForegroundColor = originalColor;
        }

        public static void Benchmark(int cycles, int runs, Action method)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            List<long> benchmarkSpeed = new List<long>();
            for (int i = 0; i < cycles; i++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                for (int j = 0; j < runs; j++)
                {
                    method();
                }
                sw.Stop();
                benchmarkSpeed.Add(sw.ElapsedMilliseconds);
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Slowest: {benchmarkSpeed.Max()}");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Average: {benchmarkSpeed.Average()}");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Fastest: {benchmarkSpeed.Min()}");

            Console.ForegroundColor = originalColor;
        }
    }
}

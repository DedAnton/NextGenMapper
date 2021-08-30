using Benchmark.Benchmarks;
using BenchmarkDotNet.Running;

namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<MapDesignerBenchmark>();
            //BenchmarkRunner.Run<Experiments>();

            //OldBigBanchmark.Run(1000);
            // 1    - 1.2225642
            // 10   - 1.3913829
            // 100  - 2.0936369
            // 1000 - 13.1445635
        }
    }
}

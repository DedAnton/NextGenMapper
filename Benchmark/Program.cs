using Benchmark.Benchmarks;
using Benchmark.Benchmarks.Experiments;
using BenchmarkDotNet.Running;
using System.Runtime.CompilerServices;
using System.Text;

namespace Benchmark;

internal class Program
{
    private static void Main()
    {
        //BenchmarkRunner.Run<GetSymbolDeclaration>();

        //var a = 100;
        //var b = (object)a;
        //var c = Unsafe.Unbox<int>(b);
        //var d = Unsafe.As<int, long>(ref Unsafe.Unbox<int>(c));
        //Console.WriteLine(c);
        //Console.WriteLine(d);
        OldBigBanchmark.Run(10000);
        // 1    - 00:00:00.9161159 -> 00:00:00.8977854
        // 10   - 00:00:00.9511389 -> 00:00:00.9611821
        // 100  - 00:00:01.6390826 -> 00:00:01.5233820
        // 1000 - 00:00:13.1432450 -> 00:00:08.8775675
    }
}
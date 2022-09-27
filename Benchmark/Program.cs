using Benchmark.Benchmarks;
using Benchmark.Benchmarks.Experiments;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using NextGenMapper.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Benchmark;

internal class Program
{
    private static void Main()
    {
        //BenchmarkRunner.Run<MapDesignerBenchmark>();
        //BenchmarkRunner.Run<CircleReferencesList>();
        //BenchmarkRunner.Run<ListStringEquality>();
        //BenchmarkRunner.Run<WhereSelect>();
        //BenchmarkRunner.Run<Iterate>();
        //BenchmarkRunner.Run<Sorting>();
        //BenchmarkRunner.Run<FirstOrDefault>();
        //BenchmarkRunner.Run<SpanTests>();
        //BenchmarkRunner.Run<StringComparision>();
        //BenchmarkRunner.Run<CharComparision>();
        //BenchmarkRunner.Run<CodeGeneratorsBenchmark>();
        //BenchmarkRunner.Run<CollectionCount>();
        //BenchmarkRunner.Run<SelectTests>();
        BenchmarkRunner.Run<ForCollectionsMapping>();

        //var benchmark = new MapDesignerBenchmark();
        //var mapPair = benchmark.GenerateCommonClassesMapPairs().ToArray().First(x => x.Name == "props_100_init");
        //GC.Collect();

        //var asd = benchmark.Properties(mapPair);

        //OldBigBanchmark.Run(1000);
        // 1    - 00:00:00.9161159 -> 00:00:00.8977854
        // 10   - 00:00:00.9511389 -> 00:00:00.9611821
        // 100  - 00:00:01.6390826 -> 00:00:01.5233820
        // 1000 - 00:00:13.1432450 -> 00:00:08.8775675
       
    }
}
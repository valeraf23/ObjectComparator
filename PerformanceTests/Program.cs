using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using ObjectsComparator.Comparator;
using ObjectsComparator.Tests.TestModels;

namespace PerformanceTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<TestPerformace>();
        }
    }

    [MemoryDiagnoser]
    [DisassemblyDiagnoser(maxDepth:3,exportCombinedDisassemblyReport:true)]
    [SimpleJob(RunStrategy.Monitoring, launchCount: 10, warmupCount: 0, targetCount: 100)]
    public class TestPerformace
    {
        [Benchmark]
        public void Test()
        {
            d1DigitalClock.GetDistinctions(_dDigitalClock);
        }
        private readonly DigitalClock d1DigitalClock = new DigitalClock(true, new[] { 1, 2 },
            new Calendar(3, new Time("2016", 1.5F, 3, 1.2, new List<string> { "", "" }, 4, 34)), "2015", 1.2F, 11,
            1.12,
            new List<string> { "df", "asd" }, 1, 9);

        private readonly DigitalClock _dDigitalClock = new DigitalClock(true, new[] { 1, 2 },
            new Calendar(3, new Time("2016", 1.5F, 3, 1.2, new List<string> { "", "" }, 4, 34)), "2015", 1.2F, 11, 1.12,
            new List<string> { "df", "asd" }, 1, 9);

    }
}

using IT.Redis.Lua.Benchmarks;

var bench = new Benchmark();

await bench.Lua();

await bench.Tran();

BenchmarkDotNet.Running.BenchmarkRunner.Run(typeof(Benchmark));

Console.WriteLine("End....");
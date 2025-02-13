```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4751/23H2/2023Update/SunValley3)
12th Gen Intel Core i7-12700F, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2


```
| Method                    | Mean     | Error    | StdDev   | Gen0   | Allocated |
|-------------------------- |---------:|---------:|---------:|-------:|----------:|
| FilterServiceWithCache    | 58.96 μs | 0.646 μs | 0.604 μs | 1.4648 |  19.22 KB |
| FilterServiceWithoutCache | 91.51 μs | 1.500 μs | 1.330 μs | 3.9063 |  50.76 KB |

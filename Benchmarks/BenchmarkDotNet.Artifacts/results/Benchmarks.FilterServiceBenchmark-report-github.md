```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4460/23H2/2023Update/SunValley3)
12th Gen Intel Core i7-12700F, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.100
  [Host]     : .NET 8.0.11 (8.0.1124.51707), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.11 (8.0.1124.51707), X64 RyuJIT AVX2


```
| Method                    | Mean     | Error    | StdDev   | Gen0   | Allocated |
|-------------------------- |---------:|---------:|---------:|-------:|----------:|
| FilterServiceWithCache    | 39.65 μs | 0.533 μs | 0.498 μs | 0.6714 |   9.04 KB |
| FilterServiceWithoutCache | 73.14 μs | 0.857 μs | 0.802 μs | 3.1738 |  41.13 KB |

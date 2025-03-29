```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.5039/23H2/2023Update/SunValley3)
12th Gen Intel Core i7-12700F, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.104
  [Host]     : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2


```
| Method                                  | Mean        | Error     | StdDev    | Gen0   | Allocated |
|---------------------------------------- |------------:|----------:|----------:|-------:|----------:|
| FilterServiceWithCache                  |  6,819.5 ns | 116.76 ns | 204.50 ns | 0.6714 |    8993 B |
| FilterServiceWithCacheAndFilterCache    |  6,759.2 ns |  94.05 ns |  83.37 ns | 0.7324 |    9633 B |
| FilterServiceWithoutCache               | 25,342.3 ns | 499.54 ns | 762.85 ns | 3.0518 |   41285 B |
| FilterServiceOneMatchOkWithCache        |    139.1 ns |   2.49 ns |   2.33 ns | 0.0317 |     416 B |
| FilterServiceOneMatchAndOkWithCache     |    416.1 ns |   6.85 ns |   6.40 ns | 0.0787 |    1032 B |
| FilterServiceOneMatchFailedWithCache    |    385.3 ns |   7.66 ns |  16.49 ns | 0.0863 |    1128 B |
| FilterServiceOneMatchAndFailedWithCache |    630.7 ns |  12.57 ns |  28.37 ns | 0.1459 |    1912 B |

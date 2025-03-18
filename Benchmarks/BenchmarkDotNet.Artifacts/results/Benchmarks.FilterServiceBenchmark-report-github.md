```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.5039/23H2/2023Update/SunValley3)
12th Gen Intel Core i7-12700F, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.104
  [Host]     : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2


```
| Method                                  | Mean        | Error     | StdDev    | Gen0   | Allocated |
|---------------------------------------- |------------:|----------:|----------:|-------:|----------:|
| FilterServiceWithCache                  | 11,892.7 ns | 217.68 ns | 203.61 ns | 0.8545 |   11641 B |
| FilterServiceWithoutCache               | 29,026.1 ns | 561.09 ns | 822.44 ns | 3.1738 |   43931 B |
| FilterServiceOneMatchOkWithCache        |    124.2 ns |   2.50 ns |   3.97 ns | 0.0317 |     416 B |
| FilterServiceOneMatchAndOkWithCache     |    380.3 ns |   4.82 ns |   4.51 ns | 0.0787 |    1032 B |
| FilterServiceOneMatchFailedWithCache    |  3,350.6 ns |  37.30 ns |  34.89 ns | 0.2022 |    2664 B |
| FilterServiceOneMatchAndFailedWithCache |  4,514.8 ns |  71.88 ns |  67.23 ns | 0.2899 |    3944 B |

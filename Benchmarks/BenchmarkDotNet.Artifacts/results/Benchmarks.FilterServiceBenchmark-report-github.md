```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.5039/23H2/2023Update/SunValley3)
12th Gen Intel Core i7-12700F, 1 CPU, 20 logical and 12 physical cores
.NET SDK 9.0.104
  [Host]     : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2


```
| Method                                  | Mean        | Error     | StdDev    | Gen0   | Allocated |
|---------------------------------------- |------------:|----------:|----------:|-------:|----------:|
| FilterServiceWithCache                  | 12,708.5 ns | 253.43 ns | 237.06 ns | 0.8545 |   11642 B |
| FilterServiceWithCacheAndFilterCache    | 16,744.0 ns | 334.56 ns | 410.87 ns | 1.0376 |   14154 B |
| FilterServiceWithoutCache               | 31,130.7 ns | 505.13 ns | 447.79 ns | 3.2959 |   43933 B |
| FilterServiceOneMatchOkWithCache        |    137.7 ns |   2.77 ns |   6.70 ns | 0.0317 |     416 B |
| FilterServiceOneMatchAndOkWithCache     |    421.1 ns |   8.25 ns |  11.29 ns | 0.0787 |    1032 B |
| FilterServiceOneMatchFailedWithCache    |  3,551.2 ns |  47.25 ns |  44.20 ns | 0.2022 |    2664 B |
| FilterServiceOneMatchAndFailedWithCache |  4,768.9 ns |  93.18 ns | 121.16 ns | 0.2899 |    3944 B |

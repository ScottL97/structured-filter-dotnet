```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4351)
Unknown processor
.NET SDK 9.0.202
  [Host]     : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method                                  | Mean         | Error     | StdDev    | Gen0   | Allocated |
|---------------------------------------- |-------------:|----------:|----------:|-------:|----------:|
| FilterServiceWithCache                  |  4,364.31 ns | 10.421 ns |  9.748 ns | 0.1373 |    7008 B |
| FilterServiceWithCacheAndFilterCache    |  4,565.58 ns | 13.594 ns | 12.716 ns | 0.1526 |    8008 B |
| FilterServiceWithoutCache               | 17,089.16 ns | 54.680 ns | 51.148 ns | 0.7019 |   36257 B |
| FilterServiceOneMatchOkWithCache        |     90.58 ns |  0.415 ns |  0.388 ns | 0.0054 |     272 B |
| FilterServiceOneMatchAndOkWithCache     |    314.35 ns |  1.247 ns |  1.105 ns | 0.0153 |     784 B |
| FilterServiceOneMatchFailedWithCache    |    183.39 ns |  1.183 ns |  0.924 ns | 0.0167 |     840 B |
| FilterServiceOneMatchAndFailedWithCache |    437.32 ns |  1.887 ns |  1.765 ns | 0.0286 |    1448 B |

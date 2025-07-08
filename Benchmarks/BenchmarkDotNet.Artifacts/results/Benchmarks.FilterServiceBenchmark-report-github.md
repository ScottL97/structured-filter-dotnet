```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4351)
Unknown processor
.NET SDK 9.0.202
  [Host]     : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method                                  | Mean         | Error     | StdDev    | Gen0   | Allocated |
|---------------------------------------- |-------------:|----------:|----------:|-------:|----------:|
| FilterServiceWithCache                  |  4,458.61 ns | 10.979 ns |  9.732 ns | 0.1678 |    8736 B |
| FilterServiceWithCacheAndFilterCache    |  4,533.27 ns | 23.769 ns | 22.234 ns | 0.1831 |    9376 B |
| FilterServiceWithoutCache               | 17,230.56 ns | 82.953 ns | 77.595 ns | 0.7324 |   37985 B |
| FilterServiceOneMatchOkWithCache        |     98.86 ns |  0.567 ns |  0.530 ns | 0.0082 |     416 B |
| FilterServiceOneMatchAndOkWithCache     |    293.54 ns |  2.140 ns |  2.002 ns | 0.0196 |    1000 B |
| FilterServiceOneMatchFailedWithCache    |    257.20 ns |  1.617 ns |  1.434 ns | 0.0224 |    1128 B |
| FilterServiceOneMatchAndFailedWithCache |    446.23 ns |  2.820 ns |  2.638 ns | 0.0372 |    1880 B |

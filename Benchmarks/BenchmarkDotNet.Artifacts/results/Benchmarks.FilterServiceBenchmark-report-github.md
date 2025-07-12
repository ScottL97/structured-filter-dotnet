```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4652)
Unknown processor
.NET SDK 9.0.205
  [Host]     : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method                                  | Mean         | Error     | StdDev    | Gen0   | Allocated |
|---------------------------------------- |-------------:|----------:|----------:|-------:|----------:|
| FilterServiceWithCache                  |  3,412.77 ns | 16.758 ns | 13.084 ns | 0.0458 |    3048 B |
| FilterServiceWithCacheAndFilterCache    |  3,577.68 ns | 12.257 ns | 10.866 ns | 0.0763 |    4240 B |
| FilterServiceWithoutCache               | 19,213.69 ns | 96.408 ns | 80.505 ns | 0.7935 |   41049 B |
| FilterServiceOneMatchOkWithCache        |     66.29 ns |  0.150 ns |  0.140 ns |      - |         - |
| FilterServiceOneMatchAndOkWithCache     |    135.65 ns |  0.277 ns |  0.259 ns |      - |         - |
| FilterServiceOneMatchFailedWithCache    |    124.65 ns |  0.954 ns |  0.846 ns | 0.0100 |     504 B |
| FilterServiceOneMatchAndFailedWithCache |    224.53 ns |  1.213 ns |  1.134 ns | 0.0122 |     616 B |

```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4351)
Unknown processor
.NET SDK 9.0.202
  [Host]     : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method                                  | Mean        | Error    | StdDev   | Gen0   | Allocated |
|---------------------------------------- |------------:|---------:|---------:|-------:|----------:|
| FilterServiceWithCache                  |  4,690.3 ns | 18.15 ns | 16.09 ns | 0.1526 |    7872 B |
| FilterServiceWithCacheAndFilterCache    |  4,648.6 ns | 18.65 ns | 17.45 ns | 0.1678 |    8584 B |
| FilterServiceWithoutCache               | 17,452.4 ns | 53.52 ns | 47.44 ns | 0.7324 |   37121 B |
| FilterServiceOneMatchOkWithCache        |    100.9 ns |  0.29 ns |  0.27 ns | 0.0068 |     344 B |
| FilterServiceOneMatchAndOkWithCache     |    302.3 ns |  1.37 ns |  1.29 ns | 0.0181 |     928 B |
| FilterServiceOneMatchFailedWithCache    |    277.6 ns |  4.65 ns |  4.35 ns | 0.0181 |     912 B |
| FilterServiceOneMatchAndFailedWithCache |    444.8 ns |  1.32 ns |  1.23 ns | 0.0315 |    1592 B |

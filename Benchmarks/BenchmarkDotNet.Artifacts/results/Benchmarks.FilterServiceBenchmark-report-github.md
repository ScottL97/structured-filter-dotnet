```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4351)
Unknown processor
.NET SDK 9.0.202
  [Host]     : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method                                  | Mean         | Error     | StdDev    | Gen0   | Allocated |
|---------------------------------------- |-------------:|----------:|----------:|-------:|----------:|
| FilterServiceWithCache                  |  4,439.29 ns | 18.268 ns | 16.194 ns | 0.1526 |    7872 B |
| FilterServiceWithCacheAndFilterCache    |  4,657.05 ns | 23.554 ns | 18.390 ns | 0.1678 |    8584 B |
| FilterServiceWithoutCache               | 17,195.68 ns | 93.913 ns | 87.846 ns | 0.7324 |   37121 B |
| FilterServiceOneMatchOkWithCache        |     88.33 ns |  0.640 ns |  0.599 ns | 0.0068 |     344 B |
| FilterServiceOneMatchAndOkWithCache     |    309.68 ns |  0.977 ns |  0.866 ns | 0.0181 |     928 B |
| FilterServiceOneMatchFailedWithCache    |    184.58 ns |  0.604 ns |  0.536 ns | 0.0181 |     912 B |
| FilterServiceOneMatchAndFailedWithCache |    448.97 ns |  2.203 ns |  2.060 ns | 0.0315 |    1592 B |

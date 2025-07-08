```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4351)
Unknown processor
.NET SDK 9.0.202
  [Host]     : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method                                  | Mean         | Error      | StdDev     | Gen0   | Allocated |
|---------------------------------------- |-------------:|-----------:|-----------:|-------:|----------:|
| FilterServiceWithCache                  |  4,553.84 ns |  21.678 ns |  20.278 ns | 0.1678 |    8992 B |
| FilterServiceWithCacheAndFilterCache    |  4,595.76 ns |  31.501 ns |  29.466 ns | 0.1831 |    9632 B |
| FilterServiceWithoutCache               | 18,315.18 ns | 131.014 ns | 122.550 ns | 0.7935 |   41281 B |
| FilterServiceOneMatchOkWithCache        |     97.97 ns |   0.479 ns |   0.448 ns | 0.0082 |     416 B |
| FilterServiceOneMatchAndOkWithCache     |    295.83 ns |   1.266 ns |   1.123 ns | 0.0205 |    1032 B |
| FilterServiceOneMatchFailedWithCache    |    265.44 ns |   0.992 ns |   0.928 ns | 0.0224 |    1128 B |
| FilterServiceOneMatchAndFailedWithCache |    448.82 ns |   1.931 ns |   1.807 ns | 0.0377 |    1912 B |

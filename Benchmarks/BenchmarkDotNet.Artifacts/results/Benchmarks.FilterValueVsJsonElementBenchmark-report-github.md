```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4652)
Unknown processor
.NET SDK 9.0.205
  [Host]     : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method                      | Mean       | Error     | StdDev    | Gen0   | Allocated |
|---------------------------- |-----------:|----------:|----------:|-------:|----------:|
| JsonElement_MatchIn_Double  | 57.7650 ns | 0.2358 ns | 0.2206 ns |      - |         - |
| FilterValue_MatchIn_Double  |  1.8525 ns | 0.0064 ns | 0.0056 ns |      - |         - |
| JsonElement_MatchEq_String  | 12.9657 ns | 0.0300 ns | 0.0281 ns |      - |         - |
| FilterValue_MatchEq_String  |  1.3967 ns | 0.0066 ns | 0.0061 ns |      - |         - |
| JsonElement_GetInt64        |  4.9113 ns | 0.0132 ns | 0.0124 ns |      - |         - |
| FilterValue_GetInt64        |  0.1840 ns | 0.0028 ns | 0.0026 ns |      - |         - |
| JsonElement_EnumerateArray  |  3.9410 ns | 0.0619 ns | 0.0579 ns |      - |         - |
| FilterValue_EnumerateArray  |  0.5715 ns | 0.0040 ns | 0.0038 ns |      - |         - |
| JsonElement_ToString_Double | 12.7280 ns | 0.0386 ns | 0.0342 ns | 0.0006 |      32 B |
| FilterValue_ToString_Double |  0.1985 ns | 0.0056 ns | 0.0052 ns |      - |         - |
| JsonElement_ToString_String | 11.8260 ns | 0.0359 ns | 0.0335 ns | 0.0006 |      32 B |
| FilterValue_ToString_String |  0.0271 ns | 0.0034 ns | 0.0032 ns |      - |         - |
| JsonElement_ToString_Array  | 16.7753 ns | 0.1050 ns | 0.0877 ns | 0.0016 |      80 B |
| FilterValue_ToString_Array  |  0.1954 ns | 0.0020 ns | 0.0016 ns |      - |         - |

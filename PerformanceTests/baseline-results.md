# Performance optimization — before/after (2026-06-09)

BenchmarkDotNet ShortRun, net8.0, `[MemoryDiagnoser]`.

## Before (baseline, commit on top of e7390ae)

| Method                               | Mean         | Gen0     | Allocated  |
|------------------------------------- |-------------:|---------:|-----------:|
| Set_Strategy_For_Collection_Ref_Type |   131.842 us |   5.3711 |   23.61 KB |
| Enumerable                           |    56.092 us |   3.0518 |   12.74 KB |
| EnumerableValueType                  |    32.866 us |   3.0518 |   12.85 KB |
| RepeatedSimpleObject_Equal           |     8.693 us |   1.3504 |    5.54 KB |
| RepeatedSimpleObject_Distinct        |     8.043 us |   1.3885 |    5.67 KB |
| LargeIntList                         | 6,163.170 us | 937.5000 | 3843.08 KB |
| LargeObjectList                      | 1,631.567 us | 181.6406 |  744.19 KB |
| Dictionary_StringKey                 | 2,588.261 us | 371.0938 | 1525.84 KB |
| Dictionary_ComplexKey                |   384.725 us |  84.9609 |  348.07 KB |
| DeepGraph                            |     9.170 us |   1.5564 |     6.4 KB |
| SameReference                        |     4.722 us |   1.0223 |    4.19 KB |

## After (all optimization steps applied)

| Method                               | Mean         | Gen0     | Allocated  |
|------------------------------------- |-------------:|---------:|-----------:|
| Set_Strategy_For_Collection_Ref_Type |    40.322 us |   2.6855 |   11.46 KB |
| Enumerable                           |    12.654 us |   1.4191 |    5.84 KB |
| EnumerableValueType                  |    12.695 us |   2.1057 |    8.65 KB |
| RepeatedSimpleObject_Equal           |     5.308 us |   0.9155 |    3.76 KB |
| RepeatedSimpleObject_Distinct        |     4.749 us |   0.9613 |    3.93 KB |
| LargeIntList                         | 5,653.794 us | 625.0000 |  2573.8 KB |
| LargeObjectList                      | 1,378.367 us | 162.1094 |  667.73 KB |
| Dictionary_StringKey                 | 2,333.668 us | 324.2188 | 1338.28 KB |
| Dictionary_ComplexKey                |   422.714 us |  82.5195 |  338.85 KB |
| DeepGraph                            |     5.075 us |   0.9308 |    3.81 KB |
| SameReference                        |     1.563 us |   0.6599 |     2.7 KB |

## Delta

| Method                               | Mean        | Allocated |
|------------------------------------- |------------:|----------:|
| Set_Strategy_For_Collection_Ref_Type | **-69%**    | -51%      |
| Enumerable                           | **-77%**    | -54%      |
| EnumerableValueType                  | **-61%**    | -33%      |
| RepeatedSimpleObject_Equal           | -39%        | -32%      |
| RepeatedSimpleObject_Distinct        | -41%        | -31%      |
| LargeIntList                         | -8%         | **-33%**  |
| LargeObjectList                      | -16%        | -10%      |
| Dictionary_StringKey                 | -10%        | -12%      |
| Dictionary_ComplexKey                | +10% (within ShortRun error bars; allocations down) | -3% |
| DeepGraph                            | -45%        | -40%      |
| SameReference                        | **-67%**    | -36%      |

```

BenchmarkDotNet v0.15.8, Linux Pop!_OS 24.04 LTS
AMD Ryzen 9 PRO 8945HS w/ Radeon 780M Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4


```
| Method                                               | Scenario        | Mean       | Error     | StdDev    | Gen0   | Allocated |
|----------------------------------------------------- |---------------- |-----------:|----------:|----------:|-------:|----------:|
| **RoutePattern_Parse**                                   | **ParseSimple**     |  **33.045 ns** | **0.6600 ns** | **0.6174 ns** | **0.0220** |     **184 B** |
| RoutePattern_GetPathSegmentCount                     | ParseSimple     |   8.547 ns | 0.1902 ns | 0.1868 ns |      - |         - |
| RoutePattern_SplitPath                               | ParseSimple     |  48.749 ns | 1.0030 ns | 1.7568 ns | 0.0239 |     200 B |
| RoutePattern_SplitPathToSpan                         | ParseSimple     |  46.313 ns | 0.9212 ns | 0.8617 ns | 0.0172 |     144 B |
| RouteIndex_MatchPreSplitToSpan                       | ParseSimple     |  36.640 ns | 0.6630 ns | 0.6202 ns |      - |         - |
| RouteIndex_MatchRouteWithCallerBufferToSpan          | ParseSimple     |  88.133 ns | 0.8995 ns | 0.7974 ns | 0.0172 |     144 B |
| RouteIndex_MatchRouteToSpan                          | ParseSimple     | 112.460 ns | 1.3630 ns | 1.2749 ns | 0.0172 |     144 B |
| RouteIndex_MatchPreSplitDetailedToSpans              | ParseSimple     |  56.267 ns | 0.8026 ns | 0.7507 ns |      - |         - |
| RouteIndex_MatchRouteWithCallerBufferDetailedToSpans | ParseSimple     | 120.779 ns | 0.7163 ns | 0.6701 ns | 0.0172 |     144 B |
| RouteIndex_MatchRouteDetailedToSpans                 | ParseSimple     | 132.803 ns | 1.5326 ns | 1.3586 ns | 0.0172 |     144 B |
| RouteIndex_MatchRouteDetailedToArray                 | ParseSimple     | 237.350 ns | 0.7842 ns | 0.6548 ns | 0.0486 |     408 B |
| **RoutePattern_Parse**                                   | **ParseParameters** | **103.963 ns** | **1.4448 ns** | **1.3515 ns** | **0.0516** |     **432 B** |
| RoutePattern_GetPathSegmentCount                     | ParseParameters |   8.367 ns | 0.1052 ns | 0.0933 ns |      - |         - |
| RoutePattern_SplitPath                               | ParseParameters |  48.806 ns | 0.3472 ns | 0.3248 ns | 0.0239 |     200 B |
| RoutePattern_SplitPathToSpan                         | ParseParameters |  47.660 ns | 0.8464 ns | 0.7067 ns | 0.0172 |     144 B |
| RouteIndex_MatchPreSplitToSpan                       | ParseParameters |  38.698 ns | 0.6306 ns | 0.5899 ns |      - |         - |
| RouteIndex_MatchRouteWithCallerBufferToSpan          | ParseParameters |  88.416 ns | 1.1487 ns | 1.0745 ns | 0.0172 |     144 B |
| RouteIndex_MatchRouteToSpan                          | ParseParameters | 116.910 ns | 2.2534 ns | 2.2131 ns | 0.0172 |     144 B |
| RouteIndex_MatchPreSplitDetailedToSpans              | ParseParameters |  62.290 ns | 0.5549 ns | 0.5190 ns |      - |         - |
| RouteIndex_MatchRouteWithCallerBufferDetailedToSpans | ParseParameters | 110.512 ns | 1.5052 ns | 1.4080 ns | 0.0172 |     144 B |
| RouteIndex_MatchRouteDetailedToSpans                 | ParseParameters | 124.071 ns | 1.7979 ns | 1.6818 ns | 0.0172 |     144 B |
| RouteIndex_MatchRouteDetailedToArray                 | ParseParameters | 241.830 ns | 1.9996 ns | 1.7726 ns | 0.0486 |     408 B |
| **RoutePattern_Parse**                                   | **ParseCatchAll**   |  **48.138 ns** | **0.9386 ns** | **0.8780 ns** | **0.0258** |     **216 B** |
| RoutePattern_GetPathSegmentCount                     | ParseCatchAll   |   6.935 ns | 0.0767 ns | 0.0718 ns |      - |         - |
| RoutePattern_SplitPath                               | ParseCatchAll   |  40.589 ns | 0.4735 ns | 0.3954 ns | 0.0200 |     168 B |
| RoutePattern_SplitPathToSpan                         | ParseCatchAll   |  39.829 ns | 0.8160 ns | 0.7633 ns | 0.0134 |     112 B |
| RouteIndex_MatchPreSplitToSpan                       | ParseCatchAll   |  22.223 ns | 0.0861 ns | 0.0805 ns |      - |         - |
| RouteIndex_MatchRouteWithCallerBufferToSpan          | ParseCatchAll   |  64.212 ns | 0.1774 ns | 0.1481 ns | 0.0134 |     112 B |
| RouteIndex_MatchRouteToSpan                          | ParseCatchAll   |  82.656 ns | 1.6658 ns | 1.5582 ns | 0.0134 |     112 B |
| RouteIndex_MatchPreSplitDetailedToSpans              | ParseCatchAll   |  42.985 ns | 0.0722 ns | 0.0640 ns |      - |         - |
| RouteIndex_MatchRouteWithCallerBufferDetailedToSpans | ParseCatchAll   |  84.024 ns | 1.4834 ns | 1.3876 ns | 0.0134 |     112 B |
| RouteIndex_MatchRouteDetailedToSpans                 | ParseCatchAll   | 102.145 ns | 0.4783 ns | 0.4240 ns | 0.0134 |     112 B |
| RouteIndex_MatchRouteDetailedToArray                 | ParseCatchAll   | 183.447 ns | 0.9662 ns | 0.9038 ns | 0.0505 |     424 B |
| **RoutePattern_Parse**                                   | **MatchDetailed**   |  **98.648 ns** | **1.1027 ns** | **1.0314 ns** | **0.0516** |     **432 B** |
| RoutePattern_GetPathSegmentCount                     | MatchDetailed   |   8.328 ns | 0.1439 ns | 0.1346 ns |      - |         - |
| RoutePattern_SplitPath                               | MatchDetailed   |  45.094 ns | 0.9237 ns | 0.8640 ns | 0.0239 |     200 B |
| RoutePattern_SplitPathToSpan                         | MatchDetailed   |  48.439 ns | 0.9881 ns | 1.5672 ns | 0.0172 |     144 B |
| RouteIndex_MatchPreSplitToSpan                       | MatchDetailed   |  36.948 ns | 0.1471 ns | 0.1376 ns |      - |         - |
| RouteIndex_MatchRouteWithCallerBufferToSpan          | MatchDetailed   |  93.575 ns | 0.6726 ns | 0.6292 ns | 0.0172 |     144 B |
| RouteIndex_MatchRouteToSpan                          | MatchDetailed   | 102.016 ns | 0.2494 ns | 0.2082 ns | 0.0172 |     144 B |
| RouteIndex_MatchPreSplitDetailedToSpans              | MatchDetailed   |  57.516 ns | 0.3570 ns | 0.3339 ns |      - |         - |
| RouteIndex_MatchRouteWithCallerBufferDetailedToSpans | MatchDetailed   | 110.119 ns | 1.2007 ns | 1.1232 ns | 0.0172 |     144 B |
| RouteIndex_MatchRouteDetailedToSpans                 | MatchDetailed   | 122.589 ns | 1.0349 ns | 0.9681 ns | 0.0172 |     144 B |
| RouteIndex_MatchRouteDetailedToArray                 | MatchDetailed   | 277.493 ns | 0.8579 ns | 0.7605 ns | 0.0486 |     408 B |

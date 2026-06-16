```

BenchmarkDotNet v0.15.8, Linux Pop!_OS 24.04 LTS
AMD Ryzen 9 PRO 8945HS w/ Radeon 780M Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4


```
| Method                                               | Scenario        | Mean       | Error     | StdDev    | Gen0   | Allocated |
|----------------------------------------------------- |---------------- |-----------:|----------:|----------:|-------:|----------:|
| **RoutePattern_Parse**                                   | **ParseSimple**     |  **77.187 ns** | **1.5819 ns** | **2.1653 ns** | **0.0526** |     **440 B** |
| RoutePattern_GetPathSegmentCount                     | ParseSimple     |   8.965 ns | 0.0763 ns | 0.0714 ns |      - |         - |
| RoutePattern_SplitPath                               | ParseSimple     |  46.929 ns | 0.9703 ns | 1.5668 ns | 0.0239 |     200 B |
| RoutePattern_SplitPathToSpan                         | ParseSimple     |  45.886 ns | 0.7740 ns | 0.6861 ns | 0.0172 |     144 B |
| RouteIndex_MatchPreSplitToSpan                       | ParseSimple     |  38.521 ns | 0.7739 ns | 0.7239 ns |      - |         - |
| RouteIndex_MatchRouteWithCallerBufferToSpan          | ParseSimple     |  88.131 ns | 1.5460 ns | 1.4461 ns | 0.0172 |     144 B |
| RouteIndex_MatchRouteToSpan                          | ParseSimple     | 106.911 ns | 1.2079 ns | 1.1299 ns | 0.0172 |     144 B |
| RouteIndex_MatchPreSplitDetailedToSpans              | ParseSimple     |  58.708 ns | 0.3663 ns | 0.3426 ns |      - |         - |
| RouteIndex_MatchRouteWithCallerBufferDetailedToSpans | ParseSimple     | 102.946 ns | 1.0682 ns | 0.9469 ns | 0.0172 |     144 B |
| RouteIndex_MatchRouteDetailedToSpans                 | ParseSimple     | 131.230 ns | 1.7396 ns | 1.5421 ns | 0.0172 |     144 B |
| RouteIndex_MatchRouteDetailedToArray                 | ParseSimple     | 229.387 ns | 0.7606 ns | 0.6742 ns | 0.0505 |     424 B |
| **RoutePattern_Parse**                                   | **ParseParameters** | **210.679 ns** | **2.3305 ns** | **2.1799 ns** | **0.1395** |    **1168 B** |
| RoutePattern_GetPathSegmentCount                     | ParseParameters |   8.973 ns | 0.0705 ns | 0.0625 ns |      - |         - |
| RoutePattern_SplitPath                               | ParseParameters |  46.283 ns | 0.9575 ns | 1.4621 ns | 0.0239 |     200 B |
| RoutePattern_SplitPathToSpan                         | ParseParameters |  48.422 ns | 0.7058 ns | 0.6602 ns | 0.0172 |     144 B |
| RouteIndex_MatchPreSplitToSpan                       | ParseParameters |  39.471 ns | 0.2566 ns | 0.2400 ns |      - |         - |
| RouteIndex_MatchRouteWithCallerBufferToSpan          | ParseParameters |  83.478 ns | 1.0986 ns | 0.9738 ns | 0.0172 |     144 B |
| RouteIndex_MatchRouteToSpan                          | ParseParameters | 107.872 ns | 1.7785 ns | 1.6636 ns | 0.0172 |     144 B |
| RouteIndex_MatchPreSplitDetailedToSpans              | ParseParameters |  59.377 ns | 0.8139 ns | 0.7614 ns |      - |         - |
| RouteIndex_MatchRouteWithCallerBufferDetailedToSpans | ParseParameters | 115.437 ns | 2.0743 ns | 1.9403 ns | 0.0172 |     144 B |
| RouteIndex_MatchRouteDetailedToSpans                 | ParseParameters | 129.387 ns | 1.5262 ns | 1.3530 ns | 0.0172 |     144 B |
| RouteIndex_MatchRouteDetailedToArray                 | ParseParameters | 236.069 ns | 3.5459 ns | 3.3168 ns | 0.0505 |     424 B |
| **RoutePattern_Parse**                                   | **ParseCatchAll**   | **117.412 ns** | **1.9216 ns** | **1.7034 ns** | **0.0792** |     **664 B** |
| RoutePattern_GetPathSegmentCount                     | ParseCatchAll   |   6.720 ns | 0.0812 ns | 0.0720 ns |      - |         - |
| RoutePattern_SplitPath                               | ParseCatchAll   |  39.166 ns | 0.8171 ns | 1.6505 ns | 0.0200 |     168 B |
| RoutePattern_SplitPathToSpan                         | ParseCatchAll   |  38.087 ns | 0.7900 ns | 0.8781 ns | 0.0134 |     112 B |
| RouteIndex_MatchPreSplitToSpan                       | ParseCatchAll   |  22.104 ns | 0.2437 ns | 0.2279 ns |      - |         - |
| RouteIndex_MatchRouteWithCallerBufferToSpan          | ParseCatchAll   |  63.394 ns | 1.2275 ns | 1.3644 ns | 0.0134 |     112 B |
| RouteIndex_MatchRouteToSpan                          | ParseCatchAll   |  80.061 ns | 0.8000 ns | 0.7484 ns | 0.0134 |     112 B |
| RouteIndex_MatchPreSplitDetailedToSpans              | ParseCatchAll   |  44.152 ns | 0.2765 ns | 0.2587 ns |      - |         - |
| RouteIndex_MatchRouteWithCallerBufferDetailedToSpans | ParseCatchAll   |  87.093 ns | 0.6083 ns | 0.4749 ns | 0.0134 |     112 B |
| RouteIndex_MatchRouteDetailedToSpans                 | ParseCatchAll   | 102.436 ns | 2.0293 ns | 2.1713 ns | 0.0134 |     112 B |
| RouteIndex_MatchRouteDetailedToArray                 | ParseCatchAll   | 182.529 ns | 3.6722 ns | 3.4350 ns | 0.0525 |     440 B |
| **RoutePattern_Parse**                                   | **MatchDetailed**   | **218.531 ns** | **4.1371 ns** | **7.5650 ns** | **0.1395** |    **1168 B** |
| RoutePattern_GetPathSegmentCount                     | MatchDetailed   |  10.249 ns | 0.1673 ns | 0.1397 ns |      - |         - |
| RoutePattern_SplitPath                               | MatchDetailed   |  44.244 ns | 0.7570 ns | 0.7081 ns | 0.0239 |     200 B |
| RoutePattern_SplitPathToSpan                         | MatchDetailed   |  48.840 ns | 0.9967 ns | 1.2240 ns | 0.0172 |     144 B |
| RouteIndex_MatchPreSplitToSpan                       | MatchDetailed   |  35.588 ns | 0.0783 ns | 0.0694 ns |      - |         - |
| RouteIndex_MatchRouteWithCallerBufferToSpan          | MatchDetailed   |  87.818 ns | 1.3194 ns | 1.2342 ns | 0.0172 |     144 B |
| RouteIndex_MatchRouteToSpan                          | MatchDetailed   | 112.284 ns | 2.1843 ns | 2.3372 ns | 0.0172 |     144 B |
| RouteIndex_MatchPreSplitDetailedToSpans              | MatchDetailed   |  58.769 ns | 0.2856 ns | 0.2532 ns |      - |         - |
| RouteIndex_MatchRouteWithCallerBufferDetailedToSpans | MatchDetailed   | 101.206 ns | 1.7419 ns | 1.6294 ns | 0.0172 |     144 B |
| RouteIndex_MatchRouteDetailedToSpans                 | MatchDetailed   | 124.397 ns | 1.4900 ns | 1.3937 ns | 0.0172 |     144 B |
| RouteIndex_MatchRouteDetailedToArray                 | MatchDetailed   | 232.033 ns | 3.4776 ns | 3.2529 ns | 0.0505 |     424 B |

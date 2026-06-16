```

BenchmarkDotNet v0.15.8, Linux Pop!_OS 24.04 LTS
AMD Ryzen 9 PRO 8945HS w/ Radeon 780M Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4


```
| Method                               | Scenario        | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------------------------------------- |---------------- |----------:|---------:|---------:|-------:|----------:|
| **RoutePattern_Parse**                   | **ParseSimple**     |  **33.01 ns** | **0.372 ns** | **0.330 ns** | **0.0220** |     **184 B** |
| RoutePattern_SplitPath               | ParseSimple     |  44.24 ns | 0.631 ns | 0.591 ns | 0.0239 |     200 B |
| RouteIndex_MatchRouteToSpan          | ParseSimple     |  84.77 ns | 0.326 ns | 0.289 ns | 0.0238 |     200 B |
| RouteIndex_MatchRouteDetailedToSpans | ParseSimple     | 104.37 ns | 0.160 ns | 0.134 ns | 0.0238 |     200 B |
| RouteIndex_MatchRouteDetailedToArray | ParseSimple     | 204.56 ns | 0.791 ns | 0.701 ns | 0.0553 |     464 B |
| **RoutePattern_Parse**                   | **ParseParameters** | **101.41 ns** | **2.064 ns** | **4.074 ns** | **0.0516** |     **432 B** |
| RoutePattern_SplitPath               | ParseParameters |  47.80 ns | 0.610 ns | 0.571 ns | 0.0239 |     200 B |
| RouteIndex_MatchRouteToSpan          | ParseParameters |  87.31 ns | 0.442 ns | 0.392 ns | 0.0238 |     200 B |
| RouteIndex_MatchRouteDetailedToSpans | ParseParameters | 116.25 ns | 0.759 ns | 0.673 ns | 0.0238 |     200 B |
| RouteIndex_MatchRouteDetailedToArray | ParseParameters | 285.66 ns | 0.790 ns | 0.660 ns | 0.0553 |     464 B |
| **RoutePattern_Parse**                   | **ParseCatchAll**   |  **47.86 ns** | **0.693 ns** | **0.648 ns** | **0.0258** |     **216 B** |
| RoutePattern_SplitPath               | ParseCatchAll   |  42.19 ns | 0.274 ns | 0.243 ns | 0.0200 |     168 B |
| RouteIndex_MatchRouteToSpan          | ParseCatchAll   |  65.73 ns | 0.263 ns | 0.246 ns | 0.0200 |     168 B |
| RouteIndex_MatchRouteDetailedToSpans | ParseCatchAll   |  98.71 ns | 0.615 ns | 0.575 ns | 0.0200 |     168 B |
| RouteIndex_MatchRouteDetailedToArray | ParseCatchAll   | 158.45 ns | 0.412 ns | 0.344 ns | 0.0572 |     480 B |
| **RoutePattern_Parse**                   | **MatchDetailed**   |  **96.41 ns** | **0.911 ns** | **0.852 ns** | **0.0516** |     **432 B** |
| RoutePattern_SplitPath               | MatchDetailed   |  44.93 ns | 0.570 ns | 0.476 ns | 0.0239 |     200 B |
| RouteIndex_MatchRouteToSpan          | MatchDetailed   |  88.27 ns | 0.285 ns | 0.253 ns | 0.0238 |     200 B |
| RouteIndex_MatchRouteDetailedToSpans | MatchDetailed   | 105.13 ns | 0.114 ns | 0.101 ns | 0.0238 |     200 B |
| RouteIndex_MatchRouteDetailedToArray | MatchDetailed   | 206.01 ns | 0.463 ns | 0.410 ns | 0.0553 |     464 B |

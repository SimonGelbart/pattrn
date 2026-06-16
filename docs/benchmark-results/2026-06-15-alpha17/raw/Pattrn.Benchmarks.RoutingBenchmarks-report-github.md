```

BenchmarkDotNet v0.15.8, Linux Pop!_OS 24.04 LTS
AMD Ryzen 9 PRO 8945HS w/ Radeon 780M Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4


```
| Method                               | Scenario        | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------------------------------------- |---------------- |----------:|---------:|---------:|-------:|----------:|
| **RoutePattern_Parse**                   | **ParseSimple**     |  **33.95 ns** | **0.324 ns** | **0.303 ns** | **0.0220** |     **184 B** |
| RoutePattern_SplitPath               | ParseSimple     |  45.18 ns | 0.391 ns | 0.327 ns | 0.0239 |     200 B |
| RouteIndex_MatchRouteToSpan          | ParseSimple     |  85.98 ns | 0.376 ns | 0.352 ns | 0.0238 |     200 B |
| RouteIndex_MatchRouteDetailedToSpans | ParseSimple     | 127.41 ns | 0.542 ns | 0.507 ns | 0.0238 |     200 B |
| RouteIndex_MatchRouteDetailedToArray | ParseSimple     | 206.39 ns | 1.097 ns | 1.026 ns | 0.0553 |     464 B |
| **RoutePattern_Parse**                   | **ParseParameters** | **107.09 ns** | **2.175 ns** | **4.539 ns** | **0.0516** |     **432 B** |
| RoutePattern_SplitPath               | ParseParameters |  46.56 ns | 0.414 ns | 0.323 ns | 0.0239 |     200 B |
| RouteIndex_MatchRouteToSpan          | ParseParameters |  97.12 ns | 0.358 ns | 0.299 ns | 0.0238 |     200 B |
| RouteIndex_MatchRouteDetailedToSpans | ParseParameters | 107.72 ns | 0.555 ns | 0.519 ns | 0.0238 |     200 B |
| RouteIndex_MatchRouteDetailedToArray | ParseParameters | 205.88 ns | 1.020 ns | 0.954 ns | 0.0553 |     464 B |
| **RoutePattern_Parse**                   | **ParseCatchAll**   |  **47.74 ns** | **0.883 ns** | **0.826 ns** | **0.0258** |     **216 B** |
| RoutePattern_SplitPath               | ParseCatchAll   |  38.46 ns | 0.165 ns | 0.154 ns | 0.0200 |     168 B |
| RouteIndex_MatchRouteToSpan          | ParseCatchAll   |  76.04 ns | 0.698 ns | 0.652 ns | 0.0200 |     168 B |
| RouteIndex_MatchRouteDetailedToSpans | ParseCatchAll   | 102.92 ns | 0.903 ns | 0.844 ns | 0.0200 |     168 B |
| RouteIndex_MatchRouteDetailedToArray | ParseCatchAll   | 160.39 ns | 0.665 ns | 0.590 ns | 0.0572 |     480 B |
| **RoutePattern_Parse**                   | **MatchDetailed**   |  **97.89 ns** | **0.842 ns** | **0.703 ns** | **0.0516** |     **432 B** |
| RoutePattern_SplitPath               | MatchDetailed   |  46.05 ns | 0.915 ns | 0.940 ns | 0.0239 |     200 B |
| RouteIndex_MatchRouteToSpan          | MatchDetailed   |  86.12 ns | 1.602 ns | 1.499 ns | 0.0238 |     200 B |
| RouteIndex_MatchRouteDetailedToSpans | MatchDetailed   | 116.11 ns | 1.131 ns | 1.058 ns | 0.0238 |     200 B |
| RouteIndex_MatchRouteDetailedToArray | MatchDetailed   | 273.40 ns | 1.977 ns | 1.752 ns | 0.0553 |     464 B |

```

BenchmarkDotNet v0.15.8, Linux Pop!_OS 24.04 LTS
AMD Ryzen 9 PRO 8945HS w/ Radeon 780M Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4


```
| Method              | Scenario             | Mean     | Error     | StdDev    | Gen0      | Gen1     | Gen2     | Allocated |
|-------------------- |--------------------- |---------:|----------:|----------:|----------:|---------:|---------:|----------:|
| **Build**               | **BuildLargeExact**      | **4.121 ms** | **0.0815 ms** | **0.1088 ms** |  **921.8750** | **820.3125** | **500.0000** |   **5.45 MB** |
| GetDiagnostics      | BuildLargeExact      | 2.407 ms | 0.0439 ms | 0.0410 ms |  410.1563 | 308.5938 | 136.7188 |   3.43 MB |
| BuildWithValidation | BuildLargeExact      | 4.905 ms | 0.0960 ms | 0.1755 ms | 1039.0625 | 875.0000 | 500.0000 |   6.23 MB |
| **Build**               | **BuildLargeParameters** | **1.872 ms** | **0.0374 ms** | **0.0367 ms** |  **498.0469** | **498.0469** | **498.0469** |   **4.98 MB** |
| GetDiagnostics      | BuildLargeParameters | 2.655 ms | 0.0511 ms | 0.0568 ms |  437.5000 | 324.2188 | 121.0938 |   3.46 MB |
| BuildWithValidation | BuildLargeParameters | 3.471 ms | 0.0664 ms | 0.0710 ms |  996.0938 | 746.0938 | 496.0938 |   6.14 MB |
| **Build**               | **GetDiagnosticsClean**  | **4.299 ms** | **0.0839 ms** | **0.1331 ms** |  **921.8750** | **820.3125** | **500.0000** |   **5.45 MB** |
| GetDiagnostics      | GetDiagnosticsClean  | 2.418 ms | 0.0338 ms | 0.0317 ms |  410.1563 | 308.5938 | 136.7188 |   3.43 MB |
| BuildWithValidation | GetDiagnosticsClean  | 4.910 ms | 0.0913 ms | 0.0938 ms | 1039.0625 | 867.1875 | 500.0000 |   6.23 MB |
| **Build**               | **GetDi(...)guous [23]** | **1.499 ms** | **0.0263 ms** | **0.0246 ms** |  **378.9063** | **283.2031** | **189.4531** |   **2.79 MB** |
| GetDiagnostics      | GetDi(...)guous [23] | 1.599 ms | 0.0303 ms | 0.0284 ms |  341.7969 | 199.2188 |  89.8438 |   2.77 MB |
| BuildWithValidation | GetDi(...)guous [23] | 1.903 ms | 0.0168 ms | 0.0158 ms |  550.7813 | 300.7813 | 191.4063 |   4.08 MB |
| **Build**               | **ValidateOnBuild**      | **1.476 ms** | **0.0184 ms** | **0.0172 ms** |  **378.9063** | **283.2031** | **189.4531** |   **2.79 MB** |
| GetDiagnostics      | ValidateOnBuild      | 1.514 ms | 0.0297 ms | 0.0342 ms |  341.7969 | 199.2188 |  89.8438 |   2.77 MB |
| BuildWithValidation | ValidateOnBuild      | 1.874 ms | 0.0113 ms | 0.0100 ms |  552.7344 | 302.7344 | 193.3594 |   4.08 MB |

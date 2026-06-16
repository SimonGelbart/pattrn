```

BenchmarkDotNet v0.15.8, Linux Pop!_OS 24.04 LTS
AMD Ryzen 9 PRO 8945HS w/ Radeon 780M Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4


```
| Method              | Scenario             | Mean     | Error     | StdDev    | Median   | Gen0      | Gen1     | Gen2     | Allocated |
|-------------------- |--------------------- |---------:|----------:|----------:|---------:|----------:|---------:|---------:|----------:|
| **Build**               | **BuildLargeExact**      | **4.064 ms** | **0.0703 ms** | **0.0658 ms** | **4.063 ms** |  **921.8750** | **820.3125** | **500.0000** |   **5.45 MB** |
| GetDiagnostics      | BuildLargeExact      | 2.305 ms | 0.0342 ms | 0.0303 ms | 2.314 ms |  410.1563 | 308.5938 | 136.7188 |   3.43 MB |
| BuildWithValidation | BuildLargeExact      | 4.700 ms | 0.0904 ms | 0.0846 ms | 4.698 ms | 1039.0625 | 875.0000 | 500.0000 |   6.23 MB |
| **Build**               | **BuildLargeParameters** | **1.829 ms** | **0.0072 ms** | **0.0064 ms** | **1.831 ms** |  **498.0469** | **498.0469** | **498.0469** |   **4.98 MB** |
| GetDiagnostics      | BuildLargeParameters | 2.581 ms | 0.0509 ms | 0.0713 ms | 2.582 ms |  437.5000 | 324.2188 | 121.0938 |   3.46 MB |
| BuildWithValidation | BuildLargeParameters | 3.345 ms | 0.0660 ms | 0.0617 ms | 3.323 ms |  996.0938 | 746.0938 | 496.0938 |   6.14 MB |
| **Build**               | **GetDiagnosticsClean**  | **4.199 ms** | **0.0465 ms** | **0.0435 ms** | **4.209 ms** |  **921.8750** | **820.3125** | **500.0000** |   **5.45 MB** |
| GetDiagnostics      | GetDiagnosticsClean  | 2.419 ms | 0.0474 ms | 0.0695 ms | 2.425 ms |  410.1563 | 308.5938 | 136.7188 |   3.43 MB |
| BuildWithValidation | GetDiagnosticsClean  | 4.691 ms | 0.0884 ms | 0.0826 ms | 4.684 ms | 1039.0625 | 875.0000 | 500.0000 |   6.23 MB |
| **Build**               | **GetDi(...)guous [23]** | **1.384 ms** | **0.0270 ms** | **0.0277 ms** | **1.405 ms** |  **378.9063** | **283.2031** | **189.4531** |   **2.79 MB** |
| GetDiagnostics      | GetDi(...)guous [23] | 1.545 ms | 0.0297 ms | 0.0417 ms | 1.558 ms |  341.7969 | 197.2656 |  89.8438 |   2.77 MB |
| BuildWithValidation | GetDi(...)guous [23] | 1.906 ms | 0.0314 ms | 0.0294 ms | 1.898 ms |  550.7813 | 300.7813 | 191.4063 |   4.08 MB |
| **Build**               | **ValidateOnBuild**      | **1.410 ms** | **0.0278 ms** | **0.0285 ms** | **1.406 ms** |  **378.9063** | **283.2031** | **189.4531** |   **2.79 MB** |
| GetDiagnostics      | ValidateOnBuild      | 1.517 ms | 0.0162 ms | 0.0126 ms | 1.524 ms |  341.7969 | 199.2188 |  89.8438 |   2.77 MB |
| BuildWithValidation | ValidateOnBuild      | 1.865 ms | 0.0131 ms | 0.0122 ms | 1.862 ms |  552.7344 | 302.7344 | 193.3594 |   4.08 MB |

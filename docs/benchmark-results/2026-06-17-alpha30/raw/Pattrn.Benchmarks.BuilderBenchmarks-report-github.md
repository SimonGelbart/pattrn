```

BenchmarkDotNet v0.15.8, Linux Pop!_OS 24.04 LTS
AMD Ryzen 9 PRO 8945HS w/ Radeon 780M Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4


```
| Method              | Scenario             | Mean     | Error     | StdDev    | Gen0      | Gen1     | Gen2     | Allocated |
|-------------------- |--------------------- |---------:|----------:|----------:|----------:|---------:|---------:|----------:|
| **Build**               | **BuildLargeExact**      | **4.334 ms** | **0.0839 ms** | **0.1090 ms** |  **953.1250** | **851.5625** | **500.0000** |   **5.89 MB** |
| GetDiagnostics      | BuildLargeExact      | 2.332 ms | 0.0462 ms | 0.1159 ms |  433.5938 | 285.1563 | 136.7188 |   3.68 MB |
| BuildWithValidation | BuildLargeExact      | 4.745 ms | 0.0852 ms | 0.0797 ms | 1062.5000 | 875.0000 | 500.0000 |   6.67 MB |
| **Build**               | **BuildLargeParameters** | **3.497 ms** | **0.0256 ms** | **0.0227 ms** |  **949.2188** | **835.9375** | **500.0000** |   **5.42 MB** |
| GetDiagnostics      | BuildLargeParameters | 2.125 ms | 0.0423 ms | 0.0825 ms |  429.6875 | 289.0625 | 140.6250 |   3.71 MB |
| BuildWithValidation | BuildLargeParameters | 4.181 ms | 0.0363 ms | 0.0339 ms | 1093.7500 | 773.4375 | 500.0000 |   6.58 MB |
| **Build**               | **GetDiagnosticsClean**  | **4.035 ms** | **0.0311 ms** | **0.0276 ms** |  **953.1250** | **851.5625** | **500.0000** |   **5.89 MB** |
| GetDiagnostics      | GetDiagnosticsClean  | 2.131 ms | 0.0249 ms | 0.0233 ms |  433.5938 | 285.1563 | 136.7188 |   3.68 MB |
| BuildWithValidation | GetDiagnosticsClean  | 4.748 ms | 0.0590 ms | 0.0552 ms | 1062.5000 | 875.0000 | 500.0000 |   6.67 MB |
| **Build**               | **GetDi(...)guous [23]** | **1.297 ms** | **0.0143 ms** | **0.0134 ms** |  **378.9063** | **283.2031** | **189.4531** |   **3.01 MB** |
| GetDiagnostics      | GetDi(...)guous [23] | 1.467 ms | 0.0290 ms | 0.0637 ms |  349.6094 | 210.9375 | 103.5156 |   2.89 MB |
| BuildWithValidation | GetDi(...)guous [23] | 2.066 ms | 0.0411 ms | 0.0710 ms |  570.3125 | 316.4063 | 183.5938 |   4.29 MB |
| **Build**               | **ValidateOnBuild**      | **1.342 ms** | **0.0251 ms** | **0.0235 ms** |  **378.9063** | **283.2031** | **189.4531** |   **3.01 MB** |
| GetDiagnostics      | ValidateOnBuild      | 1.395 ms | 0.0271 ms | 0.0290 ms |  349.6094 | 212.8906 | 105.4688 |   2.89 MB |
| BuildWithValidation | ValidateOnBuild      | 2.035 ms | 0.0399 ms | 0.0688 ms |  566.4063 | 320.3125 | 187.5000 |   4.29 MB |

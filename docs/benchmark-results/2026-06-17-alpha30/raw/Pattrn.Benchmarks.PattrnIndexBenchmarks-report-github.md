```

BenchmarkDotNet v0.15.8, Linux Pop!_OS 24.04 LTS
AMD Ryzen 9 PRO 8945HS w/ Radeon 780M Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4


```
| Method                       | Scenario             | Mean          | Error       | StdDev      | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------------------- |--------------------- |--------------:|------------:|------------:|------:|--------:|-------:|-------:|----------:|------------:|
| **NaiveScan_MatchToArray**       | **ExactOnlySparseDeep**  |  **3,509.203 ns** |  **27.9358 ns** |  **26.1311 ns** | **1.000** |    **0.01** | **0.0114** |      **-** |     **104 B** |        **1.00** |
| Trie_GetMatchCountUpperBound | ExactOnlySparseDeep  |     27.471 ns |   0.3079 ns |   0.2730 ns | 0.008 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToSpan             | ExactOnlySparseDeep  |     31.301 ns |   0.2860 ns |   0.2676 ns | 0.009 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToArray            | ExactOnlySparseDeep  |     46.171 ns |   0.8188 ns |   0.7659 ns | 0.013 |    0.00 | 0.0172 |      - |     144 B |        1.38 |
| Trie_MatchDetailedToSpans    | ExactOnlySparseDeep  |     36.614 ns |   0.2696 ns |   0.2522 ns | 0.010 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchDetailedToArray    | ExactOnlySparseDeep  |    128.127 ns |   2.1032 ns |   1.7563 ns | 0.037 |    0.00 | 0.0219 |      - |     184 B |        1.77 |
|                              |                      |               |             |             |       |         |        |        |           |             |
| **NaiveScan_MatchToArray**       | **ExactOnlyWideFanOut**  | **16,799.532 ns** | **331.0756 ns** | **276.4631 ns** | **1.000** |    **0.02** |      **-** |      **-** |     **104 B** |        **1.00** |
| Trie_GetMatchCountUpperBound | ExactOnlyWideFanOut  |     16.275 ns |   0.1464 ns |   0.1369 ns | 0.001 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToSpan             | ExactOnlyWideFanOut  |     27.034 ns |   0.1885 ns |   0.1763 ns | 0.002 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToArray            | ExactOnlyWideFanOut  |     34.934 ns |   0.4095 ns |   0.3420 ns | 0.002 |    0.00 | 0.0172 |      - |     144 B |        1.38 |
| Trie_MatchDetailedToSpans    | ExactOnlyWideFanOut  |     24.736 ns |   0.3009 ns |   0.2815 ns | 0.001 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchDetailedToArray    | ExactOnlyWideFanOut  |     74.729 ns |   1.4373 ns |   1.2742 ns | 0.004 |    0.00 | 0.0219 |      - |     184 B |        1.77 |
|                              |                      |               |             |             |       |         |        |        |           |             |
| **NaiveScan_MatchToArray**       | **WildcardSparse**       |  **1,200.363 ns** |  **13.3857 ns** |  **12.5210 ns** |  **1.00** |    **0.01** | **0.0134** |      **-** |     **112 B** |        **1.00** |
| Trie_GetMatchCountUpperBound | WildcardSparse       |     58.953 ns |   0.4819 ns |   0.4508 ns |  0.05 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToSpan             | WildcardSparse       |     67.948 ns |   0.5204 ns |   0.4868 ns |  0.06 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToArray            | WildcardSparse       |    114.777 ns |   1.3488 ns |   1.1957 ns |  0.10 |    0.00 | 0.0267 |      - |     224 B |        2.00 |
| Trie_MatchDetailedToSpans    | WildcardSparse       |     95.024 ns |   1.1240 ns |   1.0514 ns |  0.08 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchDetailedToArray    | WildcardSparse       |    291.229 ns |   4.7748 ns |   4.2327 ns |  0.24 |    0.00 | 0.0620 |      - |     520 B |        4.64 |
|                              |                      |               |             |             |       |         |        |        |           |             |
| **NaiveScan_MatchToArray**       | **WildcardDense**        | **29,450.284 ns** | **562.7456 ns** | **577.8983 ns** | **1.000** |    **0.03** |      **-** |      **-** |     **112 B** |        **1.00** |
| Trie_GetMatchCountUpperBound | WildcardDense        |     52.699 ns |   0.4731 ns |   0.4426 ns | 0.002 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToSpan             | WildcardDense        |     72.919 ns |   0.7329 ns |   0.6856 ns | 0.002 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToArray            | WildcardDense        |    109.079 ns |   2.1633 ns |   2.3147 ns | 0.004 |    0.00 | 0.0267 |      - |     224 B |        2.00 |
| Trie_MatchDetailedToSpans    | WildcardDense        |     99.676 ns |   0.3559 ns |   0.3155 ns | 0.003 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchDetailedToArray    | WildcardDense        |    287.721 ns |   5.6485 ns |   7.1436 ns | 0.010 |    0.00 | 0.0620 |      - |     520 B |        4.64 |
|                              |                      |               |             |             |       |         |        |        |           |             |
| **NaiveScan_MatchToArray**       | **PrefixExactOnly**      | **29,505.866 ns** | **583.1528 ns** | **545.4815 ns** | **1.000** |    **0.03** |      **-** |      **-** |     **104 B** |        **1.00** |
| Trie_GetMatchCountUpperBound | PrefixExactOnly      |     18.259 ns |   0.1374 ns |   0.1147 ns | 0.001 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToSpan             | PrefixExactOnly      |     37.249 ns |   0.0863 ns |   0.0807 ns | 0.001 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToArray            | PrefixExactOnly      |     61.242 ns |   0.4970 ns |   0.4405 ns | 0.002 |    0.00 | 0.0210 |      - |     176 B |        1.69 |
| Trie_MatchDetailedToSpans    | PrefixExactOnly      |     61.667 ns |   0.2252 ns |   0.2107 ns | 0.002 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchDetailedToArray    | PrefixExactOnly      |    137.796 ns |   0.5646 ns |   0.4715 ns | 0.005 |    0.00 | 0.0353 |      - |     296 B |        2.85 |
|                              |                      |               |             |             |       |         |        |        |           |             |
| **NaiveScan_MatchToArray**       | **PrefixWildcard**       | **29,122.429 ns** | **145.9666 ns** | **129.3956 ns** | **1.000** |    **0.01** |      **-** |      **-** |     **176 B** |        **1.00** |
| Trie_GetMatchCountUpperBound | PrefixWildcard       |     69.847 ns |   0.9527 ns |   0.8911 ns | 0.002 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToSpan             | PrefixWildcard       |     92.178 ns |   1.0599 ns |   0.9396 ns | 0.003 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToArray            | PrefixWildcard       |    169.992 ns |   1.6305 ns |   1.4454 ns | 0.006 |    0.00 | 0.0343 |      - |     288 B |        1.64 |
| Trie_MatchDetailedToSpans    | PrefixWildcard       |    140.062 ns |   1.3940 ns |   1.2357 ns | 0.005 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchDetailedToArray    | PrefixWildcard       |    360.388 ns |   7.2260 ns |   6.0341 ns | 0.012 |    0.00 | 0.0753 |      - |     632 B |        3.59 |
|                              |                      |               |             |             |       |         |        |        |           |             |
| **NaiveScan_MatchToArray**       | **Dupli(...)icate [25]** |  **1,497.565 ns** |  **27.7700 ns** |  **25.9761 ns** |  **1.00** |    **0.02** | **0.1106** |      **-** |     **928 B** |        **1.00** |
| Trie_GetMatchCountUpperBound | Dupli(...)icate [25] |     37.688 ns |   0.2170 ns |   0.2030 ns |  0.03 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToSpan             | Dupli(...)icate [25] |    168.826 ns |   1.4720 ns |   1.3769 ns |  0.11 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToArray            | Dupli(...)icate [25] |    805.945 ns |  15.7385 ns |  21.0104 ns |  0.54 |    0.02 | 0.2308 | 0.0010 |    1936 B |        2.09 |
| Trie_MatchDetailedToSpans    | Dupli(...)icate [25] |  1,771.259 ns |   9.0589 ns |   7.5646 ns |  1.18 |    0.02 |      - |      - |         - |        0.00 |
| Trie_MatchDetailedToArray    | Dupli(...)icate [25] |  3,038.827 ns |  56.7722 ns |  90.0467 ns |  2.03 |    0.07 | 1.5984 | 0.0496 |   13384 B |       14.42 |
|                              |                      |               |             |             |       |         |        |        |           |             |
| **NaiveScan_MatchToArray**       | **Dupli(...)cates [32]** |  **1,184.899 ns** |  **23.6315 ns** |  **40.7632 ns** |  **1.00** |    **0.05** | **0.3910** |      **-** |    **3280 B** |        **1.00** |
| Trie_GetMatchCountUpperBound | Dupli(...)cates [32] |     38.005 ns |   0.3301 ns |   0.3088 ns |  0.03 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToSpan             | Dupli(...)cates [32] |     57.350 ns |   0.3661 ns |   0.3245 ns |  0.05 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToArray            | Dupli(...)cates [32] |    376.910 ns |   7.4807 ns |  15.1114 ns |  0.32 |    0.02 | 0.3576 | 0.0014 |    2992 B |        0.91 |
| Trie_MatchDetailedToSpans    | Dupli(...)cates [32] |  1,122.453 ns |  12.4256 ns |  11.0150 ns |  0.95 |    0.03 |      - |      - |         - |        0.00 |
| Trie_MatchDetailedToArray    | Dupli(...)cates [32] |  4,756.187 ns |  54.9165 ns |  48.6820 ns |  4.02 |    0.14 | 3.4332 | 0.1068 |   28744 B |        8.76 |
|                              |                      |               |             |             |       |         |        |        |           |             |
| **NaiveScan_MatchToArray**       | **NoMatch**              | **21,368.671 ns** | **363.1875 ns** | **626.4806 ns** | **1.001** |    **0.04** |      **-** |      **-** |      **32 B** |        **1.00** |
| Trie_GetMatchCountUpperBound | NoMatch              |      9.169 ns |   0.0801 ns |   0.0749 ns | 0.000 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToSpan             | NoMatch              |      7.640 ns |   0.0911 ns |   0.0808 ns | 0.000 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToArray            | NoMatch              |      9.900 ns |   0.2095 ns |   0.2152 ns | 0.000 |    0.00 | 0.0057 |      - |      48 B |        1.50 |
| Trie_MatchDetailedToSpans    | NoMatch              |     11.468 ns |   0.0894 ns |   0.0836 ns | 0.001 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchDetailedToArray    | NoMatch              |     15.275 ns |   0.1363 ns |   0.1275 ns | 0.001 |    0.00 |      - |      - |         - |        0.00 |
|                              |                      |               |             |             |       |         |        |        |           |             |
| **NaiveScan_MatchToArray**       | **ParameterCaptures**    |  **7,009.406 ns** | **121.5824 ns** | **144.7352 ns** | **1.000** |    **0.03** | **0.0076** |      **-** |     **104 B** |        **1.00** |
| Trie_GetMatchCountUpperBound | ParameterCaptures    |     36.072 ns |   0.3422 ns |   0.3201 ns | 0.005 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToSpan             | ParameterCaptures    |     35.138 ns |   0.4568 ns |   0.4272 ns | 0.005 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToArray            | ParameterCaptures    |     50.124 ns |   0.8602 ns |   0.8047 ns | 0.007 |    0.00 | 0.0172 |      - |     144 B |        1.38 |
| Trie_MatchDetailedToSpans    | ParameterCaptures    |     51.632 ns |   0.5714 ns |   0.5345 ns | 0.007 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchDetailedToArray    | ParameterCaptures    |    159.655 ns |   3.2093 ns |   3.8204 ns | 0.023 |    0.00 | 0.0277 |      - |     232 B |        2.23 |
|                              |                      |               |             |             |       |         |        |        |           |             |
| **NaiveScan_MatchToArray**       | **CatchAllTerminal**     |  **7,003.386 ns** | **137.8012 ns** | **147.4457 ns** | **1.000** |    **0.03** | **0.0076** |      **-** |     **104 B** |        **1.00** |
| Trie_GetMatchCountUpperBound | CatchAllTerminal     |     33.877 ns |   0.4069 ns |   0.3806 ns | 0.005 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToSpan             | CatchAllTerminal     |     36.319 ns |   0.3352 ns |   0.2799 ns | 0.005 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchToArray            | CatchAllTerminal     |     52.104 ns |   1.0392 ns |   1.4224 ns | 0.007 |    0.00 | 0.0172 |      - |     144 B |        1.38 |
| Trie_MatchDetailedToSpans    | CatchAllTerminal     |     59.284 ns |   0.8401 ns |   0.7859 ns | 0.008 |    0.00 |      - |      - |         - |        0.00 |
| Trie_MatchDetailedToArray    | CatchAllTerminal     |    164.960 ns |   2.7388 ns |   2.5618 ns | 0.024 |    0.00 | 0.0391 |      - |     328 B |        3.15 |

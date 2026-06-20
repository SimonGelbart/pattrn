window.BENCHMARK_DATA = {
  "lastUpdate": 1781952279709,
  "repoUrl": "https://github.com/SimonGelbart/pattrn",
  "entries": {
    "Pattrn Benchmarks": [
      {
        "commit": {
          "author": {
            "name": "Simon G.",
            "username": "SimonGelbart",
            "email": "simon.gelbart@efrei.net"
          },
          "committer": {
            "name": "Simon G.",
            "username": "SimonGelbart",
            "email": "simon.gelbart@efrei.net"
          },
          "id": "32480cf09e3be2de801544f7fbe1c4019073fd0e",
          "message": "ci: publish and compare benchmark history",
          "timestamp": "2026-06-20T08:29:30Z",
          "url": "https://github.com/SimonGelbart/pattrn/commit/32480cf09e3be2de801544f7fbe1c4019073fd0e"
        },
        "date": 1781948128141,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.Build(Scenario: BuildLargeExact)",
            "value": 4298038.437988281,
            "unit": "ns",
            "range": "± 80220.52830996855"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.GetDiagnostics(Scenario: BuildLargeExact)",
            "value": 3110236.8500976562,
            "unit": "ns",
            "range": "± 60402.37159897679"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.BuildWithValidation(Scenario: BuildLargeExact)",
            "value": 4828847.723214285,
            "unit": "ns",
            "range": "± 72937.50880481537"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.Build(Scenario: BuildLargeParameters)",
            "value": 3748702.354073661,
            "unit": "ns",
            "range": "± 42608.116939792504"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.GetDiagnostics(Scenario: BuildLargeParameters)",
            "value": 3115006.2963541667,
            "unit": "ns",
            "range": "± 30380.128620918807"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.BuildWithValidation(Scenario: BuildLargeParameters)",
            "value": 4372627.209558823,
            "unit": "ns",
            "range": "± 88971.49931186749"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: ExactOnlySparseDeep)",
            "value": 6483.146412658692,
            "unit": "ns",
            "range": "± 15.437167598313886"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: ExactOnlySparseDeep)",
            "value": 49.115177252462935,
            "unit": "ns",
            "range": "± 0.025550225138036568"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: ExactOnlySparseDeep)",
            "value": 54.52546212502888,
            "unit": "ns",
            "range": "± 0.09016083958143116"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: ExactOnlySparseDeep)",
            "value": 59.17721346446446,
            "unit": "ns",
            "range": "± 0.06913587950157689"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: ExactOnlySparseDeep)",
            "value": 57.4683798511823,
            "unit": "ns",
            "range": "± 0.04017687343669699"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: ExactOnlySparseDeep)",
            "value": 73.42096307447979,
            "unit": "ns",
            "range": "± 0.16975274574383617"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: ExactOnlyWideFanOut)",
            "value": 28062.76104482015,
            "unit": "ns",
            "range": "± 16.21124235569771"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: ExactOnlyWideFanOut)",
            "value": 22.15146037723337,
            "unit": "ns",
            "range": "± 0.020106204017872494"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: ExactOnlyWideFanOut)",
            "value": 25.71533113718033,
            "unit": "ns",
            "range": "± 0.03144961391269772"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: ExactOnlyWideFanOut)",
            "value": 29.494530396802084,
            "unit": "ns",
            "range": "± 0.030377257506247278"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: ExactOnlyWideFanOut)",
            "value": 29.076990195115407,
            "unit": "ns",
            "range": "± 0.03529071146894131"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: ExactOnlyWideFanOut)",
            "value": 42.503581072602955,
            "unit": "ns",
            "range": "± 0.06929414093653447"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: DuplicateHeavyDeduplicate)",
            "value": 2781.916867182805,
            "unit": "ns",
            "range": "± 9.707023607457275"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: DuplicateHeavyDeduplicate)",
            "value": 76.04949350357056,
            "unit": "ns",
            "range": "± 0.12028051352868055"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: DuplicateHeavyDeduplicate)",
            "value": 313.36862595876056,
            "unit": "ns",
            "range": "± 0.0702462772781117"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: DuplicateHeavyDeduplicate)",
            "value": 1424.1057188851494,
            "unit": "ns",
            "range": "± 4.907321078489597"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: DuplicateHeavyDeduplicate)",
            "value": 3094.496820449829,
            "unit": "ns",
            "range": "± 3.78775259352449"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: DuplicateHeavyDeduplicate)",
            "value": 5222.639224712665,
            "unit": "ns",
            "range": "± 44.97558878457808"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: DuplicateHeavyPreserveDuplicates)",
            "value": 1964.0249051411947,
            "unit": "ns",
            "range": "± 35.66808890078641"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: DuplicateHeavyPreserveDuplicates)",
            "value": 74.38610651859871,
            "unit": "ns",
            "range": "± 0.029762209002453834"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: DuplicateHeavyPreserveDuplicates)",
            "value": 113.59347484203485,
            "unit": "ns",
            "range": "± 0.13096353979784092"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: DuplicateHeavyPreserveDuplicates)",
            "value": 673.1385339101156,
            "unit": "ns",
            "range": "± 5.744927376446458"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: DuplicateHeavyPreserveDuplicates)",
            "value": 1982.9867829542893,
            "unit": "ns",
            "range": "± 4.007127538704839"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: DuplicateHeavyPreserveDuplicates)",
            "value": 8845.99552001953,
            "unit": "ns",
            "range": "± 141.13771507028764"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: CatchAllTerminal)",
            "value": 11893.303861177885,
            "unit": "ns",
            "range": "± 14.58833271452342"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: CatchAllTerminal)",
            "value": 64.56100444610303,
            "unit": "ns",
            "range": "± 0.03256129127852152"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: CatchAllTerminal)",
            "value": 73.09964076110295,
            "unit": "ns",
            "range": "± 0.02947665032687157"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: CatchAllTerminal)",
            "value": 99.3334977371352,
            "unit": "ns",
            "range": "± 0.7948426398047895"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: CatchAllTerminal)",
            "value": 101.97830427544457,
            "unit": "ns",
            "range": "± 0.07816278803198581"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: CatchAllTerminal)",
            "value": 296.14298875515277,
            "unit": "ns",
            "range": "± 0.2770664554960037"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.Build(Scenario: GetDiagnosticsClean)",
            "value": 4296374.755514706,
            "unit": "ns",
            "range": "± 87537.78163890181"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.GetDiagnostics(Scenario: GetDiagnosticsClean)",
            "value": 3140594.0205078125,
            "unit": "ns",
            "range": "± 88936.23899398238"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.BuildWithValidation(Scenario: GetDiagnosticsClean)",
            "value": 4907868.7475,
            "unit": "ns",
            "range": "± 126858.46861608331"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.Build(Scenario: GetDiagnosticsAmbiguous)",
            "value": 1295970.7332589286,
            "unit": "ns",
            "range": "± 9354.988306138886"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.GetDiagnostics(Scenario: GetDiagnosticsAmbiguous)",
            "value": 2351598.52734375,
            "unit": "ns",
            "range": "± 36571.524982090916"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.BuildWithValidation(Scenario: GetDiagnosticsAmbiguous)",
            "value": 2800663.992057292,
            "unit": "ns",
            "range": "± 31032.898079301507"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_Parse(Scenario: ParseSimple)",
            "value": 126.11778864860534,
            "unit": "ns",
            "range": "± 1.2341386714557296"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_GetPathSegmentCount(Scenario: ParseSimple)",
            "value": 15.882011755307515,
            "unit": "ns",
            "range": "± 0.11928646839221753"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_SplitPath(Scenario: ParseSimple)",
            "value": 83.94709553501822,
            "unit": "ns",
            "range": "± 2.0661428825053423"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_SplitPathToSpan(Scenario: ParseSimple)",
            "value": 92.48365255258977,
            "unit": "ns",
            "range": "± 4.397995333139663"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchPreSplitToSpan(Scenario: ParseSimple)",
            "value": 75.7502144575119,
            "unit": "ns",
            "range": "± 0.04338478601195967"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteWithCallerBufferToSpan(Scenario: ParseSimple)",
            "value": 154.0562032699585,
            "unit": "ns",
            "range": "± 1.0734902146861547"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteToSpan(Scenario: ParseSimple)",
            "value": 202.98604912757872,
            "unit": "ns",
            "range": "± 1.117157164756172"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchPreSplitDetailedToSpans(Scenario: ParseSimple)",
            "value": 113.4934127009832,
            "unit": "ns",
            "range": "± 0.05621871627434714"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteWithCallerBufferDetailedToSpans(Scenario: ParseSimple)",
            "value": 189.64321223894754,
            "unit": "ns",
            "range": "± 1.3718587494063985"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteDetailedToSpans(Scenario: ParseSimple)",
            "value": 218.31154938844534,
            "unit": "ns",
            "range": "± 0.7873636487614488"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteDetailedToArray(Scenario: ParseSimple)",
            "value": 435.5586993013109,
            "unit": "ns",
            "range": "± 1.9418281314697812"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_Parse(Scenario: ParseParameters)",
            "value": 373.9609805515834,
            "unit": "ns",
            "range": "± 2.7356851282129466"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_GetPathSegmentCount(Scenario: ParseParameters)",
            "value": 15.63756456474463,
            "unit": "ns",
            "range": "± 0.09357152736128295"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_SplitPath(Scenario: ParseParameters)",
            "value": 76.4754250049591,
            "unit": "ns",
            "range": "± 0.5128346657941703"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_SplitPathToSpan(Scenario: ParseParameters)",
            "value": 76.60278813441595,
            "unit": "ns",
            "range": "± 0.6034637108907053"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchPreSplitToSpan(Scenario: ParseParameters)",
            "value": 89.29886880942753,
            "unit": "ns",
            "range": "± 0.03184230566317305"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteWithCallerBufferToSpan(Scenario: ParseParameters)",
            "value": 164.1832250016076,
            "unit": "ns",
            "range": "± 0.26350986681139227"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteToSpan(Scenario: ParseParameters)",
            "value": 190.71863136291503,
            "unit": "ns",
            "range": "± 1.5737057162112045"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchPreSplitDetailedToSpans(Scenario: ParseParameters)",
            "value": 110.78548990090688,
            "unit": "ns",
            "range": "± 0.09036596494496867"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteWithCallerBufferDetailedToSpans(Scenario: ParseParameters)",
            "value": 237.71460860570272,
            "unit": "ns",
            "range": "± 1.7627630103686798"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteDetailedToSpans(Scenario: ParseParameters)",
            "value": 226.39473079045612,
            "unit": "ns",
            "range": "± 1.5596181274612997"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteDetailedToArray(Scenario: ParseParameters)",
            "value": 463.3111124356588,
            "unit": "ns",
            "range": "± 0.7692024327168034"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_Parse(Scenario: ParseCatchAll)",
            "value": 246.8358461516244,
            "unit": "ns",
            "range": "± 0.2773142483162568"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_GetPathSegmentCount(Scenario: ParseCatchAll)",
            "value": 12.89488654755629,
            "unit": "ns",
            "range": "± 0.03234109597129872"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_SplitPath(Scenario: ParseCatchAll)",
            "value": 73.91759470303853,
            "unit": "ns",
            "range": "± 0.8477694420484604"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_SplitPathToSpan(Scenario: ParseCatchAll)",
            "value": 68.48209585462298,
            "unit": "ns",
            "range": "± 0.7453384220397968"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchPreSplitToSpan(Scenario: ParseCatchAll)",
            "value": 50.15568979886862,
            "unit": "ns",
            "range": "± 0.021746125279160512"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteWithCallerBufferToSpan(Scenario: ParseCatchAll)",
            "value": 119.12778823192303,
            "unit": "ns",
            "range": "± 0.11982027148630205"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteToSpan(Scenario: ParseCatchAll)",
            "value": 148.38277528967177,
            "unit": "ns",
            "range": "± 0.5806041355706061"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchPreSplitDetailedToSpans(Scenario: ParseCatchAll)",
            "value": 97.21745391533925,
            "unit": "ns",
            "range": "± 0.058521214384313576"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteWithCallerBufferDetailedToSpans(Scenario: ParseCatchAll)",
            "value": 156.17192537443978,
            "unit": "ns",
            "range": "± 0.6477527269931452"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteDetailedToSpans(Scenario: ParseCatchAll)",
            "value": 188.29552105267842,
            "unit": "ns",
            "range": "± 0.7129271010831827"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteDetailedToArray(Scenario: ParseCatchAll)",
            "value": 366.5029626993033,
            "unit": "ns",
            "range": "± 2.337647883495769"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_Parse(Scenario: MatchDetailed)",
            "value": 364.706677709307,
            "unit": "ns",
            "range": "± 1.8262350746102576"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_GetPathSegmentCount(Scenario: MatchDetailed)",
            "value": 16.197900271415712,
            "unit": "ns",
            "range": "± 0.15596858860258545"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_SplitPath(Scenario: MatchDetailed)",
            "value": 86.04212263822555,
            "unit": "ns",
            "range": "± 1.648572968071047"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_SplitPathToSpan(Scenario: MatchDetailed)",
            "value": 78.31806858948299,
            "unit": "ns",
            "range": "± 0.47610668191941236"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchPreSplitToSpan(Scenario: MatchDetailed)",
            "value": 83.81001142355112,
            "unit": "ns",
            "range": "± 0.04390592274893057"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteWithCallerBufferToSpan(Scenario: MatchDetailed)",
            "value": 187.1592617034912,
            "unit": "ns",
            "range": "± 0.26466005535560316"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteToSpan(Scenario: MatchDetailed)",
            "value": 194.4699614683787,
            "unit": "ns",
            "range": "± 2.432328787052033"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchPreSplitDetailedToSpans(Scenario: MatchDetailed)",
            "value": 107.6239636199815,
            "unit": "ns",
            "range": "± 0.045620847602066694"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteWithCallerBufferDetailedToSpans(Scenario: MatchDetailed)",
            "value": 184.28479999762314,
            "unit": "ns",
            "range": "± 0.6657518340518436"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteDetailedToSpans(Scenario: MatchDetailed)",
            "value": 244.5304491360982,
            "unit": "ns",
            "range": "± 1.1663012874284957"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteDetailedToArray(Scenario: MatchDetailed)",
            "value": 503.67396647135416,
            "unit": "ns",
            "range": "± 3.8067996643198048"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: WildcardSparse)",
            "value": 2099.816983631679,
            "unit": "ns",
            "range": "± 5.582253470964896"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: WildcardSparse)",
            "value": 110.74600675106049,
            "unit": "ns",
            "range": "± 0.3475016803972996"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: WildcardSparse)",
            "value": 135.4648841711191,
            "unit": "ns",
            "range": "± 0.05902648201502176"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: WildcardSparse)",
            "value": 211.1775007645289,
            "unit": "ns",
            "range": "± 0.707217642887983"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: WildcardSparse)",
            "value": 199.4172542461982,
            "unit": "ns",
            "range": "± 0.1331503580478696"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: WildcardSparse)",
            "value": 531.7641344706218,
            "unit": "ns",
            "range": "± 0.6528533979163812"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: WildcardDense)",
            "value": 50653.67680576869,
            "unit": "ns",
            "range": "± 42.07440788221643"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: WildcardDense)",
            "value": 99.05270653046094,
            "unit": "ns",
            "range": "± 0.059171831999894314"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: WildcardDense)",
            "value": 133.20796506745475,
            "unit": "ns",
            "range": "± 0.16659754572598223"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: WildcardDense)",
            "value": 203.17615609509605,
            "unit": "ns",
            "range": "± 0.6180302200520901"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: WildcardDense)",
            "value": 194.67047843566309,
            "unit": "ns",
            "range": "± 0.05945948059453818"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: WildcardDense)",
            "value": 494.0889114652361,
            "unit": "ns",
            "range": "± 0.9576623542918375"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: PrefixExactOnly)",
            "value": 50431.61707481971,
            "unit": "ns",
            "range": "± 40.37843318988547"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: PrefixExactOnly)",
            "value": 38.025370343526205,
            "unit": "ns",
            "range": "± 0.03157731916897458"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: PrefixExactOnly)",
            "value": 56.41752511033645,
            "unit": "ns",
            "range": "± 0.02528324434324322"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: PrefixExactOnly)",
            "value": 95.40515085629055,
            "unit": "ns",
            "range": "± 0.5318986540664576"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: PrefixExactOnly)",
            "value": 90.2054715523353,
            "unit": "ns",
            "range": "± 0.1431344155735943"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: PrefixExactOnly)",
            "value": 176.44516272204262,
            "unit": "ns",
            "range": "± 0.2913964177810602"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: PrefixWildcard)",
            "value": 51693.32298060826,
            "unit": "ns",
            "range": "± 28.486725402699275"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: PrefixWildcard)",
            "value": 124.52501858983722,
            "unit": "ns",
            "range": "± 0.25338435076652643"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: PrefixWildcard)",
            "value": 167.31689350421613,
            "unit": "ns",
            "range": "± 0.06294974496617584"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: PrefixWildcard)",
            "value": 263.95243802437415,
            "unit": "ns",
            "range": "± 0.30218582777382136"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: PrefixWildcard)",
            "value": 230.96759722914015,
            "unit": "ns",
            "range": "± 0.4491826127501187"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: PrefixWildcard)",
            "value": 632.6677531514849,
            "unit": "ns",
            "range": "± 0.6430572810568025"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: NoMatch)",
            "value": 36275.22036743164,
            "unit": "ns",
            "range": "± 27.201225558015476"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: NoMatch)",
            "value": 12.169345614101205,
            "unit": "ns",
            "range": "± 0.007901026615435621"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: NoMatch)",
            "value": 12.60625109275182,
            "unit": "ns",
            "range": "± 0.013185769774563696"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: NoMatch)",
            "value": 11.81088708979743,
            "unit": "ns",
            "range": "± 0.027870850991791582"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: NoMatch)",
            "value": 26.043125975699652,
            "unit": "ns",
            "range": "± 0.6404172804831209"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: NoMatch)",
            "value": 19.852287430029648,
            "unit": "ns",
            "range": "± 0.00770493983799599"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: ParameterCaptures)",
            "value": 12178.22190729777,
            "unit": "ns",
            "range": "± 8.09057196943608"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: ParameterCaptures)",
            "value": 63.8603550195694,
            "unit": "ns",
            "range": "± 0.05202454252463622"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: ParameterCaptures)",
            "value": 72.8411995490392,
            "unit": "ns",
            "range": "± 0.03615933994907483"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: ParameterCaptures)",
            "value": 99.77064337180211,
            "unit": "ns",
            "range": "± 0.20668204049794475"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: ParameterCaptures)",
            "value": 92.09717699459621,
            "unit": "ns",
            "range": "± 0.15660023701440062"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: ParameterCaptures)",
            "value": 298.5444303580693,
            "unit": "ns",
            "range": "± 0.6295622144566262"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.Build(Scenario: ValidateOnBuild)",
            "value": 1304576.7749720982,
            "unit": "ns",
            "range": "± 11778.144196329025"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.GetDiagnostics(Scenario: ValidateOnBuild)",
            "value": 2355928.5559895835,
            "unit": "ns",
            "range": "± 38334.11566011416"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.BuildWithValidation(Scenario: ValidateOnBuild)",
            "value": 2777557.490104167,
            "unit": "ns",
            "range": "± 29788.615912017172"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Simon G.",
            "username": "SimonGelbart",
            "email": "simon.gelbart@efrei.net"
          },
          "committer": {
            "name": "Simon G.",
            "username": "SimonGelbart",
            "email": "simon.gelbart@efrei.net"
          },
          "id": "a84c83d15fc78a881b827cc7a2e6200802baf41d",
          "message": "ci: publish custom benchmark dashboard",
          "timestamp": "2026-06-20T09:40:42Z",
          "url": "https://github.com/SimonGelbart/pattrn/commit/a84c83d15fc78a881b827cc7a2e6200802baf41d"
        },
        "date": 1781952279416,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.Build(Scenario: BuildLargeExact)",
            "value": 4346329.8857421875,
            "unit": "ns",
            "range": "± 111641.53429530072"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.GetDiagnostics(Scenario: BuildLargeExact)",
            "value": 3381761.6676352895,
            "unit": "ns",
            "range": "± 119529.76432434101"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.BuildWithValidation(Scenario: BuildLargeExact)",
            "value": 5048105.033854167,
            "unit": "ns",
            "range": "± 104801.71971252246"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.Build(Scenario: BuildLargeParameters)",
            "value": 3896588.645596591,
            "unit": "ns",
            "range": "± 94704.61827212444"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.GetDiagnostics(Scenario: BuildLargeParameters)",
            "value": 3314070.4230363173,
            "unit": "ns",
            "range": "± 111938.74193470288"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.BuildWithValidation(Scenario: BuildLargeParameters)",
            "value": 4436670.430147059,
            "unit": "ns",
            "range": "± 90655.83609189955"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: ExactOnlySparseDeep)",
            "value": 6471.065794881185,
            "unit": "ns",
            "range": "± 17.210539696273248"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: ExactOnlySparseDeep)",
            "value": 48.28316353376095,
            "unit": "ns",
            "range": "± 0.03505717948937055"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: ExactOnlySparseDeep)",
            "value": 54.50096421516859,
            "unit": "ns",
            "range": "± 0.04302822954222121"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: ExactOnlySparseDeep)",
            "value": 59.56856485775539,
            "unit": "ns",
            "range": "± 0.06425515731660492"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: ExactOnlySparseDeep)",
            "value": 57.83270997660501,
            "unit": "ns",
            "range": "± 0.027123041731301818"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: ExactOnlySparseDeep)",
            "value": 74.52305497114475,
            "unit": "ns",
            "range": "± 0.28743749930363277"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: ExactOnlyWideFanOut)",
            "value": 28040.233744694637,
            "unit": "ns",
            "range": "± 7.336243076978141"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: ExactOnlyWideFanOut)",
            "value": 22.10639364215044,
            "unit": "ns",
            "range": "± 0.020447309519637125"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: ExactOnlyWideFanOut)",
            "value": 25.4023309304164,
            "unit": "ns",
            "range": "± 0.06289052312343102"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: ExactOnlyWideFanOut)",
            "value": 37.74395873943965,
            "unit": "ns",
            "range": "± 0.0967741798115116"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: ExactOnlyWideFanOut)",
            "value": 33.06312152972588,
            "unit": "ns",
            "range": "± 0.04805075293556358"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: ExactOnlyWideFanOut)",
            "value": 49.4773083456925,
            "unit": "ns",
            "range": "± 0.17781141930363878"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: DuplicateHeavyDeduplicate)",
            "value": 2772.624820963542,
            "unit": "ns",
            "range": "± 18.96216624613248"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: DuplicateHeavyDeduplicate)",
            "value": 76.25091381256397,
            "unit": "ns",
            "range": "± 0.06354770754705091"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: DuplicateHeavyDeduplicate)",
            "value": 311.28524357931957,
            "unit": "ns",
            "range": "± 0.23437952442944382"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: DuplicateHeavyDeduplicate)",
            "value": 1367.5598546541655,
            "unit": "ns",
            "range": "± 5.043786389904682"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: DuplicateHeavyDeduplicate)",
            "value": 3086.9850635528564,
            "unit": "ns",
            "range": "± 1.9684589398762018"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: DuplicateHeavyDeduplicate)",
            "value": 5223.957996913365,
            "unit": "ns",
            "range": "± 38.2597623112564"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: DuplicateHeavyPreserveDuplicates)",
            "value": 1976.237998668964,
            "unit": "ns",
            "range": "± 8.356790605014785"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: DuplicateHeavyPreserveDuplicates)",
            "value": 78.25553474823634,
            "unit": "ns",
            "range": "± 0.056591813400924874"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: DuplicateHeavyPreserveDuplicates)",
            "value": 109.59462528427441,
            "unit": "ns",
            "range": "± 0.05837337285567542"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: DuplicateHeavyPreserveDuplicates)",
            "value": 646.717965888977,
            "unit": "ns",
            "range": "± 5.815087719356119"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: DuplicateHeavyPreserveDuplicates)",
            "value": 1969.833175114223,
            "unit": "ns",
            "range": "± 3.694654483433019"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: DuplicateHeavyPreserveDuplicates)",
            "value": 8415.435694013324,
            "unit": "ns",
            "range": "± 102.5926422750164"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: CatchAllTerminal)",
            "value": 12009.130247849684,
            "unit": "ns",
            "range": "± 10.4670560438915"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: CatchAllTerminal)",
            "value": 63.79128712415695,
            "unit": "ns",
            "range": "± 0.02178067738365735"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: CatchAllTerminal)",
            "value": 71.7171658362661,
            "unit": "ns",
            "range": "± 0.038698970052101274"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: CatchAllTerminal)",
            "value": 99.33931320508322,
            "unit": "ns",
            "range": "± 0.21290919861159366"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: CatchAllTerminal)",
            "value": 103.72447694341342,
            "unit": "ns",
            "range": "± 0.01700596215081811"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: CatchAllTerminal)",
            "value": 339.93823773520336,
            "unit": "ns",
            "range": "± 2.6891079632623516"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.Build(Scenario: GetDiagnosticsClean)",
            "value": 4201319.359375,
            "unit": "ns",
            "range": "± 84307.76195513565"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.GetDiagnostics(Scenario: GetDiagnosticsClean)",
            "value": 3357659.2411099137,
            "unit": "ns",
            "range": "± 97973.41311799883"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.BuildWithValidation(Scenario: GetDiagnosticsClean)",
            "value": 5055353.468125,
            "unit": "ns",
            "range": "± 134404.73279901207"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.Build(Scenario: GetDiagnosticsAmbiguous)",
            "value": 1447506.4754284273,
            "unit": "ns",
            "range": "± 40084.94051130897"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.GetDiagnostics(Scenario: GetDiagnosticsAmbiguous)",
            "value": 2516489.08203125,
            "unit": "ns",
            "range": "± 47788.622354902676"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.BuildWithValidation(Scenario: GetDiagnosticsAmbiguous)",
            "value": 2918325.290885417,
            "unit": "ns",
            "range": "± 29835.52841787883"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_Parse(Scenario: ParseSimple)",
            "value": 147.07378137111664,
            "unit": "ns",
            "range": "± 1.0313502445716678"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_GetPathSegmentCount(Scenario: ParseSimple)",
            "value": 15.772173124055067,
            "unit": "ns",
            "range": "± 0.07537961026926752"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_SplitPath(Scenario: ParseSimple)",
            "value": 76.51014672006879,
            "unit": "ns",
            "range": "± 1.1953910399235894"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_SplitPathToSpan(Scenario: ParseSimple)",
            "value": 90.05651273197599,
            "unit": "ns",
            "range": "± 3.427893186475642"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchPreSplitToSpan(Scenario: ParseSimple)",
            "value": 74.30545990665753,
            "unit": "ns",
            "range": "± 0.03496983423116775"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteWithCallerBufferToSpan(Scenario: ParseSimple)",
            "value": 181.90689020156861,
            "unit": "ns",
            "range": "± 1.9038840801811672"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteToSpan(Scenario: ParseSimple)",
            "value": 204.47942745685577,
            "unit": "ns",
            "range": "± 4.067334242529833"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchPreSplitDetailedToSpans(Scenario: ParseSimple)",
            "value": 106.92471686999004,
            "unit": "ns",
            "range": "± 0.07595625791587425"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteWithCallerBufferDetailedToSpans(Scenario: ParseSimple)",
            "value": 183.43332505226135,
            "unit": "ns",
            "range": "± 0.6232865968544761"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteDetailedToSpans(Scenario: ParseSimple)",
            "value": 225.07552491823833,
            "unit": "ns",
            "range": "± 1.220534908719514"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteDetailedToArray(Scenario: ParseSimple)",
            "value": 430.17983780588423,
            "unit": "ns",
            "range": "± 2.051031511294521"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_Parse(Scenario: ParseParameters)",
            "value": 377.7586897441319,
            "unit": "ns",
            "range": "± 5.152368671661004"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_GetPathSegmentCount(Scenario: ParseParameters)",
            "value": 15.824931623680252,
            "unit": "ns",
            "range": "± 0.23827240215509338"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_SplitPath(Scenario: ParseParameters)",
            "value": 84.16256787776948,
            "unit": "ns",
            "range": "± 2.5588319021505472"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_SplitPathToSpan(Scenario: ParseParameters)",
            "value": 85.36280523027692,
            "unit": "ns",
            "range": "± 2.4747527012544945"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchPreSplitToSpan(Scenario: ParseParameters)",
            "value": 86.53337745941602,
            "unit": "ns",
            "range": "± 0.07638361176831794"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteWithCallerBufferToSpan(Scenario: ParseParameters)",
            "value": 169.72148050580705,
            "unit": "ns",
            "range": "± 0.7201812663821513"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteToSpan(Scenario: ParseParameters)",
            "value": 195.57161799498968,
            "unit": "ns",
            "range": "± 0.8634705839331143"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchPreSplitDetailedToSpans(Scenario: ParseParameters)",
            "value": 106.8962470094363,
            "unit": "ns",
            "range": "± 0.054047480794325455"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteWithCallerBufferDetailedToSpans(Scenario: ParseParameters)",
            "value": 193.01347897847492,
            "unit": "ns",
            "range": "± 1.7746319501533674"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteDetailedToSpans(Scenario: ParseParameters)",
            "value": 234.06263060569762,
            "unit": "ns",
            "range": "± 2.333793816171331"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteDetailedToArray(Scenario: ParseParameters)",
            "value": 443.15608536402385,
            "unit": "ns",
            "range": "± 1.9443619583285108"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_Parse(Scenario: ParseCatchAll)",
            "value": 206.73341917991638,
            "unit": "ns",
            "range": "± 1.0145144034768931"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_GetPathSegmentCount(Scenario: ParseCatchAll)",
            "value": 12.950386581676346,
            "unit": "ns",
            "range": "± 0.039432319906528165"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_SplitPath(Scenario: ParseCatchAll)",
            "value": 73.29854525725047,
            "unit": "ns",
            "range": "± 2.1906397091493903"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_SplitPathToSpan(Scenario: ParseCatchAll)",
            "value": 76.92427164713541,
            "unit": "ns",
            "range": "± 1.4231079463682028"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchPreSplitToSpan(Scenario: ParseCatchAll)",
            "value": 50.7706353465716,
            "unit": "ns",
            "range": "± 0.02299483264373763"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteWithCallerBufferToSpan(Scenario: ParseCatchAll)",
            "value": 120.86384643041171,
            "unit": "ns",
            "range": "± 0.5331567557235358"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteToSpan(Scenario: ParseCatchAll)",
            "value": 149.53464891115826,
            "unit": "ns",
            "range": "± 0.5719860359194406"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchPreSplitDetailedToSpans(Scenario: ParseCatchAll)",
            "value": 83.96701296170552,
            "unit": "ns",
            "range": "± 0.09972620375750862"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteWithCallerBufferDetailedToSpans(Scenario: ParseCatchAll)",
            "value": 157.97566945212228,
            "unit": "ns",
            "range": "± 1.3088875248309635"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteDetailedToSpans(Scenario: ParseCatchAll)",
            "value": 203.57613838513691,
            "unit": "ns",
            "range": "± 0.8232181116597571"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteDetailedToArray(Scenario: ParseCatchAll)",
            "value": 351.6987318992615,
            "unit": "ns",
            "range": "± 2.136863986491165"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_Parse(Scenario: MatchDetailed)",
            "value": 371.51832521878754,
            "unit": "ns",
            "range": "± 3.8321424588491304"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_GetPathSegmentCount(Scenario: MatchDetailed)",
            "value": 16.235367633899052,
            "unit": "ns",
            "range": "± 0.1484216824098961"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_SplitPath(Scenario: MatchDetailed)",
            "value": 83.48239354689916,
            "unit": "ns",
            "range": "± 1.24009949124107"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RoutePattern_SplitPathToSpan(Scenario: MatchDetailed)",
            "value": 79.69906013745528,
            "unit": "ns",
            "range": "± 0.39304739527569527"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchPreSplitToSpan(Scenario: MatchDetailed)",
            "value": 75.08395925362905,
            "unit": "ns",
            "range": "± 0.04040983458854701"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteWithCallerBufferToSpan(Scenario: MatchDetailed)",
            "value": 157.5071562767029,
            "unit": "ns",
            "range": "± 0.8710126577033734"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteToSpan(Scenario: MatchDetailed)",
            "value": 193.22086582865035,
            "unit": "ns",
            "range": "± 2.2447973788870317"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchPreSplitDetailedToSpans(Scenario: MatchDetailed)",
            "value": 106.71434123175484,
            "unit": "ns",
            "range": "± 0.1374642448181164"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteWithCallerBufferDetailedToSpans(Scenario: MatchDetailed)",
            "value": 196.32989510468073,
            "unit": "ns",
            "range": "± 0.8479531019389899"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteDetailedToSpans(Scenario: MatchDetailed)",
            "value": 226.6029419263204,
            "unit": "ns",
            "range": "± 1.6606384720015945"
          },
          {
            "name": "Pattrn.Benchmarks.RoutingBenchmarks.RouteIndex_MatchRouteDetailedToArray(Scenario: MatchDetailed)",
            "value": 476.4960231781006,
            "unit": "ns",
            "range": "± 1.206460191185971"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: WildcardSparse)",
            "value": 2090.5632068089076,
            "unit": "ns",
            "range": "± 4.44536412469851"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: WildcardSparse)",
            "value": 102.90272839711263,
            "unit": "ns",
            "range": "± 0.03517857159524012"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: WildcardSparse)",
            "value": 145.7107340189127,
            "unit": "ns",
            "range": "± 0.1582188152285929"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: WildcardSparse)",
            "value": 206.27738908358984,
            "unit": "ns",
            "range": "± 0.5402859345605708"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: WildcardSparse)",
            "value": 173.84937917269193,
            "unit": "ns",
            "range": "± 0.08161490834732729"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: WildcardSparse)",
            "value": 499.5600880895342,
            "unit": "ns",
            "range": "± 0.47359390900122034"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: WildcardDense)",
            "value": 51375.21241251627,
            "unit": "ns",
            "range": "± 108.55359267398764"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: WildcardDense)",
            "value": 109.52966975248776,
            "unit": "ns",
            "range": "± 0.2317250634799702"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: WildcardDense)",
            "value": 137.89520416940962,
            "unit": "ns",
            "range": "± 0.07806673280387597"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: WildcardDense)",
            "value": 219.53155270644598,
            "unit": "ns",
            "range": "± 0.3294025131662669"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: WildcardDense)",
            "value": 175.84902816159385,
            "unit": "ns",
            "range": "± 0.10877594984088927"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: WildcardDense)",
            "value": 503.3462302344186,
            "unit": "ns",
            "range": "± 1.3702976963037152"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: PrefixExactOnly)",
            "value": 49835.052337646484,
            "unit": "ns",
            "range": "± 25.205124600716857"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: PrefixExactOnly)",
            "value": 36.85773486324719,
            "unit": "ns",
            "range": "± 0.025454039516705582"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: PrefixExactOnly)",
            "value": 57.823911954959236,
            "unit": "ns",
            "range": "± 0.03262574769566488"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: PrefixExactOnly)",
            "value": 96.01086688893182,
            "unit": "ns",
            "range": "± 0.502236534925635"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: PrefixExactOnly)",
            "value": 90.90207202945437,
            "unit": "ns",
            "range": "± 0.17284864895185345"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: PrefixExactOnly)",
            "value": 177.89831248351507,
            "unit": "ns",
            "range": "± 0.728576521082979"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: PrefixWildcard)",
            "value": 51499.00126139323,
            "unit": "ns",
            "range": "± 94.06758184494088"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: PrefixWildcard)",
            "value": 130.7944621489598,
            "unit": "ns",
            "range": "± 0.0643770546680467"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: PrefixWildcard)",
            "value": 166.87715580830206,
            "unit": "ns",
            "range": "± 0.07529121961710278"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: PrefixWildcard)",
            "value": 258.1460826580341,
            "unit": "ns",
            "range": "± 0.8933912768590696"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: PrefixWildcard)",
            "value": 228.86383830584012,
            "unit": "ns",
            "range": "± 0.3544806992647981"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: PrefixWildcard)",
            "value": 650.277857046861,
            "unit": "ns",
            "range": "± 2.208516680038699"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: NoMatch)",
            "value": 36402.27336707482,
            "unit": "ns",
            "range": "± 23.029574591706798"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: NoMatch)",
            "value": 18.674812023128784,
            "unit": "ns",
            "range": "± 0.01798891946672046"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: NoMatch)",
            "value": 12.711045588765826,
            "unit": "ns",
            "range": "± 0.022384806387600518"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: NoMatch)",
            "value": 21.52116166628324,
            "unit": "ns",
            "range": "± 0.036494417477650684"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: NoMatch)",
            "value": 26.252226850161186,
            "unit": "ns",
            "range": "± 0.012038965526051635"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: NoMatch)",
            "value": 15.464246802605116,
            "unit": "ns",
            "range": "± 0.03157479011400012"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.NaiveScan_MatchToArray(Scenario: ParameterCaptures)",
            "value": 12088.451377281775,
            "unit": "ns",
            "range": "± 50.14321968478022"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound(Scenario: ParameterCaptures)",
            "value": 63.71610724925995,
            "unit": "ns",
            "range": "± 0.02465936839874904"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToSpan(Scenario: ParameterCaptures)",
            "value": 71.70865855614345,
            "unit": "ns",
            "range": "± 0.07048608722475047"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchToArray(Scenario: ParameterCaptures)",
            "value": 101.89051004818508,
            "unit": "ns",
            "range": "± 0.5025624308656755"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToSpans(Scenario: ParameterCaptures)",
            "value": 93.7284134881837,
            "unit": "ns",
            "range": "± 0.16843271722228567"
          },
          {
            "name": "Pattrn.Benchmarks.PattrnIndexBenchmarks.Trie_MatchDetailedToArray(Scenario: ParameterCaptures)",
            "value": 295.25204478777374,
            "unit": "ns",
            "range": "± 1.1195642976603655"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.Build(Scenario: ValidateOnBuild)",
            "value": 1465839.9305555555,
            "unit": "ns",
            "range": "± 31150.491908006723"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.GetDiagnostics(Scenario: ValidateOnBuild)",
            "value": 2557280.988914696,
            "unit": "ns",
            "range": "± 85044.67522082303"
          },
          {
            "name": "Pattrn.Benchmarks.BuilderBenchmarks.BuildWithValidation(Scenario: ValidateOnBuild)",
            "value": 3030427.8889508927,
            "unit": "ns",
            "range": "± 52481.80825099599"
          }
        ]
      }
    ]
  }
}
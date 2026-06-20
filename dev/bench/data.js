window.BENCHMARK_DATA = {
  "lastUpdate": 1781948128924,
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
      }
    ]
  }
}
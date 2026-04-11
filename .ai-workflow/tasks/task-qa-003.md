# 浠诲姟: TASK-QA-003 M4 楠岃瘉涓庡叧鍗曟敹鏁?
## 鐩爣锛圙oal锛?瀹屾垚 M4 鐨?Build/Test/Smoke/Perf 涓庤川閲忛棬绂佹敹鏁涳紝琛ラ綈褰掓。涓変欢濂楀苟鎻愪氦 Human 澶嶉獙鍏冲崟銆?
## 浠诲姟鏉ユ簮锛圱askSource锛?DispatchAgent

## 璁″垝寮曠敤锛堝吋瀹瑰埆鍚嶏細PlanRef锛?`PLAN-M4-2026-04-11`

## 閲岀▼纰戝紩鐢紙鍏煎鍒悕锛歁ilestoneRef锛?`M4-SceneRenderPipeline`

## 鎵ц浠ｇ悊锛圗xecutionAgent锛?Exec-QA

## 浼樺厛绾э紙Priority锛?P0
> 璇存槑锛氫紭鍏堢骇涓诲畾涔夋潵鑷?`璁″垝寮曠敤`锛汥ispatch 浠呭厑璁稿悓閲岀▼纰戝唴寰皟銆?
## 涓绘ā鍧楀綊灞烇紙PrimaryModule锛?Engine.App

## 娆＄骇妯″潡锛圫econdaryModules锛?- Engine.Scene
- Engine.Render
- Engine.Platform

## 杈圭晫鍚堝悓璺緞锛圔oundaryContractPath锛?- `.ai-workflow/boundaries/engine-app.md`

## 鍩虹嚎寮曠敤锛圔aselineRef锛?- `references/project-baseline.md`

## 骞惰璁″垝锛圥arallelPlan锛?- ParallelGroup: `G4`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SCENE-002`
  - `TASK-REND-004`
  - `TASK-APP-003`

## 鑼冨洿锛圫cope锛?- AllowedModules:
  - Engine.App
- AllowedFiles:
  - M4 楠岃瘉璇佹嵁涓庡叧鍗曞噯澶囩浉鍏虫枃浠?  - 蹇呰娴嬭瘯鏂囦欢
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/**`

## 璺ㄦā鍧楁爣璁帮紙CrossModule锛?false

## 闈炶寖鍥达紙OutOfScope锛?- 涓嶆柊澧?M4 浠ュ鍔熻兘
- 涓嶄慨鏀?Scene/Render 涓氬姟閫昏緫鐩爣
- 涓嶆浛浠?Human 鎵ц鏈€缁堝叧鍗?- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `src/Engine.Render/**`
  - `src/Engine.Asset/**`

## 渚濊禆绾︽潫锛圖ependencyContract锛?- AllowedDependsOn:
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.Render`
  - `Engine.App -> Engine.Platform`
- ForbiddenDependsOn:
  - 涓鸿繃闂ㄧ鎵╁ぇ浠诲姟鑼冨洿
  - 闅愬紡淇敼宸查獙鏀惰涔?
## 杈圭晫鍚屾璁″垝锛圔oundarySyncPlan锛?- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `[]`
- ChangeLogRequired: `true`

## 楠屾敹鏍囧噯锛圓cceptance锛?- Build: `dotnet build -c Debug` 涓?`dotnet build -c Release` 閫氳繃
- Test: `dotnet test` 閫氳繃锛涙柊澧?璋冩暣鏈€灏忛摼璺祴璇曢€氳繃
- Smoke: 鍙惎鍔ㄥ苟鎸佺画娓叉煋 30 绉掍互涓婏紝鍏抽棴鍚庨€€鍑虹爜 `0`
- Perf: 鐩告瘮 M3 鏃犳槑鏄鹃€€鍖栵紙甯ф椂闂存棤寮傚父鎶栧姩锛?- CodeQuality:
  - NoNewHighRisk: `true`
  - MustFixCount: `0`
  - MustFixDisposition: `none`
- DesignQuality:
  - DQ-1 鑱岃矗鍗曚竴锛圫RP锛? `pass`
  - DQ-2 渚濊禆鍙嶈浆锛圖IP锛? `pass`
  - DQ-3 鎵╁睍鐐逛繚鐣欙紙OCP-oriented锛? `pass`
  - DQ-4 寮€闂€ц瘎浼帮紙鍙€夛級: `warn`

## 浜や粯鐗╋紙Deliverables锛?- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)

## 鐘舵€侊紙Status锛?Done

## 瀹屾垚搴︼紙Completion锛?`100`

## 缂洪櫡鍥炴祦瀛楁锛圖efect Triage锛?- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pass`

## 褰掓。锛圓rchive锛?- ArchivePath: `.ai-workflow/archive/2026-04/TASK-QA-003.md`
- ClosedAt: `2026-04-11 12:05`
- Summary:
  - 瀹屾垚 M4 Build/Test/Smoke/Perf 鍏ㄩ噺闂ㄧ澶嶆牳骞跺洖濉瘉鎹?  - 琛ュ厖 `Engine.Render.Tests` 鐙珛娴嬭瘯閾捐矾楠岃瘉锛岃鐩栧満鏅彁浜ゆ秷璐规渶灏忛摼璺?  - 鏀舵暃 M4 璐ㄩ噺鏉＄洰锛歂oNewHighRisk=true銆丮ustFixCount=0銆丮ustFixDisposition=none
  - 瀹屾垚褰掓。涓変欢濂楀苟鍏冲崟瀹屾垚锛屼笌 Human 楠屾敹缁撹瀵归綈`- FilesChanged:
  - `.ai-workflow/tasks/task-qa-003.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-QA-003.md`
  - `.ai-workflow/boundaries/engine-app.md`
- ValidationEvidence:
  - Build(Debug): pass锛坄dotnet build -c Debug -m:1`锛屽瓨鍦ㄧ幆澧冪骇 CS1668 璀﹀憡锛?  - Build(Release): pass锛坄dotnet build -c Release -m:1`锛?  - Test: pass锛坄dotnet test -m:1` + `dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1`锛?  - Smoke: pass锛坄ANS_ENGINE_USE_NATIVE_WINDOW=false`锛宍ANS_ENGINE_AUTO_EXIT_SECONDS=30`锛宍ExitCode=0`锛岀害 `30.14s`锛?  - Perf: pass锛坄ANS_ENGINE_USE_NATIVE_WINDOW=false`锛宍ANS_ENGINE_AUTO_EXIT_SECONDS=45`锛宍ExitCode=0`锛岀害 `45.13s`锛?  - CodeQuality: pass锛圢oNewHighRisk=true锛孧ustFixCount=0锛孧ustFixDisposition=none锛?  - DesignQuality: DQ-1 pass / DQ-2 pass / DQ-3 pass / DQ-4 warn锛堝彲鎺ュ彈锛?- ModuleAttributionCheck: pass



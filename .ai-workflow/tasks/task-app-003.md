# 浠诲姟: TASK-APP-003 M4 鎻愪氦娴佺▼缂栨帓閰嶅

## 鐩爣锛圙oal锛?淇濇寔 `Engine.App` 缂栨帓鑱岃矗涓嶅彉锛屽畬鎴?M4 鍦烘櫙鏇存柊/鎻愪氦/娓叉煋闃舵琛旀帴涓庣敓鍛藉懆鏈熸敹鍙ｉ厤濂椼€?
## 浠诲姟鏉ユ簮锛圱askSource锛?DispatchAgent

## 璁″垝寮曠敤锛堝吋瀹瑰埆鍚嶏細PlanRef锛?`PLAN-M4-2026-04-11`

## 閲岀▼纰戝紩鐢紙鍏煎鍒悕锛歁ilestoneRef锛?`M4-SceneRenderPipeline`

## 鎵ц浠ｇ悊锛圗xecutionAgent锛?Exec-App

## 浼樺厛绾э紙Priority锛?P0
> 璇存槑锛氫紭鍏堢骇涓诲畾涔夋潵鑷?`璁″垝寮曠敤`锛汥ispatch 浠呭厑璁稿悓閲岀▼纰戝唴寰皟銆?
## 涓绘ā鍧楀綊灞烇紙PrimaryModule锛?Engine.App

## 娆＄骇妯″潡锛圫econdaryModules锛?- Engine.Scene
- Engine.Render

## 杈圭晫鍚堝悓璺緞锛圔oundaryContractPath锛?- `.ai-workflow/boundaries/engine-app.md`

## 鍩虹嚎寮曠敤锛圔aselineRef锛?- `references/project-baseline.md`

## 骞惰璁″垝锛圥arallelPlan锛?- ParallelGroup: `G3`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SCENE-002`
  - `TASK-REND-004`

## 鑼冨洿锛圫cope锛?- AllowedModules:
  - Engine.App
- AllowedFiles:
  - 搴旂敤缂栨帓涓庣敓鍛藉懆鏈熼厤濂楁枃浠?  - 搴旂敤妯″潡娴嬭瘯鏂囦欢
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/**`

## 璺ㄦā鍧楁爣璁帮紙CrossModule锛?false

## 闈炶寖鍥达紙OutOfScope锛?- 涓嶅湪 `Engine.App` 瀹炵幇鍏蜂綋缁樺埗閫昏緫
- 涓嶆墿灞曟潗璐?璧勬簮绯荤粺
- 涓嶅苟琛屾帹杩?M5 鑼冨洿
- OutOfScopePaths:
  - `src/Engine.Render/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Asset/**`

## 渚濊禆绾︽潫锛圖ependencyContract锛?- AllowedDependsOn:
  - `Engine.App -> Engine.Core`
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.Render`
- ForbiddenDependsOn:
  - `Engine.App` 鍐呯洿鎺ュ疄鐜版覆鏌撳悗绔粏鑺?  - `Engine.App` 鍐呯洿鎺ュ疄鐜板満鏅唴閮ㄩ€昏緫

## 杈圭晫鍚屾璁″垝锛圔oundarySyncPlan锛?- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `[]`
- ChangeLogRequired: `true`

## 楠屾敹鏍囧噯锛圓cceptance锛?- Build: `dotnet build -c Debug` 涓?`dotnet build -c Release` 閫氳繃
- Test: `dotnet test` 閫氳繃锛岀紪鎺掗摼璺浉鍏虫祴璇曢€氳繃
- Smoke: 搴旂敤鍙惎鍔ㄥ苟鎸佺画娓叉煋 30 绉掍互涓婏紝鍏抽棴鍚庨€€鍑虹爜 `0`
- Perf: 涓诲惊鐜皟搴︽棤鏄庢樉閫€鍖?
## 浜や粯鐗╋紙Deliverables锛?- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)

## 鐘舵€侊紙Status锛?Review

## 瀹屾垚搴︼紙Completion锛?`95`

## 缂洪櫡鍥炴祦瀛楁锛圖efect Triage锛?- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pass`

## 褰掓。锛圓rchive锛?- ArchivePath: `.ai-workflow/archive/2026-04/TASK-APP-003.md`
- ClosedAt: `2026-04-11 11:55`
- Summary:
  - `Engine.App` 鍦ㄧ粍鍚堟牴涓皢 `SceneGraphService` 浣滀负 `ISceneRenderContractProvider` 娉ㄥ叆 `NullRenderer`
  - 涓诲惊鐜淮鎸?`ProcessEvents -> Input/Time -> Render -> Present` 缂栨帓鑱岃矗锛屼笉寮曞叆娓叉煋鍚庣缁嗚妭
  - M4 閾捐矾瀹炵幇鈥滃満鏅緭鍑?-> 娓叉煋娑堣垂鈥濈殑搴旂敤灞傝鎺?  - 鍚屾鏇存柊 `Engine.App` 杈圭晫鍚堝悓鍙樻洿璁板綍
- FilesChanged:
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/tasks/task-app-003.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-APP-003.md`
- ValidationEvidence:
  - Build(Debug): pass锛坄dotnet build -c Debug -m:1`锛屽瓨鍦ㄧ幆澧冪骇 CS1668 璀﹀憡锛?  - Build(Release): pass锛坄dotnet build -c Release -m:1`锛屽瓨鍦ㄧ幆澧冪骇 CS1668 璀﹀憡锛?  - Test: fail -> pass锛堥杞?`dotnet test -m:1` 鍥?`CS2012` 鏂囦欢鍗犵敤澶辫触锛屾竻鐞嗚繘绋嬪悗澶嶈窇閫氳繃锛?  - Smoke: pass锛坄ANS_ENGINE_USE_NATIVE_WINDOW=false`锛宍ANS_ENGINE_AUTO_EXIT_SECONDS=30`锛宍ExitCode=0`锛岀害 `30.15s`锛?  - Perf: pass锛坄ANS_ENGINE_USE_NATIVE_WINDOW=false`锛宍ANS_ENGINE_AUTO_EXIT_SECONDS=45`锛宍ExitCode=0`锛岀害 `45.15s`锛?- ModuleAttributionCheck: pass


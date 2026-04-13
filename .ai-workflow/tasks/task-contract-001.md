# 浠诲姟: TASK-CONTRACT-001 M4 鐙珛濂戠害灞傚缓绔嬩笌杈圭晫钀界洏

## 鐩爣锛圙oal锛?寤虹珛鐙珛濂戠害灞傦紙`src/Engine.Contracts`锛夊強鏈€灏忔覆鏌撹緭鍏ュ绾︾被鍨嬶紝鏄庣‘鍏舵ā鍧楄竟鐣屼笌渚濊禆瑙勫垯锛屼綔涓哄悗缁?Scene/Render/App 鏀归€犵殑鍓嶇疆渚濊禆銆?
## 浠诲姟鏉ユ簮锛圱askSource锛?DispatchAgent

## 璁″垝寮曠敤锛堝吋瀹瑰埆鍚嶏細PlanRef锛?`PLAN-M4B-2026-04-13`

## 閲岀▼纰戝紩鐢紙鍏煎鍒悕锛歁ilestoneRef锛?`M4-SceneRenderPipeline`

## 鎵ц浠ｇ悊锛圗xecutionAgent锛?Exec-Scene

## 浼樺厛绾э紙Priority锛?P0
> 璇存槑锛氫紭鍏堢骇涓诲畾涔夋潵鑷?`璁″垝寮曠敤`锛汥ispatch 浠呭厑璁稿悓閲岀▼纰戝唴寰皟銆?
## 涓绘ā鍧楀綊灞烇紙PrimaryModule锛?鍏朵粬锛堥渶璇存槑锛?
## 娆＄骇妯″潡锛圫econdaryModules锛?- Engine.Scene
- Engine.Render
- Engine.App

## 杈圭晫鍚堝悓璺緞锛圔oundaryContractPath锛?- `.ai-workflow/boundaries/engine-contracts.md`

## 鍩虹嚎寮曠敤锛圔aselineRef锛?- `references/project-baseline.md`

## 骞惰璁″垝锛圥arallelPlan锛?- ParallelGroup: `G2`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-REND-005`

## 鑼冨洿锛圫cope锛?- AllowedModules:
  - Engine.Contracts
- AllowedFiles:
  - 濂戠害灞傞」鐩笌鏈€灏忓绾︾被鍨嬪畾涔?  - 濂戠害灞傛渶灏忔祴璇曟枃浠?- AllowedPaths:
  - `src/Engine.Contracts/**`
  - `tests/**`
> 璇存槑锛歚AllowedPaths` 浠呯敤浜庢簮鐮?娴嬭瘯鏀瑰姩鑼冨洿锛屼笉鍖呭惈杈圭晫鏂囨。璺緞銆?
## 璺ㄦā鍧楁爣璁帮紙CrossModule锛?true

## 闈炶寖鍥达紙OutOfScope锛?- 涓嶅湪鏈崱鍐呭畬鎴?Render/Scene 娑堣垂鏀归€?- 涓嶅紩鍏ユ潗璐ㄧ郴缁熴€佽祫婧愬鍏ヤ綋绯?- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `src/Engine.Render/**`
  - `src/Engine.App/**`
  - `src/Engine.Asset/**`
  - `src/Engine.Platform/**`

## 渚濊禆绾︽潫锛圖ependencyContract锛?- AllowedDependsOn:
  - `Engine.Contracts -> Engine.Core`
- ForbiddenDependsOn:
  - `Engine.Contracts -> Engine.Scene`
  - `Engine.Contracts -> Engine.Render`
  - `Engine.Contracts -> Engine.App`

## 杈圭晫鍙樻洿璇锋眰锛圔oundaryChangeRequest锛?- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 杈圭晫鍚屾璁″垝锛圔oundarySyncPlan锛?- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-contracts.md`
- ChangeLogRequired: `true`
> 璇存槑锛歚BoundaryDocsToUpdate` 涓虹嫭绔嬭鍒欙紝涓嶅彈 `AllowedPaths` 闄愬埗銆?> 瑙﹀彂鏉′欢锛氫粎褰?`NewFilesExpected=true` 鎴栨墽琛屼腑瀹為檯鏂板婧愮爜/娴嬭瘯鏂囦欢鏃讹紝鎵嶅己鍒舵墽琛岃竟鐣屾枃妗ｆ洿鏂般€?
## 楠屾敹鏍囧噯锛圓cceptance锛?- Build: `dotnet build -c Debug` 涓?`dotnet build -c Release` 閫氳繃
- Test: `dotnet test` 閫氳繃锛屽绾﹀眰鏈€灏忔祴璇曢€氳繃
- Smoke: 濂戠害灞傛帴鍏ュ悗搴旂敤鍙惎鍔ㄤ笖涓嶅穿婧?- Perf: 濂戠害灞傚紩鍏ュ悗鏃犳槑鏄炬€ц兘閫€鍖?
## 浜や粯鐗╋紙Deliverables锛?- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)

## 鐘舵€侊紙Status锛?Done

## 瀹屾垚搴︼紙Completion锛?`100`

## 缂洪櫡鍥炴祦瀛楁锛圖efect Triage锛?- FailureType: `PostAcceptanceBug`
- DetectedAt: `2026-04-13`
- ReopenReason:
- OriginTaskId: `TASK-REND-004`
- HumanSignoff: `pass`

## 褰掓。锛圓rchive锛?- ArchivePath: `.ai-workflow/archive/2026-04/TASK-CONTRACT-001.md`
- ClosedAt: `2026-04-13 21:00`
- Summary:
  - 鏂板缓鐙珛濂戠害灞?`Engine.Contracts`锛屼笅娌夋渶灏忔覆鏌撹緭鍏ュ绾︾被鍨嬨€?  - 鏂板缓 `Engine.Contracts.Tests` 骞惰ˉ鍏呮渶灏忓绾︽祴璇曠敤渚嬨€?  - 鍚屾 `engine-contracts` 杈圭晫鏂囨。涓庡彉鏇磋褰曪紝瀹屾垚褰掓。涓変欢濂椼€?- FilesChanged:
  - `AnsEngine.sln`
  - `src/Engine.Contracts/Engine.Contracts.csproj`
  - `src/Engine.Contracts/SceneRenderContracts.cs`
  - `tests/Engine.Contracts.Tests/Engine.Contracts.Tests.csproj`
  - `tests/Engine.Contracts.Tests/SceneRenderContractsTests.cs`
  - `.ai-workflow/boundaries/engine-contracts.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-CONTRACT-001.md`
- ValidationEvidence:
  - Build(Debug): pass锛坄dotnet build -c Debug -m:1`锛?  - Build(Release): pass锛坄dotnet build -c Release -m:1`锛?  - Test: pass锛坄dotnet test -m:1`锛宍Engine.Contracts.Tests` 2/2锛?  - Smoke: pass锛坄ANS_ENGINE_USE_NATIVE_WINDOW=false`锛?5s 鍙ｅ緞锛宍ExitCode=0`锛?  - Perf: pass锛坄ANS_ENGINE_USE_NATIVE_WINDOW=false`锛?0s 鍙ｅ緞锛宍ExitCode=0`锛?- ModuleAttributionCheck: pass

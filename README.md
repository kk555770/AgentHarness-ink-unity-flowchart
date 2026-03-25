# ink-Unity integration

這個 repo 不是單純的上游 `ink-unity-integration` 鏡像。
它目前是：

- canonical narrative graph / schema / JSON control plane
- current GraphToolkit authoring workflow
- current Unity Runtime / save / load / rollback
- docs / tests / CI / versioned plans

## Start Here

1. `Documentation/CurrentMilestones.md`
2. `Documentation/DocsIndex.md`
3. `Documentation/ARCHITECTURE.md`
4. `Documentation/PLANS.md`
5. `Documentation/PRODUCT_SENSE.md`
6. `Documentation/QUALITY_SCORE.md`
7. `Documentation/RELIABILITY.md`
8. `Documentation/SECURITY.md`

## Knowledge Map

- `Documentation/`
  - 正式知識入口
- `PlanningWithFiles/`
  - 版本化工作記錄
- `Packages/com.opsidanos.ink/`
  - Runtime / canonical core / projection service
- `Assets/Editor/FlowChart/`
  - current GraphToolkit authoring shell
- `Assets/Editor/Tests/`
  - EditMode / contract / canonical / projection tests
- `Assets/Tests/PlayMode/`
  - Runtime / UI / save-load-rollback tests

## Most Useful Docs

- `Documentation/NarrativeGraphArchitecture.md`
  - 架構北極星
- `Documentation/AuthoringToolStrategy.md`
  - 作者工具長期方向
- `Documentation/CurrentAuthoringWorkflow.md`
  - 現行 GraphToolkit workflow
- `Documentation/CanonicalGraphSchema.md`
  - canonical truth 的責任邊界
- `Documentation/CanonicalGraphApiSpec.md`
  - canonical control plane 語意 API
- `Documentation/CanonicalGraphJsonContract.md`
  - 第一版 JSON wire contract
- `Documentation/DeveloperModeOutputContract.md`
  - current projection / runtime contract
- `Documentation/GraphToolkitSpec.md`
  - Graph Toolkit Editor 工具規格
- `Documentation/UIToolkitSpec.md`
  - UI Toolkit UXML / USS 規格

## Runtime Entry

- `Packages/com.opsidanos.ink/README.md`

## Legacy / Upstream Docs

- `Packages/Ink/README.md`
- `Documentation/InkPlayerWindow.md`

## Validation

- GitHub Actions：`.github/workflows/CI.yml`
- Repo guard：`python3 Tools/repo_guard.py`
- 本地測試：`Tools/run_tests.sh`

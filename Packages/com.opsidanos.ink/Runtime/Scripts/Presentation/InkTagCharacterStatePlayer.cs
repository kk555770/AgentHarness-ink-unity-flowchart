// ===== 變更開始 =====
// 2026/01/25 Opsidanos (修改原因：支援可排程的轉場 steps（可循序/可同時），並在動作期間把角色置頂且結束後重置層級)
// 預期結果：每句 char 可用 steps 控制 appear/move/disappear 的先後與同時；動作中的角色會暫時在角色層最上面，整串結束後回到基準層級
using System;
using System.Collections;
using System.Collections.Generic;
using OpsidanosInk.Runtime.Story;
using OpsidanosInk.Runtime.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace OpsidanosInk.Runtime.Presentation
{
    public sealed class InkTagCharacterStatePlayer : MonoBehaviour, IAdvanceBlocker
    {
        [Serializable]
        private sealed class ActorExpressionTextureBinding
        {
            public string Actor;
            public string Expression;
            public bool IsDefault;
            public Texture2D Texture;
        }

        [Serializable]
        private sealed class CharTagPayload
        {
            public string mode;
            public TransitionPayload transition;
            public SlotPayload left;
            public SlotPayload center;
            public SlotPayload right;
        }

        [Serializable]
        private sealed class TransitionPayload
        {
            public float appear = -1f;
            public float disappear = -1f;
            public float fade = -1f;
            public float move = -1f;
            public bool instant;
            public TransitionStepPayload[] steps;
        }

        [Serializable]
        private sealed class TransitionStepPayload
        {
            public string[] actions;
            public bool raise = true;
            public string[] raiseActors;
        }

        [Serializable]
        private sealed class SlotPayload
        {
            public string actor;
            public string expr;
        }

        private enum Slot
        {
            Left,
            Center,
            Right
        }

        [Flags]
        private enum TransitionActions
        {
            None = 0,
            Appear = 1 << 0,
            Move = 1 << 1,
            Disappear = 1 << 2
        }

        private sealed class TransitionStep
        {
            public TransitionActions Actions;
            public bool Raise;
            public string[] RaiseActors;
        }

        private struct SlotState
        {
            public string Actor;
            public string Expression;

            public bool IsEmpty => string.IsNullOrEmpty(Actor);
        }

        [Header("Refs")]
        [SerializeField] private InkTagEventRouter tagEventRouter;
        [SerializeField] private UIDocument uiDocument;
        // ===== 變更開始 =====
        // 2026/01/25 Opsidanos (修改原因：改用集中式 ResourceMap（JSON）做資源映射，避免每個元件各自維護 bindings)
        // 預期結果：char JSON 的 actor/expr 只要提供名稱，就能從同一份資源映射取得 Texture2D
        [SerializeField] private InkResourceMap resourceMap;
        // ===== 變更結束 =====

        [Header("UXML")]
        [SerializeField] private string characterLayerElementName = "CharacterLayer";
        [SerializeField] private string characterLeftElementName = "CharacterLeft";
        [SerializeField] private string characterCenterElementName = "CharacterCenter";
        [SerializeField] private string characterRightElementName = "CharacterRight";

        [Header("Default Transition (seconds)")]
        [SerializeField] private float defaultAppearDurationSeconds = 0.3f;
        [FormerlySerializedAs("defaultFadeDurationSeconds")]
        [SerializeField] private float defaultDisappearDurationSeconds = 0.3f;
        [SerializeField] private float defaultMoveDurationSeconds = 0.3f;

        [Header("Bindings")]
        [SerializeField] private List<ActorExpressionTextureBinding> bindings = new List<ActorExpressionTextureBinding>();

        [Header("Debug")]
        [SerializeField] private bool logCharacter = true;

        private VisualElement characterLayerElement;
        private VisualElement characterLeftElement;
        private VisualElement characterCenterElement;
        private VisualElement characterRightElement;
        private VisualElement actorLayerElement;
        private readonly Dictionary<string, VisualElement> actorElements = new Dictionary<string, VisualElement>(StringComparer.Ordinal);
        // ===== 變更開始 =====
        // 2026/02/01 Opsidanos (修改原因：倒退/讀檔時要套用相反的 char 層級規則，需要一個可用來插隊的 anchor，並記住目前輸出來源)
        // 預期結果：Normal 維持現有置頂；Restore 會用 anchor 讓先做的動作在上面（含 ForceComplete）
        private VisualElement rollbackLayerAnchorElement;
        private StoryOutputSource activeOutputSource = StoryOutputSource.Normal;
        // ===== 變更結束 =====

        private SlotState currentLeft;
        private SlotState currentCenter;
        private SlotState currentRight;

        private Coroutine playRoutine;
        private int playVersion;
        // ===== 變更開始 =====
        // 2026/01/25 Opsidanos (修改原因：支援快速連點時「強制刷新到終點」，需要記住目前這句的目標狀態並能停止 UI Toolkit 動畫)
        // 預期結果：點擊推進時若角色動畫尚未結束，可立即套用終點狀態並停止所有動畫，避免下一句疊加造成錯亂
        private SlotState pendingTargetLeft;
        private SlotState pendingTargetCenter;
        private SlotState pendingTargetRight;
        private bool hasPendingTarget;
        private readonly List<IValueAnimation> activeAnimations = new List<IValueAnimation>();
        // ===== 變更結束 =====

        // ===== 變更開始 =====
        // 2026/01/25 Opsidanos (修改原因：讓 Presenter 能判斷角色演出是否還在跑)
        // 預期結果：當 coroutine 或動畫仍在跑時 IsBusy=true，Presenter 會先 ForceComplete 再允許推進
        public bool IsBusy
        {
            get
            {
                if (playRoutine != null)
                {
                    return true;
                }

                for (int i = 0; i < activeAnimations.Count; i++)
                {
                    IValueAnimation animation = activeAnimations[i];
                    if (animation != null && animation.isRunning)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void ForceComplete()
        {
            ForceCompleteToEnd();
        }
        // ===== 變更結束 =====

        private void Awake()
        {
            if (tagEventRouter == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagCharacterStatePlayer 尚未指定 Tag Event Router。", this);
                enabled = false;
                return;
            }

            if (uiDocument == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagCharacterStatePlayer 尚未指定 UIDocument。", this);
                enabled = false;
                return;
            }
        }

        private void OnEnable()
        {
            if (tagEventRouter == null)
            {
                return;
            }

            tagEventRouter.CharacterTagReceived += OnCharacterTagReceived;
        }

        private void OnDisable()
        {
            if (tagEventRouter == null)
            {
                return;
            }

            tagEventRouter.CharacterTagReceived -= OnCharacterTagReceived;
        }

        private void OnCharacterTagReceived(StoryOutput output, InkTag tag)
        {
            if (tag == null || !tag.HasValue || string.IsNullOrWhiteSpace(tag.Value))
            {
                Debug.LogError("[OpsidanosInk] InkTagCharacterStatePlayer 收到不合法的 char tag（需要 JSON 值）。", this);
                return;
            }

            // ===== 變更開始 =====
            // 2026/02/01 Opsidanos (修改原因：記錄目前輸出來源，讓 ForceComplete 也能套用相同的層級規則)
            // 預期結果：Normal/Restore 都能一致套用（不會只改到 coroutine 那條路）
            activeOutputSource = output != null ? output.Source : StoryOutputSource.Normal;
            // ===== 變更結束 =====

            // ===== 變更開始 =====
            // 2026/01/25 Opsidanos (修改原因：若上一句的角色演出還沒結束，先強制刷新到終點再開始新一句)
            // 預期結果：快速推進時不會讓兩句的角色動畫疊在一起，狀態保持一致
            if (IsBusy)
            {
                ForceCompleteToEnd();
            }
            // ===== 變更結束 =====

            // ===== 變更開始 =====
            // 2026/01/25 Opsidanos (修改原因：steps 新增 raise/raiseActors，需要改用 TransitionStep 並在播放時套用)
            // 預期結果：每個 steps 可用 raise=false 關閉自動置頂；也可用 raiseActors 在無動作時單純調整層級
            if (!TryParseCharTag(output, tag.Value, out SlotState left, out SlotState center, out SlotState right, out float appearDuration, out float moveDuration, out float disappearDuration, out List<TransitionStep> steps))
            {
                return;
            }
            // ===== 變更結束 =====

            playVersion++;

            // ===== 變更開始 =====
            // 2026/01/25 Opsidanos (修改原因：記錄目前這句的目標狀態，讓 ForceComplete 可以把演出刷新到終點)
            // 預期結果：Presenter 觸發 ForceComplete 時，角色會直接到最後位置與最後表情（並移除應消失者）
            pendingTargetLeft = left;
            pendingTargetCenter = center;
            pendingTargetRight = right;
            hasPendingTarget = true;
            StopActiveAnimations();
            // ===== 變更結束 =====

            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }

            playRoutine = StartCoroutine(PlayCharTransition(output, left, center, right, appearDuration, moveDuration, disappearDuration, steps, playVersion));
        }

        private IEnumerator PlayCharTransition(
            StoryOutput output,
            SlotState targetLeft,
            SlotState targetCenter,
            SlotState targetRight,
            float appearDuration,
            float moveDuration,
            float disappearDuration,
            List<TransitionStep> steps,
            int version)
        {
            VisualElement layer = GetCharacterLayerElement();
            if (layer == null)
            {
                EndTransitionIfCurrent(version);
                yield break;
            }

            VisualElement leftElement = GetCharacterLeftElement();
            VisualElement centerElement = GetCharacterCenterElement();
            VisualElement rightElement = GetCharacterRightElement();

            if (leftElement == null || centerElement == null || rightElement == null)
            {
                EndTransitionIfCurrent(version);
                yield break;
            }

            EnsureActorLayer(layer);
            // ===== 變更開始 =====
            // 2026/02/01 Opsidanos (修改原因：倒退/讀檔時需要套用相反的 char 層級規則，先在整個 transition 內記住模式)
            // 預期結果：isRestore=true 時，動作中置頂與結束重排都會走相反規則
            bool isRestore = output != null && output.Source == StoryOutputSource.Restore;
            // ===== 變更結束 =====
            ClearSlotVisual(leftElement);
            ClearSlotVisual(centerElement);
            ClearSlotVisual(rightElement);

            yield return null;
            if (version != playVersion)
            {
                yield break;
            }

            Rect leftRect = GetSlotRect(leftElement);
            Rect centerRect = GetSlotRect(centerElement);
            Rect rightRect = GetSlotRect(rightElement);

            var currentByActor = new Dictionary<string, Slot>(StringComparer.Ordinal);
            var currentStateByActor = new Dictionary<string, SlotState>(StringComparer.Ordinal);
            var targetByActor = new Dictionary<string, Slot>(StringComparer.Ordinal);
            var targetStateByActor = new Dictionary<string, SlotState>(StringComparer.Ordinal);

            BuildByActor(currentByActor, currentLeft, Slot.Left);
            BuildByActor(currentByActor, currentCenter, Slot.Center);
            BuildByActor(currentByActor, currentRight, Slot.Right);

            BuildStateByActor(currentStateByActor, currentLeft);
            BuildStateByActor(currentStateByActor, currentCenter);
            BuildStateByActor(currentStateByActor, currentRight);

            BuildByActor(targetByActor, targetLeft, Slot.Left);
            BuildByActor(targetByActor, targetCenter, Slot.Center);
            BuildByActor(targetByActor, targetRight, Slot.Right);

            BuildStateByActor(targetStateByActor, targetLeft);
            BuildStateByActor(targetStateByActor, targetCenter);
            BuildStateByActor(targetStateByActor, targetRight);

            EnsureActorElementsForCurrentState(currentByActor, currentStateByActor, leftRect, centerRect, rightRect);
            // ===== 變更開始 =====
            // 2026/02/01 Opsidanos (修改原因：倒退/讀檔時，固定重排也要相反，避免一開始就用錯順序)
            // 預期結果：Restore 時以相反順序重排（右→中→左）
            SyncActorElementsToSlotOrder(currentLeft, currentCenter, currentRight, isRestore);
            // ===== 變更結束 =====

            var appearActors = new List<string>();
            var stayActors = new List<string>();
            var moveActors = new List<string>();

            foreach (KeyValuePair<string, Slot> pair in targetByActor)
            {
                string actor = pair.Key;
                Slot toSlot = pair.Value;

                if (currentByActor.TryGetValue(actor, out Slot fromSlot))
                {
                    if (fromSlot == toSlot)
                    {
                        stayActors.Add(actor);
                    }
                    else
                    {
                        moveActors.Add(actor);
                    }

                    continue;
                }

                appearActors.Add(actor);
            }

            var disappearActors = new List<string>();
            foreach (KeyValuePair<string, Slot> pair in currentByActor)
            {
                if (targetByActor.ContainsKey(pair.Key))
                {
                    continue;
                }

                disappearActors.Add(pair.Key);
            }

            // 套用「目標表情」：讓移動中的角色用新的表情移動
            for (int i = 0; i < stayActors.Count; i++)
            {
                ApplyTargetTexture(stayActors[i], targetStateByActor);
            }

            for (int i = 0; i < moveActors.Count; i++)
            {
                ApplyTargetTexture(moveActors[i], targetStateByActor);
            }

            TransitionActions requiredActions = TransitionActions.None;
            if (appearActors.Count > 0)
            {
                requiredActions |= TransitionActions.Appear;
            }

            if (moveActors.Count > 0)
            {
                requiredActions |= TransitionActions.Move;
            }

            if (disappearActors.Count > 0)
            {
                requiredActions |= TransitionActions.Disappear;
            }

            // ===== 變更開始 =====
            // 2026/01/25 Opsidanos (修改原因：steps 新增 raise/raiseActors，需要在排程與播放階段套用「不置頂」與「只改層級」)
            // 預期結果：raise=false 時不自動置頂；raiseActors 可在無動作時調整層級，且層級只在角色層內變動
            List<TransitionStep> schedule = BuildTransitionSchedule(output, steps, requiredActions);
            if (schedule.Count == 0)
            {
                Debug.LogError($"[OpsidanosInk] char.transition.steps 排程為空（沒有任何可執行動作）。OutputId={output.OutputId}", this);
            }

            // ===== 變更開始 =====
            // 2026/02/01 Opsidanos (修改原因：倒退/讀檔（Restore）時，動作中置頂規則要相反：先做的在上面)
            // 預期結果：Restore 時用 anchor 插隊；Normal 維持原本 BringToFront()
            HashSet<string> rollbackRaisedActors = isRestore ? new HashSet<string>(StringComparer.Ordinal) : null;
            if (isRestore)
            {
                EnsureRollbackLayerAnchor();
                rollbackLayerAnchorElement.BringToFront();
            }
            // ===== 變更結束 =====

            for (int stepIndex = 0; stepIndex < schedule.Count; stepIndex++)
            {
                TransitionStep step = schedule[stepIndex];
                TransitionActions stepActions = step.Actions;

                bool doAppear = (stepActions & TransitionActions.Appear) != 0;
                bool doMove = (stepActions & TransitionActions.Move) != 0;
                bool doDisappear = (stepActions & TransitionActions.Disappear) != 0;

                // 先把需要 appear 的角色建立出來（這樣才能一起置頂與一起做動畫）
                if (doAppear && appearActors.Count > 0)
                {
                    for (int i = 0; i < appearActors.Count; i++)
                    {
                        string actor = appearActors[i];
                        if (actorElements.ContainsKey(actor))
                        {
                            continue;
                        }

                        if (!targetByActor.TryGetValue(actor, out Slot toSlot))
                        {
                            Debug.LogError($"[OpsidanosInk] InkTagCharacterStatePlayer 找不到 actor=\"{actor}\" 的目標槽位（無法 appear）。", this);
                            continue;
                        }

                        if (!targetStateByActor.TryGetValue(actor, out SlotState state))
                        {
                            Debug.LogError($"[OpsidanosInk] InkTagCharacterStatePlayer 找不到 actor=\"{actor}\" 的目標狀態（無法 appear）。", this);
                            continue;
                        }

                        Texture2D texture = ResolveTexture(actor, state.Expression);
                        VisualElement actorElement = CreateActorElement(actor, texture);
                        SetRect(actorElement, GetRectForSlot(toSlot, leftRect, centerRect, rightRect));
                        actorElement.style.opacity = appearDuration > 0f ? 0f : 1f;
                        actorLayerElement.Add(actorElement);
                        actorElements[actor] = actorElement;
                    }
                }

                // 這一步有動作的人，暫時置頂（不會影響對話框/名字框，因為只在角色層內排序）
                var actorsToRaise = new List<string>();
                if (step.RaiseActors != null && step.RaiseActors.Length > 0)
                {
                    for (int i = 0; i < step.RaiseActors.Length; i++)
                    {
                        string actor = step.RaiseActors[i];
                        if (string.IsNullOrEmpty(actor))
                        {
                            continue;
                        }

                        if (!actorElements.ContainsKey(actor))
                        {
                            Debug.LogError(
                                $"[OpsidanosInk] char.transition.steps[{stepIndex}].raiseActors 指定 actor=\"{actor}\"，但此時畫面上不存在（也不在本步 appear）。OutputId={output.OutputId}",
                                this);
                            continue;
                        }

                        if (!actorsToRaise.Contains(actor))
                        {
                            actorsToRaise.Add(actor);
                        }
                    }
                }

                if (step.Raise)
                {
                    AddActorsToList(actorsToRaise, doAppear ? appearActors : null);
                    AddActorsToList(actorsToRaise, doMove ? moveActors : null);
                    AddActorsToList(actorsToRaise, doDisappear ? disappearActors : null);
                }
                // ===== 變更開始 =====
                // 2026/02/01 Opsidanos (修改原因：倒退/讀檔要讓「先做的」留在上面，所以不能每一步都 BringToFront)
                // 預期結果：Normal：後做的在上；Restore：先做的在上（同一個角色不會被後續步驟往下拖）
                if (isRestore)
                {
                    PlaceActorsInFrontOfRollbackAnchorPreserveOrder(actorsToRaise, rollbackRaisedActors);
                }
                else
                {
                    BringActorsToFrontPreserveOrder(actorsToRaise);
                }
                // ===== 變更結束 =====

                // 同步開始：appear / move / disappear（可以同時發生）
                if (doAppear && appearActors.Count > 0)
                {
                    for (int i = 0; i < appearActors.Count; i++)
                    {
                        string actor = appearActors[i];
                        if (!actorElements.TryGetValue(actor, out VisualElement actorElement))
                        {
                            continue;
                        }

                        if (appearDuration > 0f)
                        {
                            AnimateOpacity(actorElement, 1f, appearDuration);
                        }
                        else
                        {
                            actorElement.style.opacity = 1f;
                        }
                    }
                }

                if (doMove && moveActors.Count > 0)
                {
                    for (int i = 0; i < moveActors.Count; i++)
                    {
                        string actor = moveActors[i];
                        if (!targetByActor.TryGetValue(actor, out Slot toSlot))
                        {
                            continue;
                        }

                        if (!actorElements.TryGetValue(actor, out VisualElement actorElement))
                        {
                            Debug.LogError($"[OpsidanosInk] InkTagCharacterStatePlayer 找不到 actor=\"{actor}\" 的 VisualElement（無法移動）。", this);
                            continue;
                        }

                        Rect toRect = GetRectForSlot(toSlot, leftRect, centerRect, rightRect);

                        if (moveDuration > 0f)
                        {
                            AnimateRect(actorElement, toRect, moveDuration);
                        }
                        else
                        {
                            SetRect(actorElement, toRect);
                        }
                    }
                }

                if (doDisappear && disappearActors.Count > 0)
                {
                    for (int i = 0; i < disappearActors.Count; i++)
                    {
                        string actor = disappearActors[i];
                        if (!actorElements.TryGetValue(actor, out VisualElement actorElement))
                        {
                            continue;
                        }

                        if (disappearDuration > 0f)
                        {
                            AnimateOpacity(actorElement, 0f, disappearDuration);
                        }
                        else
                        {
                            actorElement.style.opacity = 0f;
                        }
                    }
                }

                float stepWait = 0f;
                if (doAppear && appearActors.Count > 0)
                {
                    stepWait = Mathf.Max(stepWait, appearDuration);
                }

                if (doMove && moveActors.Count > 0)
                {
                    stepWait = Mathf.Max(stepWait, moveDuration);
                }

                if (doDisappear && disappearActors.Count > 0)
                {
                    stepWait = Mathf.Max(stepWait, disappearDuration);
                }

                if (stepWait > 0f)
                {
                    yield return new WaitForSeconds(stepWait);
                }

                if (version != playVersion)
                {
                    yield break;
                }

                // 補齊最終狀態（避免動畫結束後停在半路）
                if (doMove && moveActors.Count > 0)
                {
                    for (int i = 0; i < moveActors.Count; i++)
                    {
                        string actor = moveActors[i];
                        if (!targetByActor.TryGetValue(actor, out Slot toSlot))
                        {
                            continue;
                        }

                        if (!actorElements.TryGetValue(actor, out VisualElement actorElement))
                        {
                            continue;
                        }

                        Rect toRect = GetRectForSlot(toSlot, leftRect, centerRect, rightRect);
                        SetRect(actorElement, toRect);
                    }
                }

                if (doAppear && appearActors.Count > 0)
                {
                    for (int i = 0; i < appearActors.Count; i++)
                    {
                        string actor = appearActors[i];
                        if (!actorElements.TryGetValue(actor, out VisualElement actorElement))
                        {
                            continue;
                        }

                        actorElement.style.opacity = 1f;
                    }
                }

                if (doDisappear && disappearActors.Count > 0)
                {
                    for (int i = 0; i < disappearActors.Count; i++)
                    {
                        string actor = disappearActors[i];
                        if (!actorElements.TryGetValue(actor, out VisualElement actorElement))
                        {
                            continue;
                        }

                        actorElement.RemoveFromHierarchy();
                        actorElements.Remove(actor);
                        // ===== 變更開始 =====
                        // 2026/02/01 Opsidanos (修改原因：倒退模式下，如果角色已消失，後續若又出現需要允許再次插隊)
                        // 預期結果：角色消失後，rollbackRaisedActors 不會卡住該 actor 的後續 raise
                        if (rollbackRaisedActors != null)
                        {
                            rollbackRaisedActors.Remove(actor);
                        }
                        // ===== 變更結束 =====
                    }
                }
            }
            // ===== 變更結束 =====

            // 整串 steps 結束後，重置角色層級（回到基準 1，不影響下一串）
            // ===== 變更開始 =====
            // 2026/02/01 Opsidanos (修改原因：倒退/讀檔時，結束後固定重排要相反)
            // 預期結果：Normal：左→中→右；Restore：右→中→左
            SyncActorElementsToSlotOrder(targetLeft, targetCenter, targetRight, isRestore);
            // ===== 變更結束 =====

            currentLeft = targetLeft;
            currentCenter = targetCenter;
            currentRight = targetRight;

            if (logCharacter)
            {
                Debug.Log(
                    $"[OpsidanosInk][CharState] OutputId={output.OutputId} L={ToDebugState(targetLeft)} C={ToDebugState(targetCenter)} R={ToDebugState(targetRight)} appear={appearDuration:0.###} move={moveDuration:0.###} disappear={disappearDuration:0.###}",
                    this);
            }

            EndTransitionIfCurrent(version);
        }

        // ===== 變更開始 =====
        // 2026/01/25 Opsidanos (修改原因：支援快速連點：把角色演出強制刷新到終點，並停止所有動畫)
        // 預期結果：上一句的角色會立刻到終點狀態（位置/透明度/是否移除/表情），下一句不會再看到錯亂或重複 appear
        private void ForceCompleteToEnd()
        {
            if (!IsBusy)
            {
                return;
            }

            if (!hasPendingTarget)
            {
                Debug.LogError("[OpsidanosInk] InkTagCharacterStatePlayer 被要求 ForceComplete，但找不到 pending 目標狀態。", this);
            }

            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }

            StopActiveAnimations();

            ApplyFinalState(hasPendingTarget ? pendingTargetLeft : currentLeft, hasPendingTarget ? pendingTargetCenter : currentCenter, hasPendingTarget ? pendingTargetRight : currentRight);

            currentLeft = hasPendingTarget ? pendingTargetLeft : currentLeft;
            currentCenter = hasPendingTarget ? pendingTargetCenter : currentCenter;
            currentRight = hasPendingTarget ? pendingTargetRight : currentRight;

            hasPendingTarget = false;
        }

        private void ApplyFinalState(SlotState targetLeft, SlotState targetCenter, SlotState targetRight)
        {
            VisualElement layer = GetCharacterLayerElement();
            if (layer == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagCharacterStatePlayer 找不到角色層，無法 ForceComplete。", this);
                return;
            }

            VisualElement leftElement = GetCharacterLeftElement();
            VisualElement centerElement = GetCharacterCenterElement();
            VisualElement rightElement = GetCharacterRightElement();

            if (leftElement == null || centerElement == null || rightElement == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagCharacterStatePlayer 找不到角色槽位，無法 ForceComplete。", this);
                return;
            }

            EnsureActorLayer(layer);
            ClearSlotVisual(leftElement);
            ClearSlotVisual(centerElement);
            ClearSlotVisual(rightElement);

            Rect leftRect = GetSlotRect(leftElement);
            Rect centerRect = GetSlotRect(centerElement);
            Rect rightRect = GetSlotRect(rightElement);

            var currentByActor = new Dictionary<string, Slot>(StringComparer.Ordinal);
            var currentStateByActor = new Dictionary<string, SlotState>(StringComparer.Ordinal);
            var targetByActor = new Dictionary<string, Slot>(StringComparer.Ordinal);
            var targetStateByActor = new Dictionary<string, SlotState>(StringComparer.Ordinal);

            BuildByActor(currentByActor, currentLeft, Slot.Left);
            BuildByActor(currentByActor, currentCenter, Slot.Center);
            BuildByActor(currentByActor, currentRight, Slot.Right);

            BuildStateByActor(currentStateByActor, currentLeft);
            BuildStateByActor(currentStateByActor, currentCenter);
            BuildStateByActor(currentStateByActor, currentRight);

            BuildByActor(targetByActor, targetLeft, Slot.Left);
            BuildByActor(targetByActor, targetCenter, Slot.Center);
            BuildByActor(targetByActor, targetRight, Slot.Right);

            BuildStateByActor(targetStateByActor, targetLeft);
            BuildStateByActor(targetStateByActor, targetCenter);
            BuildStateByActor(targetStateByActor, targetRight);

            EnsureActorElementsForCurrentState(currentByActor, currentStateByActor, leftRect, centerRect, rightRect);

            var appearActors = new List<string>();
            var stayActors = new List<string>();
            var moveActors = new List<string>();

            foreach (KeyValuePair<string, Slot> pair in targetByActor)
            {
                string actor = pair.Key;
                Slot toSlot = pair.Value;

                if (currentByActor.TryGetValue(actor, out Slot fromSlot))
                {
                    if (fromSlot == toSlot)
                    {
                        stayActors.Add(actor);
                    }
                    else
                    {
                        moveActors.Add(actor);
                    }

                    continue;
                }

                appearActors.Add(actor);
            }

            var disappearActors = new List<string>();
            foreach (KeyValuePair<string, Slot> pair in currentByActor)
            {
                if (targetByActor.ContainsKey(pair.Key))
                {
                    continue;
                }

                disappearActors.Add(pair.Key);
            }

            for (int i = 0; i < stayActors.Count; i++)
            {
                ApplyTargetTexture(stayActors[i], targetStateByActor);
            }

            for (int i = 0; i < moveActors.Count; i++)
            {
                ApplyTargetTexture(moveActors[i], targetStateByActor);
            }

            for (int i = 0; i < appearActors.Count; i++)
            {
                string actor = appearActors[i];
                if (!targetByActor.TryGetValue(actor, out Slot toSlot))
                {
                    continue;
                }

                if (!targetStateByActor.TryGetValue(actor, out SlotState state))
                {
                    continue;
                }

                Texture2D texture = ResolveTexture(actor, state.Expression);

                if (!actorElements.TryGetValue(actor, out VisualElement actorElement))
                {
                    actorElement = CreateActorElement(actor, texture);
                    actorLayerElement.Add(actorElement);
                    actorElements[actor] = actorElement;
                }
                else
                {
                    actorElement.style.backgroundImage = new StyleBackground(texture);
                }

                SetRect(actorElement, GetRectForSlot(toSlot, leftRect, centerRect, rightRect));
                actorElement.style.opacity = 1f;
            }

            for (int i = 0; i < moveActors.Count; i++)
            {
                string actor = moveActors[i];
                if (!targetByActor.TryGetValue(actor, out Slot toSlot))
                {
                    continue;
                }

                if (!actorElements.TryGetValue(actor, out VisualElement actorElement))
                {
                    continue;
                }

                Rect toRect = GetRectForSlot(toSlot, leftRect, centerRect, rightRect);
                SetRect(actorElement, toRect);
                actorElement.style.opacity = 1f;
            }

            for (int i = 0; i < disappearActors.Count; i++)
            {
                string actor = disappearActors[i];
                if (!actorElements.TryGetValue(actor, out VisualElement actorElement))
                {
                    continue;
                }

                actorElement.style.opacity = 0f;
                actorElement.RemoveFromHierarchy();
                actorElements.Remove(actor);
            }

            // ===== 變更開始 =====
            // 2026/02/01 Opsidanos (修改原因：ForceComplete 也要套用倒退/讀檔的相反重排規則，避免點擊後順序跳回正常)
            // 預期結果：Restore 時：右→中→左；Normal 時：左→中→右
            bool reverseSlotOrder = activeOutputSource == StoryOutputSource.Restore;
            SyncActorElementsToSlotOrder(targetLeft, targetCenter, targetRight, reverseSlotOrder);
            // ===== 變更結束 =====
        }

        private void StopActiveAnimations()
        {
            for (int i = 0; i < activeAnimations.Count; i++)
            {
                IValueAnimation animation = activeAnimations[i];
                if (animation == null)
                {
                    continue;
                }

                if (animation.isRunning)
                {
                    animation.Stop();
                }
            }

            activeAnimations.Clear();
        }

        private void EndTransitionIfCurrent(int version)
        {
            if (version != playVersion)
            {
                return;
            }

            playRoutine = null;
            hasPendingTarget = false;
            StopActiveAnimations();
        }
        // ===== 變更結束 =====
        private bool TryParseCharTag(
            StoryOutput output,
            string json,
            out SlotState left,
            out SlotState center,
            out SlotState right,
            out float appearDuration,
            out float moveDuration,
            out float disappearDuration,
            out List<TransitionStep> steps)
        {
            left = default;
            center = default;
            right = default;
            appearDuration = defaultAppearDurationSeconds;
            moveDuration = defaultMoveDurationSeconds;
            disappearDuration = defaultDisappearDurationSeconds;
            steps = null;

            string trimmed = json.Trim();
            if (string.Equals(trimmed, "clear", StringComparison.Ordinal))
            {
                return true;
            }

            if (!trimmed.StartsWith("{", StringComparison.Ordinal))
            {
                Debug.LogError($"[OpsidanosInk] char tag 值必須是 JSON 物件（以 {{ 開頭）。OutputId={output.OutputId} Value=\"{trimmed}\"", this);
                return false;
            }

            CharTagPayload payload;
            try
            {
                payload = JsonUtility.FromJson<CharTagPayload>(trimmed);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OpsidanosInk] 解析 char JSON 失敗。OutputId={output.OutputId} Value=\"{trimmed}\" Error={ex.Message}", this);
                return false;
            }

            if (payload == null)
            {
                Debug.LogError($"[OpsidanosInk] 解析 char JSON 失敗（payload 為 null）。OutputId={output.OutputId} Value=\"{trimmed}\"", this);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(payload.mode) && string.Equals(payload.mode.Trim(), "patch", StringComparison.Ordinal))
            {
                Debug.LogError($"[OpsidanosInk] 目前尚未支援 char.mode=\"patch\"，請先用預設 replace。OutputId={output.OutputId}", this);
            }

            if (payload.transition != null)
            {
                if (payload.transition.appear >= 0f)
                {
                    appearDuration = payload.transition.appear;
                }

                if (payload.transition.disappear >= 0f)
                {
                    disappearDuration = payload.transition.disappear;
                }
                else if (payload.transition.fade >= 0f)
                {
                    disappearDuration = payload.transition.fade;
                }

                if (payload.transition.fade >= 0f)
                {
                    Debug.LogError($"[OpsidanosInk] char.transition.fade 已改名為 disappear，請改用 char.transition.disappear。OutputId={output.OutputId}", this);
                }

                if (payload.transition.move >= 0f)
                {
                    moveDuration = payload.transition.move;
                }

                if (payload.transition.instant)
                {
                    moveDuration = 0f;
                }

                if (payload.transition.steps != null && payload.transition.steps.Length > 0)
                {
                    steps = ParseSteps(output, payload.transition.steps);
                }
            }

            if (appearDuration < 0f)
            {
                Debug.LogError($"[OpsidanosInk] char.transition.appear 不可小於 0。OutputId={output.OutputId} appear={appearDuration}", this);
                return false;
            }

            if (moveDuration < 0f)
            {
                Debug.LogError($"[OpsidanosInk] char.transition.move 不可小於 0。OutputId={output.OutputId} move={moveDuration}", this);
                return false;
            }

            if (disappearDuration < 0f)
            {
                Debug.LogError($"[OpsidanosInk] char.transition.disappear 不可小於 0。OutputId={output.OutputId} disappear={disappearDuration}", this);
                return false;
            }

            left = ToSlotState(payload.left);
            center = ToSlotState(payload.center);
            right = ToSlotState(payload.right);

            EnsureNoDuplicateActor(output, left, Slot.Left, center, Slot.Center, right, Slot.Right);
            return true;
        }

        private List<TransitionStep> ParseSteps(StoryOutput output, TransitionStepPayload[] steps)
        {
            var parsed = new List<TransitionStep>();

            for (int i = 0; i < steps.Length; i++)
            {
                TransitionStepPayload step = steps[i];
                if (step == null)
                {
                    Debug.LogError($"[OpsidanosInk] char.transition.steps[{i}] 是 null。OutputId={output.OutputId}", this);
                    continue;
                }

                TransitionActions mask = TransitionActions.None;
                if (step.actions != null)
                {
                    for (int j = 0; j < step.actions.Length; j++)
                    {
                        string action = step.actions[j];
                        if (string.IsNullOrWhiteSpace(action))
                        {
                            continue;
                        }

                        string trimmed = action.Trim();

                        if (string.Equals(trimmed, "appear", StringComparison.OrdinalIgnoreCase))
                        {
                            mask |= TransitionActions.Appear;
                            continue;
                        }

                        if (string.Equals(trimmed, "move", StringComparison.OrdinalIgnoreCase))
                        {
                            mask |= TransitionActions.Move;
                            continue;
                        }

                        if (string.Equals(trimmed, "disappear", StringComparison.OrdinalIgnoreCase))
                        {
                            mask |= TransitionActions.Disappear;
                            continue;
                        }

                        if (string.Equals(trimmed, "fade", StringComparison.OrdinalIgnoreCase))
                        {
                            Debug.LogError($"[OpsidanosInk] char.transition.steps 的 action=\"fade\" 已改名為 \"disappear\"。OutputId={output.OutputId}", this);
                            mask |= TransitionActions.Disappear;
                            continue;
                        }

                        Debug.LogError($"[OpsidanosInk] char.transition.steps[{i}] 出現不支援的 action=\"{trimmed}\"（只支援 appear/move/disappear）。OutputId={output.OutputId}", this);
                    }
                }

                string[] raiseActors = ParseRaiseActors(output, i, step.raiseActors);
                if (mask == TransitionActions.None && (raiseActors == null || raiseActors.Length == 0))
                {
                    Debug.LogError($"[OpsidanosInk] char.transition.steps[{i}] 同時缺少 actions 與 raiseActors（至少要有一個）。OutputId={output.OutputId}", this);
                    continue;
                }

                parsed.Add(new TransitionStep
                {
                    Actions = mask,
                    Raise = step.raise,
                    RaiseActors = raiseActors
                });
            }

            return parsed;
        }

        private string[] ParseRaiseActors(StoryOutput output, int stepIndex, string[] raiseActors)
        {
            if (raiseActors == null || raiseActors.Length == 0)
            {
                return null;
            }

            var list = new List<string>();

            for (int i = 0; i < raiseActors.Length; i++)
            {
                string actor = raiseActors[i];
                if (string.IsNullOrWhiteSpace(actor))
                {
                    continue;
                }

                string trimmed = actor.Trim();
                if (list.Contains(trimmed))
                {
                    Debug.LogError($"[OpsidanosInk] char.transition.steps[{stepIndex}].raiseActors 出現重複 actor：\"{trimmed}\"。OutputId={output.OutputId}", this);
                    continue;
                }

                list.Add(trimmed);
            }

            return list.Count > 0 ? list.ToArray() : null;
        }

        private List<TransitionStep> BuildTransitionSchedule(StoryOutput output, List<TransitionStep> customSteps, TransitionActions requiredActions)
        {
            List<TransitionStep> baseSteps;
            if (customSteps != null && customSteps.Count > 0)
            {
                baseSteps = customSteps;
            }
            else
            {
                baseSteps = new List<TransitionStep>
                {
                    new TransitionStep { Actions = TransitionActions.Appear, Raise = true },
                    new TransitionStep { Actions = TransitionActions.Move, Raise = true },
                    new TransitionStep { Actions = TransitionActions.Disappear, Raise = true }
                };
            }

            var schedule = new List<TransitionStep>();
            TransitionActions scheduled = TransitionActions.None;

            for (int i = 0; i < baseSteps.Count; i++)
            {
                TransitionStep step = baseSteps[i];
                TransitionActions duplicated = step.Actions & scheduled;
                if (duplicated != TransitionActions.None)
                {
                    Debug.LogError($"[OpsidanosInk] char.transition.steps 重複安排動作：{duplicated}（每個動作最多出現一次）。OutputId={output.OutputId}", this);
                }

                TransitionActions unique = step.Actions & ~scheduled;
                bool hasRaiseActors = step.RaiseActors != null && step.RaiseActors.Length > 0;
                if (unique == TransitionActions.None && !hasRaiseActors)
                {
                    continue;
                }

                schedule.Add(new TransitionStep
                {
                    Actions = unique,
                    Raise = step.Raise,
                    RaiseActors = step.RaiseActors
                });
                scheduled |= unique;
            }

            TransitionActions missing = requiredActions & ~scheduled;
            if (missing != TransitionActions.None)
            {
                Debug.LogError($"[OpsidanosInk] char.transition.steps 缺少必要動作：{missing}（我會補到排程最後面，確保畫面狀態正確）。OutputId={output.OutputId}", this);

                if ((missing & TransitionActions.Appear) != 0)
                {
                    schedule.Add(new TransitionStep { Actions = TransitionActions.Appear, Raise = true });
                }

                if ((missing & TransitionActions.Move) != 0)
                {
                    schedule.Add(new TransitionStep { Actions = TransitionActions.Move, Raise = true });
                }

                if ((missing & TransitionActions.Disappear) != 0)
                {
                    schedule.Add(new TransitionStep { Actions = TransitionActions.Disappear, Raise = true });
                }
            }

            return schedule;
        }

        private static SlotState ToSlotState(SlotPayload payload)
        {
            if (payload == null)
            {
                return default;
            }

            if (string.IsNullOrWhiteSpace(payload.actor))
            {
                return default;
            }

            string actor = payload.actor.Trim();
            string expr = string.IsNullOrWhiteSpace(payload.expr) ? null : payload.expr.Trim();

            return new SlotState { Actor = actor, Expression = expr };
        }

        private void EnsureNoDuplicateActor(
            StoryOutput output,
            SlotState left,
            Slot leftSlot,
            SlotState center,
            Slot centerSlot,
            SlotState right,
            Slot rightSlot)
        {
            if (!left.IsEmpty && !center.IsEmpty && string.Equals(left.Actor, center.Actor, StringComparison.Ordinal))
            {
                Debug.LogError($"[OpsidanosInk] char JSON 同一個 actor 不可同時出現在兩個槽位：\"{left.Actor}\"（{leftSlot} 與 {centerSlot}）。OutputId={output.OutputId}", this);
            }

            if (!left.IsEmpty && !right.IsEmpty && string.Equals(left.Actor, right.Actor, StringComparison.Ordinal))
            {
                Debug.LogError($"[OpsidanosInk] char JSON 同一個 actor 不可同時出現在兩個槽位：\"{left.Actor}\"（{leftSlot} 與 {rightSlot}）。OutputId={output.OutputId}", this);
            }

            if (!center.IsEmpty && !right.IsEmpty && string.Equals(center.Actor, right.Actor, StringComparison.Ordinal))
            {
                Debug.LogError($"[OpsidanosInk] char JSON 同一個 actor 不可同時出現在兩個槽位：\"{center.Actor}\"（{centerSlot} 與 {rightSlot}）。OutputId={output.OutputId}", this);
            }
        }

        private void BuildByActor(Dictionary<string, Slot> dict, SlotState state, Slot slot)
        {
            if (state.IsEmpty)
            {
                return;
            }

            if (dict.ContainsKey(state.Actor))
            {
                Debug.LogError($"[OpsidanosInk] 目前角色狀態出現重複 actor：\"{state.Actor}\"。請確認上一句的狀態是否正確。", this);
                return;
            }

            dict.Add(state.Actor, slot);
        }

        private void BuildStateByActor(Dictionary<string, SlotState> dict, SlotState state)
        {
            if (state.IsEmpty)
            {
                return;
            }

            if (dict.ContainsKey(state.Actor))
            {
                return;
            }

            dict.Add(state.Actor, state);
        }

        private Texture2D ResolveTexture(string actor, string expression)
        {
            // ===== 變更開始 =====
            // 2026/01/25 Opsidanos (修改原因：支援 ResourceMap；若未指定 ResourceMap 則沿用舊 bindings（相容）)
            // 預期結果：有指定 ResourceMap 時以 ResourceMap 為準；沒有時仍可用舊 bindings 驗證
            if (resourceMap != null)
            {
                if (resourceMap.TryGetActorExpressionTexture(actor, expression, out Texture2D texture, out _))
                {
                    return texture;
                }

                return null;
            }
            // ===== 變更結束 =====

            ActorExpressionTextureBinding binding = FindBinding(actor, expression);
            if (binding == null)
            {
                Debug.LogError($"[OpsidanosInk] InkTagCharacterStatePlayer 找不到 actor=\"{actor}\" expr=\"{expression ?? "(default)"}\" 的對照設定。", this);
                return null;
            }

            if (binding.Texture == null)
            {
                Debug.LogError($"[OpsidanosInk] InkTagCharacterStatePlayer 的 actor=\"{actor}\" expr=\"{binding.Expression}\" 尚未指定 Texture2D。", this);
                return null;
            }

            return binding.Texture;
        }

        private ActorExpressionTextureBinding FindBinding(string actor, string expression)
        {
            if (bindings == null)
            {
                return null;
            }

            bool hasExpression = !string.IsNullOrWhiteSpace(expression);
            string expr = hasExpression ? expression.Trim() : null;

            ActorExpressionTextureBinding defaultBinding = null;

            for (int i = 0; i < bindings.Count; i++)
            {
                ActorExpressionTextureBinding binding = bindings[i];
                if (binding == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(binding.Actor))
                {
                    continue;
                }

                if (!string.Equals(binding.Actor.Trim(), actor, StringComparison.Ordinal))
                {
                    continue;
                }

                if (binding.IsDefault)
                {
                    defaultBinding = binding;
                }

                if (!hasExpression)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(binding.Expression))
                {
                    continue;
                }

                if (!string.Equals(binding.Expression.Trim(), expr, StringComparison.Ordinal))
                {
                    continue;
                }

                return binding;
            }

            return defaultBinding;
        }

        private void EnsureActorLayer(VisualElement layer)
        {
            if (actorLayerElement != null)
            {
                return;
            }

            actorLayerElement = new VisualElement
            {
                name = "CharacterActorLayer",
                pickingMode = PickingMode.Ignore
            };

            actorLayerElement.AddToClassList("character-actors-layer");
            layer.Add(actorLayerElement);
        }

        private static void AddActorsToList(List<string> target, List<string> actors)
        {
            if (actors == null || actors.Count == 0)
            {
                return;
            }

            for (int i = 0; i < actors.Count; i++)
            {
                string actor = actors[i];
                if (string.IsNullOrEmpty(actor))
                {
                    continue;
                }

                if (target.Contains(actor))
                {
                    continue;
                }

                target.Add(actor);
            }
        }

        // ===== 變更開始 =====
        // 2026/02/01 Opsidanos (修改原因：倒退/讀檔時需要一個固定存在的 anchor，才能把「後做的」插到更下面)
        // 預期結果：Restore 模式可用 PlaceInFront(anchor) 達成「先做的在上面」
        private void EnsureRollbackLayerAnchor()
        {
            if (actorLayerElement == null)
            {
                return;
            }

            if (rollbackLayerAnchorElement != null && rollbackLayerAnchorElement.parent == actorLayerElement)
            {
                return;
            }

            rollbackLayerAnchorElement = new VisualElement
            {
                name = "CharacterRollbackLayerAnchor",
                pickingMode = PickingMode.Ignore
            };

            rollbackLayerAnchorElement.style.display = DisplayStyle.None;
            actorLayerElement.Add(rollbackLayerAnchorElement);
        }

        private void PlaceActorsInFrontOfRollbackAnchorPreserveOrder(List<string> actors, HashSet<string> alreadyRaised)
        {
            if (actors == null || actors.Count == 0)
            {
                return;
            }

            if (actorLayerElement == null || rollbackLayerAnchorElement == null)
            {
                return;
            }

            var indexed = new List<(string Actor, int Index)>();

            for (int i = 0; i < actors.Count; i++)
            {
                string actor = actors[i];
                if (alreadyRaised != null && alreadyRaised.Contains(actor))
                {
                    continue;
                }

                if (!actorElements.TryGetValue(actor, out VisualElement element))
                {
                    continue;
                }

                int index = GetChildIndex(actorLayerElement, element);
                if (index < 0)
                {
                    continue;
                }

                indexed.Add((actor, index));
            }

            indexed.Sort((a, b) => a.Index.CompareTo(b.Index));

            for (int i = indexed.Count - 1; i >= 0; i--)
            {
                string actor = indexed[i].Actor;
                if (!actorElements.TryGetValue(actor, out VisualElement element))
                {
                    continue;
                }

                element.PlaceInFront(rollbackLayerAnchorElement);
                alreadyRaised?.Add(actor);
            }
        }
        // ===== 變更結束 =====

        private void BringActorsToFrontPreserveOrder(List<string> actors)
        {
            if (actors == null || actors.Count == 0)
            {
                return;
            }

            if (actorLayerElement == null)
            {
                return;
            }

            var indexed = new List<(string Actor, int Index)>();

            for (int i = 0; i < actors.Count; i++)
            {
                string actor = actors[i];
                if (!actorElements.TryGetValue(actor, out VisualElement element))
                {
                    continue;
                }

                int index = GetChildIndex(actorLayerElement, element);
                indexed.Add((actor, index));
            }

            indexed.Sort((a, b) => a.Index.CompareTo(b.Index));

            for (int i = 0; i < indexed.Count; i++)
            {
                string actor = indexed[i].Actor;
                if (!actorElements.TryGetValue(actor, out VisualElement element))
                {
                    continue;
                }

                element.BringToFront();
            }
        }

        private static int GetChildIndex(VisualElement parent, VisualElement child)
        {
            if (parent == null || child == null)
            {
                return -1;
            }

            int index = 0;
            foreach (VisualElement element in parent.Children())
            {
                if (element == child)
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        // ===== 變更開始 =====
        // 2026/02/01 Opsidanos (修改原因：倒退/讀檔時，動作結束後的固定重排也要相反)
        // 預期結果：Normal：左→中→右（右邊最上）；Restore：右→中→左（左邊最上）
        private void SyncActorElementsToSlotOrder(SlotState left, SlotState center, SlotState right, bool reverseSlotOrder)
        {
            if (actorLayerElement == null)
            {
                return;
            }

            if (!reverseSlotOrder)
            {
                if (!left.IsEmpty && actorElements.TryGetValue(left.Actor, out VisualElement leftElement))
                {
                    leftElement.BringToFront();
                }

                if (!center.IsEmpty && actorElements.TryGetValue(center.Actor, out VisualElement centerElement))
                {
                    centerElement.BringToFront();
                }

                if (!right.IsEmpty && actorElements.TryGetValue(right.Actor, out VisualElement rightElement))
                {
                    rightElement.BringToFront();
                }

                return;
            }

            if (!right.IsEmpty && actorElements.TryGetValue(right.Actor, out VisualElement rightElementReverse))
            {
                rightElementReverse.BringToFront();
            }

            if (!center.IsEmpty && actorElements.TryGetValue(center.Actor, out VisualElement centerElementReverse))
            {
                centerElementReverse.BringToFront();
            }

            if (!left.IsEmpty && actorElements.TryGetValue(left.Actor, out VisualElement leftElementReverse))
            {
                leftElementReverse.BringToFront();
            }
        }
        // ===== 變更結束 =====

        private static void ClearSlotVisual(VisualElement slotElement)
        {
            slotElement.style.backgroundImage = new StyleBackground((Texture2D)null);
            slotElement.style.opacity = 1f;
        }

        private Rect GetSlotRect(VisualElement slotElement)
        {
            Rect world = slotElement.worldBound;
            Vector2 local = actorLayerElement.WorldToLocal(new Vector2(world.x, world.y));
            return new Rect(local.x, local.y, world.width, world.height);
        }

        private static Rect GetRectForSlot(Slot slot, Rect leftRect, Rect centerRect, Rect rightRect)
        {
            switch (slot)
            {
                case Slot.Left:
                    return leftRect;
                case Slot.Center:
                    return centerRect;
                case Slot.Right:
                    return rightRect;
                default:
                    return default;
            }
        }

        private void EnsureActorElementsForCurrentState(
            Dictionary<string, Slot> currentByActor,
            Dictionary<string, SlotState> currentStateByActor,
            Rect leftRect,
            Rect centerRect,
            Rect rightRect)
        {
            // 先清掉「不在目前狀態」的角色（避免上一次動畫被中斷後殘留）
            var actorsToRemove = new List<string>();
            foreach (KeyValuePair<string, VisualElement> pair in actorElements)
            {
                if (currentByActor.ContainsKey(pair.Key))
                {
                    continue;
                }

                actorsToRemove.Add(pair.Key);
            }

            for (int i = 0; i < actorsToRemove.Count; i++)
            {
                string actor = actorsToRemove[i];
                if (!actorElements.TryGetValue(actor, out VisualElement actorElement))
                {
                    continue;
                }

                actorElement.RemoveFromHierarchy();
                actorElements.Remove(actor);
            }

            foreach (KeyValuePair<string, Slot> pair in currentByActor)
            {
                string actor = pair.Key;
                Slot slot = pair.Value;

                if (!actorElements.TryGetValue(actor, out VisualElement actorElement))
                {
                    if (!currentStateByActor.TryGetValue(actor, out SlotState state))
                    {
                        Debug.LogError($"[OpsidanosInk] InkTagCharacterStatePlayer 找不到 actor=\"{actor}\" 的目前狀態（無法建立角色元素）。", this);
                        continue;
                    }

                    Texture2D texture = ResolveTexture(actor, state.Expression);
                    actorElement = CreateActorElement(actor, texture);
                    actorLayerElement.Add(actorElement);
                    actorElements[actor] = actorElement;
                }
                else
                {
                    if (currentStateByActor.TryGetValue(actor, out SlotState state))
                    {
                        Texture2D texture = ResolveTexture(actor, state.Expression);
                        actorElement.style.backgroundImage = new StyleBackground(texture);
                    }
                    else
                    {
                        Debug.LogError($"[OpsidanosInk] InkTagCharacterStatePlayer 找不到 actor=\"{actor}\" 的目前狀態（無法同步表情）。", this);
                    }
                }

                SetRect(actorElement, GetRectForSlot(slot, leftRect, centerRect, rightRect));
                actorElement.style.opacity = 1f;
            }
        }

        private void ApplyTargetTexture(string actor, Dictionary<string, SlotState> targetStateByActor)
        {
            if (!actorElements.TryGetValue(actor, out VisualElement actorElement))
            {
                Debug.LogError($"[OpsidanosInk] InkTagCharacterStatePlayer 找不到 actor=\"{actor}\" 的 VisualElement（無法套用表情）。", this);
                return;
            }

            if (!targetStateByActor.TryGetValue(actor, out SlotState state))
            {
                Debug.LogError($"[OpsidanosInk] InkTagCharacterStatePlayer 找不到 actor=\"{actor}\" 的目標狀態（無法套用表情）。", this);
                return;
            }

            Texture2D texture = ResolveTexture(actor, state.Expression);
            actorElement.style.backgroundImage = new StyleBackground(texture);
        }

        private static VisualElement CreateActorElement(string actor, Texture2D texture)
        {
            var actorElement = new VisualElement
            {
                name = $"Actor_{actor}",
                pickingMode = PickingMode.Ignore
            };

            actorElement.userData = actor;
            actorElement.AddToClassList("vn-image");
            actorElement.AddToClassList("vn-image-bottom");
            actorElement.AddToClassList("character-actor");
            actorElement.style.backgroundImage = new StyleBackground(texture);
            actorElement.style.opacity = 1f;
            return actorElement;
        }

        private static void SetRect(VisualElement element, Rect rect)
        {
            element.style.left = rect.x;
            element.style.top = rect.y;
            element.style.width = rect.width;
            element.style.height = rect.height;
        }

        private void AnimateOpacity(VisualElement element, float opacity, float durationSeconds)
        {
            if (durationSeconds <= 0f)
            {
                element.style.opacity = opacity;
                return;
            }

            int durationMs = Mathf.RoundToInt(durationSeconds * 1000f);
            IValueAnimation animation = element.experimental.animation.Start(new StyleValues { opacity = opacity }, durationMs);
            if (animation != null)
            {
                activeAnimations.Add(animation);
            }
        }

        private void AnimateRect(VisualElement element, Rect rect, float durationSeconds)
        {
            if (durationSeconds <= 0f)
            {
                SetRect(element, rect);
                return;
            }

            int durationMs = Mathf.RoundToInt(durationSeconds * 1000f);
            IValueAnimation animation = element.experimental.animation.Start(new StyleValues { left = rect.x, top = rect.y, width = rect.width, height = rect.height }, durationMs);
            if (animation != null)
            {
                activeAnimations.Add(animation);
            }
        }

        private static string ToDebugState(SlotState state)
        {
            if (state.IsEmpty)
            {
                return "null";
            }

            string expr = string.IsNullOrWhiteSpace(state.Expression) ? "(default)" : state.Expression;
            return $"{state.Actor}/{expr}";
        }

        private VisualElement GetCharacterLayerElement()
        {
            if (characterLayerElement != null)
            {
                return characterLayerElement;
            }

            characterLayerElement = GetElement(characterLayerElementName, "角色層");
            return characterLayerElement;
        }

        private VisualElement GetCharacterLeftElement()
        {
            if (characterLeftElement != null)
            {
                return characterLeftElement;
            }

            characterLeftElement = GetElement(characterLeftElementName, "左立繪槽位");
            return characterLeftElement;
        }

        private VisualElement GetCharacterCenterElement()
        {
            if (characterCenterElement != null)
            {
                return characterCenterElement;
            }

            characterCenterElement = GetElement(characterCenterElementName, "中立繪槽位");
            return characterCenterElement;
        }

        private VisualElement GetCharacterRightElement()
        {
            if (characterRightElement != null)
            {
                return characterRightElement;
            }

            characterRightElement = GetElement(characterRightElementName, "右立繪槽位");
            return characterRightElement;
        }

        private VisualElement GetElement(string elementName, string readableName)
        {
            VisualElement root = uiDocument != null ? uiDocument.rootVisualElement : null;
            if (root == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagCharacterStatePlayer 找不到 rootVisualElement。", this);
                return null;
            }

            VisualElement element = root.Q<VisualElement>(elementName);
            if (element == null)
            {
                Debug.LogError($"[OpsidanosInk] InkTagCharacterStatePlayer 找不到 name=\"{elementName}\" 的{readableName}元素。", this);
                return null;
            }

            return element;
        }
    }
}
// ===== 變更結束 =====

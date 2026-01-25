// ===== 變更開始 =====
// 2026/01/25 Opsidanos (修改原因：新增 shake Tag 的最小演出元件，讓 # shake 真的產生畫面抖動效果)
// 預期結果：收到 shake tag 時，背景（預設 name="Background"）會在指定秒數內抖動，且不影響對話框/選項的點擊
using System.Collections;
using OpsidanosInk.Runtime.Story;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpsidanosInk.Runtime.Presentation
{
    public sealed class InkTagShakePlayer : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private InkTagEventRouter tagEventRouter;
        [SerializeField] private UIDocument uiDocument;

        [Header("UXML")]
        [SerializeField] private string shakeTargetElementName = "Background";

        [Header("Settings")]
        [SerializeField] private float defaultDurationSeconds = 0.3f;
        [SerializeField] private float defaultStrengthPixels = 16f;
        [SerializeField] private bool useUnscaledTime = true;

        [Header("Debug")]
        [SerializeField] private bool logShake = true;

        private VisualElement shakeTargetElement;
        private Coroutine shakeRoutine;
        private int shakeVersion;
        private bool hasBaseTranslate;
        private StyleTranslate baseTranslate;

        private void Awake()
        {
            if (tagEventRouter == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagShakePlayer 尚未指定 Tag Event Router。", this);
                enabled = false;
                return;
            }

            if (uiDocument == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagShakePlayer 尚未指定 UIDocument。", this);
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

            tagEventRouter.ShakeTagReceived += OnShakeTagReceived;
        }

        private void OnDisable()
        {
            if (tagEventRouter != null)
            {
                tagEventRouter.ShakeTagReceived -= OnShakeTagReceived;
            }

            StopShakeAndReset();
        }

        private void OnShakeTagReceived(StoryOutput output, InkTag tag)
        {
            if (tag == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagShakePlayer 收到 null 的 shake tag。", this);
                return;
            }

            if (tag.HasValue && !string.IsNullOrWhiteSpace(tag.Value))
            {
                Debug.LogError($"[OpsidanosInk] shake tag 目前不支援帶值，請先用純 # shake。Value=\"{tag.Value}\"", this);
                return;
            }

            if (defaultDurationSeconds < 0f)
            {
                Debug.LogError($"[OpsidanosInk] InkTagShakePlayer 的 defaultDurationSeconds 不可小於 0。duration={defaultDurationSeconds}", this);
                return;
            }

            if (defaultStrengthPixels < 0f)
            {
                Debug.LogError($"[OpsidanosInk] InkTagShakePlayer 的 defaultStrengthPixels 不可小於 0。strength={defaultStrengthPixels}", this);
                return;
            }

            VisualElement target = GetShakeTargetElement();
            if (target == null)
            {
                return;
            }

            EnsureBaseTranslate(target);
            ResetTargetTranslate(target);

            shakeVersion++;

            if (shakeRoutine != null)
            {
                StopCoroutine(shakeRoutine);
                shakeRoutine = null;
            }

            shakeRoutine = StartCoroutine(PlayShake(output, target, defaultDurationSeconds, defaultStrengthPixels, shakeVersion));

            if (logShake)
            {
                Debug.Log($"[OpsidanosInk][Shake] OutputId={output.OutputId} target={shakeTargetElementName} duration={defaultDurationSeconds:0.###} strength={defaultStrengthPixels:0.###}", this);
            }
        }

        private IEnumerator PlayShake(StoryOutput output, VisualElement target, float durationSeconds, float strengthPixels, int version)
        {
            float elapsed = 0f;

            while (elapsed < durationSeconds)
            {
                if (version != shakeVersion)
                {
                    yield break;
                }

                float t = durationSeconds > 0f ? Mathf.Clamp01(elapsed / durationSeconds) : 1f;
                float damper = 1f - t;
                Vector2 offset = Random.insideUnitCircle * (strengthPixels * damper);

                target.style.translate = new Translate(
                    new Length(offset.x, LengthUnit.Pixel),
                    new Length(offset.y, LengthUnit.Pixel),
                    0f);

                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }

            ResetTargetTranslate(target);

            if (version == shakeVersion)
            {
                shakeRoutine = null;
            }
        }

        private void StopShakeAndReset()
        {
            if (shakeRoutine != null)
            {
                StopCoroutine(shakeRoutine);
                shakeRoutine = null;
            }

            if (shakeTargetElement != null)
            {
                EnsureBaseTranslate(shakeTargetElement);
                ResetTargetTranslate(shakeTargetElement);
            }
        }

        private void EnsureBaseTranslate(VisualElement target)
        {
            if (hasBaseTranslate)
            {
                return;
            }

            baseTranslate = target.style.translate;
            hasBaseTranslate = true;
        }

        private void ResetTargetTranslate(VisualElement target)
        {
            if (!hasBaseTranslate)
            {
                return;
            }

            target.style.translate = baseTranslate;
        }

        private VisualElement GetShakeTargetElement()
        {
            if (shakeTargetElement != null)
            {
                return shakeTargetElement;
            }

            VisualElement root = uiDocument != null ? uiDocument.rootVisualElement : null;
            if (root == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagShakePlayer 找不到 rootVisualElement。", this);
                return null;
            }

            shakeTargetElement = root.Q<VisualElement>(shakeTargetElementName);
            if (shakeTargetElement == null)
            {
                Debug.LogError($"[OpsidanosInk] InkTagShakePlayer 找不到 name=\"{shakeTargetElementName}\" 的目標元素。", this);
                return null;
            }

            return shakeTargetElement;
        }
    }
}
// ===== 變更結束 =====

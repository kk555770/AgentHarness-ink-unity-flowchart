// ===== 變更開始 =====
// 2026/01/21 Opsidanos (修改原因：建立玩家模式最小 UI Toolkit Presenter)
// 預期結果：收到 StoryOutput 後更新 UI，並能點選「下一句」與選項推進故事；顯示/隱藏只切換 class
using System.Collections.Generic;
using OpsidanosInk.Runtime.Story;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpsidanosInk.Runtime.UI
{
    public sealed class VNPlayerPresenter : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private InkStoryEngine storyEngine;

        private Label speakerLabel;
        private Label bodyLabel;
        private Button continueButton;
        private VisualElement choicesContainer;

        private void OnEnable()
        {
            if (storyEngine != null)
            {
                storyEngine.OutputGenerated += OnStoryOutput;
            }
        }

        private void OnDisable()
        {
            if (storyEngine != null)
            {
                storyEngine.OutputGenerated -= OnStoryOutput;
            }
        }

        private void Start()
        {
            if (uiDocument == null)
            {
                Debug.LogError("[OpsidanosInk] VNPlayerPresenter 尚未指定 Ui Document。", this);
                enabled = false;
                return;
            }

            if (storyEngine == null)
            {
                Debug.LogError("[OpsidanosInk] VNPlayerPresenter 尚未指定 Story Engine。", this);
                enabled = false;
                return;
            }

            VisualElement root = uiDocument.rootVisualElement;
            speakerLabel = root.Q<Label>("SpeakerLabel");
            bodyLabel = root.Q<Label>("BodyLabel");
            continueButton = root.Q<Button>("ContinueButton");
            choicesContainer = root.Q<VisualElement>("ChoicesContainer");

            if (speakerLabel == null || bodyLabel == null || continueButton == null || choicesContainer == null)
            {
                Debug.LogError("[OpsidanosInk] VNPlayer.uxml 缺少必要的元素（SpeakerLabel / BodyLabel / ContinueButton / ChoicesContainer）。", this);
                enabled = false;
                return;
            }

            continueButton.clicked += OnClickContinue;
        }

        private void OnDestroy()
        {
            if (continueButton != null)
            {
                continueButton.clicked -= OnClickContinue;
            }
        }

        private void OnClickContinue()
        {
            storyEngine.Continue();
        }

        private void OnStoryOutput(StoryOutput output)
        {
            if (speakerLabel != null)
            {
                speakerLabel.text = string.IsNullOrWhiteSpace(output.Speaker) ? "" : output.Speaker;
            }

            if (bodyLabel != null)
            {
                bodyLabel.text = output.LineText ?? "";
            }

            if (choicesContainer == null)
            {
                return;
            }

            choicesContainer.Clear();

            if (output.Choices.Count > 0)
            {
                continueButton.AddToClassList("vn-hidden");
                choicesContainer.RemoveFromClassList("vn-hidden");

                for (int i = 0; i < output.Choices.Count; i++)
                {
                    ChoiceOutput choice = output.Choices[i];
                    var button = new Button(() => storyEngine.ChooseChoice(choice.Index))
                    {
                        text = choice.Text
                    };
                    button.AddToClassList("vn-choice-button");
                    choicesContainer.Add(button);
                }
            }
            else
            {
                choicesContainer.AddToClassList("vn-hidden");

                if (output.LineText == "（故事結束）")
                {
                    continueButton.AddToClassList("vn-hidden");
                    return;
                }

                continueButton.RemoveFromClassList("vn-hidden");
            }
        }
    }
}
// ===== 變更結束 =====

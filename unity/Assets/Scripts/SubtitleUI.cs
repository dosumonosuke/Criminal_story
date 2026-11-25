using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CriminalStory
{
    /// <summary>
    /// All 2D UI, built in code so the project needs no hand-made scene
    /// objects: chapter caption, the single objective, an interact prompt,
    /// the subtitle panel, a white "haze" overlay (newborn vision in
    /// Chapter 0), a black fade overlay, and the end panel.
    /// </summary>
    public class SubtitleUI : MonoBehaviour
    {
        private Text _chapterLabel;
        private Text _objectiveLabel;
        private Text _controlsLabel;
        private Text _promptLabel;
        private GameObject _dialoguePanel;
        private Text _speakerLabel;
        private Text _lineLabel;
        private Image _hazeOverlay;
        private Image _fadeOverlay;
        private GameObject _endPanel;
        private Button _freeRoamButton;
        private Button _restartButton;

        public void Build()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            Font font = BuildUtil.DefaultFont();

            _chapterLabel = MakeText("ChapterLabel", transform, font, 26,
                new Color(1f, 1f, 1f), TextAnchor.UpperLeft);
            Place(_chapterLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(36f, -28f), new Vector2(900f, 40f));

            _objectiveLabel = MakeText("ObjectiveLabel", transform, font, 21,
                new Color(1f, 0.87f, 0.5f), TextAnchor.UpperLeft);
            Place(_objectiveLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(36f, -70f), new Vector2(1000f, 34f));

            _controlsLabel = MakeText("ControlsLabel", transform, font, 15,
                new Color(1f, 1f, 1f, 0.65f), TextAnchor.LowerLeft);
            Place(_controlsLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(36f, 24f), new Vector2(900f, 26f));

            _promptLabel = MakeText("PromptLabel", transform, font, 24,
                new Color(1f, 0.92f, 0.55f), TextAnchor.MiddleCenter);
            Place(_promptLabel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-350f, 190f), new Vector2(700f, 40f));
            _promptLabel.text = "";

            BuildDialoguePanel(font);
            BuildEndPanel(font);

            // Haze (white, newborn vision) — above the world, below subtitles.
            _hazeOverlay = MakeOverlay("HazeOverlay", new Color(1f, 0.99f, 0.96f, 1f), 1);
            // Fade (black) — topmost except the end panel.
            _fadeOverlay = MakeOverlay("FadeOverlay", new Color(0f, 0f, 0f, 0f), 2);
            _endPanel.transform.SetAsLastSibling();
        }

        // ---------- public API used by StoryDirector ----------

        public void SetChapter(string text)
        {
            _chapterLabel.text = text;
        }

        public void SetObjective(string text)
        {
            _objectiveLabel.text = text;
        }

        public void SetControlsHint(string text)
        {
            _controlsLabel.text = text;
        }

        public void ShowPrompt(string text)
        {
            _promptLabel.text = text;
        }

        public void ShowLine(string speaker, string text)
        {
            _dialoguePanel.SetActive(true);
            _speakerLabel.text = speaker;
            _lineLabel.text = text;
        }

        public void HideDialogue()
        {
            _dialoguePanel.SetActive(false);
        }

        public void SetHaze(float alpha)
        {
            Color c = _hazeOverlay.color;
            c.a = Mathf.Clamp01(alpha);
            _hazeOverlay.color = c;
        }

        public void FadeTo(float targetAlpha, float duration, Action onDone)
        {
            StopAllCoroutines();
            StartCoroutine(FadeRoutine(targetAlpha, duration, onDone));
        }

        public void ShowEndPanel(Action onFreeRoam, Action onRestart)
        {
            _endPanel.SetActive(true);
            _freeRoamButton.onClick.RemoveAllListeners();
            _restartButton.onClick.RemoveAllListeners();
            if (onFreeRoam != null)
            {
                _freeRoamButton.onClick.AddListener(() => onFreeRoam());
            }
            if (onRestart != null)
            {
                _restartButton.onClick.AddListener(() => onRestart());
            }
        }

        public void HideEndPanel()
        {
            _endPanel.SetActive(false);
        }

        // ---------- construction helpers ----------

        private IEnumerator FadeRoutine(float targetAlpha, float duration, Action onDone)
        {
            float start = _fadeOverlay.color.a;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = duration > 0f ? Mathf.Clamp01(t / duration) : 1f;
                Color c = _fadeOverlay.color;
                c.a = Mathf.Lerp(start, targetAlpha, k);
                _fadeOverlay.color = c;
                yield return null;
            }
            Color finalColor = _fadeOverlay.color;
            finalColor.a = targetAlpha;
            _fadeOverlay.color = finalColor;
            if (onDone != null)
            {
                onDone();
            }
        }

        private Image MakeOverlay(string name, Color color, int siblingOrder)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            go.transform.SetSiblingIndex(siblingOrder);
            return img;
        }

        private void BuildDialoguePanel(Font font)
        {
            GameObject panel = new GameObject("DialoguePanel");
            panel.transform.SetParent(transform, false);
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.offsetMin = new Vector2(60f, 36f);
            rt.offsetMax = new Vector2(-60f, 196f);
            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.72f);
            bg.raycastTarget = false;

            _speakerLabel = MakeText("Speaker", panel.transform, font, 21,
                new Color(1f, 0.85f, 0.45f), TextAnchor.UpperLeft);
            Place(_speakerLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(22f, -12f), new Vector2(1100f, 30f));

            _lineLabel = MakeText("Line", panel.transform, font, 25,
                Color.white, TextAnchor.UpperLeft);
            _lineLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            _lineLabel.verticalOverflow = VerticalWrapMode.Overflow;
            Place(_lineLabel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(22f, -48f), new Vector2(-44f, 76f));

            Text hint = MakeText("Hint", panel.transform, font, 14,
                new Color(1f, 1f, 1f, 0.55f), TextAnchor.LowerLeft);
            Place(hint.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(22f, 10f), new Vector2(600f, 22f));
            hint.text = "クリック／スペース：次へ";

            _dialoguePanel = panel;
            _dialoguePanel.SetActive(false);
        }

        private void BuildEndPanel(Font font)
        {
            GameObject panel = new GameObject("EndPanel");
            panel.transform.SetParent(transform, false);
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(680f, 330f);
            rt.anchoredPosition = Vector2.zero;
            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.07f, 0.92f);

            Text title = MakeText("EndTitle", panel.transform, font, 24,
                Color.white, TextAnchor.UpperCenter);
            Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(24f, -18f), new Vector2(-48f, 36f));
            title.text = StoryData.EndPanelTitle;

            Text body = MakeText("EndBody", panel.transform, font, 18,
                new Color(0.92f, 0.92f, 0.92f), TextAnchor.UpperLeft);
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.verticalOverflow = VerticalWrapMode.Overflow;
            Place(body.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(28f, -64f), new Vector2(-56f, 180f));
            body.text = StoryData.EndPanelBody;

            _freeRoamButton = MakeButton("FreeRoamButton", panel.transform, font,
                "このまま町を見回す", new Vector2(28f, 26f));
            _restartButton = MakeButton("RestartButton", panel.transform, font,
                "最初から（第0章へ）", new Vector2(288f, 26f));

            _endPanel = panel;
            _endPanel.SetActive(false);
        }

        private Button MakeButton(string name, Transform parent, Font font,
            string label, Vector2 anchoredPos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(240f, 44f);
            Image img = go.AddComponent<Image>();
            img.color = new Color(0.20f, 0.22f, 0.28f, 1f);
            Button button = go.AddComponent<Button>();
            button.targetGraphic = img;

            Text text = MakeText("Label", go.transform, font, 18,
                Color.white, TextAnchor.MiddleCenter);
            RectTransform trt = text.rectTransform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            text.text = label;
            return button;
        }

        private Text MakeText(string name, Transform parent, Font font, int size,
            Color color, TextAnchor anchor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.color = color;
            text.alignment = anchor;
            text.raycastTarget = false;
            // Soft shadow so subtitles stay readable on bright backgrounds.
            Shadow shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
            return text;
        }

        private void Place(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Vector2 size)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }
    }
}

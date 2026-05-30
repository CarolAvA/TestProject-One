using UnityEngine;
using UnityEngine.UI;

namespace ReverseSurvivorPrototype
{
    public sealed class RuntimeUiConfig
    {
        private const string ConfigResourcePath = "ReverseSurvivorConfig/MusicManiacConfigDatabase";

        private readonly MusicManiacConfigDatabase database;

        public RuntimeUiConfig()
        {
            database = Resources.Load<MusicManiacConfigDatabase>(ConfigResourcePath);
        }

        public UiLayoutConfig Layout(string id)
        {
            return database == null ? null : database.uiLayouts.Find(item => item != null && item.id == id);
        }

        public UiTextConfig Text(string id)
        {
            return database == null ? null : database.uiTexts.Find(item => item != null && item.id == id);
        }

        public UiButtonGroupConfig ButtonGroup(string id)
        {
            return database == null ? null : database.uiButtonGroups.Find(item => item != null && item.id == id);
        }

        public PrototypeHud.Anchor ToHudAnchor(UiAnchorPreset anchor)
        {
            switch (anchor)
            {
                case UiAnchorPreset.TopRight: return PrototypeHud.Anchor.TopRight;
                case UiAnchorPreset.BottomLeft: return PrototypeHud.Anchor.BottomLeft;
                case UiAnchorPreset.TopCenter: return PrototypeHud.Anchor.TopCenter;
                case UiAnchorPreset.BottomStretch: return PrototypeHud.Anchor.BottomStretch;
                case UiAnchorPreset.TopStretch: return PrototypeHud.Anchor.TopStretch;
                case UiAnchorPreset.Stretch: return PrototypeHud.Anchor.Stretch;
                default: return PrototypeHud.Anchor.TopLeft;
            }
        }

        public void ApplyLayout(GameObject target, string id)
        {
            var layout = Layout(id);
            if (target == null || layout == null)
            {
                return;
            }

            target.SetActive(layout.visible);
            var image = target.GetComponent<Image>();
            if (image != null)
            {
                image.color = layout.backgroundColor;
            }

            var rect = target.GetComponent<RectTransform>();
            if (rect != null)
            {
                PrototypeHud.ApplyAnchor(rect, ToHudAnchor(layout.anchor));
                rect.anchoredPosition = layout.position;
                rect.sizeDelta = layout.size;
                if (layout.anchor == UiAnchorPreset.Stretch)
                {
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                }
            }
        }

        public void ApplyText(Text target, string id)
        {
            var config = Text(id);
            if (target == null || config == null)
            {
                return;
            }

            target.gameObject.SetActive(config.visible);
            target.fontSize = Mathf.Max(1, config.fontSize);
            target.color = config.color;
            target.alignment = config.alignment;
            if (!string.IsNullOrEmpty(config.overrideText))
            {
                target.text = config.overrideText;
            }
        }
    }
}

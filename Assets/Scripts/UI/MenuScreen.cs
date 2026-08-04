using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Wallfall
{
    /// <summary>
    /// Startup screen: layered MountainDusk backdrop, minimal centered type,
    /// a thin flowing gold rule under the title, one glass CTA. Clean and modern.
    /// </summary>
    public class MenuScreen : MonoBehaviour
    {
        public void Build(System.Action onPlay)
        {
            var font = SpriteBank.UiFont();
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var canvasGo = new GameObject("MenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);

            // root object, NOT a child of the menu — the menu destroys itself on PLAY
            if (FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            // layered dusk mountains backdrop (fallback: deep plum)
            bool anyLayer = false;
            foreach (var tex in SpriteBank.MenuBackgroundLayers())
            {
                var bgRt = NewRect("bg", canvasGo.transform);
                Anchor(bgRt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var img = bgRt.gameObject.AddComponent<RawImage>();
                img.texture = tex;
                img.raycastTarget = false;
                anyLayer = true;
            }
            if (!anyLayer)
            {
                var bgRt = NewRect("bg", canvasGo.transform);
                Anchor(bgRt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                bgRt.gameObject.AddComponent<Image>().color = SpriteFactory.Plum;
            }

            // soft scrim so the type zone reads clean
            var scrim = NewRect("scrim", canvasGo.transform).gameObject.AddComponent<Image>();
            scrim.color = new Color(0.05f, 0.04f, 0.1f, 0.45f);
            scrim.raycastTarget = false;
            Anchor(scrim.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // title block
            var title = NewText("title", canvasGo.transform, font, 110, SpriteFactory.Cream);
            Anchor(title.rectTransform, new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.66f), new Vector2(-700, -80), new Vector2(700, 80));
            title.text = "WALLFALL";
            var outline = title.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            outline.effectDistance = new Vector2(4, -4);

            // thin flowing gold rule under the title
            var ruleRt = NewRect("rule", canvasGo.transform);
            Anchor(ruleRt, new Vector2(0.5f, 0.575f), new Vector2(0.5f, 0.575f), new Vector2(-180, -3), new Vector2(180, 3));
            var ruleBorder = ruleRt.gameObject.AddComponent<ChromaBorder>();
            ruleBorder.Accent = SpriteFactory.Gold;
            ruleBorder.BorderWidth = 3f;
            ruleBorder.raycastTarget = false;

            var tagline = NewText("tagline", canvasGo.transform, font, 20, new Color(0.95f, 0.92f, 0.85f, 0.9f));
            Anchor(tagline.rectTransform, new Vector2(0.5f, 0.53f), new Vector2(0.5f, 0.53f), new Vector2(-500, -18), new Vector2(500, 18));
            tagline.text = "FOUR LANES · ONE ARMY · THE WALLS DROP AT ROUND 6";

            // single glass CTA
            var btnImg = NewRect("play", canvasGo.transform).gameObject.AddComponent<Image>();
            btnImg.color = new Color(0.045f, 0.04f, 0.09f, 0.6f);
            Anchor(btnImg.rectTransform, new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), new Vector2(-170, -34), new Vector2(170, 34));
            var edgeRt = NewRect("edge", btnImg.transform);
            Anchor(edgeRt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var edge = edgeRt.gameObject.AddComponent<ChromaBorder>();
            edge.Accent = SpriteFactory.Gold;
            edge.BorderWidth = 2.5f;
            edge.raycastTarget = false;

            var btn = btnImg.gameObject.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            var colors = btn.colors;
            colors.highlightedColor = new Color(1.8f, 1.8f, 2f, 1f);
            colors.pressedColor = new Color(2.4f, 2.4f, 2.6f, 1f);
            btn.colors = colors;

            var btnLabel = NewText("label", btnImg.transform, font, 26, SpriteFactory.Gold);
            Anchor(btnLabel.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            btnLabel.text = "PLAY VS AI";
            btn.onClick.AddListener(() =>
            {
                Sfx.Play("Select - 1", 0.6f);
                Music.Play();
                onPlay?.Invoke();
                Destroy(gameObject);
            });

            // quiet footer
            var hint = NewText("hint", canvasGo.transform, font, 13, new Color(0.9f, 0.88f, 0.82f, 0.55f));
            Anchor(hint.rectTransform, new Vector2(0.5f, 0.05f), new Vector2(0.5f, 0.05f), new Vector2(-620, -24), new Vector2(620, 24));
            hint.text = "drag units · right-click for details · S sell · 1-4 lanes · R reroll · X buy xp · T market";
        }

        RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        Text NewText(string name, Transform parent, Font font, int size, Color color)
        {
            var rt = NewRect(name, parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = font;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        static void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = min; rt.anchorMax = max;
            rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        }
    }
}

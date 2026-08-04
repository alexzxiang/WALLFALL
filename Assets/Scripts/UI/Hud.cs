using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Wallfall
{
    /// <summary>
    /// Glass UI: every surface is a clean translucent dark fill with a crisp animated chroma
    /// outline (ThinFrame + UiPulse). No opaque bars, no legacy pill sprites. Market is an
    /// icon grid with prices under icons and hover tooltips. UI sounds from the JDSherbert pack.
    /// </summary>
    public class Hud : MonoBehaviour
    {
        MatchController _match;
        BoardsPresenter _presenter;
        DragController _drag;
        Font _font;
        RectTransform _canvasRect;

        Text _roundText, _timerText, _bannerText, _promptText, _speedText;
        Text[] _walletTexts = new Text[4];
        Text[] _laneRailTexts = new Text[GameConfig.LaneCount];
        Image[] _laneRailFills = new Image[GameConfig.LaneCount];
        ChromaBorder[] _laneRailEdges = new ChromaBorder[GameConfig.LaneCount];
        Button[] _shopButtons = new Button[GameConfig.ShopSlots];
        Text[] _shopNames = new Text[GameConfig.ShopSlots];
        Text[] _shopCosts = new Text[GameConfig.ShopSlots];
        Text[] _shopBonds = new Text[GameConfig.ShopSlots];
        Image[] _shopFills = new Image[GameConfig.ShopSlots];
        ChromaBorder[] _shopEdges = new ChromaBorder[GameConfig.ShopSlots];
        Image[] _shopPortraits = new Image[GameConfig.ShopSlots];
        GameObject _gameOverPanel, _spendPanel, _kitPanel, _sidebar, _hoverTip;
        Text _gameOverText, _sidebarText, _hoverTipText;
        Image _sidebarPortrait;
        UnitInstance _sidebarUnit;
        GameObject _sidebarSell;
        RectTransform _shopBarRect;
        GameObject _sellOverlay;
        Text _sellText;
        Image _sellFill;
        Text _walletLevelText;
        readonly List<(Button btn, Text price, ChromaBorder edge, System.Func<(string, bool)> refresh)> _marketTiles =
            new List<(Button, Text, ChromaBorder, System.Func<(string, bool)>)>();
        readonly List<(Button btn, ChromaBorder edge, int lane, Kit kit)> _kitButtons = new List<(Button, ChromaBorder, int, Kit)>();
        readonly List<(GameObject root, Image fill, ChromaBorder edge, Image icon, Text label)> _bondRows =
            new List<(GameObject, Image, ChromaBorder, Image, Text)>();
        readonly string[] _bondTips = new string[8];
        Image[] _laneRailHearts = new Image[GameConfig.LaneCount];
        Text[] _laneRailHp = new Text[GameConfig.LaneCount];
        Image[][] _laneRailMinis = new Image[GameConfig.LaneCount][];
        float _bannerUntil;
        int _lastTickSecond = -1;
        Phase _lastPhase = Phase.Planning;

        static readonly Color GlassFill = new Color(0.045f, 0.04f, 0.09f, 0.55f);
        static readonly Color GlassFillLight = new Color(0.13f, 0.11f, 0.2f, 0.6f);
        static readonly Color CreamCol = SpriteFactory.Cream;
        static readonly Color Lavender = new Color(0.72f, 0.66f, 0.86f);

        public void Build(MatchController match, BoardsPresenter presenter, DragController drag)
        {
            _match = match;
            _presenter = presenter;
            _drag = drag;
            _font = SpriteBank.UiFont();
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            _canvasRect = canvasGo.GetComponent<RectTransform>();

            if (FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            BuildRoundChip(canvasGo.transform);
            BuildLaneRail(canvasGo.transform);
            BuildBondTracker(canvasGo.transform);
            BuildShopBar(canvasGo.transform);
            BuildSellOverlay(canvasGo.transform);
            BuildMarket(canvasGo.transform);
            BuildKitPicker(canvasGo.transform);
            BuildSidebar(canvasGo.transform);
            BuildHoverTip(canvasGo.transform);
            BuildBanner(canvasGo.transform);
            BuildGameOver(canvasGo.transform);

            _match.StateChanged += Refresh;
            _match.Announcement += OnAnnouncement;
            _presenter.FocusChanged += Refresh;
            Refresh();
        }

        // ---------- glass construction kit ----------

        RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        /// <summary>Clean translucent surface — no sprite, no texture, just glass.</summary>
        Image Glass(string name, Transform parent, float alpha = 0.55f)
        {
            var rt = NewRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(GlassFill.r, GlassFill.g, GlassFill.b, alpha);
            return img;
        }

        /// <summary>Sleek animated border: a color gradient that flows around the perimeter.</summary>
        ChromaBorder ChromaEdge(Image host, Color accent, bool rainbow = false, float alpha = 0.95f)
        {
            var border = NewRect("edge", host.transform).gameObject.AddComponent<ChromaBorder>();
            border.raycastTarget = false;
            border.Accent = accent;
            border.Rainbow = rainbow;
            Anchor(border.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return border;
        }

        /// <summary>Hover juice: gentle grow on pointer enter, restore on exit, soft tick sound.</summary>
        void HoverJuice(GameObject go, float scale = 1.05f)
        {
            AddHover(go,
                () => { go.transform.localScale = Vector3.one * scale; Sfx.Play("Cursor - 3", 0.18f); },
                () => go.transform.localScale = Vector3.one);
        }

        Text NewText(string name, Transform parent, int size, Color color, TextAnchor anchor = TextAnchor.MiddleCenter, bool wrap = false)
        {
            var rt = NewRect(name, parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = _font;
            t.fontSize = Mathf.RoundToInt(size * 1.3f);
            t.color = color;
            t.alignment = anchor;
            t.supportRichText = true;
            t.raycastTarget = false;
            t.horizontalOverflow = wrap ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
            t.verticalOverflow = wrap ? VerticalWrapMode.Truncate : VerticalWrapMode.Overflow;
            return t;
        }

        static void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = min; rt.anchorMax = max;
            rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        }

        /// <summary>Glass button: translucent fill + chroma edge + bright label + click sound.</summary>
        Button GlassButton(string name, Transform parent, string label, int fontSize, Color accent, System.Action onClick, out Text labelText)
        {
            var img = Glass(name, parent, 0.5f);
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.highlightedColor = new Color(1.7f, 1.7f, 1.9f, 1f);
            colors.pressedColor = new Color(2.2f, 2.2f, 2.4f, 1f);
            btn.colors = colors;
            ChromaEdge(img, accent);
            labelText = NewText("label", img.transform, fontSize, accent);
            Anchor(labelText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            labelText.text = label;
            btn.onClick.AddListener(() => { Sfx.Play("Cursor - 1", 0.45f); onClick(); });
            HoverJuice(img.gameObject, 1.04f);
            return btn;
        }

        void AddHover(GameObject go, System.Action enter, System.Action exit)
        {
            var trig = go.AddComponent<EventTrigger>();
            var e1 = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            e1.callback.AddListener(_ => enter());
            var e2 = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            e2.callback.AddListener(_ => exit());
            trig.triggers.Add(e1);
            trig.triggers.Add(e2);
        }

        static Color TierColor(int cost)
        {
            switch (cost)
            {
                case 1: return new Color(0.75f, 0.78f, 0.85f);
                case 2: return new Color(0.45f, 0.86f, 0.5f);
                case 3: return new Color(0.42f, 0.75f, 0.95f);
                case 4: return new Color(0.78f, 0.55f, 0.95f);
                default: return SpriteFactory.Gold;
            }
        }

        // ---------- sections ----------

        void BuildRoundChip(Transform canvas)
        {
            var chip = Glass("RoundChip", canvas, 0.55f);
            Anchor(chip.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(-260, -54), new Vector2(260, -6));
            ChromaEdge(chip, SpriteFactory.Gold, false, 0.8f);

            _roundText = NewText("Round", chip.transform, 16, CreamCol);
            Anchor(_roundText.rectTransform, new Vector2(0, 0), new Vector2(0.78f, 1), new Vector2(16, 0), Vector2.zero);

            _timerText = NewText("Timer", chip.transform, 20, SpriteFactory.Gold);
            Anchor(_timerText.rectTransform, new Vector2(0.78f, 0), new Vector2(1, 1), Vector2.zero, new Vector2(-12, 0));
        }

        void BuildLaneRail(Transform canvas)
        {
            var rail = NewRect("LaneRail", canvas);
            Anchor(rail, new Vector2(0, 0.32f), new Vector2(0, 0.94f), new Vector2(8, 0), new Vector2(206, 0));

            for (int i = 0; i < GameConfig.LaneCount; i++)
            {
                int lane = i;
                var fill = Glass($"Lane{i + 1}", rail, 0.5f);
                float top = 1f - i * 0.25f;
                Anchor(fill.rectTransform, new Vector2(0, top - 0.24f), new Vector2(1, top), new Vector2(0, 4), new Vector2(0, -4));
                var btn = fill.gameObject.AddComponent<Button>();
                btn.targetGraphic = fill;
                btn.onClick.AddListener(() => _presenter.FocusLane(lane));
                _laneRailFills[i] = fill;
                _laneRailEdges[i] = ChromaEdge(fill, SpriteFactory.LaneAccent[i], false, 0.9f);

                // bed heart + HP, top-left of the card
                var heartRt = NewRect("heart", fill.transform);
                Anchor(heartRt, new Vector2(0, 1), new Vector2(0, 1), new Vector2(8, -34), new Vector2(34, -8));
                var heart = heartRt.gameObject.AddComponent<Image>();
                heart.sprite = SpriteBank.UiHeart();
                heart.color = SpriteFactory.LaneAccent[i];
                heart.preserveAspect = true;
                heart.raycastTarget = false;
                heart.enabled = heart.sprite != null;
                _laneRailHearts[i] = heart;

                var hp = NewText("hp", fill.transform, 13, CreamCol, TextAnchor.MiddleLeft);
                Anchor(hp.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(40, -36), new Vector2(-6, -6));
                _laneRailHp[i] = hp;

                var t = NewText("info", fill.transform, 11, CreamCol, TextAnchor.MiddleLeft);
                Anchor(t.rectTransform, new Vector2(0, 0.32f), new Vector2(1, 0.62f), new Vector2(10, 0), new Vector2(-6, 0));
                _laneRailTexts[i] = t;

                // mini snapshot of your board on this lane
                _laneRailMinis[i] = new Image[8];
                for (int m = 0; m < 8; m++)
                {
                    var miniRt = NewRect($"mini{m}", fill.transform);
                    Anchor(miniRt, new Vector2(0, 0), new Vector2(0, 0), new Vector2(10 + m * 23, 4), new Vector2(30 + m * 23, 26));
                    var mini = miniRt.gameObject.AddComponent<Image>();
                    mini.preserveAspect = true;
                    mini.raycastTarget = false;
                    mini.enabled = false;
                    _laneRailMinis[i][m] = mini;
                }
            }
        }

        void BuildBondTracker(Transform canvas)
        {
            // TFT-style: a slim column of icon badges to the RIGHT of the lane rail
            var rail = NewRect("TraitTracker", canvas);
            Anchor(rail, new Vector2(0, 0.32f), new Vector2(0, 0.94f), new Vector2(212, 0), new Vector2(278, 0));

            for (int i = 0; i < 8; i++)
            {
                int idx = i;
                var fill = Glass($"trait{i}", rail, 0.45f);
                Anchor(fill.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -(i + 1) * 58 + 2), new Vector2(0, -i * 58 - 2));
                var edge = ChromaEdge(fill, SpriteFactory.Gold, false, 0.8f);
                edge.BorderWidth = 2f;

                var iconRt = NewRect("icon", fill.transform);
                Anchor(iconRt, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(-16, -36), new Vector2(16, -4));
                var icon = iconRt.gameObject.AddComponent<Image>();
                icon.preserveAspect = true;
                icon.raycastTarget = false;

                var label = NewText("label", fill.transform, 11, CreamCol);
                Anchor(label.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 2), new Vector2(0, 20));

                AddHover(fill.gameObject, () => ShowHoverTip(_bondTips[idx] ?? ""), HideHoverTip);

                _bondRows.Add((fill.gameObject, fill, edge, icon, label));
                fill.gameObject.SetActive(false);
            }
        }

        void RefreshBondTracker()
        {
            var laneUnits = _match.You.Lanes[_presenter.FocusedLane].Units;

            // count origins (incl. dual-origin & Sylvan weight) and classes, remember members
            var rows = new List<(string name, string icon, int count, int[] bps, string desc, List<string> members)>();
            void Collect<T>(T key, string icon, int[] bps, string desc, System.Func<UnitDef, bool> has, System.Func<UnitDef, int> weight)
            {
                int c = 0; var members = new List<string>();
                foreach (var pu in laneUnits)
                    if (has(pu.Unit.Def)) { c += weight(pu.Unit.Def); members.Add(pu.Unit.Def.Name + new string('★', pu.Unit.Star)); }
                if (c > 0) rows.Add((key.ToString(), icon, c, bps, desc, members));
            }
            foreach (var o in TraitInfo.AllOrigins)
                Collect(o, TraitInfo.IconFile(o), TraitInfo.Breakpoints(o), TraitInfo.Describe(o),
                    d => d.Origin == o || d.Origin2 == o, d => o == Origin.Sylvan ? d.SylvanWeight : 1);
            foreach (var k in TraitInfo.AllClasses)
                Collect(k, TraitInfo.IconFile(k), TraitInfo.Breakpoints(k), TraitInfo.Describe(k),
                    d => d.Class == k, d => 1);

            rows.Sort((a, b) =>
            {
                int ta = TraitInfo.Tier(a.count, a.bps), tb = TraitInfo.Tier(b.count, b.bps);
                return tb != ta ? tb.CompareTo(ta) : b.count.CompareTo(a.count);
            });

            for (int i = 0; i < _bondRows.Count; i++)
            {
                var row = _bondRows[i];
                if (i >= rows.Count) { row.root.SetActive(false); continue; }

                var (name, icon, count, bps, desc, members) = rows[i];
                int tier = TraitInfo.Tier(count, bps);
                int nextBp = tier < bps.Length ? bps[tier] : bps[bps.Length - 1];
                bool active = tier > 0;
                bool maxed = tier >= bps.Length;

                row.root.SetActive(true);
                row.icon.sprite = SpriteBank.TraitIcon(icon);
                row.icon.enabled = row.icon.sprite != null;
                row.icon.color = active ? Color.white : new Color(0.6f, 0.6f, 0.7f);
                string countCol = maxed ? "#F27EA9" : active ? "#FFD447" : "#8d81a8";
                row.label.text = $"<color={countCol}>{count}/{nextBp}</color>";
                row.label.color = active ? CreamCol : new Color(0.62f, 0.58f, 0.72f);
                row.edge.Accent = maxed ? SpriteFactory.Hex("#F27EA9") : active ? SpriteFactory.Gold : new Color(0.45f, 0.42f, 0.55f);
                row.edge.Animate = active;
                row.edge.SetVerticesDirty();

                string mem = string.Join(", ", members);
                _bondTips[i] = $"{name.ToUpper()}  tier {tier}\n{desc}\n<color=#bdb2d4>{mem}</color>";
            }
        }

        void BuildShopBar(Transform canvas)
        {
            var bar = Glass("ShopBar", canvas, 0.5f);
            Anchor(bar.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-560, 6), new Vector2(560, 124));
            ChromaEdge(bar, SpriteFactory.Gold, rainbow: false, alpha: 0.75f);
            _shopBarRect = bar.rectTransform;

            // wallet: its own glass panel with a gold flowing outline, centered above the shop
            var walletPanel = Glass("Wallet", canvas, 0.6f);
            ChromaEdge(walletPanel, SpriteFactory.Gold, false, 0.8f);
            Anchor(walletPanel.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-330, 128), new Vector2(330, 168));
            var wallet = walletPanel.rectTransform;
            for (int i = 0; i < 4; i++)
            {
                float x0 = 16 + i * 104;
                var icon = NewRect($"gem{i}", wallet);
                Anchor(icon, new Vector2(0, 0), new Vector2(0, 1), new Vector2(x0, 8), new Vector2(x0 + 28, -8));
                var img = icon.gameObject.AddComponent<Image>();
                img.sprite = SpriteBank.GemIcon(i);
                img.color = SpriteBank.GemTint(i);
                img.preserveAspect = true;
                img.raycastTarget = false;
                img.enabled = img.sprite != null;

                _walletTexts[i] = NewText($"amt{i}", wallet, 16, WalletColor(i), TextAnchor.MiddleLeft);
                Anchor(_walletTexts[i].rectTransform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(x0 + 33, 0), new Vector2(x0 + 100, 0));
                var shadow = _walletTexts[i].gameObject.AddComponent<Outline>();
                shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
                shadow.effectDistance = new Vector2(2, -2);
            }

            _walletLevelText = NewText("lvl", wallet, 14, CreamCol, TextAnchor.MiddleRight);
            Anchor(_walletLevelText.rectTransform, new Vector2(0.66f, 0), new Vector2(1f, 1), Vector2.zero, new Vector2(-16, 0));
            var lvlShadow = _walletLevelText.gameObject.AddComponent<Outline>();
            lvlShadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            lvlShadow.effectDistance = new Vector2(2, -2);

            for (int i = 0; i < GameConfig.ShopSlots; i++)
            {
                int slot = i;
                var fill = Glass($"Card{i}", bar.transform, 0.55f);
                float x0 = 152 + i * 138;
                Anchor(fill.rectTransform, Vector2.zero, Vector2.zero, new Vector2(x0, 10), new Vector2(x0 + 130, 112));
                var btn = fill.gameObject.AddComponent<Button>();
                btn.targetGraphic = fill;
                btn.onClick.AddListener(() =>
                {
                    if (_match.BuyFromShop(slot)) Sfx.Play("Select - 1", 0.55f);
                    else Sfx.Play("Error - 1", 0.4f);
                });
                _shopButtons[i] = btn;
                _shopFills[i] = fill;
                _shopEdges[i] = ChromaEdge(fill, Color.white, false, 0.9f);
                HoverJuice(fill.gameObject, 1.06f);

                var pr = NewRect("portrait", fill.transform);
                Anchor(pr, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(-24, -54), new Vector2(24, -8));
                _shopPortraits[i] = pr.gameObject.AddComponent<Image>();
                _shopPortraits[i].preserveAspect = true;
                _shopPortraits[i].raycastTarget = false;

                _shopNames[i] = NewText("name", fill.transform, 12, CreamCol);
                Anchor(_shopNames[i].rectTransform, new Vector2(0, 0.3f), new Vector2(1, 0.46f), Vector2.zero, Vector2.zero);
                _shopBonds[i] = NewText("bond", fill.transform, 10, Lavender);
                Anchor(_shopBonds[i].rectTransform, new Vector2(0, 0.16f), new Vector2(1, 0.3f), Vector2.zero, Vector2.zero);
                _shopCosts[i] = NewText("cost", fill.transform, 12, SpriteFactory.Gold);
                Anchor(_shopCosts[i].rectTransform, new Vector2(0, 0.02f), new Vector2(1, 0.16f), Vector2.zero, Vector2.zero);
            }

            Text _;
            var reroll = GlassButton("Reroll", bar.transform, "REROLL (R) 2g", 12, SpriteFactory.Gold, () =>
            {
                if (!_match.Reroll()) Sfx.Play("Error - 1", 0.4f);
            }, out _);
            Anchor(reroll.GetComponent<RectTransform>(), new Vector2(0, 0.5f), new Vector2(0, 1), new Vector2(12, 4), new Vector2(142, -8));

            var xp = GlassButton("BuyXp", bar.transform, "BUY XP (X) 4g", 12, CreamCol, () =>
            {
                if (!_match.BuyXp()) Sfx.Play("Error - 1", 0.4f);
            }, out _);
            Anchor(xp.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(0, 0.5f), new Vector2(12, 8), new Vector2(142, -4));

            var ready = GlassButton("Ready", bar.transform, "READY", 17, SpriteFactory.Hex("#8FFF9E"), () => _match.PlayerReady(), out _);
            Anchor(ready.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 1), new Vector2(-140, 12), new Vector2(-12, -12));

            var spend = GlassButton("SpendToggle", canvas, "MARKET (T)", 12, SpriteFactory.Hex("#6EE7F0"), ToggleSpendPanel, out _);
            Anchor(spend.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-278, 132), new Vector2(-148, 170));

            var speed = GlassButton("Speed", canvas, "SPD x1 (F)", 12, SpriteFactory.Hex("#F27EA9"), () => _match.ToggleFightSpeed(), out _speedText);
            Anchor(speed.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-138, 132), new Vector2(-12, 170));
        }

        static Color WalletColor(int i) => SpriteFactory.LaneAccent[Mathf.Clamp(i, 0, 3)];

        void BuildSellOverlay(Transform canvas)
        {
            var fill = Glass("SellOverlay", canvas, 0.82f);
            fill.color = new Color(0.35f, 0.06f, 0.08f, 0.82f);
            fill.raycastTarget = false;
            Anchor(fill.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-560, 6), new Vector2(560, 124));
            var edge = ChromaEdge(fill, SpriteFactory.HpRed, false, 0.95f);
            edge.BorderWidth = 3f;
            _sellFill = fill;

            _sellText = NewText("label", fill.transform, 22, SpriteFactory.Cream);
            Anchor(_sellText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            _sellOverlay = fill.gameObject;
            _sellOverlay.SetActive(false);
        }

        /// <summary>Shown while dragging a unit: the shop becomes a TFT-style sell zone.</summary>
        public void ShowSellOverlay(UnitInstance u)
        {
            _sellText.text = $"SELL {u.Def.Name} — {u.SellValue}g";
            _sellOverlay.SetActive(true);
        }

        public void SetSellHover(bool over)
        {
            if (!_sellOverlay.activeSelf) return;
            _sellFill.color = over ? new Color(0.55f, 0.09f, 0.12f, 0.92f) : new Color(0.35f, 0.06f, 0.08f, 0.82f);
            _sellText.fontSize = Mathf.RoundToInt((over ? 26 : 22) * 1.3f);
        }

        public void HideSellOverlay()
        {
            if (_sellOverlay != null) _sellOverlay.SetActive(false);
        }

        public bool IsSidebarOpen => _sidebar != null && _sidebar.activeSelf;

        public bool IsMarketOpen => _spendPanel != null && _spendPanel.activeSelf;

        public bool IsOverSidebar(Vector2 screenPos) =>
            _sidebar != null && _sidebar.activeSelf &&
            RectTransformUtility.RectangleContainsScreenPoint(_sidebar.GetComponent<RectTransform>(), screenPos, null);

        // ---------- market: icon grid, price under icon, tooltip on hover ----------

        void BuildMarket(Transform canvas)
        {
            var panel = Glass("Market", canvas, 0.6f);
            Anchor(panel.rectTransform, new Vector2(1, 0.2f), new Vector2(1, 0.97f), new Vector2(-330, 0), new Vector2(-8, 0));
            ChromaEdge(panel, SpriteFactory.LaneAccent[1], false, 0.8f);
            _spendPanel = panel.gameObject;

            // scrollable body so the grid never bleeds past the panel
            var viewport = NewRect("viewport", panel.transform);
            Anchor(viewport, new Vector2(0, 0), new Vector2(1, 1), new Vector2(5, 50), new Vector2(-5, -8));
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = NewRect("content", viewport);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1f);
            var scroll = panel.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28f;

            float y = -6;
            const int cols = 4;
            const float tileW = 72f, tileH = 82f, gap = 6f;

            void Header(string txt, Color col, int size = 14)
            {
                var t = NewText("hdr", content, size, col, TextAnchor.MiddleLeft);
                Anchor(t.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(14, y - 24), new Vector2(-14, y));
                t.text = txt;
                y -= 28;
            }

            int gridIndex = 0;
            void Tile(string iconKey, Color accent, System.Action onClick,
                      System.Func<(string, bool)> refresh, System.Func<string> hover)
            {
                int col = gridIndex % cols;
                int row = gridIndex / cols;
                float x0 = 12 + col * (tileW + gap);
                float ty = y - row * (tileH + gap);

                var fill = Glass("tile", content, 0.5f);
                Anchor(fill.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(x0, ty - tileH), new Vector2(x0 + tileW, ty));
                var btn = fill.gameObject.AddComponent<Button>();
                btn.targetGraphic = fill;
                btn.onClick.AddListener(() => { Sfx.Play("Cursor - 2", 0.45f); onClick(); });
                var edge = ChromaEdge(fill, accent, false, 0.7f);
                HoverJuice(fill.gameObject, 1.07f);

                var iconRt = NewRect("icon", fill.transform);
                Anchor(iconRt, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(-22, -50), new Vector2(22, -8));
                var icon = iconRt.gameObject.AddComponent<Image>();
                icon.sprite = SpriteBank.MarketIcon(iconKey);
                icon.preserveAspect = true;
                icon.raycastTarget = false;
                icon.enabled = icon.sprite != null;

                var price = NewText("price", fill.transform, 12, CreamCol);
                Anchor(price.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 6), new Vector2(0, 34));

                AddHover(fill.gameObject, () => ShowHoverTip(hover()), HideHoverTip);
                _marketTiles.Add((btn, price, edge, refresh));
                gridIndex++;
            }
            void EndGrid()
            {
                int rows = (gridIndex + cols - 1) / cols;
                y -= rows * (tileH + gap) + 6;
                gridIndex = 0;
            }

            Header("MARKET", SpriteFactory.Gold, 17);

            Header("IRON — targets focused lane", SpriteFactory.LaneAccent[0]);
            foreach (ConsumableKind c in System.Enum.GetValues(typeof(ConsumableKind)))
            {
                var kind = c;
                Tile(kind.ToString(), SpriteFactory.LaneAccent[0],
                    () => { if (!_match.BuyConsumable(kind, _presenter.FocusedLane)) Sfx.Play("Error - 1", 0.4f); },
                    () => ($"{ConsumableInfo.Cost(kind, _match.You.Lanes[_presenter.FocusedLane].Kit)}i",
                           _match.You.Iron >= ConsumableInfo.Cost(kind, _match.You.Lanes[_presenter.FocusedLane].Kit)),
                    () => $"{ConsumableInfo.DisplayName(kind)}\n{ConsumableInfo.Describe(kind)}");
            }
            EndGrid();

            Header("EMERALD POWERS", SpriteFactory.LaneAccent[2]);
            foreach (PowerKind pk in System.Enum.GetValues(typeof(PowerKind)))
            {
                var kind = pk;
                Tile(kind.ToString(), SpriteFactory.LaneAccent[2],
                    () => { if (!_match.BuyPower(kind, _presenter.FocusedLane)) Sfx.Play("Error - 1", 0.4f); },
                    () => ($"{PowerInfo.Cost(kind)}e", _match.You.Emeralds >= PowerInfo.Cost(kind)),
                    () => $"{PowerInfo.DisplayName(kind)}\n{PowerInfo.Describe(kind)}");
            }
            EndGrid();

            Header("DIAMOND FORGE — click, then click a unit", SpriteFactory.LaneAccent[1]);
            foreach (var item in ItemCatalog.All)
            {
                var it = item;
                Tile(it.Id, SpriteFactory.LaneAccent[1],
                    () => BeginEquip(it),
                    () => ($"{ItemDef.DiamondCost}d", _match.You.Diamonds >= ItemDef.ForgeCost),
                    () => $"{it.Name.ToUpper()}\n{ItemDesc(it)}");
            }
            Tile("duplicate", SpriteFactory.LaneAccent[1],
                () => BeginDuplicate(),
                () => ($"{ItemCatalog.DuplicateCost}d", _match.You.Diamonds >= ItemCatalog.DuplicateCost),
                () => "DIAMOND DUPLICATE\nAdds a copy of a 1★ unit (cost ≤3) to your bench");
            EndGrid();

            content.sizeDelta = new Vector2(0f, -y + 12f);

            _promptText = NewText("prompt", panel.transform, 12, SpriteFactory.Gold, TextAnchor.MiddleCenter, wrap: true);
            Anchor(_promptText.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(10, 8), new Vector2(-10, 44));
            _promptText.text = "";

            _spendPanel.SetActive(false);
        }

        static string ItemDesc(ItemDef it)
        {
            var parts = new List<string>();
            if (it.AdMult > 1f) parts.Add($"+{Mathf.RoundToInt((it.AdMult - 1) * 100)}% attack damage");
            if (it.ApMult > 1f) parts.Add($"+{Mathf.RoundToInt((it.ApMult - 1) * 100)}% ability power");
            if (it.HpMult > 1f) parts.Add($"+{Mathf.RoundToInt((it.HpMult - 1) * 100)}% health");
            if (it.AsMult > 1f) parts.Add($"+{Mathf.RoundToInt((it.AsMult - 1) * 100)}% attack speed");
            if (it.Lifesteal > 0f) parts.Add($"{Mathf.RoundToInt(it.Lifesteal * 100)}% lifesteal");
            if (it.Thorns > 0f) parts.Add($"reflects {Mathf.RoundToInt(it.Thorns * 100)}% of attacks");
            if (it.RangeBonus > 0) parts.Add($"+{it.RangeBonus} range");
            if (it.ManaBonus > 0) parts.Add($"+{it.ManaBonus} mana per attack");
            return string.Join(", ", parts);
        }

        public void ToggleSpendPanel()
        {
            bool opening = !_spendPanel.activeSelf;
            _spendPanel.SetActive(opening);
            Sfx.Play(opening ? "Popup Open - 1" : "Popup Close - 1", 0.5f);
            if (opening) HideUnitSidebar();
            else { ClearPending(); HideHoverTip(); }
            Refresh();
        }

        void BeginEquip(ItemDef item)
        {
            _promptText.text = $"Click a unit to equip {item.Name}";
            _drag.PendingUnitClick = u =>
            {
                bool ok = _match.BuyAndEquipItem(item, u);
                if (ok) Sfx.Play("Select - 2", 0.55f);
                _promptText.text = ok ? "" : "Can't equip (slots / diamonds)";
                return ok;
            };
        }

        void BeginDuplicate()
        {
            _promptText.text = "Click a 1★ unit (cost ≤3) to duplicate";
            _drag.PendingUnitClick = u =>
            {
                bool ok = _match.DiamondDuplicate(u);
                if (ok) Sfx.Play("Select - 2", 0.55f);
                _promptText.text = ok ? "" : "Can't duplicate that unit";
                return ok;
            };
        }

        void ClearPending()
        {
            if (_drag != null) _drag.PendingUnitClick = null;
            if (_promptText != null) _promptText.text = "";
        }

        // ---------- hover tip ----------

        void BuildHoverTip(Transform canvas)
        {
            var tip = Glass("HoverTip", canvas, 0.88f);
            tip.raycastTarget = false;
            var rt = tip.rectTransform;
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(300, 130);
            ChromaEdge(tip, SpriteFactory.Gold, false, 0.7f);
            _hoverTip = tip.gameObject;
            _hoverTipText = NewText("body", tip.transform, 12, CreamCol, TextAnchor.MiddleLeft, wrap: true);
            Anchor(_hoverTipText.rectTransform, Vector2.zero, Vector2.one, new Vector2(14, 10), new Vector2(-14, -10));
            _hoverTip.SetActive(false);
        }

        void ShowHoverTip(string text)
        {
            _hoverTipText.text = text;
            _hoverTip.SetActive(true);

            // place beside the cursor, clamped to the canvas
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, mouse.position.ReadValue(), null, out var local);
                var half = _canvasRect.rect.size * 0.5f;
                var rt = _hoverTip.GetComponent<RectTransform>();
                float x = local.x + half.x + 22f;
                if (x + 305f > _canvasRect.rect.width) x = local.x + half.x - 322f; // flip to the left near the right edge
                float y = Mathf.Clamp(local.y + half.y, 80f, _canvasRect.rect.height - 80f);
                rt.anchoredPosition = new Vector2(x, y);
            }
        }

        void HideHoverTip()
        {
            if (_hoverTip != null) _hoverTip.SetActive(false);
        }

        // ---------- unit sidebar ----------

        void BuildSidebar(Transform canvas)
        {
            var panel = Glass("UnitSidebar", canvas, 0.72f);
            Anchor(panel.rectTransform, new Vector2(1, 0.28f), new Vector2(1, 0.93f), new Vector2(-330, 0), new Vector2(-8, 0));
            ChromaEdge(panel, SpriteFactory.Gold, false, 0.8f);
            _sidebar = panel.gameObject;

            var frame = Glass("portraitFrame", panel.transform, 0.4f);
            Anchor(frame.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(-58, -132), new Vector2(58, -16));
            ChromaEdge(frame, CreamCol, false, 0.6f);
            var pr = NewRect("portrait", frame.transform);
            Anchor(pr, Vector2.zero, Vector2.one, new Vector2(10, 10), new Vector2(-10, -10));
            _sidebarPortrait = pr.gameObject.AddComponent<Image>();
            _sidebarPortrait.preserveAspect = true;
            _sidebarPortrait.raycastTarget = false;

            _sidebarText = NewText("body", panel.transform, 13, CreamCol, TextAnchor.UpperLeft, wrap: true);
            Anchor(_sidebarText.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(18, 60), new Vector2(-18, -144));

            var close = GlassButton("Close", panel.transform, "X", 13, SpriteFactory.Hex("#FF8E8E"), HideUnitSidebar, out var closeLbl);
            Anchor(close.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-46, -46), new Vector2(-10, -10));

            var sell = GlassButton("Sell", panel.transform, "SELL", 14, SpriteFactory.Gold, () =>
            {
                if (_sidebarUnit != null && _match.CanAct)
                {
                    _match.SellUnit(_sidebarUnit);
                    Sfx.Play("Select - 2", 0.55f);
                }
                HideUnitSidebar();
            }, out var sellLbl);
            Anchor(sell.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-90, 10), new Vector2(90, 50));
            _sidebarSell = sell.gameObject;

            _sidebar.SetActive(false);
        }

        public void ShowUnitSidebar(UnitInstance u)
        {
            if (_spendPanel.activeSelf) _spendPanel.SetActive(false);
            _sidebarUnit = u;
            bool yours = _match.You.LaneOf(u) >= 0 || _match.You.Bench.Contains(u);
            _sidebarSell.SetActive(yours && _match.CanSell(u));

            var d = u.Def;
            float scale = u.StatScale;
            var portrait = SpriteBank.UnitPortrait(d.SpriteKey);
            _sidebarPortrait.enabled = portrait != null;
            if (portrait != null)
            {
                _sidebarPortrait.sprite = portrait;
                _sidebarPortrait.color = d.Tint;
            }

            string items = u.Items.Count == 0 ? "<color=#8d81a8>none</color>" : "";
            foreach (var it in u.Items) items += $"\n  ◆ {it.Name} — {ItemDesc(it)}";

            string traits = $"<color=#F27EA9>{d.Origin}</color> · <color=#6EE7F0>{d.Class}</color>";
            if (d.Origin2.HasValue) traits = $"<color=#F27EA9>{d.Origin}</color> · <color=#F27EA9>{d.Origin2}</color> · <color=#6EE7F0>{d.Class}</color>";
            string sig = d.Sig != Signature.None
                ? $"\n<color=#FFD447>★ {d.Sig}</color>\n<color=#bdb2d4>{TraitInfo.Describe(d.Sig)}</color>"
                : "";

            _sidebarText.text =
                $"<size=22><b>{d.Name}</b></size>  <color=#FFD447>{new string('★', u.Star)}</color>\n" +
                $"<color=#8d81a8>cost {d.Cost}g · sells for {u.SellValue}g</color>\n" +
                $"{traits}{sig}\n\n" +
                $"<color=#6EE7F0>{d.Ability.Name}</color>\n<color=#bdb2d4>{d.Ability.Desc}</color>\n\n" +
                $"<color=#6EDB78>STATS</color>\n" +
                $"  HP {Mathf.RoundToInt(d.Hp * scale)}   AD {Mathf.RoundToInt(d.Ad * scale + u.BonusAd)}\n" +
                $"  AS {d.AttackSpeed:0.00}   Range {d.Range}   Armor {d.Armor}   Mana {d.ManaStart}/{d.ManaMax}\n\n" +
                $"<color=#FFD447>ITEMS</color>{items}";

            var sellLabel = _sidebarSell.GetComponentInChildren<Text>();
            if (sellLabel != null) sellLabel.text = $"SELL {u.SellValue}g (S)";

            Sfx.Play("Popup Open - 1", 0.4f);
            _sidebar.SetActive(true);
        }

        public void HideUnitSidebar()
        {
            if (_sidebar != null && _sidebar.activeSelf) Sfx.Play("Popup Close - 1", 0.35f);
            if (_sidebar != null) _sidebar.SetActive(false);
        }

        // ---------- kit picker ----------

        void BuildKitPicker(Transform canvas)
        {
            var panel = Glass("KitPicker", canvas, 0.78f);
            Anchor(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-620, -280), new Vector2(620, 280));
            ChromaEdge(panel, SpriteFactory.Gold, false, 0.8f);
            _kitPanel = panel.gameObject;

            var title = NewText("title", panel.transform, 20, SpriteFactory.Gold);
            Anchor(title.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -44), Vector2.zero);
            title.text = "CHOOSE A KIT FOR EACH LANE";
            var sub = NewText("sub", panel.transform, 12, Lavender);
            Anchor(sub.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -68), new Vector2(0, -44));
            sub.text = "hidden from your opponent until the walls drop at round 6";

            for (int lane = 0; lane < GameConfig.LaneCount; lane++)
            {
                float rowTop = -78 - lane * 116;
                var laneLabel = NewText($"lane{lane}", panel.transform, 14, SpriteFactory.LaneAccent[lane], TextAnchor.MiddleCenter);
                Anchor(laneLabel.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(14, rowTop - 108), new Vector2(126, rowTop));
                laneLabel.text = $"LANE {lane + 1}\n{SpriteFactory.LaneNames[lane]}";

                for (int k = 0; k < KitInfo.AllKits.Length; k++)
                {
                    var kit = KitInfo.AllKits[k];
                    int laneIdx = lane;

                    var fill = Glass($"kit_{lane}_{k}", panel.transform, 0.45f);
                    float x0 = 134 + k * 180;
                    Anchor(fill.rectTransform, new Vector2(0, 1), new Vector2(0, 1),
                        new Vector2(x0, rowTop - 108), new Vector2(x0 + 172, rowTop - 2));
                    var btn = fill.gameObject.AddComponent<Button>();
                    btn.targetGraphic = fill;
                    btn.onClick.AddListener(() => { Sfx.Play("Cursor - 1", 0.45f); _match.SelectKit(laneIdx, kit); });
                    var edge = ChromaEdge(fill, SpriteFactory.LaneAccent[lane], false, 0.55f);

                    var nameT = NewText("name", fill.transform, 13, CreamCol);
                    Anchor(nameT.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(8, -32), new Vector2(-8, -8));
                    nameT.text = kit.ToString().ToUpper();

                    var descT = NewText("desc", fill.transform, 10, Lavender, TextAnchor.UpperCenter, wrap: true);
                    Anchor(descT.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(12, 10), new Vector2(-12, -34));
                    descT.text = KitInfo.Describe(kit);

                    _kitButtons.Add((btn, edge, laneIdx, kit));
                }
            }
            _kitPanel.SetActive(false);
        }

        // ---------- banner & game over ----------

        void BuildBanner(Transform canvas)
        {
            _bannerText = NewText("Banner", canvas, 30, CreamCol);
            Anchor(_bannerText.rectTransform, new Vector2(0.5f, 0.74f), new Vector2(0.5f, 0.74f), new Vector2(-500, -40), new Vector2(500, 40));
            var outline = _bannerText.gameObject.AddComponent<Outline>();
            outline.effectColor = SpriteFactory.Line;
            outline.effectDistance = new Vector2(2, -2);
            _bannerText.text = "";
        }

        void BuildGameOver(Transform canvas)
        {
            var scrim = NewRect("GameOver", canvas).gameObject.AddComponent<Image>();
            scrim.color = new Color(0.04f, 0.035f, 0.08f, 0.9f);
            Anchor(scrim.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var panel = Glass("card", scrim.transform, 0.7f);
            Anchor(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-380, -150), new Vector2(380, 150));
            ChromaEdge(panel, SpriteFactory.Gold, false, 0.9f);

            _gameOverText = NewText("Result", panel.transform, 36, SpriteFactory.Gold);
            Anchor(_gameOverText.rectTransform, new Vector2(0, 0.5f), new Vector2(1, 0.95f), Vector2.zero, Vector2.zero);

            var btn = GlassButton("Restart", panel.transform, "PLAY AGAIN", 18, SpriteFactory.Gold, () =>
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex), out var restartLbl);
            Anchor(btn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), new Vector2(-130, -28), new Vector2(130, 28));

            _gameOverPanel = scrim.gameObject;
            _gameOverPanel.SetActive(false);
        }

        // ---------- refresh ----------

        void OnAnnouncement(int lane, string msg)
        {
            _bannerText.text = msg;
            _bannerUntil = Time.time + 2.2f;

            if (_match.Phase == Phase.GameOver)
            {
                _gameOverPanel.SetActive(true);
                _gameOverText.text = msg;
            }
        }

        void Refresh()
        {
            var you = _match.You;
            var foe = _match.Foe;

            _roundText.text = $"ROUND {_match.Round} · {PhaseName()}";
            _walletTexts[0].text = you.Iron.ToString();
            _walletTexts[1].text = you.Diamonds.ToString();
            _walletTexts[2].text = you.Emeralds.ToString();
            _walletTexts[3].text = you.Gold.ToString();
            _walletLevelText.text = you.Level >= GameConfig.MaxLevel
                ? $"LV {you.Level} MAX · cap {you.UnitCap}"
                : $"LV {you.Level} · {you.Xp}/{GameConfig.XpForLevel[you.Level + 1]}XP · cap {you.UnitCap}";

            for (int i = 0; i < GameConfig.LaneCount; i++)
            {
                var mine = you.Lanes[i];
                var theirs = foe.Lanes[i];
                string res = ResultMark(i);
                string pot = _match.LanePots[i] > 0 ? $" POT+{_match.LanePots[i]}" : "";
                string kit = mine.Kit != Kit.None ? $" · {mine.Kit.ToString().ToUpper()}" : "";

                string hp = !mine.Alive ? "ANCHOR LOST"
                    : _match.WallsUp ? $"{mine.Hp} vs ???"
                    : !theirs.Alive ? $"{mine.Hp} · UNCONTESTED"
                    : $"{mine.Hp} vs {theirs.Hp}";
                _laneRailHp[i].text = hp;
                _laneRailHp[i].color = mine.Alive ? CreamCol : new Color(0.55f, 0.5f, 0.65f);
                _laneRailHearts[i].color = mine.Alive
                    ? SpriteFactory.LaneAccent[i]
                    : new Color(0.3f, 0.28f, 0.38f);

                _laneRailTexts[i].text = $"L{i + 1} {SpriteFactory.LaneNames[i]}{kit}{res}{pot}";
                _laneRailTexts[i].color = mine.Alive ? Lavender : new Color(0.45f, 0.42f, 0.55f);

                // mini snapshot: your units on this lane
                var minis = _laneRailMinis[i];
                for (int m = 0; m < minis.Length; m++)
                {
                    if (m < mine.Units.Count)
                    {
                        var unit = mine.Units[m].Unit;
                        var portrait = SpriteBank.UnitPortrait(unit.Def.SpriteKey);
                        minis[m].enabled = portrait != null;
                        if (portrait != null)
                        {
                            minis[m].sprite = portrait;
                            minis[m].color = unit.Def.Tint;
                        }
                    }
                    else minis[m].enabled = false;
                }

                bool focused = _presenter != null && _presenter.FocusedLane == i;
                _laneRailFills[i].color = focused ? GlassFillLight : GlassFill;
                _laneRailEdges[i].Animate = focused;
                _laneRailEdges[i].SetVerticesDirty();
            }

            for (int i = 0; i < GameConfig.ShopSlots; i++)
            {
                string id = you.Shop[i];
                _shopButtons[i].interactable = id != null && _match.CanShop;
                if (id == null)
                {
                    _shopNames[i].text = "";
                    _shopBonds[i].text = "";
                    _shopCosts[i].text = "";
                    _shopPortraits[i].enabled = false;
                    _shopEdges[i].Animate = false;
                    _shopEdges[i].Accent = new Color(0.4f, 0.4f, 0.5f);
                    _shopEdges[i].SetVerticesDirty();
                    _shopFills[i].color = new Color(GlassFill.r, GlassFill.g, GlassFill.b, 0.25f);
                }
                else
                {
                    var def = UnitCatalog.Get(id);
                    int owned = 0;
                    foreach (var u in you.AllOwnedUnits()) if (u.Def == def && u.Star == 1) owned++;
                    _shopNames[i].text = def.Name;
                    _shopBonds[i].text = $"{def.Origin}·{def.Class}" + (owned > 0 ? $" ▲{owned}/3" : "");
                    _shopCosts[i].text = $"{def.Cost}g";
                    bool afford = you.Gold >= def.Cost;
                    _shopEdges[i].Animate = afford;
                    _shopEdges[i].Accent = TierColor(def.Cost);
                    _shopEdges[i].SetVerticesDirty();
                    _shopFills[i].color = afford ? GlassFill : new Color(GlassFill.r, GlassFill.g, GlassFill.b, 0.3f);

                    var portrait = SpriteBank.UnitPortrait(def.SpriteKey);
                    _shopPortraits[i].enabled = portrait != null;
                    if (portrait != null)
                    {
                        _shopPortraits[i].sprite = portrait;
                        _shopPortraits[i].color = def.Tint;
                    }
                }
            }

            _speedText.text = $"SPD x{_match.FightSpeed} (F)";

            RefreshBondTracker();

            bool showKits = _match.Phase == Phase.Planning && _match.Round == 1 && !you.KitsChosen;
            _kitPanel.SetActive(showKits);
            if (showKits)
            {
                foreach (var (btn, edge, lane, kit) in _kitButtons)
                {
                    bool takenElsewhere = false;
                    foreach (var l in you.Lanes)
                        if (l.Index != lane && l.Kit == kit) takenElsewhere = true;
                    btn.interactable = !takenElsewhere;
                    bool chosen = you.Lanes[lane].Kit == kit;
                    edge.Animate = chosen;
                    edge.enabled = !takenElsewhere; // taken kits lose their outline entirely
                    edge.SetVerticesDirty();
                }
            }

            if (_spendPanel.activeSelf)
            {
                foreach (var (btn, price, edge, refresh) in _marketTiles)
                {
                    var (txt, afford) = refresh();
                    price.text = txt;
                    btn.interactable = afford && _match.CanAct;
                    price.color = afford ? CreamCol : new Color(0.55f, 0.5f, 0.65f);
                    edge.Animate = afford;
                    edge.SetVerticesDirty();
                }
            }
        }

        /// <summary>True when a screen point is over the shop bar (drag a unit here to sell it).</summary>
        public bool IsOverShopBar(Vector2 screenPos) =>
            _shopBarRect != null &&
            RectTransformUtility.RectangleContainsScreenPoint(_shopBarRect, screenPos, null);

        string ResultMark(int lane)
        {
            var r = _match.RoundResults[lane];
            if (r == null) return _match.FightingLane == lane ? " · ..." : "";
            if (r == FightResult.Draw) return " · =";
            return r == FightResult.SideAWins ? " · WIN" : " · LOSS";
        }

        string PhaseName()
        {
            string phase;
            switch (_match.Phase)
            {
                case Phase.Planning: phase = "PLAN"; break;
                case Phase.Reveal: phase = "REVEAL"; break;
                case Phase.Fighting: phase = $"FIGHT L{_match.FightingLane + 1}"; break;
                case Phase.Resolve: phase = "RESOLVE"; break;
                default: phase = "GAME OVER"; break;
            }
            return _match.WallsUp ? $"WALLS · {phase}" : phase;
        }

        void Update()
        {
            if (_match == null) return;
            _timerText.text = _match.Phase == Phase.Planning
                ? Mathf.CeilToInt(Mathf.Max(0, _match.PlanningTimeLeft)).ToString()
                : "";
            _timerText.color = _match.PlanningTimeLeft < 5f && _match.Phase == Phase.Planning
                ? SpriteFactory.HpRed : SpriteFactory.Gold;

            // TFT-style round bell: chime at 5s, tick each second, rising tick on the last
            if (_match.Phase == Phase.Planning)
            {
                int sec = Mathf.CeilToInt(Mathf.Max(0f, _match.PlanningTimeLeft));
                if (sec <= 5 && sec >= 1 && sec != _lastTickSecond)
                {
                    _lastTickSecond = sec;
                    if (sec == 5) Sfx.Play("Popup Open - 1", 0.65f, 1.5f);      // the bell
                    Sfx.Play("Cursor - 5", 0.5f, sec == 1 ? 1.35f : 1f);        // the tick
                }
                if (sec > 5) _lastTickSecond = -1;
            }
            else if (_lastPhase == Phase.Planning || _lastPhase == Phase.Reveal)
            {
                if (_match.Phase == Phase.Fighting) Sfx.Play("Select - 2", 0.6f, 0.9f); // battle begins
                _lastTickSecond = -1;
            }
            _lastPhase = _match.Phase;

            if (_bannerText.text.Length > 0 && Time.time > _bannerUntil && _match.Phase != Phase.GameOver)
                _bannerText.text = "";
        }
    }
}

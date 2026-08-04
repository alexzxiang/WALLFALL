using UnityEngine;
using UnityEngine.InputSystem;

namespace Wallfall
{
    /// <summary>
    /// Planning-phase input: drag your units between bench and the focused board
    /// (transfers validated/priced by MatchController). Hover highlights the tile under a unit,
    /// right-click opens the TFT-style detail sidebar, S sells the hovered unit,
    /// 1-4 / Q / E switch lanes, R reroll, X buy XP, T market, Enter ready.
    /// </summary>
    public class DragController : MonoBehaviour
    {
        public MatchController Match;
        public BoardsPresenter Presenter;
        public Camera Cam;
        public Hud Hud;

        /// <summary>When set (by the spend panel), the next unit click is consumed by this instead of dragging.</summary>
        public System.Func<UnitInstance, bool> PendingUnitClick;

        UnitView _dragging;
        UnitView _hovered;
        Vector3 _dragOrigin;

        Vector3 MouseWorld()
        {
            Vector2 sp = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            var w = Cam.ScreenToWorldPoint(new Vector3(sp.x, sp.y, 10f));
            w.z = 0f;
            return w;
        }

        void Update()
        {
            HandleKeys();

            if (Match == null || Mouse.current == null) return;

            UpdateHover();

            // right-click: TFT-style unit detail sidebar (any unit, any phase)
            if (Mouse.current.rightButton.wasPressedThisFrame && _dragging == null && Hud != null)
            {
                if (_hovered != null) Hud.ShowUnitSidebar(_hovered.Unit);
                else Hud.HideUnitSidebar();
            }

            // clicking anywhere outside the detail sidebar dismisses it — in every phase
            if (Mouse.current.leftButton.wasPressedThisFrame && Hud != null && Hud.IsSidebarOpen)
            {
                if (!Hud.IsOverSidebar(Mouse.current.position.ReadValue()))
                    Hud.HideUnitSidebar();
                else
                    return; // let the sidebar's own buttons handle the click
            }

            if (!Match.CanAct)
            {
                CancelDrag();
                return;
            }

            Vector3 world = MouseWorld();

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (PendingUnitClick != null)
                {
                    var target = Presenter.PickYourUnit(world);
                    if (target != null) PendingUnitClick(target.Unit);
                    PendingUnitClick = null;
                    return;
                }
                _dragging = Presenter.PickYourUnit(world);
                if (_dragging != null)
                {
                    _dragOrigin = _dragging.transform.position;
                    if (Hud != null) Hud.ShowSellOverlay(_dragging.Unit);
                }
            }

            if (_dragging != null)
            {
                _dragging.SetPosition(world, instant: true);
                if (Hud != null) Hud.SetSellHover(Hud.IsOverShopBar(Mouse.current.position.ReadValue()));

                var board = Presenter.Boards[Presenter.FocusedLane];
                board.HighlightCell(board.YourCellFromWorld(world));

                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    board.HighlightCell(null);
                    if (Hud != null) Hud.HideSellOverlay();

                    // TFT gesture: drop a unit on the shop bar to sell it
                    if (Hud != null && Hud.IsOverShopBar(Mouse.current.position.ReadValue()))
                    {
                        Match.SellUnit(_dragging.Unit);
                        Sfx.Play("Select - 2", 0.55f);
                        _dragging = null;
                        Presenter.Refresh();
                        return;
                    }

                    var cell = board.YourCellFromWorld(world);
                    int? benchSlot = Presenter.BenchSlotFromWorld(world);

                    bool moved = false;
                    if (cell.HasValue)
                        moved = Match.MoveUnit(_dragging.Unit, Presenter.FocusedLane, cell.Value.x, cell.Value.y);
                    else if (benchSlot.HasValue)
                        moved = Match.MoveUnit(_dragging.Unit, -1, 0, 0);

                    if (!moved) _dragging.SetPosition(_dragOrigin, instant: true);
                    _dragging = null;
                    Presenter.Refresh();
                }
            }
        }

        void HandleKeys()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.digit1Key.wasPressedThisFrame) Presenter.FocusLane(0);
            if (kb.digit2Key.wasPressedThisFrame) Presenter.FocusLane(1);
            if (kb.digit3Key.wasPressedThisFrame) Presenter.FocusLane(2);
            if (kb.digit4Key.wasPressedThisFrame) Presenter.FocusLane(3);
            if (kb.qKey.wasPressedThisFrame) Presenter.FocusLane((Presenter.FocusedLane + 3) % 4);
            if (kb.eKey.wasPressedThisFrame) Presenter.FocusLane((Presenter.FocusedLane + 1) % 4);

            if (kb.fKey.wasPressedThisFrame && Match != null) Match.ToggleFightSpeed();

            // scroll wheel cycles lanes (unless the market is open — it scrolls its own grid)
            var mouse = Mouse.current;
            if (mouse != null && (Hud == null || !Hud.IsMarketOpen))
            {
                float scroll = mouse.scroll.ReadValue().y;
                if (scroll > 0.01f) Presenter.FocusLane((Presenter.FocusedLane + 3) % 4);
                else if (scroll < -0.01f) Presenter.FocusLane((Presenter.FocusedLane + 1) % 4);
            }

            if (Match == null) return;

            // shop economy hotkeys work through fights, TFT-style
            if (Match.CanShop)
            {
                if (kb.rKey.wasPressedThisFrame) Match.Reroll();
                if (kb.xKey.wasPressedThisFrame) Match.BuyXp();
                if (kb.sKey.wasPressedThisFrame && _hovered != null && _hovered.IsYours) Match.SellUnit(_hovered.Unit);
            }
            if (kb.tKey.wasPressedThisFrame && Hud != null) Hud.ToggleSpendPanel();
            if (kb.escapeKey.wasPressedThisFrame && Hud != null) { Hud.HideUnitSidebar(); PendingUnitClick = null; }
            if (Match.CanAct && kb.enterKey.wasPressedThisFrame) Match.PlayerReady();
        }

        void CancelDrag()
        {
            if (_dragging == null) return;
            if (Hud != null) Hud.HideSellOverlay();
            _dragging.SetPosition(_dragOrigin, instant: true);
            _dragging = null;
            Presenter.Refresh();
        }

        void UpdateHover()
        {
            _hovered = _dragging == null && Match.Phase != Phase.GameOver
                ? Presenter.PickAnyUnit(MouseWorld())
                : null;
            Presenter.SetHoveredUnit(_hovered);
        }
    }
}

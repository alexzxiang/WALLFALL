using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wallfall
{
    /// <summary>
    /// Owns the four BoardViews, the bench row, the lane-focus camera, and keeps unit views
    /// in sync with what the player is ALLOWED to see:
    ///   rounds 1-5 planning -> walls (nothing), War planning -> frozen last-fight snapshot,
    ///   reveal/fights/resolve -> live. Your own fresh moves render ghosted until reveal.
    /// </summary>
    public class BoardsPresenter : MonoBehaviour
    {
        public MatchController Match;
        public Camera Cam;

        public BoardView[] Boards { get; private set; }
        public int FocusedLane { get; private set; }

        readonly Dictionary<int, UnitView> _views = new Dictionary<int, UnitView>();
        readonly List<UnitView> _snapViews = new List<UnitView>();
        Transform _benchRoot;
        Vector3[] _benchSlots;
        Vector3 _shake;

        public event System.Action FocusChanged;

        public void Build(MatchController match, Camera cam)
        {
            Match = match;
            Cam = cam;

            Boards = new BoardView[GameConfig.LaneCount];
            for (int i = 0; i < GameConfig.LaneCount; i++)
            {
                var go = new GameObject($"Board_L{i + 1}");
                go.transform.SetParent(transform, false);
                Boards[i] = go.AddComponent<BoardView>();
                Boards[i].Build(i);
            }

            _benchRoot = new GameObject("Bench").transform;
            _benchRoot.SetParent(transform, false);
            _benchSlots = new Vector3[GameConfig.BenchSize + 4];

            Match.StateChanged += Refresh;
            Match.FightStarted += OnFightStarted;
            Match.BigMoment += () => StartCoroutine(Shake());

            FocusLane(0, instant: true);
            Refresh();
        }

        public void FocusLane(int lane, bool instant = false)
        {
            int prev = FocusedLane;
            FocusedLane = Mathf.Clamp(lane, 0, GameConfig.LaneCount - 1);
            if (!instant && prev != FocusedLane) Sfx.Play("Swipe - 1", 0.5f);
            if (Match != null) Match.OnLaneViewed(FocusedLane);
            LayoutBench();
            RefreshBenchPositions();
            FocusChanged?.Invoke();
        }

        void LayoutBench()
        {
            var board = Boards[FocusedLane];
            // sits between the board's bottom row and the shop bar — never behind it
            Vector3 origin = board.CellWorld(0, 0) + new Vector3(-0.35f, -1.28f, 0f);
            for (int i = 0; i < _benchSlots.Length; i++)
                _benchSlots[i] = origin + new Vector3(i * 1.25f, 0f, 0f);
            _benchRoot.position = origin;

            if (_benchMarkers == null)
            {
                // a proper bench: backdrop strip + label + big slot outlines
                var strip = new GameObject("benchStrip");
                strip.transform.SetParent(_benchRoot, false);
                strip.transform.localPosition = new Vector3((GameConfig.BenchSize - 1) * 1.25f * 0.5f, 0f, 0f);
                var stripSr = strip.AddComponent<SpriteRenderer>();
                stripSr.sprite = SpriteFactory.Square;
                stripSr.color = new Color(0.06f, 0.05f, 0.1f, 0.55f);
                stripSr.sortingOrder = -4;
                stripSr.transform.localScale = new Vector3(GameConfig.BenchSize * 1.25f + 0.6f, 1.18f, 1f);

                var lbl = new GameObject("benchLabel");
                lbl.transform.SetParent(_benchRoot, false);
                lbl.transform.localPosition = new Vector3(-1.05f, 0f, 0f);
                var tm = lbl.AddComponent<TextMesh>();
                var pixelFont = SpriteBank.UiFont();
                if (pixelFont != null) { tm.font = pixelFont; lbl.GetComponent<MeshRenderer>().material = pixelFont.material; }
                tm.text = "BENCH";
                tm.characterSize = 0.1f;
                tm.fontSize = 30;
                tm.anchor = TextAnchor.MiddleCenter;
                tm.alignment = TextAlignment.Center;
                lbl.transform.Rotate(0f, 0f, 90f);
                tm.color = new Color(0.95f, 0.92f, 0.85f, 0.7f);
                lbl.GetComponent<MeshRenderer>().sortingOrder = -3;

                _benchMarkers = new SpriteRenderer[GameConfig.BenchSize];
                for (int i = 0; i < GameConfig.BenchSize; i++)
                {
                    var go = new GameObject($"benchSlot{i}");
                    go.transform.SetParent(_benchRoot, false);
                    go.transform.localPosition = new Vector3(i * 1.25f, 0f, 0f);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = SpriteFactory.HexOutline;
                    sr.color = new Color(1f, 1f, 1f, 0.4f);
                    sr.sortingOrder = -3;
                    sr.transform.localScale = Vector3.one * 0.95f;
                    _benchMarkers[i] = sr;
                }
            }
        }
        SpriteRenderer[] _benchMarkers;

        UnitView _hovered;

        /// <summary>Gold hex outline under the hovered unit — board or bench.</summary>
        public void SetHoveredUnit(UnitView v)
        {
            if (_hovered == v) return;
            _hovered = v;
            Boards[FocusedLane].SetHoverOutline(v != null ? (Vector3?)v.transform.position : null);
        }

        void UpdateHoverOutline()
        {
            Boards[FocusedLane].SetHoverOutline(
                _hovered != null && _hovered.gameObject.activeSelf ? (Vector3?)_hovered.transform.position : null);
        }

        public Vector3 BenchSlotWorld(int i) => _benchSlots[Mathf.Clamp(i, 0, _benchSlots.Length - 1)];

        public int? BenchSlotFromWorld(Vector3 world, float maxDist = 0.5f)
        {
            for (int i = 0; i < GameConfig.BenchSize; i++)
                if (Vector2.Distance(world, _benchSlots[i]) < maxDist) return i;
            return null;
        }

        IEnumerator Shake()
        {
            float t = 0f;
            while (t < 0.35f)
            {
                t += Time.deltaTime;
                float amp = Mathf.Lerp(0.22f, 0f, t / 0.35f);
                _shake = new Vector3(Random.Range(-amp, amp), Random.Range(-amp, amp), 0f);
                yield return null;
            }
            _shake = Vector3.zero;
        }

        void LateUpdate()
        {
            var target = Boards[FocusedLane].Center + new Vector3(0f, -1.3f, -10f);
            Cam.transform.position = Vector3.Lerp(Cam.transform.position, target, 10f * Time.deltaTime) + _shake;

            UpdateHoverOutline();

            if (Match.Phase == Phase.Fighting && Match.CurrentFight != null)
                SyncFight();
        }

        void OnFightStarted(int lane)
        {
            FocusLane(lane);
            // ensure views exist AND are alive/visible for everything in the fight
            foreach (var cu in Match.CurrentFight.Units)
            {
                var v = GetView(cu.Source, cu.Side == 0);
                v.ResetBars();      // reactivates views left inactive by a death in an earlier fight
                v.SetTint(1f, false);
                v.SetPosition(Boards[lane].LocalToWorld(cu.Pos), instant: true);
                v.SetFightMode(true);
            }
        }

        /// <summary>Full model->view sync respecting the fog rules.</summary>
        public void Refresh()
        {
            var seen = new HashSet<int>();
            bool planning = Match.Phase == Phase.Planning;
            bool wallsVisual = Match.WallsUp && planning;
            // NEVER show the opponent's units during the Walls phase — not even during fights.
            // (They were rendering on the enemy half mid-PvE, stacked inside the creeps.)
            bool foeLive = Match.FoeBoardsLive && !Match.WallsUp;

            // units in the running fight belong to SyncFight — don't reposition or resurrect them here
            var inFight = new HashSet<int>();
            if (Match.CurrentFight != null)
                foreach (var cu in Match.CurrentFight.Units) inFight.Add(cu.Source.Id);

            // snapshot ghosts are rebuilt from scratch every refresh
            foreach (var v in _snapViews) if (v != null) Destroy(v.gameObject);
            _snapViews.Clear();

            for (int lane = 0; lane < GameConfig.LaneCount; lane++)
            {
                var board = Boards[lane];
                board.SetWalls(wallsVisual);
                board.UpdateState(Match.You.Lanes[lane], Match.Foe.Lanes[lane], false);
                board.SetLastSeen(Match.SnapshotRound, foeLive || Match.WallsUp);
                board.SetKitLabel(Match.You.Lanes[lane].Kit, Match.Foe.Lanes[lane].Kit, !Match.WallsUp);

                foreach (var p in Match.You.Lanes[lane].Units)
                {
                    seen.Add(p.Unit.Id);
                    if (inFight.Contains(p.Unit.Id)) continue;
                    var v = GetView(p.Unit, true);
                    v.ResetBars();
                    v.RefreshItems();
                    v.SetTint(1f, false);
                    v.SetPosition(board.YourCellWorld(p.Col, p.Row));
                }

                if (foeLive)
                {
                    foreach (var p in Match.Foe.Lanes[lane].Units)
                    {
                        seen.Add(p.Unit.Id);
                        if (inFight.Contains(p.Unit.Id)) continue;
                        var v = GetView(p.Unit, false);
                        v.ResetBars();
                        v.RefreshItems();
                        v.SetTint(1f, false);
                        v.SetPosition(board.FoeCellWorld(p.Col, p.Row));
                    }
                }
                else if (!Match.WallsUp)
                {
                    // frozen last-fight snapshot
                    foreach (var snap in Match.FoeSnapshot[lane])
                    {
                        var dummy = new UnitInstance(snap.Def) { Star = snap.Star };
                        var go = new GameObject($"Snap_{snap.Def.Name}");
                        go.transform.SetParent(transform, false);
                        var v = go.AddComponent<UnitView>();
                        v.Setup(dummy, false);
                        v.SetTint(0.85f, frozen: true);
                        v.SetPosition(board.FoeCellWorld(snap.Col, snap.Row), instant: true);
                        _snapViews.Add(v);
                    }
                }
            }

            RefreshBenchPositions();
            foreach (var u in Match.You.Bench) seen.Add(u.Id);

            // never delete views that belong to a running fight (PvE creeps live only there)
            if (Match.CurrentFight != null)
                foreach (var cu in Match.CurrentFight.Units) seen.Add(cu.Source.Id);

            foreach (var kv in _views.Where(kv => !seen.Contains(kv.Key)).ToList())
            {
                Destroy(kv.Value.gameObject);
                _views.Remove(kv.Key);
            }
            foreach (var v in _views.Values) { v.RefreshStars(); v.RefreshItems(); }
        }

        void RefreshBenchPositions()
        {
            for (int i = 0; i < Match.You.Bench.Count && i < _benchSlots.Length; i++)
            {
                var v = GetView(Match.You.Bench[i], true);
                v.ResetBars();
                v.RefreshItems();
                v.SetTint(1f, false);
                v.SetPosition(_benchSlots[i]);
            }
        }

        void SyncFight()
        {
            var sim = Match.CurrentFight;
            var board = Boards[Match.FightingLane];
            foreach (var cu in sim.Units)
            {
                // recreate on the fly if anything destroyed the view mid-fight
                var v = GetView(cu.Source, cu.Side == 0);
                v.SetFightMode(true);
                v.SetPosition(board.LocalToWorld(cu.Pos));
                v.SetCombat(cu);
            }
        }

        public UnitView GetView(UnitInstance u, bool isYours)
        {
            if (_views.TryGetValue(u.Id, out var v)) return v;
            var go = new GameObject($"Unit_{u.Def.Name}_{u.Id}");
            go.transform.SetParent(transform, false);
            v = go.AddComponent<UnitView>();
            v.Setup(u, isYours);
            _views[u.Id] = v;
            return v;
        }

        public UnitView PickYourUnit(Vector3 world, float maxDist = 0.45f)
        {
            UnitView best = null; float bestD = maxDist;
            foreach (var v in _views.Values)
            {
                if (!v.IsYours || !v.gameObject.activeSelf) continue;
                int lane = Match.You.LaneOf(v.Unit);
                bool onBench = lane < 0 && Match.You.Bench.Contains(v.Unit);
                if (lane != FocusedLane && !onBench) continue;
                float d = Vector2.Distance(world, v.transform.position);
                if (d < bestD) { bestD = d; best = v; }
            }
            return best;
        }

        /// <summary>Any visible unit near a world point — yours, enemy, or frozen snapshot (for tooltips).</summary>
        public UnitView PickAnyUnit(Vector3 world, float maxDist = 0.5f)
        {
            UnitView best = PickYourUnit(world, maxDist);
            float bestD = best != null ? Vector2.Distance(world, best.transform.position) : maxDist;

            foreach (var v in _views.Values)
            {
                if (v.IsYours || !v.gameObject.activeSelf) continue;
                float d = Vector2.Distance(world, v.transform.position);
                if (d < bestD) { bestD = d; best = v; }
            }
            foreach (var v in _snapViews)
            {
                if (v == null || !v.gameObject.activeSelf) continue;
                float d = Vector2.Distance(world, v.transform.position);
                if (d < bestD) { bestD = d; best = v; }
            }
            return best;
        }
    }
}

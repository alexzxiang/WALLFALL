using UnityEngine;

namespace Wallfall
{
    /// <summary>
    /// Renders one lane's full 6x5 hex board: biome backdrop, tiles (enemy half frozen-tinted),
    /// center seam, bed markers. Converts between world space and board cells.
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        public int LaneIndex { get; private set; }

        SpriteRenderer[,] _tiles;
        SpriteRenderer _backdrop;
        TextMesh _label, _lastSeen, _kitText;
        bool _wallsUp;

        public const float LaneSpacing = 40f; // far enough apart that neighbor backdrops never bleed into view
        const int Rows = GameConfig.BoardRowsPerSide * 2;
        const int Cols = GameConfig.BoardCols;

        public static Vector3 LaneOrigin(int lane) => new Vector3(lane * LaneSpacing, 0f, 0f);

        /// <summary>Board-local combat position (continuous) to world.</summary>
        public Vector3 LocalToWorld(Vector2 local) => transform.position + new Vector3(local.x, local.y, 0f);

        public Vector3 CellWorld(int col, int row)
        {
            Vector2 p = HexUtil.ToWorld(col, row);
            return transform.position + new Vector3(p.x, p.y, 0f);
        }

        /// <summary>World position for a unit on YOUR half using your-half coords (rows 0-2).</summary>
        public Vector3 YourCellWorld(int col, int row) => CellWorld(col, row);

        /// <summary>World position for an ENEMY unit given their your-half coords (mirrored to rows 3-5).</summary>
        public Vector3 FoeCellWorld(int col, int row) =>
            CellWorld(Cols - 1 - col, Rows - 1 - row);

        public Vector3 Center => transform.position + new Vector3(
            (Cols - 0.5f) * HexUtil.Width * 0.5f,
            (Rows - 1) * HexUtil.RowStep * 0.5f, 0f);

        /// <summary>Nearest your-half cell to a world point, or null if too far.</summary>
        public Vector2Int? YourCellFromWorld(Vector3 world, float maxDist = 0.45f)
        {
            Vector2Int best = default; float bestD = maxDist; bool found = false;
            for (int r = 0; r < GameConfig.BoardRowsPerSide; r++)
                for (int c = 0; c < Cols; c++)
                {
                    float d = Vector2.Distance(world, CellWorld(c, r));
                    if (d < bestD) { bestD = d; best = new Vector2Int(c, r); found = true; }
                }
            return found ? best : (Vector2Int?)null;
        }

        public void Build(int laneIndex)
        {
            LaneIndex = laneIndex;
            transform.position = LaneOrigin(laneIndex);

            // biome backdrop: real parallax layers when available, flat color fallback
            _backdrop = NewSprite("backdrop", SpriteFactory.Square, SpriteFactory.LaneBiome[laneIndex], -12);
            _backdrop.transform.localPosition = Center - transform.position;
            _backdrop.transform.localScale = new Vector3(26f, 16f, 1f);

            var bgLayers = SpriteBank.LaneBackground(laneIndex);
            if (bgLayers != null)
            {
                for (int i = 0; i < bgLayers.Length; i++)
                {
                    var layer = NewSprite($"bg{i}", bgLayers[i], new Color(0.86f, 0.84f, 0.94f, 1f), -11 + i);
                    layer.transform.position = Center + new Vector3(0f, 0.6f - i * 0.4f, 0f);
                    // cover the visible area regardless of source aspect (padded for drift)
                    float w = bgLayers[i].bounds.size.x;
                    layer.transform.localScale = Vector3.one * Mathf.Max(1f, 28f / w);
                    // nearer layers drift more — a slow living parallax
                    var drift = layer.gameObject.AddComponent<AmbientDrift>();
                    drift.Amplitude = 0.25f + 0.3f * i;
                    drift.Speed = 0.02f + 0.012f * i;
                    drift.Phase = laneIndex * 1.7f + i;
                }
            }

            // floating light specks tinted to the lane
            var specks = new GameObject("specks");
            specks.transform.SetParent(transform, false);
            var amb = specks.AddComponent<AmbientSpecks>();
            amb.Tint = SpriteFactory.LaneAccent[laneIndex];
            amb.Center = Center + new Vector3(0f, 0.5f, 0f);
            amb.Width = 9f; amb.Height = 8f;

            _tiles = new SpriteRenderer[Cols, Rows];
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                {
                    var tile = NewSprite($"hex_{c}_{r}", SpriteFactory.HexFill, TileColor(r), -5);
                    tile.transform.position = CellWorld(c, r);
                    tile.transform.localScale = Vector3.one * 0.92f; // TFT-style visible gaps between hexes
                    _tiles[c, r] = tile;
                }

            // seam
            var seam = NewSprite("seam", SpriteFactory.Square, SpriteFactory.LaneAccent[laneIndex], -4);
            float seamY = (GameConfig.BoardRowsPerSide - 0.5f) * HexUtil.RowStep + 0.13f;
            seam.transform.localPosition = new Vector3(Center.x - transform.position.x, seamY, 0f);
            seam.transform.localScale = new Vector3(6.2f, 0.06f, 1f);
            var seamCol = SpriteFactory.LaneAccent[laneIndex]; seamCol.a = 0.55f;
            seam.color = seamCol;


            float topY = (Rows - 1) * HexUtil.RowStep + HexUtil.Size + transform.position.y;
            // single combined line kept BELOW the round chip so nothing hides behind it
            _label = NewText("label",
                new Vector3(Center.x, topY + 0.12f, 0f), 0.6f,
                SpriteFactory.LaneAccent[laneIndex]);
            _label.text = $"L{laneIndex + 1} · {SpriteFactory.LaneNames[laneIndex]}";
            _kitText = _label; // kit info merges into the label line

            _lastSeen = NewText("lastSeen",
                CellWorld(Cols - 1, Rows - 1) + new Vector3(0.4f, 0.55f, 0f), 0.5f, SpriteFactory.Frozen);
            _lastSeen.text = "";
        }

        /// <summary>Rounds 1-5: the enemy half is a wall. No information at all.</summary>
        public void SetWalls(bool up)
        {
            if (_wallsUp == up) return;
            _wallsUp = up;
            var brick = SpriteFactory.Hex("#463A5C");
            for (int r = GameConfig.BoardRowsPerSide; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    _tiles[c, r].color = up ? brick : TileColor(r);
            if (up) _lastSeen.text = "THE WALLS";
        }

        public void SetLastSeen(int snapshotRound, bool live)
        {
            if (_wallsUp) return;
            _lastSeen.text = live ? "" : $"LAST SEEN R{snapshotRound}";
        }

        public void SetKitLabel(Kit yours, Kit theirs, bool theirsVisible)
        {
            string a = yours == Kit.None ? "?" : yours.ToString().ToUpper();
            string b = !theirsVisible ? "?" : theirs == Kit.None ? "?" : theirs.ToString().ToUpper();
            _label.text = $"L{LaneIndex + 1} {SpriteFactory.LaneNames[LaneIndex]}  ·  {a} vs {b}";
        }

        Color TileColor(int row)
        {
            bool enemyHalf = row >= GameConfig.BoardRowsPerSide;
            // sprite = crisp white outline + translucent fill; the tint sets the whole look
            return enemyHalf
                ? new Color(0.55f, 0.65f, 0.85f, 0.85f)
                : new Color(0.96f, 0.93f, 0.86f, 0.9f);
        }

        public void UpdateState(LaneState yours, LaneState foes, bool revealFresh)
        {
            float dim = yours.Alive || foes.Alive ? 1f : 0.5f;
            _backdrop.color = SpriteFactory.LaneBiome[LaneIndex] * dim;
        }

        SpriteRenderer _hover;

        /// <summary>Animated chroma outline over the hex a hovered unit stands on (null hides it).</summary>
        public void SetHoverOutline(Vector3? worldPos)
        {
            if (_hover == null)
                _hover = NewSprite("hover", SpriteFactory.HexOutline, SpriteFactory.Gold, -2);
            _hover.enabled = worldPos.HasValue;
            if (worldPos.HasValue)
            {
                _hover.transform.position = worldPos.Value;
                // slow hue drift + breathing scale — reads alive, not static
                _hover.color = Color.HSVToRGB(Mathf.Repeat(Time.time * 0.2f, 1f), 0.45f, 1f);
                _hover.transform.localScale = Vector3.one * (1.05f + 0.05f * Mathf.Sin(Time.time * 5f));
            }
        }

        public void HighlightCell(Vector2Int? cell)
        {
            for (int r = 0; r < GameConfig.BoardRowsPerSide; r++)
                for (int c = 0; c < Cols; c++)
                    _tiles[c, r].color = TileColor(r);
            if (cell.HasValue)
                _tiles[cell.Value.x, cell.Value.y].color = SpriteFactory.Gold;
        }

        SpriteRenderer NewSprite(string name, Sprite sprite, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }

        TextMesh NewText(string name, Vector3 worldPos, float size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.position = worldPos;
            var tm = go.AddComponent<TextMesh>();
            var pixelFont = SpriteBank.UiFont();
            if (pixelFont != null)
            {
                tm.font = pixelFont;
                go.GetComponent<MeshRenderer>().material = pixelFont.material;
            }
            tm.text = "";
            tm.characterSize = 0.1f;
            tm.fontSize = Mathf.RoundToInt(size * 78);
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = color;
            var mr = go.GetComponent<MeshRenderer>();
            mr.sortingOrder = 20;
            return tm;
        }
    }
}

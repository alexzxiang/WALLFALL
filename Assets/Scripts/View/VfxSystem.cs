using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wallfall
{
    /// <summary>
    /// Lightweight combat VFX driven from the sim's per-tick flags:
    /// skill-icon pops on cast, arrow projectiles for ranged attacks,
    /// dust puffs on death, gem bursts on bed break. All itch.io assets with safe fallbacks.
    /// </summary>
    public class VfxSystem : MonoBehaviour
    {
        BoardsPresenter _presenter;
        MatchController _match;
        readonly HashSet<int> _aliveLastTick = new HashSet<int>();

        public void Build(MatchController match, BoardsPresenter presenter)
        {
            _match = match;
            _presenter = presenter;
            _match.FightStarted += _ => _aliveLastTick.Clear();
            _match.LaneDied += OnLaneDied;
        }

        void LateUpdate()
        {
            var sim = _match != null ? _match.CurrentFight : null;
            if (sim == null || _match.FightingLane < 0) return;
            var board = _presenter.Boards[_match.FightingLane];

            foreach (var cu in sim.Units)
            {
                Vector3 pos = board.LocalToWorld(cu.Pos);

                if (cu.JustCast)
                {
                    StartCoroutine(IconPop(SpriteBank.SkillIcon(cu.Source.Def.Ability), pos + Vector3.up * 0.75f));
                    StartCoroutine(CastRing(pos));
                }

                if (cu.JustHit)
                    StartCoroutine(HitSpark(pos));

                if (cu.JustAttacked && cu.Range >= 3)
                {
                    var target = NearestEnemyPos(sim, cu, board);
                    if (target.HasValue) StartCoroutine(ArrowShot(pos + Vector3.up * 0.25f, target.Value + Vector3.up * 0.25f));
                }

                bool wasAlive = _aliveLastTick.Contains(cu.Source.Id);
                if (wasAlive && !cu.Alive)
                    StartCoroutine(DustPuff(pos));
            }

            _aliveLastTick.Clear();
            foreach (var cu in sim.Units) if (cu.Alive) _aliveLastTick.Add(cu.Source.Id);
        }

        Vector3? NearestEnemyPos(CombatSim sim, CombatUnit from, BoardView board)
        {
            CombatUnit best = null; float bestD = float.MaxValue;
            foreach (var o in sim.Units)
            {
                if (!o.Alive || o.Side == from.Side) continue;
                float d = Vector2.Distance(from.Pos, o.Pos);
                if (d < bestD) { bestD = d; best = o; }
            }
            return best == null ? (Vector3?)null : board.LocalToWorld(best.Pos);
        }

        void OnLaneDied(int lane, bool yours)
        {
            var board = _presenter.Boards[lane];
            Vector3 pos = yours
                ? board.CellWorld(0, 0) + new Vector3(-1.15f, 0f, 0f)
                : board.CellWorld(GameConfig.BoardCols - 1, GameConfig.BoardRowsPerSide * 2 - 1) + new Vector3(1.15f, 0f, 0f);
            StartCoroutine(GemBurst(pos, SpriteFactory.LaneAccent[lane]));
        }

        SpriteRenderer Spawn(Sprite sprite, Vector3 pos, Color color, int order = 30)
        {
            var go = new GameObject("vfx");
            go.transform.SetParent(transform, false);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }

        IEnumerator CastRing(Vector3 pos)
        {
            var sr = Spawn(SpriteFactory.Circle, pos, new Color(1f, 1f, 1f, 0.55f), 8);
            float t = 0f;
            while (t < 0.32f)
            {
                t += Time.deltaTime;
                float k = t / 0.32f;
                sr.transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 1.6f, k);
                var c = sr.color; c.a = 0.55f * (1f - k); sr.color = c;
                yield return null;
            }
            Destroy(sr.gameObject);
        }

        IEnumerator HitSpark(Vector3 pos)
        {
            for (int i = 0; i < 2; i++)
            {
                var sr = Spawn(SpriteFactory.Square, pos + (Vector3)(Random.insideUnitCircle * 0.15f),
                    Random.value > 0.5f ? SpriteFactory.Gold : SpriteFactory.HpRed, 31);
                sr.transform.localScale = Vector3.one * Random.Range(0.06f, 0.11f);
                StartCoroutine(SparkFly(sr));
            }
            yield break;
        }

        IEnumerator SparkFly(SpriteRenderer sr)
        {
            Vector3 vel = new Vector3(Random.Range(-1.2f, 1.2f), Random.Range(0.6f, 1.8f), 0f);
            float t = 0f;
            while (t < 0.28f)
            {
                t += Time.deltaTime;
                vel += Vector3.down * (7f * Time.deltaTime);
                sr.transform.position += vel * Time.deltaTime;
                var c = sr.color; c.a = 1f - t / 0.28f; sr.color = c;
                yield return null;
            }
            Destroy(sr.gameObject);
        }

        IEnumerator IconPop(Sprite icon, Vector3 pos)
        {
            if (icon == null) yield break;
            var sr = Spawn(icon, pos, Color.white);
            float t = 0f;
            while (t < 0.55f)
            {
                t += Time.deltaTime;
                sr.transform.position = pos + Vector3.up * (t * 0.8f);
                var c = sr.color; c.a = t < 0.35f ? 1f : 1f - (t - 0.35f) / 0.2f; sr.color = c;
                yield return null;
            }
            Destroy(sr.gameObject);
        }

        IEnumerator ArrowShot(Vector3 from, Vector3 to)
        {
            var sprite = SpriteBank.Arrow() ?? SpriteFactory.Square;
            var sr = Spawn(sprite, from, Color.white);
            if (sprite == SpriteFactory.Square) { sr.transform.localScale = new Vector3(0.3f, 0.06f, 1f); sr.color = SpriteFactory.Cream; }
            Vector3 dir = to - from;
            sr.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            float t = 0f;
            const float dur = 0.14f;
            while (t < dur)
            {
                t += Time.deltaTime;
                sr.transform.position = Vector3.Lerp(from, to, t / dur);
                yield return null;
            }
            Destroy(sr.gameObject);
        }

        IEnumerator DustPuff(Vector3 pos)
        {
            var frames = SpriteBank.DustFrames();
            if (frames == null)
            {
                // fallback: four scattering pixel squares
                for (int i = 0; i < 4; i++)
                    StartCoroutine(Scatter(pos, SpriteFactory.Frozen));
                yield break;
            }
            var sr = Spawn(frames[0], pos, Color.white);
            for (int i = 0; i < frames.Length; i++)
            {
                sr.sprite = frames[i];
                yield return new WaitForSeconds(0.06f);
            }
            Destroy(sr.gameObject);
        }

        IEnumerator GemBurst(Vector3 pos, Color color)
        {
            for (int i = 0; i < 8; i++)
                StartCoroutine(Scatter(pos, color));
            yield break;
        }

        IEnumerator Scatter(Vector3 pos, Color color)
        {
            var sr = Spawn(SpriteFactory.Square, pos, color);
            sr.transform.localScale = Vector3.one * Random.Range(0.08f, 0.16f);
            Vector3 vel = new Vector3(Random.Range(-1.6f, 1.6f), Random.Range(1f, 2.6f), 0f);
            float t = 0f;
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                vel += Vector3.down * (6f * Time.deltaTime);
                sr.transform.position += vel * Time.deltaTime;
                var c = sr.color; c.a = 1f - t / 0.6f; sr.color = c;
                yield return null;
            }
            Destroy(sr.gameObject);
        }
    }
}

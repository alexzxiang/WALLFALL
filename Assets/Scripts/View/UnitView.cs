using UnityEngine;

namespace Wallfall
{
    /// <summary>
    /// One unit on a board or bench. Uses the itch.io idle animation when available
    /// (SpriteBank), falls back to the procedural blob otherwise.
    /// </summary>
    public class UnitView : MonoBehaviour
    {
        public UnitInstance Unit { get; private set; }
        public bool IsYours { get; private set; }

        SpriteRenderer _body, _face, _hpBack, _hpFill, _manaBack, _manaFill, _shield;
        readonly System.Collections.Generic.List<SpriteRenderer> _itemPips = new System.Collections.Generic.List<SpriteRenderer>();
        TextMesh _stars;
        Sprite[] _frames;
        Color _baseTint = Color.white;
        float _animClock;
        float _flashUntil;
        Vector3 _targetPos;
        bool _snap = true;
        float _alpha = 1f;
        bool _frozen;
        float _lungeStart = -10f;
        bool _fightMode;
        float _walkSpeed = 1.3f;
        float _barY = 0.6f;

        public void Setup(UnitInstance unit, bool isYours)
        {
            Unit = unit;
            IsYours = isYours;

            _walkSpeed = unit.Def.MoveSpeed * HexUtil.Width * 1.15f;
            _frames = SpriteBank.UnitFrames(unit.Def.SpriteKey);
            bool hasArt = _frames != null && _frames.Length > 0;

            if (hasArt)
            {
                _baseTint = unit.Def.Tint;
                _body = NewSr("body", _frames[0], _baseTint, 10);
                _body.flipX = !isYours; // side-view art faces right by default
                _animClock = Random.value * 2f;
            }
            else
            {
                _baseTint = unit.Def.Tint;
                _body = NewSr("body", SpriteFactory.BlobBody, _baseTint, 10);
                _face = NewSr("face", SpriteFactory.BlobFace, Color.white, 11);
                if (!isYours) { _body.flipX = true; _face.flipX = true; }
            }

            _hpBack = NewSr("hpBack", SpriteFactory.Square, SpriteFactory.Line, 12);
            _hpBack.transform.localPosition = new Vector3(0f, _barY, 0f);
            _hpBack.transform.localScale = new Vector3(0.66f, 0.09f, 1f);
            _hpFill = NewSr("hpFill", SpriteFactory.Square, isYours ? SpriteFactory.Hex("#6EDB78") : SpriteFactory.HpRed, 13);
            SetBar(_hpFill, _barY, 1f);

            _manaBack = NewSr("manaBack", SpriteFactory.Square, SpriteFactory.Line, 12);
            _manaBack.transform.localPosition = new Vector3(0f, _barY - 0.1f, 0f);
            _manaBack.transform.localScale = new Vector3(0.66f, 0.055f, 1f);
            _manaFill = NewSr("manaFill", SpriteFactory.Square, SpriteFactory.XpBlue, 13);
            SetBar(_manaFill, _barY - 0.1f, 0f);

            _shield = NewSr("shield", SpriteFactory.Circle, new Color(1f, 1f, 1f, 0.25f), 9);
            _shield.transform.localScale = Vector3.one * 0.8f;
            _shield.enabled = false;

            var starsGo = new GameObject("stars");
            starsGo.transform.SetParent(transform, false);
            starsGo.transform.localPosition = new Vector3(0f, 0.3f, 0f); // on the sprite, not the bars
            _stars = starsGo.AddComponent<TextMesh>();
            var pixelFont = SpriteBank.UiFont();
            if (pixelFont != null)
            {
                _stars.font = pixelFont;
                starsGo.GetComponent<MeshRenderer>().material = pixelFont.material;
            }
            _stars.characterSize = 0.1f;
            _stars.fontSize = 38;
            _stars.anchor = TextAnchor.MiddleCenter;
            _stars.color = SpriteFactory.Gold;
            starsGo.GetComponent<MeshRenderer>().sortingOrder = 16;
            RefreshStars();
        }

        public void RefreshStars() => _stars.text = new string('*', Unit.Star);

        public void SetTint(float alpha, bool frozen)
        {
            _alpha = alpha;
            _frozen = frozen;
            if (_face != null) { var c = _face.color; c.a = alpha; _face.color = c; }
            var s = _stars.color; s.a = alpha; _stars.color = s;
        }

        public void RefreshItems()
        {
            for (int i = _itemPips.Count; i < Unit.Items.Count; i++)
            {
                var pip = NewSr($"item{i}", SpriteFactory.Square, SpriteFactory.Gold, 14);
                pip.transform.localPosition = new Vector3(-0.18f + i * 0.16f, -0.56f, 0f);
                pip.transform.localScale = Vector3.one * 0.12f;
                _itemPips.Add(pip);
            }
            for (int i = 0; i < _itemPips.Count; i++)
                _itemPips[i].enabled = i < Unit.Items.Count;
        }

        public void SetPosition(Vector3 pos, bool instant = false)
        {
            _targetPos = pos;
            if (instant || _snap) { transform.position = pos; _snap = false; }
        }

        public void SetCombat(CombatUnit cu)
        {
            SetBar(_hpFill, _barY, Mathf.Clamp01(cu.Hp / cu.MaxHp));
            SetBar(_manaFill, _barY - 0.1f, cu.ManaMax > 0 ? Mathf.Clamp01((float)cu.Mana / cu.ManaMax) : 0f);
            _shield.enabled = cu.Shield > 0f;
            if (cu.JustHit) _flashUntil = Time.time + 0.09f;
            if (cu.JustAttacked && cu.Range <= 1) _lungeStart = Time.time;
            gameObject.SetActive(cu.Alive);
        }

        /// <summary>Fight mode: constant-speed gliding between cells (TFT feel) instead of snappy UI lerp.</summary>
        public void SetFightMode(bool on) => _fightMode = on;

        public void ResetBars()
        {
            _fightMode = false;
            SetBar(_hpFill, _barY, 1f);
            SetBar(_manaFill, _barY - 0.1f, 0f);
            _shield.enabled = false;
            gameObject.SetActive(true);
        }

        void SetBar(SpriteRenderer fill, float y, float t)
        {
            float w = 0.66f * t;
            fill.transform.localScale = new Vector3(w, fill == _hpFill ? 0.09f : 0.055f, 1f);
            fill.transform.localPosition = new Vector3(-(0.66f - w) * 0.5f, y, 0f);
        }

        void Update()
        {
            if (_fightMode)
            {
                // smooth march at the unit's own pace; big jumps (leaps, blinks) become quick dashes
                float dist = Vector3.Distance(transform.position, _targetPos);
                float speed = Mathf.Max(_walkSpeed, dist * 5f);
                transform.position = Vector3.MoveTowards(transform.position, _targetPos, speed * Time.deltaTime);
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, _targetPos, 14f * Time.deltaTime);
            }

            // melee lunge toward the enemy side (yours face up-board, foes down)
            float lt = (Time.time - _lungeStart) / 0.16f;
            _body.transform.localPosition = lt >= 0f && lt <= 1f
                ? new Vector3(0f, (IsYours ? 1f : -1f) * Mathf.Sin(lt * Mathf.PI) * 0.22f, 0f)
                : Vector3.zero;

            // idle animation at 8 fps
            if (_frames != null && _frames.Length > 1)
            {
                _animClock += Time.deltaTime;
                int frame = (int)(_animClock * 8f) % _frames.Length;
                _body.sprite = _frames[frame];
            }

            bool flashing = Time.time < _flashUntil;
            Color c = _baseTint;
            if (flashing) c = Color.Lerp(c, SpriteFactory.HpRed, 0.7f);
            if (_frozen) c = Color.Lerp(c, SpriteFactory.Frozen, 0.6f);
            c.a = _alpha;
            _body.color = c;
        }

        SpriteRenderer NewSr(string name, Sprite sprite, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }
    }
}

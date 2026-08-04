using UnityEngine;

namespace Wallfall
{
    /// <summary>
    /// Drop this on an empty GameObject in an empty scene (or use menu WALLFALL > Setup Scene).
    /// Shows the startup menu, then builds the whole match at runtime on PLAY.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        Camera _cam;

        void Start()
        {
            _cam = Camera.main;
            if (_cam == null)
            {
                var camGo = new GameObject("Main Camera", typeof(Camera));
                camGo.tag = "MainCamera";
                _cam = camGo.GetComponent<Camera>();
            }
            _cam.orthographic = true;
            _cam.orthographicSize = 5.1f;
            _cam.backgroundColor = SpriteFactory.PlumDeep;
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.transform.position = new Vector3(0f, 0f, -10f);

            var menuGo = new GameObject("Menu");
            menuGo.AddComponent<MenuScreen>().Build(BuildGame);
        }

        void BuildGame()
        {
            var match = gameObject.AddComponent<MatchController>();

            var presenterGo = new GameObject("Boards");
            var presenter = presenterGo.AddComponent<BoardsPresenter>();

            var hudGo = new GameObject("HUD");
            var hud = hudGo.AddComponent<Hud>();

            var vfxGo = new GameObject("VFX");
            var vfx = vfxGo.AddComponent<VfxSystem>();

            var input = gameObject.AddComponent<DragController>();
            input.Match = match;
            input.Presenter = presenter;
            input.Cam = _cam;
            input.Hud = hud;

            match.Begin();
            presenter.Build(match, _cam);
            hud.Build(match, presenter, input);
            vfx.Build(match, presenter);
        }
    }
}

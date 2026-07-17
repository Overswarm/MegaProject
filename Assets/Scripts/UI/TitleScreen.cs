using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MB
{
    /// Title: NEW GAME / CONTINUE (when a save exists).
    public class TitleScreen : MonoBehaviour
    {
        readonly string[] options = { "NEW GAME", "CONTINUE" };
        Text[] texts;
        int cursor;
        int lastMy;
        bool hasSave;

        void Start()
        {
            Time.timeScale = 1f;
            hasSave = GameManager.HasSave;

            var canvas = UIBuilder.NewCanvas("TitleCanvas", 10);
            UIBuilder.AddImage(canvas.transform, null, new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(2000, 2000), new Color(0.02f, 0.02f, 0.08f, 1f));

            UIBuilder.AddText(canvas.transform, "MEGABOT", 34, new Vector2(0.5f, 0.5f),
                new Vector2(0, 55), new Vector2(400, 44), TextAnchor.MiddleCenter,
                new Color(0.47f, 0.75f, 1f));
            UIBuilder.AddText(canvas.transform, "A MEGA MAN STYLE DEMO", 9, new Vector2(0.5f, 0.5f),
                new Vector2(0, 30), new Vector2(400, 14), TextAnchor.MiddleCenter, Color.gray);

            texts = new Text[options.Length];
            for (int i = 0; i < options.Length; i++)
                texts[i] = UIBuilder.AddText(canvas.transform, options[i], 12,
                    new Vector2(0.5f, 0.5f), new Vector2(0, -20 - i * 22),
                    new Vector2(300, 18), TextAnchor.MiddleCenter, Color.white);

            UIBuilder.AddText(canvas.transform, "ARROWS/WASD MOVE   Z/SPACE JUMP   X/K FIRE\nQ,E SWAP WEAPON   ENTER PAUSE", 8,
                new Vector2(0.5f, 0), new Vector2(0, 14), new Vector2(420, 26),
                TextAnchor.MiddleCenter, new Color(0.6f, 0.6f, 0.7f));
        }

        void Update()
        {
            int my = GameInput.MoveY;
            if (my != lastMy && my != 0)
            {
                cursor = (cursor + (my < 0 ? 1 : -1) + options.Length) % options.Length;
                Sfx.Play(SfxId.Cursor);
            }
            lastMy = my;

            for (int i = 0; i < texts.Length; i++)
            {
                bool enabled = i == 0 || hasSave;
                texts[i].text = (i == cursor ? "> " : "  ") + options[i];
                texts[i].color = !enabled ? new Color(0.35f, 0.35f, 0.4f)
                    : (i == cursor ? Color.yellow : Color.white);
            }

            if (GameInput.SubmitPressed || GameInput.PausePressed)
            {
                if (cursor == 0)
                {
                    GameManager.WipeSave();
                    GameManager.I.NewGame();
                }
                else
                {
                    if (!hasSave) { Sfx.Play(SfxId.Buzz); return; }
                    GameManager.I.LoadSave();
                }
                Sfx.Play(SfxId.Select);
                SceneManager.LoadScene("StageSelect");
            }
        }
    }
}

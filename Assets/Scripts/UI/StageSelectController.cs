using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MB
{
    /// Classic 3x3 stage select. The 8 outer cells map to Stages.All slots;
    /// the center is locked ("DR.W"). Cleared stages are marked and replayable.
    public class StageSelectController : MonoBehaviour
    {
        // grid cell (col,row) -> slot index; -1 = center
        static readonly int[,] SlotAt = { { 0, 3, 5 }, { 1, -1, 6 }, { 2, 4, 7 } };

        Image[,] cells = new Image[3, 3];
        Text nameText;
        Text infoText;
        int cx = 1, cy = 1;
        int lastMx, lastMy;
        bool launching;

        void Start()
        {
            Time.timeScale = 1f;
            var gm = GameManager.I;

            var canvas = UIBuilder.NewCanvas("StageSelectCanvas", 10);
            UIBuilder.AddImage(canvas.transform, null, new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(2000, 2000), new Color(0.03f, 0.03f, 0.12f, 1f));
            UIBuilder.AddText(canvas.transform, "SELECT STAGE", 14, new Vector2(0.5f, 1),
                new Vector2(0, -12), new Vector2(300, 18), TextAnchor.MiddleCenter, Color.white);

            const float cell = 52f, gap = 8f;
            for (int col = 0; col < 3; col++)
                for (int row = 0; row < 3; row++)
                {
                    float x = (col - 1) * (cell + gap);
                    float y = (1 - row) * (cell + gap) + 2;
                    var bg = UIBuilder.AddImage(canvas.transform, null, new Vector2(0.5f, 0.5f),
                        new Vector2(x, y), new Vector2(cell, cell), new Color(0.12f, 0.12f, 0.22f, 1f));
                    cells[col, row] = bg;

                    int slot = SlotAt[col, row];
                    if (slot < 0)
                    {
                        UIBuilder.AddText(bg.transform, "DR.W", 10, new Vector2(0.5f, 0.5f),
                            Vector2.zero, new Vector2(60, 14), TextAnchor.MiddleCenter, Color.gray);
                        continue;
                    }

                    var stage = Stages.BySlot(slot);
                    if (stage != null && stage.available)
                    {
                        var icon = UIBuilder.AddImage(bg.transform, "boss_idle", new Vector2(0.5f, 0.5f),
                            new Vector2(0, 4), new Vector2(24, 32), (Color)stage.theme);
                        icon.preserveAspect = true;
                        if (gm.Cleared.Contains(slot))
                        {
                            icon.color = new Color(0.25f, 0.25f, 0.3f, 1f);
                            UIBuilder.AddText(bg.transform, "CLEAR", 8, new Vector2(0.5f, 0),
                                new Vector2(0, 4), new Vector2(60, 10), TextAnchor.MiddleCenter, Color.yellow);
                        }
                    }
                    else
                    {
                        UIBuilder.AddText(bg.transform, "?", 18, new Vector2(0.5f, 0.5f),
                            Vector2.zero, new Vector2(30, 24), TextAnchor.MiddleCenter,
                            new Color(0.4f, 0.4f, 0.5f));
                    }
                }

            nameText = UIBuilder.AddText(canvas.transform, "", 11, new Vector2(0.5f, 0),
                new Vector2(0, 30), new Vector2(300, 16), TextAnchor.MiddleCenter, Color.white);
            infoText = UIBuilder.AddText(canvas.transform,
                $"REST {Mathf.Max(0, gm.Lives)}   E x{gm.ETanks}   W x{gm.WTanks}", 9,
                new Vector2(0.5f, 0), new Vector2(0, 12), new Vector2(300, 14),
                TextAnchor.MiddleCenter, new Color(0.7f, 0.7f, 0.8f));
        }

        void Update()
        {
            if (launching) return;

            int mx = GameInput.MoveX, my = GameInput.MoveY;
            if (mx != lastMx && mx != 0) { cx = Mathf.Clamp(cx + mx, 0, 2); Sfx.Play(SfxId.Cursor); }
            if (my != lastMy && my != 0) { cy = Mathf.Clamp(cy - my, 0, 2); Sfx.Play(SfxId.Cursor); }
            lastMx = mx; lastMy = my;

            // highlight
            for (int col = 0; col < 3; col++)
                for (int row = 0; row < 3; row++)
                    cells[col, row].color = (col == cx && row == cy)
                        ? new Color(0.85f, 0.75f, 0.2f, 1f)
                        : new Color(0.12f, 0.12f, 0.22f, 1f);

            int slot = SlotAt[cx, cy];
            var stage = slot >= 0 ? Stages.BySlot(slot) : null;
            nameText.text = stage == null ? "?????" : stage.bossName;

            if (GameInput.SubmitPressed)
            {
                if (stage != null && stage.available && !string.IsNullOrEmpty(stage.sceneName))
                    StartCoroutine(Launch(stage));
                else
                    Sfx.Play(SfxId.Buzz);
            }
        }

        IEnumerator Launch(StageData stage)
        {
            launching = true;
            Sfx.Play(SfxId.Select);
            // flash the chosen cell
            var cellImg = cells[cx, cy];
            for (int i = 0; i < 6; i++)
            {
                cellImg.color = i % 2 == 0 ? Color.white : new Color(0.85f, 0.75f, 0.2f);
                yield return new WaitForSeconds(0.09f);
            }
            yield return new WaitForSeconds(0.25f);
            GameManager.I.BeginStage(stage);
        }
    }
}

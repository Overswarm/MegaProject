using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MB
{
    /// Classic weapon-select pause screen: equip any unlocked weapon (with
    /// energy bars), use E-Tanks / W-Tanks, or exit the stage. Runs on
    /// unscaled time while the game is frozen.
    public class PauseMenu : MonoBehaviour
    {
        public bool IsOpen { get; private set; }

        class Row
        {
            public string label;
            public System.Action activate;
            public WeaponId? weapon;
            public Text text;
            public Image bar;
        }

        StageController sc;
        Canvas canvas;
        readonly List<Row> rows = new List<Row>();
        int cursor;
        int lastMy;
        bool busy;   // during tank refills

        public void Toggle(StageController stage)
        {
            if (IsOpen) Close();
            else Open(stage);
        }

        public void ForceClose()
        {
            if (IsOpen) Close();
        }

        void Open(StageController stage)
        {
            sc = stage;
            IsOpen = true;
            Time.timeScale = 0f;
            Sfx.Play(SfxId.Select, 0.35f);
            Build();
        }

        void Close()
        {
            IsOpen = false;
            busy = false;
            Time.timeScale = 1f;
            if (canvas != null) Destroy(canvas.gameObject);
            Sfx.Play(SfxId.Select, 0.35f);
        }

        void Build()
        {
            if (canvas != null) Destroy(canvas.gameObject);
            rows.Clear();
            cursor = 0;

            canvas = UIBuilder.NewCanvas("PauseCanvas", 25);
            canvas.transform.SetParent(transform, false);
            UIBuilder.AddImage(canvas.transform, null, new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(2000, 2000), new Color(0.02f, 0.02f, 0.1f, 0.96f));
            UIBuilder.AddText(canvas.transform, "- WEAPONS -", 12, new Vector2(0.5f, 1),
                new Vector2(0, -18), new Vector2(300, 16), TextAnchor.MiddleCenter, Color.white);

            var gm = GameManager.I;
            var weapons = sc.Player.GetComponent<WeaponSystem>();

            float y = -48f;
            foreach (var w in weapons.UnlockedOrdered)
            {
                var wCopy = w;
                var row = new Row
                {
                    label = GameConfig.LetterOf(w) + "  " + GameConfig.NameOf(w),
                    weapon = w,
                    activate = () => { weapons.Select(wCopy); Sfx.Play(SfxId.Select, 0.3f); Refresh(); }
                };
                row.text = UIBuilder.AddText(canvas.transform, row.label, 10, new Vector2(0.5f, 1),
                    new Vector2(-60, y), new Vector2(220, 14), TextAnchor.MiddleLeft, Color.white);
                if (w != WeaponId.Buster)
                {
                    row.bar = UIBuilder.AddBar(canvas.transform, new Vector2(0.5f, 1),
                        new Vector2(125, y - 8), GameConfig.TintFor(w), true);
                }
                rows.Add(row);
                y -= 20f;
            }

            y -= 8f;
            var eRow = new Row { activate = UseETank };
            eRow.text = UIBuilder.AddText(canvas.transform, "", 10, new Vector2(0.5f, 1),
                new Vector2(-60, y), new Vector2(220, 14), TextAnchor.MiddleLeft, Color.white);
            eRow.label = "E-TANK";
            rows.Add(eRow);
            y -= 20f;

            var wRow = new Row { activate = UseWTank };
            wRow.text = UIBuilder.AddText(canvas.transform, "", 10, new Vector2(0.5f, 1),
                new Vector2(-60, y), new Vector2(220, 14), TextAnchor.MiddleLeft, Color.white);
            wRow.label = "W-TANK";
            rows.Add(wRow);
            y -= 28f;

            var exitRow = new Row { label = "EXIT STAGE", activate = ExitStage };
            exitRow.text = UIBuilder.AddText(canvas.transform, "", 10, new Vector2(0.5f, 1),
                new Vector2(-60, y), new Vector2(220, 14), TextAnchor.MiddleLeft, Color.white);
            rows.Add(exitRow);

            UIBuilder.AddText(canvas.transform, "REST " + Mathf.Max(0, gm.Lives), 10,
                new Vector2(0.5f, 0), new Vector2(0, 16), new Vector2(200, 14),
                TextAnchor.MiddleCenter, Color.white);

            Refresh();
        }

        void Refresh()
        {
            if (sc == null || sc.Player == null) return;
            var gm = GameManager.I;
            var weapons = sc.Player.GetComponent<WeaponSystem>();

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                string label = r.label;
                if (label == "E-TANK") label = $"E-TANK  x {gm.ETanks}";
                if (label == "W-TANK") label = $"W-TANK  x {gm.WTanks}";
                r.text.text = (i == cursor ? "> " : "  ") + label;

                if (r.weapon.HasValue && r.weapon.Value == weapons.Current)
                    r.text.color = GameConfig.TintFor(r.weapon.Value);
                else
                    r.text.color = i == cursor ? Color.yellow : Color.white;

                if (r.bar != null && r.weapon.HasValue)
                    r.bar.fillAmount = (float)gm.GetEnergy(r.weapon.Value) / GameConfig.MaxWeaponEnergy;
            }
        }

        void Update()
        {
            if (!IsOpen || busy) return;

            int my = GameInput.MoveY;
            if (my != lastMy && my != 0)
            {
                cursor = (cursor + (my < 0 ? 1 : -1) + rows.Count) % rows.Count;
                Sfx.Play(SfxId.Cursor, 0.3f);
                Refresh();
            }
            lastMy = my;

            if (GameInput.SubmitPressed)
                rows[cursor].activate?.Invoke();
        }

        void UseETank()
        {
            var gm = GameManager.I;
            if (gm.ETanks <= 0 || sc.PlayerHealth == null ||
                sc.PlayerHealth.HP >= GameConfig.MaxHealth)
            {
                Sfx.Play(SfxId.Buzz, 0.25f);
                return;
            }
            gm.ETanks--;
            gm.Save();
            StartCoroutine(RunBusy(sc.PlayerHealth.RefillRoutine()));
        }

        void UseWTank()
        {
            var gm = GameManager.I;
            var weapons = sc.Player.GetComponent<WeaponSystem>();
            bool valid = gm.WTanks > 0 && weapons.Current != WeaponId.Buster &&
                         weapons.CurrentEnergy < GameConfig.MaxWeaponEnergy;
            if (!valid)
            {
                Sfx.Play(SfxId.Buzz, 0.25f);
                return;
            }
            gm.WTanks--;
            gm.Save();
            StartCoroutine(RunBusy(FillWeapon(weapons.Current)));
        }

        IEnumerator FillWeapon(WeaponId w)
        {
            Sfx.Play(SfxId.ETank);
            var gm = GameManager.I;
            while (gm.GetEnergy(w) < GameConfig.MaxWeaponEnergy)
            {
                gm.AddEnergy(w, 1);
                Sfx.Play(SfxId.Energy, 0.3f);
                Refresh();
                yield return new WaitForSecondsRealtime(0.035f);
            }
        }

        IEnumerator RunBusy(IEnumerator routine)
        {
            busy = true;
            yield return StartCoroutine(routine);
            busy = false;
            Refresh();
        }

        void ExitStage()
        {
            Sfx.Play(SfxId.Select);
            Close();
            UnityEngine.SceneManagement.SceneManager.LoadScene("StageSelect");
        }
    }
}

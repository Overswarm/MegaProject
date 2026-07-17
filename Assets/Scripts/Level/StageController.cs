using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MB
{
    /// One per stage scene. Spawns the player at the active checkpoint with the
    /// teleport beam + READY text, owns the HUD/pause menu, and runs the death,
    /// boss door, boss intro and victory sequences.
    public class StageController : MonoBehaviour
    {
        public static StageController I { get; private set; }

        public Rect stageBounds = new Rect(-2, -4, 112, 24);
        public Color theme = new Color(0.4f, 0.45f, 0.55f);

        public PlayerController Player { get; private set; }
        public PlayerHealth PlayerHealth { get; private set; }
        public BossController ActiveBoss { get; private set; }
        public bool CutsceneActive { get; private set; }

        CameraFollow camFollow;
        PauseMenu pauseMenu;
        bool gameOverOpen;

        void Awake()
        {
            I = this;
            var gm = GameManager.I;
            if (gm.CurrentStage == null)
                gm.CurrentStage = Stages.BySlot(0);   // played directly in the editor
            Time.timeScale = 1f;

            var cam = Camera.main;
            if (cam == null)
            {
                var cgo = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = cgo.AddComponent<Camera>();
                cgo.AddComponent<AudioListener>();
            }
            camFollow = cam.GetComponent<CameraFollow>();
            if (camFollow == null) camFollow = cam.gameObject.AddComponent<CameraFollow>();
            camFollow.bounds = stageBounds;
            cam.clearFlags = CameraClearFlags.SolidColor;
            var bg = (Color)GameManager.I.CurrentStage.theme * 0.16f;
            bg.a = 1f;
            cam.backgroundColor = bg;

            new GameObject("HUD").AddComponent<HUD>();
            pauseMenu = new GameObject("PauseMenu").AddComponent<PauseMenu>();
        }

        void Start()
        {
            StartCoroutine(SpawnRoutine());
        }

        void Update()
        {
            if (GameInput.PausePressed && !CutsceneActive && !gameOverOpen &&
                Player != null && PlayerHealth != null && !PlayerHealth.IsDead)
                pauseMenu.Toggle(this);
        }

        IEnumerator SpawnRoutine()
        {
            CutsceneActive = true;

            var pos = SpawnPos();
            var pgo = EntityFactory.CreatePlayer(pos);
            Player = pgo.GetComponent<PlayerController>();
            PlayerHealth = pgo.GetComponent<PlayerHealth>();
            camFollow.target = pgo.transform;
            camFollow.SnapTo(stageBounds);

            var canvas = UIBuilder.NewCanvas("ReadyCanvas", 20);
            var ready = UIBuilder.AddText(canvas.transform, "READY", 16,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200, 30),
                TextAnchor.MiddleCenter, Color.white);
            Sfx.Play(SfxId.Ready);

            StartCoroutine(Blink(ready, 1.4f));
            yield return StartCoroutine(Player.BeamInRoutine());
            yield return new WaitForSeconds(0.4f);
            Destroy(canvas.gameObject);

            CutsceneActive = false;
        }

        IEnumerator Blink(Text t, float dur)
        {
            float e = 0;
            while (e < dur && t != null)
            {
                e += 0.12f;
                if (t != null) t.enabled = !t.enabled;
                yield return new WaitForSeconds(0.12f);
            }
            if (t != null) t.enabled = true;
        }

        Vector2 SpawnPos()
        {
            int idx = GameManager.I.CheckpointIndex;
            Checkpoint best = null;
            foreach (var c in Object.FindObjectsByType<Checkpoint>(FindObjectsSortMode.None))
                if (c.index <= idx && (best == null || c.index > best.index)) best = c;
            return best != null ? (Vector2)best.transform.position : Vector2.zero;
        }

        // ---------- death / game over ----------

        public void OnPlayerDeath()
        {
            StartCoroutine(DeathRoutine());
        }

        IEnumerator DeathRoutine()
        {
            CutsceneActive = true;
            pauseMenu.ForceClose();
            yield return new WaitForSeconds(2.6f);

            var gm = GameManager.I;
            gm.Lives--;
            if (gm.Lives >= 0)
                SceneManager.LoadScene(gameObject.scene.name);
            else
                StartCoroutine(GameOverRoutine());
        }

        IEnumerator GameOverRoutine()
        {
            gameOverOpen = true;
            var canvas = UIBuilder.NewCanvas("GameOver", 30);
            UIBuilder.AddImage(canvas.transform, null, new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(2000, 2000), new Color(0, 0, 0, 0.9f));
            UIBuilder.AddText(canvas.transform, "GAME OVER", 20, new Vector2(0.5f, 0.5f),
                new Vector2(0, 50), new Vector2(300, 30), TextAnchor.MiddleCenter, Color.white);

            string[] options = { "CONTINUE", "STAGE SELECT" };
            var texts = new Text[2];
            for (int i = 0; i < 2; i++)
                texts[i] = UIBuilder.AddText(canvas.transform, options[i], 12,
                    new Vector2(0.5f, 0.5f), new Vector2(0, -10 - i * 22),
                    new Vector2(300, 20), TextAnchor.MiddleCenter, Color.white);

            int cursor = 0;
            int lastMy = 0;
            while (true)
            {
                for (int i = 0; i < 2; i++)
                {
                    texts[i].text = (i == cursor ? "> " : "  ") + options[i];
                    texts[i].color = i == cursor ? Color.yellow : Color.white;
                }
                int my = GameInput.MoveY;
                if (my != lastMy && my != 0)
                {
                    cursor = (cursor + (my < 0 ? 1 : -1) + 2) % 2;
                    Sfx.Play(SfxId.Cursor);
                }
                lastMy = my;

                if (GameInput.SubmitPressed || GameInput.PausePressed)
                {
                    Sfx.Play(SfxId.Select);
                    var gm = GameManager.I;
                    gm.Lives = GameConfig.StartingLives;
                    gm.RefillAllWeapons();
                    if (cursor == 0)
                        SceneManager.LoadScene(gameObject.scene.name);
                    else
                        SceneManager.LoadScene("StageSelect");
                    yield break;
                }
                yield return null;
            }
        }

        // ---------- boss flow ----------

        public void RunBossDoor(BossDoor door)
        {
            StartCoroutine(BossDoorSeq(door));
        }

        IEnumerator BossDoorSeq(BossDoor door)
        {
            CutsceneActive = true;
            Player.SetFrozen(true);
            yield return new WaitForSeconds(0.15f);

            yield return StartCoroutine(door.Open());
            yield return StartCoroutine(Player.WalkTo(door.transform.position.x + 1.8f));
            yield return StartCoroutine(door.Close());

            if (door.isFinal && door.boss != null)
            {
                ActiveBoss = door.boss;
                yield return StartCoroutine(camFollow.TransitionTo(door.roomBounds, 1.4f));
                yield return StartCoroutine(door.boss.IntroRoutine(door.boss.transform.position));
            }

            Player.SetFrozen(false);
            CutsceneActive = false;
        }

        public void OnBossDefeated(BossController boss)
        {
            StartCoroutine(VictoryRoutine(boss));
        }

        IEnumerator VictoryRoutine(BossController boss)
        {
            CutsceneActive = true;
            Player.SetFrozen(true);

            // clear stray enemy shots
            foreach (var p in Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None))
                if (!p.fromPlayer) Destroy(p.gameObject);

            Sfx.Play(SfxId.ExplodeBig);
            Vector3 at = boss.transform.position + Vector3.up;
            for (int i = 0; i < 3; i++)
            {
                EntityFactory.CreateDeathOrbs(at, (Color)GameManager.I.CurrentStage.theme);
                Sfx.Play(SfxId.ExplodeSmall);
                if (i == 1)
                {
                    var bsr = boss.GetComponent<SpriteRenderer>();
                    if (bsr != null) bsr.enabled = false;
                }
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForSeconds(0.8f);

            var gm = GameManager.I;
            bool firstClear = !gm.Unlocked.Contains(gm.CurrentStage.reward);
            gm.OnStageCleared();

            var canvas = UIBuilder.NewCanvas("WeaponGet", 30);
            UIBuilder.AddImage(canvas.transform, null, new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(2000, 2000), new Color(0, 0, 0, 0.92f));
            string msg = firstClear
                ? "YOU GOT\n\n" + GameConfig.NameOf(gm.CurrentStage.reward)
                : "STAGE CLEAR";
            UIBuilder.AddText(canvas.transform, msg, 16, new Vector2(0.5f, 0.5f),
                new Vector2(0, 20), new Vector2(400, 80), TextAnchor.MiddleCenter, Color.white);
            UIBuilder.AddText(canvas.transform, "PRESS START", 10, new Vector2(0.5f, 0.5f),
                new Vector2(0, -50), new Vector2(300, 20), TextAnchor.MiddleCenter, Color.yellow);
            Sfx.Play(SfxId.OneUp);

            Destroy(boss.gameObject);
            yield return new WaitForSeconds(0.6f);
            while (!GameInput.SubmitPressed && !GameInput.PausePressed)
                yield return null;

            Sfx.Play(SfxId.Select);
            SceneManager.LoadScene("StageSelect");
        }
    }
}

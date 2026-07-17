using UnityEngine;

namespace MB
{
    /// Picks the right placeholder sprite each frame from controller state,
    /// applies weapon tint, hurt/invuln blinking and charge flicker.
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerVisual : MonoBehaviour
    {
        [HideInInspector] public string overrideKey;

        SpriteRenderer sr;
        PlayerController controller;
        PlayerHealth health;
        PlayerShooter shooter;
        WeaponSystem weapons;
        float runClock;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            controller = GetComponent<PlayerController>();
            health = GetComponent<PlayerHealth>();
            shooter = GetComponent<PlayerShooter>();
            weapons = GetComponent<WeaponSystem>();
            sr.sortingOrder = 10;
        }

        void LateUpdate()
        {
            if (controller == null || sr == null) return;

            if (controller.State == PlayerController.PState.Dead)
            {
                sr.enabled = false;
                return;
            }

            // invulnerability blink
            if (health != null && health.InvulnTimer > 0f &&
                controller.State != PlayerController.PState.Frozen)
            {
                sr.enabled = Mathf.Repeat(Time.time, 0.12f) < 0.06f;
            }
            else sr.enabled = true;

            string key = overrideKey;
            if (string.IsNullOrEmpty(key))
            {
                switch (controller.State)
                {
                    case PlayerController.PState.Slide: key = "player_slide"; break;
                    case PlayerController.PState.Climb: key = "player_idle"; break;
                    case PlayerController.PState.Hurt: key = "player_jump"; break;
                    default:
                        if (!controller.IsGrounded) key = "player_jump";
                        else if (Mathf.Abs(GameInput.MoveX) > 0)
                        {
                            runClock += Time.deltaTime;
                            key = (Mathf.Repeat(runClock, 0.24f) < 0.12f) ? "player_run0" : "player_run1";
                        }
                        else { key = "player_idle"; runClock = 0; }
                        break;
                }
            }

            sr.sprite = SpriteFactory.Get(key);
            sr.flipX = controller.Facing < 0;

            // tint: weapon color, flickering while charged
            Color tint = weapons != null ? weapons.CurrentTint : Color.white;
            if (shooter != null && shooter.ChargeTier > 0)
            {
                float f = Mathf.Repeat(Time.time, 0.12f) < 0.06f ? 1f : 0f;
                if (shooter.ChargeTier == 2)
                    tint = Color.Lerp(tint, Color.white, f);
                else
                    tint = Color.Lerp(tint, new Color(1f, 0.8f, 0.4f), f * 0.6f);
            }
            sr.color = tint;
        }
    }
}

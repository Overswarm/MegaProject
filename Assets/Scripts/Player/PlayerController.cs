using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MB
{
    /// NES-style movement: instant acceleration, jump height cut on release,
    /// down+jump slide (with ceiling checks), ladder climbing, hurt knockback.
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        public enum PState { Normal, Slide, Climb, Hurt, Frozen, Dead }

        public PState State { get; private set; } = PState.Normal;
        public int Facing { get; private set; } = 1;
        public bool IsGrounded { get; private set; }
        public Ladder CurrentLadder { get; private set; }

        Rigidbody2D rb;
        BoxCollider2D box;

        static readonly Vector2 StandSize = new Vector2(0.75f, 1.35f);
        static readonly Vector2 StandOffset = new Vector2(0f, 0.675f);
        static readonly Vector2 SlideSize = new Vector2(0.75f, 0.7f);
        static readonly Vector2 SlideOffset = new Vector2(0f, 0.35f);

        float slideTimer;
        float hurtTimer;
        readonly List<Ladder> ladders = new List<Ladder>();
        Collider2D ignoredTopPlatform;

        public System.Action OnLanded;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            box = GetComponent<BoxCollider2D>();
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.gravityScale = GameConfig.Gravity / -Physics2D.gravity.y;
            box.size = StandSize;
            box.offset = StandOffset;
        }

        void Update()
        {
            if (State == PState.Dead) return;

            bool wasGrounded = IsGrounded;
            IsGrounded = CheckGrounded();
            if (!wasGrounded && IsGrounded && State != PState.Climb) OnLanded?.Invoke();

            switch (State)
            {
                case PState.Normal: TickNormal(); break;
                case PState.Slide: TickSlide(); break;
                case PState.Climb: TickClimb(); break;
                case PState.Hurt: TickHurt(); break;
                case PState.Frozen:
                    rb.linearVelocity = new Vector2(0, Mathf.Max(rb.linearVelocity.y, -GameConfig.MaxFallSpeed));
                    break;
            }

            // clamp fall speed
            if (rb.linearVelocity.y < -GameConfig.MaxFallSpeed)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -GameConfig.MaxFallSpeed);
        }

        // ---------- states ----------

        void TickNormal()
        {
            int mx = GameInput.MoveX;
            int my = GameInput.MoveY;

            if (mx != 0) Facing = mx;
            float vx = mx * GameConfig.RunSpeed;
            float vy = rb.linearVelocity.y;

            // ladder grab
            var ladder = BestLadder();
            if (ladder != null)
            {
                float feet = transform.position.y;
                bool atTop = feet >= ladder.TopY - 0.15f;
                if (my > 0 && !atTop) { StartClimb(ladder, false); return; }
                if (my < 0 && IsGrounded && atTop) { StartClimb(ladder, true); return; }
            }

            // slide: down + jump on the ground
            if (GameInput.JumpPressed && my < 0 && IsGrounded)
            {
                StartSlide();
                return;
            }

            if (GameInput.JumpPressed && IsGrounded)
                vy = GameConfig.JumpVelocity;

            // NES variable jump: releasing jump kills upward velocity
            if (vy > 0f && !GameInput.JumpHeld)
                vy = 0f;

            rb.linearVelocity = new Vector2(vx, vy);
        }

        void StartSlide()
        {
            State = PState.Slide;
            slideTimer = GameConfig.SlideDuration;
            box.size = SlideSize;
            box.offset = SlideOffset;
        }

        void TickSlide()
        {
            slideTimer -= Time.deltaTime;
            bool ceiling = CeilingAbove();

            int mx = GameInput.MoveX;
            bool wantsCancel = (mx != 0 && mx != Facing);
            bool wantsJump = GameInput.JumpPressed;

            if (!ceiling && (slideTimer <= 0f || wantsCancel || wantsJump || !IsGrounded))
            {
                EndSlide();
                if (wantsCancel) Facing = mx;
                if (wantsJump && IsGrounded)
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, GameConfig.JumpVelocity);
                return;
            }

            rb.linearVelocity = new Vector2(Facing * GameConfig.SlideSpeed, rb.linearVelocity.y);
        }

        void EndSlide()
        {
            State = PState.Normal;
            box.size = StandSize;
            box.offset = StandOffset;
        }

        void StartClimb(Ladder ladder, bool fromTop)
        {
            State = PState.Climb;
            CurrentLadder = ladder;
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            if (State == PState.Slide) EndSlide();
            box.size = StandSize;
            box.offset = StandOffset;

            float y = fromTop ? ladder.TopY - 0.6f : transform.position.y;
            transform.position = new Vector3(ladder.CenterX, y, 0);

            if (ladder.TopPlatform != null)
            {
                ignoredTopPlatform = ladder.TopPlatform;
                Physics2D.IgnoreCollision(box, ignoredTopPlatform, true);
            }
        }

        void TickClimb()
        {
            if (CurrentLadder == null) { ExitClimb(); return; }

            int my = GameInput.MoveY;
            int mx = GameInput.MoveX;
            if (mx != 0) Facing = mx;

            rb.linearVelocity = new Vector2(0, my * GameConfig.ClimbSpeed);

            float feet = transform.position.y;

            // top out
            if (my > 0 && feet >= CurrentLadder.TopY - 0.02f)
            {
                transform.position = new Vector3(CurrentLadder.CenterX, CurrentLadder.TopY + 0.02f, 0);
                ExitClimb();
                return;
            }
            // touch ground below
            if (my < 0 && IsGrounded && feet < CurrentLadder.TopY - 0.5f)
            {
                ExitClimb();
                return;
            }
            // jump off
            if (GameInput.JumpPressed)
            {
                ExitClimb();
                return;
            }
            // slid off the ladder zone
            if (!ladders.Contains(CurrentLadder))
                ExitClimb();
        }

        void ExitClimb()
        {
            State = PState.Normal;
            CurrentLadder = null;
            rb.gravityScale = GameConfig.Gravity / -Physics2D.gravity.y;
            if (ignoredTopPlatform != null)
            {
                Physics2D.IgnoreCollision(box, ignoredTopPlatform, false);
                ignoredTopPlatform = null;
            }
        }

        void TickHurt()
        {
            hurtTimer -= Time.deltaTime;
            rb.linearVelocity = new Vector2(-Facing * GameConfig.KnockbackX, rb.linearVelocity.y);
            if (hurtTimer <= 0f) State = PState.Normal;
        }

        // ---------- external control ----------

        public void Hurt(Vector2 from)
        {
            if (State == PState.Dead) return;
            if (State == PState.Climb) ExitClimb();
            if (State == PState.Slide) EndSlide();
            if (State == PState.Frozen) return;
            State = PState.Hurt;
            hurtTimer = GameConfig.HurtDuration;
            Facing = from.x > transform.position.x ? 1 : -1;
            rb.linearVelocity = new Vector2(-Facing * GameConfig.KnockbackX, 1.5f);
        }

        public void SetFrozen(bool frozen)
        {
            if (State == PState.Dead) return;
            if (frozen)
            {
                if (State == PState.Climb) ExitClimb();
                if (State == PState.Slide) EndSlide();
                State = PState.Frozen;
                rb.linearVelocity = Vector2.zero;
            }
            else if (State == PState.Frozen)
            {
                State = PState.Normal;
            }
        }

        public void MarkDead()
        {
            State = PState.Dead;
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
            box.enabled = false;
        }

        /// Walk automatically to an x position (used for boss door sequences).
        public IEnumerator WalkTo(float x)
        {
            SetFrozen(true);
            State = PState.Frozen;
            int dir = x > transform.position.x ? 1 : -1;
            Facing = dir;
            float timeout = 4f;
            while (Mathf.Abs(transform.position.x - x) > 0.1f && timeout > 0f)
            {
                rb.linearVelocity = new Vector2(dir * GameConfig.RunSpeed, rb.linearVelocity.y);
                timeout -= Time.deltaTime;
                yield return null;
            }
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        /// Teleport beam arrival, classic style.
        public IEnumerator BeamInRoutine()
        {
            var visual = GetComponent<PlayerVisual>();
            SetFrozen(true);
            rb.simulated = false;
            Vector3 target = transform.position;
            transform.position = target + Vector3.up * 12f;
            if (visual != null) visual.overrideKey = "beam";
            Sfx.Play(SfxId.Teleport);

            float t = 0f, dur = 0.35f;
            while (t < dur)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(target + Vector3.up * 12f, target, t / dur);
                yield return null;
            }
            transform.position = target;
            // materialize flash
            for (int i = 0; i < 3; i++)
            {
                if (visual != null) visual.overrideKey = (i % 2 == 0) ? "player_idle" : "beam";
                yield return new WaitForSeconds(0.06f);
            }
            if (visual != null) visual.overrideKey = null;
            rb.simulated = true;
            SetFrozen(false);
        }

        // ---------- queries ----------

        bool CheckGrounded()
        {
            if (rb.linearVelocity.y > 0.5f) return false;
            Vector2 origin = (Vector2)transform.position + new Vector2(0, 0.1f);
            var hit = Physics2D.BoxCast(origin, new Vector2(box.size.x * 0.9f, 0.1f), 0f,
                Vector2.down, 0.18f, Layers.GroundMask);
            return hit.collider != null;
        }

        bool CeilingAbove()
        {
            Vector2 origin = (Vector2)transform.position + new Vector2(0, SlideSize.y + 0.05f);
            var hit = Physics2D.BoxCast(origin, new Vector2(box.size.x * 0.9f, 0.05f), 0f,
                Vector2.up, StandSize.y - SlideSize.y, Layers.GroundMask);
            return hit.collider != null;
        }

        Ladder BestLadder()
        {
            Ladder best = null;
            float bestDist = float.MaxValue;
            foreach (var l in ladders)
            {
                if (l == null) continue;
                float d = Mathf.Abs(l.CenterX - transform.position.x);
                if (d < 0.6f && d < bestDist) { best = l; bestDist = d; }
            }
            return best;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var l = other.GetComponent<Ladder>();
            if (l != null && !ladders.Contains(l)) ladders.Add(l);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            var l = other.GetComponent<Ladder>();
            if (l != null) ladders.Remove(l);
        }
    }
}

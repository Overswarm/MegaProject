using UnityEngine;
using UnityEngine.InputSystem;

namespace MB
{
    /// All input actions are defined in code so the project needs no .inputactions asset.
    /// Keyboard: WASD/arrows move, Space/Z jump, X/K fire, Q/E or [ ] weapon swap, Enter/Esc/P pause.
    /// Gamepad: dpad/stick move, south jump, west fire, shoulders weapon swap, start pause.
    public static class GameInput
    {
        public static InputAction Move { get; private set; }
        public static InputAction Jump { get; private set; }
        public static InputAction Fire { get; private set; }
        public static InputAction WeaponPrev { get; private set; }
        public static InputAction WeaponNext { get; private set; }
        public static InputAction Pause { get; private set; }

        static bool _init;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void AutoInit() { EnsureInit(); }

        public static void EnsureInit()
        {
            if (_init) return;
            _init = true;

            Move = new InputAction("Move", InputActionType.Value);
            Move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            Move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            Move.AddBinding("<Gamepad>/dpad");
            Move.AddBinding("<Gamepad>/leftStick");

            Jump = new InputAction("Jump", InputActionType.Button);
            Jump.AddBinding("<Keyboard>/space");
            Jump.AddBinding("<Keyboard>/z");
            Jump.AddBinding("<Gamepad>/buttonSouth");

            Fire = new InputAction("Fire", InputActionType.Button);
            Fire.AddBinding("<Keyboard>/x");
            Fire.AddBinding("<Keyboard>/k");
            Fire.AddBinding("<Gamepad>/buttonWest");

            WeaponPrev = new InputAction("WeaponPrev", InputActionType.Button);
            WeaponPrev.AddBinding("<Keyboard>/q");
            WeaponPrev.AddBinding("<Keyboard>/leftBracket");
            WeaponPrev.AddBinding("<Gamepad>/leftShoulder");

            WeaponNext = new InputAction("WeaponNext", InputActionType.Button);
            WeaponNext.AddBinding("<Keyboard>/e");
            WeaponNext.AddBinding("<Keyboard>/rightBracket");
            WeaponNext.AddBinding("<Gamepad>/rightShoulder");

            Pause = new InputAction("Pause", InputActionType.Button);
            Pause.AddBinding("<Keyboard>/enter");
            Pause.AddBinding("<Keyboard>/escape");
            Pause.AddBinding("<Keyboard>/p");
            Pause.AddBinding("<Gamepad>/start");

            Move.Enable(); Jump.Enable(); Fire.Enable();
            WeaponPrev.Enable(); WeaponNext.Enable(); Pause.Enable();
        }

        public static Vector2 MoveRaw { get { EnsureInit(); return Move.ReadValue<Vector2>(); } }
        public static int MoveX { get { var v = MoveRaw.x; return v > 0.4f ? 1 : v < -0.4f ? -1 : 0; } }
        public static int MoveY { get { var v = MoveRaw.y; return v > 0.4f ? 1 : v < -0.4f ? -1 : 0; } }

        public static bool JumpPressed { get { EnsureInit(); return Jump.WasPressedThisFrame(); } }
        public static bool JumpHeld { get { EnsureInit(); return Jump.IsPressed(); } }
        public static bool JumpReleased { get { EnsureInit(); return Jump.WasReleasedThisFrame(); } }
        public static bool FirePressed { get { EnsureInit(); return Fire.WasPressedThisFrame(); } }
        public static bool FireHeld { get { EnsureInit(); return Fire.IsPressed(); } }
        public static bool FireReleased { get { EnsureInit(); return Fire.WasReleasedThisFrame(); } }
        public static bool PrevPressed { get { EnsureInit(); return WeaponPrev.WasPressedThisFrame(); } }
        public static bool NextPressed { get { EnsureInit(); return WeaponNext.WasPressedThisFrame(); } }
        public static bool PausePressed { get { EnsureInit(); return Pause.WasPressedThisFrame(); } }

        /// Menu confirm: jump or fire.
        public static bool SubmitPressed { get { return JumpPressed || FirePressed; } }
    }
}

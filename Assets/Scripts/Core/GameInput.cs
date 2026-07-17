using UnityEngine;
using UnityEngine.InputSystem;

namespace MB
{
    /// All input actions are defined in code so the project needs no .inputactions asset.
    /// Keyboard: WASD/arrows move, Space/Z jump, X/K fire, Q/E or [ ] weapon swap, Enter/Esc/P pause.
    /// Gamepad: dpad/stick move, south jump, west fire, shoulders weapon swap, start pause.
    ///
    /// Actions live in a proper InputActionMap with explicit control layouts, and
    /// initialization is lazy / after scene load so devices are registered first.
    public static class GameInput
    {
        public static InputAction Move { get; private set; }
        public static InputAction Jump { get; private set; }
        public static InputAction Fire { get; private set; }
        public static InputAction WeaponPrev { get; private set; }
        public static InputAction WeaponNext { get; private set; }
        public static InputAction Pause { get; private set; }

        static InputActionMap map;
        static bool _init;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoInit() { EnsureInit(); }

        public static void EnsureInit()
        {
            if (_init) return;
            _init = true;

            map = new InputActionMap("Gameplay");

            Move = map.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            Move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            Move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            Move.AddBinding("<Gamepad>/dpad");
            Move.AddBinding("<Gamepad>/leftStick");

            Jump = map.AddAction("Jump", InputActionType.Button, expectedControlLayout: "Button");
            Jump.AddBinding("<Keyboard>/space");
            Jump.AddBinding("<Keyboard>/z");
            Jump.AddBinding("<Gamepad>/buttonSouth");

            Fire = map.AddAction("Fire", InputActionType.Button, expectedControlLayout: "Button");
            Fire.AddBinding("<Keyboard>/x");
            Fire.AddBinding("<Keyboard>/k");
            Fire.AddBinding("<Gamepad>/buttonWest");

            WeaponPrev = map.AddAction("WeaponPrev", InputActionType.Button, expectedControlLayout: "Button");
            WeaponPrev.AddBinding("<Keyboard>/q");
            WeaponPrev.AddBinding("<Keyboard>/leftBracket");
            WeaponPrev.AddBinding("<Gamepad>/leftShoulder");

            WeaponNext = map.AddAction("WeaponNext", InputActionType.Button, expectedControlLayout: "Button");
            WeaponNext.AddBinding("<Keyboard>/e");
            WeaponNext.AddBinding("<Keyboard>/rightBracket");
            WeaponNext.AddBinding("<Gamepad>/rightShoulder");

            Pause = map.AddAction("Pause", InputActionType.Button, expectedControlLayout: "Button");
            Pause.AddBinding("<Keyboard>/enter");
            Pause.AddBinding("<Keyboard>/escape");
            Pause.AddBinding("<Keyboard>/p");
            Pause.AddBinding("<Gamepad>/start");

            map.Enable();
        }

        public static Vector2 MoveRaw
        {
            get
            {
                EnsureInit();
                var v = Move.ReadValue<Vector2>();
                // never let corrupt event data reach gameplay
                if (float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsInfinity(v.x) || float.IsInfinity(v.y))
                    return Vector2.zero;
                return v;
            }
        }

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

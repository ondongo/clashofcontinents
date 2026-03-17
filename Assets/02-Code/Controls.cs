namespace ClashOfContinents
{
    using UnityEngine.InputSystem;

    public class Controls
    {
        public MainActionMap Main { get; }

        public Controls()
        {
            Main = new MainActionMap();
        }

        public void Enable()  => Main.Enable();
        public void Disable() => Main.Disable();

        public class MainActionMap
        {
            public InputAction Move           { get; }
            public InputAction TouchZoom      { get; }
            public InputAction PointerClick   { get; }
            public InputAction PointerPosition { get; }
            public InputAction MouseScroll    { get; }
            public InputAction TouchPosition0 { get; }
            public InputAction TouchPosition1 { get; }

            public MainActionMap()
            {
                Move = new InputAction("Move", InputActionType.Button);
                Move.AddBinding("<Mouse>/leftButton");
                Move.AddBinding("<Touchscreen>/primaryTouch/press");

                TouchZoom = new InputAction("TouchZoom", InputActionType.Button);
                TouchZoom.AddBinding("<Touchscreen>/touch1/press");

                PointerClick = new InputAction("PointerClick", InputActionType.Button);
                PointerClick.AddBinding("<Mouse>/leftButton");
                PointerClick.AddBinding("<Touchscreen>/primaryTouch/tap");

                PointerPosition = new InputAction(
                    "PointerPosition",
                    InputActionType.Value,
                    expectedControlType: "Vector2");
                PointerPosition.AddBinding("<Mouse>/position");
                PointerPosition.AddBinding("<Touchscreen>/primaryTouch/position");

                MouseScroll = new InputAction(
                    "MouseScroll",
                    InputActionType.Value,
                    expectedControlType: "Axis");
                MouseScroll.AddBinding("<Mouse>/scroll/y");

                TouchPosition0 = new InputAction(
                    "TouchPosition0",
                    InputActionType.Value,
                    expectedControlType: "Vector2");
                TouchPosition0.AddBinding("<Touchscreen>/touch0/position");

                TouchPosition1 = new InputAction(
                    "TouchPosition1",
                    InputActionType.Value,
                    expectedControlType: "Vector2");
                TouchPosition1.AddBinding("<Touchscreen>/touch1/position");
            }

            public void Enable()
            {
                Move.Enable();
                TouchZoom.Enable();
                PointerClick.Enable();
                PointerPosition.Enable();
                MouseScroll.Enable();
                TouchPosition0.Enable();
                TouchPosition1.Enable();
            }

            public void Disable()
            {
                Move.Disable();
                TouchZoom.Disable();
                PointerClick.Disable();
                PointerPosition.Disable();
                MouseScroll.Disable();
                TouchPosition0.Disable();
                TouchPosition1.Disable();
            }
        }
    }
}

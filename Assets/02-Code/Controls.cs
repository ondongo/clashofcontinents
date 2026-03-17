namespace ClashOfContinents
{
    using UnityEngine.InputSystem;

    /************************************************************/
    /*                        CONTROLS                          */
    /*                                                          */                                                     
    /*  Expose un action map "Main" avec toutes les actions     */
    /*  utilisees par CameraController :                        */
    /*    - Move          : pression/relache du pointeur        */
    /*    - TouchZoom     : second doigt (pinch zoom)           */
    /*    - PointerClick  : tap ou clic discret                 */
    /*    - PointerPosition : position courante du pointeur     */
    /*    - MouseScroll   : molette souris (axe Y)              */
    /*    - TouchPosition0/1 : positions des deux doigts pinch  */
    /************************************************************/
    public class Controls
    {
        public MainActionMap Main { get; }

        public Controls()
        {
            Main = new MainActionMap();
        }

        public void Enable()  => Main.Enable();
        public void Disable() => Main.Disable();

        /************************************************************/
        /*                    MAIN ACTION MAP                       */
        /************************************************************/
        public class MainActionMap
        {
            /* Detecte quand le pointeur primaire est presse/relache */
            public InputAction Move           { get; }

            /* Detecte l apparition du second doigt pour le zoom */
            public InputAction TouchZoom      { get; }

            /* Tap ou clic discret sur l ecran */
            public InputAction PointerClick   { get; }

            /* Position continue du pointeur principal */
            public InputAction PointerPosition { get; }

            /* Molette de la souris (retourne float, axe Y) */
            public InputAction MouseScroll    { get; }

            /* Position du premier doigt (pinch) */
            public InputAction TouchPosition0 { get; }

            /* Position du second doigt (pinch) */
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

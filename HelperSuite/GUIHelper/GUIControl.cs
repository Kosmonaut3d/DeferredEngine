using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace HelperSuite.GUIHelper
{
    public static class GUIControl
    {

        //UIWasUsed needs to be resettet each updated period to false.
        //It is useful for other parts of the program so they know they are obscured by UI and don't trigger actions.
        public static bool UIElementEngaged = false;
        public static bool UIWasUsed = false;

        public static MouseState LastMouseState;
        public static MouseState CurrentMouseState;
        private static Vector2 mousePosition = Vector2.Zero;
        public static int ScreenWidth;
        public static int ScreenHeight;

        public static void Initialize(int width, int height)
        {
            UpdateResolution(width, height);
        }

        public static void Update(MouseState lastMouseState, MouseState currentMouseState)
        {
            UIWasUsed = false;

            LastMouseState = lastMouseState;
            CurrentMouseState = currentMouseState;

            mousePosition.X = CurrentMouseState.X;
            mousePosition.Y = CurrentMouseState.Y;
        }

        public static void UpdateResolution(int width, int height)
        {
            ScreenWidth = width;
            ScreenHeight = height;
        }

        public static bool IsLMBPressed()
        {
            return CurrentMouseState.LeftButton == ButtonState.Pressed;
        }
        public static bool WasLMBClicked()
        {
            return CurrentMouseState.LeftButton == ButtonState.Pressed &&
                   LastMouseState.LeftButton == ButtonState.Released;
        }

        public static Vector2 GetMousePosition()
        {
            return mousePosition;
        }

    }
}

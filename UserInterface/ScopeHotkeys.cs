using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace loki_bms_csharp.UserInterface
{
    public static class ScopeHotkeys
    {
        public static double RubberBandSetTime = 0.75;
        private static DateTime rubberBandDown;
        private static int heldKey = -1;

        public static void OnKeyDown(KeyEventArgs e)
        {
            if((int)e.Key >= (int)Key.D0 && (int)e.Key <= (int)Key.D9 && (int)e.Key != heldKey)
            {
                rubberBandDown = DateTime.Now;
                heldKey = (int)e.Key;
                System.Diagnostics.Debug.WriteLine("Rubber Band check...");
            }
        }

        public static void OnKeyUp(KeyEventArgs e)
        {
            if ((int)e.Key >= (int)Key.D0 && (int)e.Key <= (int)Key.D9)
            {
                heldKey = -1;
                int pressedNumber = (int)e.Key - 34;

                bool isSetZoom = (DateTime.Now - rubberBandDown).TotalSeconds >= RubberBandSetTime;
                System.Diagnostics.Debug.WriteLine($"Rubber Band {(isSetZoom? "set":"recalled")} preset {pressedNumber}");

                if (isSetZoom)
                {
                    ZoomPreset preset = new ZoomPreset(UserData.ViewCenter, UserData.ZoomIncrement);
                    UserData.SetViewPreset(pressedNumber, preset);
                }
                else
                {
                    UserData.SnapToView(pressedNumber);
                }
            }
        }
    }
}

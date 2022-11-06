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
        private static ref Settings.ViewSettings ViewSettings
        {
            get => ref ProgramData.ViewSettings;
        }

        public static InputData OnKeyDown(KeyEventArgs e)
        {
            int key = -1;

            if((int)e.Key >= (int)Key.D0 && (int)e.Key <= (int)Key.D9 && (int)e.Key != heldKey)
            {
                rubberBandDown = DateTime.Now;
                heldKey = (int)e.Key;
                key = heldKey;
                System.Diagnostics.Debug.WriteLine("Rubber Band check...");
            }

            return new KeyboardInputData { ZoomPreset = key, RequiresRedraw = false };
        }

        public static InputData OnKeyUp(KeyEventArgs e)
        {
            bool redraw = false;
            int key = -1;
            bool zoomSet = false;

            if ((int)e.Key >= (int)Key.D0 && (int)e.Key <= (int)Key.D9)
            {
                heldKey = -1;
                key = (int)e.Key - 34;
                redraw = true;

                zoomSet = (DateTime.Now - rubberBandDown).TotalSeconds >= RubberBandSetTime;
                System.Diagnostics.Debug.WriteLine($"Rubber Band {(zoomSet ? "set":"recalled")} preset {key}");

                if (zoomSet)
                {
                    ZoomPreset preset = new ZoomPreset(ViewSettings.ViewCenter, ViewSettings.ZoomIncrement);
                    ViewSettings.SetViewPreset(key, preset);
                }
                else
                {
                    ViewSettings.SnapToView(key);
                }
            }

            return new KeyboardInputData { RequiresRedraw = redraw, ZoomPreset = key, ZoomPresetChanged = zoomSet };
        }
    }
}

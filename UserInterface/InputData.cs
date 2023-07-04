using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace loki_bms_csharp.UserInterface
{
    public abstract class InputData
    {
        public bool RequiresRedraw = false;
    }

    public class MouseInputData : InputData
    {
        public MouseClickState MouseButtons = MouseClickState.None;
        public bool DoubleClicked = false;
        public bool RightClickMenuOpen = false;
        public Rect? RightClickMenuPos;
    }

    public class KeyboardInputData: InputData
    {
        public int ZoomPreset;
        public bool ZoomPresetChanged;
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using SkiaSharp;
using SkiaSharp.Views.WPF;

namespace loki_bms_csharp.UserInterface
{
    public static class ScopeMouseInput
    {
        public static MouseClickState ClickState = MouseClickState.None;

        public static double DoubleClickTime = 0.25;
        static DateTime LastLeftClick;
        static bool DoubleClickFired;

        public static void OnMouseWheel(MouseWheelEventArgs e)
        {
            SKElement canvas = UserData.MainWindow.ScopeCanvas;

            Point mousePos = e.GetPosition(canvas);
            SKSize size = canvas.CanvasSize;
            if (mousePos.X >= 0 && mousePos.X <= size.Width && mousePos.Y >= 0 && mousePos.Y < size.Height)
            {
                int change = Math.Sign(e.Delta);
                UserData.SetZoom(UserData.ZoomIncrement - change);
            }
        }

        public static void OnMouseDown (MouseButtonEventArgs e)
        {
            ClickState = (MouseClickState)(GetButtonValue(e.ChangedButton) | (int)ClickState);

            if(ClickState == MouseClickState.Left && CheckDoubleClick())
            {
                RecenterCamera(e);
            }
            else if (ClickState == (MouseClickState)5)
            {
                System.Diagnostics.Debug.WriteLine("Left-Right Click Combo fired");
            }
        }

        public static bool CheckDoubleClick()
        {
            DateTime now = DateTime.Now;

            if ((now - LastLeftClick).TotalSeconds <= DoubleClickTime && !DoubleClickFired)
            {
                DoubleClickFired = true;
            }
            else
            {
                DoubleClickFired = false;
            }

            LastLeftClick = now;
            return DoubleClickFired;
        }

        public static void RecenterCamera (MouseButtonEventArgs e)
        {
            Vector64 clickPoint = clickToScreenPoint(e.GetPosition(UserData.MainWindow.ScopeCanvas));

            if (clickPoint.magnitude <= MathL.Conversions.EarthRadius)
            {
                Vector64 WorldOrigin = UserData.CameraMatrix.PointToTangentSpace((0, 0, 0));

                double r = MathL.Conversions.EarthRadius;
                double rawIntersect = Math.Sqrt(r * r - clickPoint.y * clickPoint.y - clickPoint.z * clickPoint.z);

                Vector64 WorldIntersectPos = UserData.CameraMatrix.PointToWorldSpace((rawIntersect + WorldOrigin.x, clickPoint.y, clickPoint.z));

                //TrackDatabase.UncorrelatedData[0].Position = WorldIntersectPos;

                LatLonCoord newCenter = MathL.Conversions.XYZToLL(WorldIntersectPos, r);
                UserData.UpdateViewPosition(newCenter);
            }
        }

        private static Vector64 clickToScreenPoint(Point pt)
        {
            SKSize size = UserData.MainWindow.ScopeCanvas.CanvasSize;

            double y = pt.X - size.Width / 2;
            double z = pt.Y - size.Height / 2;

            return new Vector64(0, y, z) * UserData.VerticalFOV / size.Height;
        }

        public static void OnMouseUp(MouseButtonEventArgs e)
        {
            int buttonValue = GetButtonValue(e.ChangedButton);

            var prevClick = ClickState;
            ClickState = (MouseClickState)(buttonValue ^ (int)ClickState);

            if((prevClick == (MouseClickState)5) && (MouseClickState)5 != ClickState)
            {
                System.Diagnostics.Debug.WriteLine("Left-right click combo lost");
            }
        }

        public static void OnMouseMove(MouseEventArgs e)
        {

        }

        public static int GetButtonValue(MouseButton b)
        {
            return b switch
            {
                MouseButton.Left => 1,
                MouseButton.Middle => 2,
                MouseButton.Right => 4,
                MouseButton.XButton1 => 8,
                MouseButton.XButton2 => 16,
                _ => 0,
            };
        }

    }
}

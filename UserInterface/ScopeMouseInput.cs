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
        public static Vector64 clickStartPoint { get; private set; }
        public static Vector64 clickDragPoint { get; private set; }
        public static Vector64 clickEndPoint { get; private set; }
        private static ref Settings.ViewSettings ViewSettings
        {
            get => ref ProgramData.ViewSettings;
        }

        public static InputData OnMouseWheel(MouseWheelEventArgs e)
        {
            SKElement canvas = ProgramData.MainWindow.ScopeCanvas;

            Point mousePos = e.GetPosition(canvas);
            SKSize size = canvas.CanvasSize;
            if (mousePos.X >= 0 && mousePos.X <= size.Width && mousePos.Y >= 0 && mousePos.Y < size.Height)
            {
                int change = Math.Sign(e.Delta);
                ViewSettings.IncrementZoom(-change);
            }

            return new MouseInputData { RequiresRedraw = true, MouseButtons = ClickState };
        }

        public static InputData OnMouseDown (MouseButtonEventArgs e)
        {
            ClickState = (MouseClickState)(GetButtonValue(e.ChangedButton) | (int)ClickState);

            Point screenPt = e.GetPosition(ProgramData.MainWindow.ScopeCanvas);

            clickStartPoint = clickToPointOnEarth(screenPt);
            clickDragPoint = clickStartPoint;
            bool redraw = false;

            if(ClickState == MouseClickState.Left && CheckDoubleClick())
            {
                redraw = true;
                RecenterCamera(clickToScreenPoint(screenPt));
            }
            else if (ClickState == (MouseClickState)5)
            {
                System.Diagnostics.Debug.WriteLine("Left-Right Click Combo fired");
            }

            return new MouseInputData { DoubleClicked = true, RequiresRedraw = redraw, MouseButtons = ClickState };
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

        public static void RecenterCamera (Vector64 screenClickPt)
        {
            if (screenClickPt.magnitude <= MathL.Conversions.EarthRadius)
            {
                Vector64 WorldOrigin = ViewSettings.CameraMatrix.PointToTangentSpace((0, 0, 0));

                double r = MathL.Conversions.EarthRadius;
                double rawIntersect = Math.Sqrt(r * r - screenClickPt.y * screenClickPt.y - screenClickPt.z * screenClickPt.z);

                Vector64 WorldIntersectPos = ViewSettings.CameraMatrix.PointToWorldSpace((rawIntersect + WorldOrigin.x, screenClickPt.y, screenClickPt.z));

                //TrackDatabase.UncorrelatedData[0].Position = WorldIntersectPos;

                LatLonCoord newCenter = MathL.Conversions.XYZToLL(WorldIntersectPos, r);
                ViewSettings.UpdateViewPosition(newCenter);
            }
        }

        public static InputData OnMouseUp(MouseButtonEventArgs e)
        {
            int buttonValue = GetButtonValue(e.ChangedButton);
            clickEndPoint = clickToPointOnEarth(e.GetPosition(ProgramData.MainWindow.ScopeCanvas));

            var prevClick = ClickState;
            ClickState = (MouseClickState)(buttonValue ^ (int)ClickState);

            if((prevClick == (MouseClickState)5) && (MouseClickState)5 != ClickState)
            {
                System.Diagnostics.Debug.WriteLine("Left-right click combo lost");
            }

            return new MouseInputData { MouseButtons = ClickState, RequiresRedraw = true };
        }

        public static InputData OnMouseMove(MouseEventArgs e)
        {
            if (ClickState != MouseClickState.None)
            {
                clickDragPoint = clickToPointOnEarth(e.GetPosition(ProgramData.MainWindow.ScopeCanvas));
            }

            return new MouseInputData { MouseButtons = ClickState, RequiresRedraw = true };
        }

        private static Vector64 clickToPointOnEarth (Point pt)
        {
            Vector64 camPoint = clickToScreenPoint(pt);
            Vector64 worldPos;
            Vector64 WorldOrigin = ViewSettings.CameraMatrix.PointToTangentSpace((0, 0, 0));

            double r = MathL.Conversions.EarthRadius;
            double rawIntersect = Math.Sqrt(r * r - camPoint.y * camPoint.y - camPoint.z * camPoint.z);


            if (camPoint.magnitude <= MathL.Conversions.EarthRadius)
            {
                worldPos = ViewSettings.CameraMatrix.PointToWorldSpace((rawIntersect + WorldOrigin.x, camPoint.y, camPoint.z));
            }
            else {
                Vector64 atEdge = (0, camPoint.y, camPoint.z);
                atEdge = atEdge.normalized * MathL.Conversions.EarthRadius;

                worldPos = ViewSettings.CameraMatrix.PointToWorldSpace((WorldOrigin.x, atEdge.y, atEdge.z));
            }

            return worldPos;
        }

        private static Vector64 clickToScreenPoint(Point pt)
        {
            SKSize size = ProgramData.MainWindow.ScopeCanvas.CanvasSize;

            double y = pt.X - size.Width / 2;
            double z = pt.Y - size.Height / 2;

            return new Vector64(0, y, z) * ViewSettings.VerticalFOV / size.Height;
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

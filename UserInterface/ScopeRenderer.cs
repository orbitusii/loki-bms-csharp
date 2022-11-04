using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using loki_bms_csharp.Database;
using loki_bms_csharp.UserInterface.Maps;

namespace loki_bms_csharp.UserInterface
{
    class ScopeRenderer: IDisposable
    {
        public SKImageInfo Info { get; private set; }
        public int Width
        {
            get
            { return Info.Width; }
        }
        public int Height
        {
            get { return Info.Height; }
        }

        public SKSurface Surface { get; private set; }
        public SKCanvas Canvas { get; private set; }
        public MathL.TangentMatrix CameraMatrix;
        public double VerticalSize = 2;
        public double PixelsPerUnit
        {
            get { return Height / VerticalSize; }
        }

        public void SetVerticalSize (double vSize)
        {
            VerticalSize = vSize;
            //CameraMatrix.SetScale(VerticalSize);
        }

        public ScopeRenderer (SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs args, MathL.TangentMatrix cameraMatrix)
        {
            Info = args.Info;
            Surface = args.Surface;
            Canvas = Surface.Canvas;
            CameraMatrix = cameraMatrix;

            Canvas.Clear(SKColors.Black);
        }

        public void DrawAxisLines()
        {
            DrawRay((0, 0, 0), (0, 0, 1), 0.25, SKColors.Blue, 3, true);
            DrawRay((0, 0, 0), (0, 1, 0), 0.25, SKColors.Green, 3, true);
            DrawRay((0, 0, 0), (1, 0, 0), 6378137, SKColors.Red, 3, false);
        }

        public void DrawLine (Vector64 from, Vector64 to, SKColor color, float width)
        {
            Vector64 localFrom = CameraMatrix.PointToTangentSpace(from);
            Vector64 localTo = CameraMatrix.PointToTangentSpace(to);

            SKPoint ptFrom = GetScreenPoint(localFrom);
            SKPoint ptTo = GetScreenPoint(localTo);

            SKPaint stroke = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = color,
                StrokeWidth = width,
            };

            Canvas.DrawLine(ptFrom, ptTo, stroke);
        }

        public void DrawRay (Vector64 from, Vector64 towards, double length, SKColor color, float width = 3, bool isRelativeToScreen = false)
        {
            Vector64 localFrom = CameraMatrix.PointToTangentSpace(from);
            Vector64 localTo = CameraMatrix.PointToTangentSpace(towards) * length * (isRelativeToScreen ? Height : PixelsPerUnit);

            SKPoint ptFrom = GetScreenPoint(localFrom);
            SKPoint ptTo = new SKPoint((float)localTo.y + Width / 2, (float)localTo.z + Height / 2);

            SKPaint stroke = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = color,
                StrokeWidth = width,
            };

            Canvas.DrawLine(ptFrom, ptTo, stroke);
        }

        public void DrawCircle(Vector64 center, double radius, SKColor color, bool isRadiusInWorldUnits = true)
        {
            if (isRadiusInWorldUnits)
            {
                radius *= PixelsPerUnit;
            }

            Vector64 localCenter = CameraMatrix.PointToTangentSpace(center);
            SKPoint canvasCenter = GetScreenPoint(localCenter);

            SKPaint brush = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = color,
            };

            Canvas.DrawCircle(canvasCenter, (float)radius, brush);
        }

        public void DrawFromDatabase ()
        {
            foreach(TrackNumber tn in TrackDatabase.LiveTracks.Keys)
            {
                TrackFile track = TrackDatabase.LiveTracks[tn];

                SKPaint brush = TrackDatabase.ColorByFFS[track.FFS];

                //Base symbol
                DrawSingleItem(track, brush);
                //Velocity leader
                DrawLine(track.Position, track.Position + track.Velocity, SKColors.White, 1);
            }

            foreach(var datum in TrackDatabase.UncorrelatedData)
            {
                DrawSingleItem(datum, TrackDatabase.DatumBrush);
            }
        }

        public void DrawSingleItem (IKinematicData track, SKPaint brush)
        {
            Vector64 screenPos = CameraMatrix.PointToTangentSpace(track.Position);

            if (Math.Abs(screenPos.x) <= MathL.Conversions.EarthRadius)
            {
                SKPoint canvasPos = GetScreenPoint(screenPos);

                Canvas.DrawCircle(canvasPos, 4, brush);
            }
        }

        public void DrawLandmassGeometry ()
        {
            SKMatrix screenMatrix = SKMatrix.CreateTranslation(Width / 2, Height / 2);
            screenMatrix.ScaleX = (float)(MathL.Conversions.EarthRadius * PixelsPerUnit);
            screenMatrix.ScaleY = screenMatrix.ScaleX;

            SKPaint landPaint = new SKPaint { Color = SKColor.FromHsl(0,0,30), Style = SKPaintStyle.Fill, StrokeWidth = 3 };
            //System.Diagnostics.Debug.WriteLine($"{DateTime.Now:h:mm:ss:fff} [ScopeRenderer]: Drawing {MapData.CachedPaths.Length} Landmasses...");

            foreach (SKPath path in MapData.CachedPaths)
            {
                using (SKPath clone = new SKPath(path))
                {
                    clone.Transform(screenMatrix);

                    //System.Diagnostics.Debug.WriteLine($"{DateTime.Now:h:mm:ss:fff} [ScopeRenderer]: path starts at {clone.Points[2]}");
                    Canvas.DrawPath(clone, landPaint);
                }
            }

            //System.Diagnostics.Debug.WriteLine($"{DateTime.Now:h:mm:ss:fff} [ScopeRenderer]: Done!");
        }

        public SKPoint GetScreenPoint (Vector64 screenPos)
        {
            Vector64 scaled = screenPos * PixelsPerUnit;

            return new SKPoint((float)scaled.y + Width / 2, (float)scaled.z + Height / 2);
        }

        public void Dispose ()
        {
        }
    }
}

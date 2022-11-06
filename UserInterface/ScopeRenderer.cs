using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using loki_bms_csharp.Database;
using loki_bms_csharp.UserInterface;

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

        public void DrawEarth()
        {
            DrawCircle((0, 0, 0), MathL.Conversions.EarthRadius, SKColor.FromHsl(215, 30, 8));

            SKPaint landPaint = new SKPaint { Color = SKColor.Parse("#303030"), Style = SKPaintStyle.Fill };
            SKPaint mapsPaint = new SKPaint { Color = SKColor.Parse("#505050"), Style = SKPaintStyle.Stroke, StrokeWidth = 1 };

            DrawWorldGeometry(ProgramData.WorldLandmasses, landPaint);
            DrawWorldGeometry(ProgramData.DCSMaps, mapsPaint);
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

        public void DrawMeasureLine (Vector64 from, Vector64 to, SKColor color, float width)
        {
            double angle = Vector64.AngleBetween(from, to);
            double arcLength = MathL.Conversions.EarthRadius * angle;

            if (arcLength < 100) return;

            double circumferenceFraction = Math.Round(arcLength / MathL.Conversions.EarthCircumference * 72);

            Vector64 segment_start = from;

            for (int i = 1; i < (int)circumferenceFraction; i++)
            {
                double t = i / circumferenceFraction;
                Vector64 segment_end = Vector64.Slerp(from, to, t);

                DrawLine(segment_start, segment_end, color, width);

                segment_start = segment_end;
            }
            DrawLine(segment_start, to, color, width);

            SKPoint textPoint = GetScreenPoint(CameraMatrix.PointToTangentSpace(to)) + new SKPoint(10, -10);
            SKPaint paint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.StrokeAndFill,

            };

            Canvas.DrawText($"{Math.Round(arcLength * MathL.Conversions.MetersToNM,1)} NM", textPoint, paint);
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

        public void DrawWorldGeometry (Geometry.MapGeometry mapData, SKPaint paint)
        {
            SKMatrix screenMatrix = SKMatrix.CreateTranslation(Width / 2, Height / 2);
            screenMatrix.ScaleX = (float)(MathL.Conversions.EarthRadius * PixelsPerUnit);
            screenMatrix.ScaleY = screenMatrix.ScaleX;

            //System.Diagnostics.Debug.WriteLine($"{DateTime.Now:h:mm:ss:fff} [ScopeRenderer]: Drawing {MapData.CachedPaths.Length} Landmasses...");

            foreach (SKPath path in mapData.CachedPaths)
            {
                if (Canvas.QuickReject(path)) continue;

                using (SKPath clone = new SKPath(path))
                {
                    clone.Transform(screenMatrix);

                    Canvas.DrawPath(clone, paint);
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using loki_bms_csharp.Database;
using loki_bms_csharp.UserInterface;

namespace loki_bms_csharp.UserInterface
{
    public class ScopeRenderer: IDisposable
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

        public List<TrackHotspot> TrackClickHotspots = new List<TrackHotspot>();
        public int ClickThrough = -1;
        
        public ScopeRenderer () { }

        public int GetTrackAtPosition (SKPoint ScreenPoint)
        {
            lock(TrackClickHotspots)
            {
                List<TrackHotspot> hotspots = TrackClickHotspots.FindAll(x => x.Bounds.Contains(ScreenPoint));

                ClickThrough = hotspots.Count > 1 ? (ClickThrough + 1) % hotspots.Count : 0;
                TrackHotspot hotspot = hotspots.Count > 0 ? hotspots[ClickThrough] : null;

                int index = hotspot?.Index ?? -1;
                return index;
            }
        }

        public void Redraw (SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs args, MathL.TangentMatrix cameraMatrix, double VFov)
        {
            Info = args.Info;
            if(Surface == null || Surface != args.Surface)
            {
                Surface = args.Surface;
            }
            if(Canvas == null || Canvas != Surface.Canvas)
            {
                Canvas = Surface.Canvas;
            }
            CameraMatrix = cameraMatrix;

            Canvas.Clear(SKColors.Black);

            SetVerticalSize(VFov);

            DrawEarth();
            if (ProgramData.ViewSettings.DrawDebug)
            {
                DrawAxisLines();
            }

            //DrawGeometry();
            DrawFromDatabase();

            if (ScopeMouseInput.ClickState == MouseClickState.Left)
            {
                DrawMeasureLine(ScopeMouseInput.clickStartPoint, ScopeMouseInput.clickDragPoint, SKColors.White, 1);
            }
        }
        public void SetVerticalSize(double vSize)
        {
            VerticalSize = vSize;
            //CameraMatrix.SetScale(VerticalSize);
        }

        public void DrawEarth()
        {
            SKPaint brush = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColor.FromHsl(215, 30, 8),
            };

            DrawCircle((0, 0, 0), MathL.Conversions.EarthRadius, brush);

            SKPaint landPaint = new SKPaint { Color = SKColor.Parse("#303030"), Style = SKPaintStyle.Fill };
            SKPaint mapsPaint = new SKPaint { Color = SKColor.Parse("#505050"), Style = SKPaintStyle.Stroke, StrokeWidth = 1 };

            DrawWorldGeometry(ProgramData.WorldLandmasses, landPaint);
            DrawWorldGeometry(ProgramData.DCSMaps, mapsPaint);
        }

        public void DrawWorldGeometry(Geometry.MapGeometry mapData, SKPaint paint)
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

        public void DrawFromDatabase()
        {
            if (ProgramData.ViewSettings.ZoomIncrement <= 9)
            {
                List<TrackDatum> dataSymbols = new List<TrackDatum>(TrackDatabase.ProcessedData);

                foreach (var datum in dataSymbols)
                {
                    DrawDatum(datum);
                }
            }

            TrackClickHotspots.Clear();

            if(ProgramData.TrackSelection.Track != null)
            {
                SKPaint brush = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColor.FromHsl(0, 0, 255, 196),
                    StrokeWidth = 1,
                };

                DrawCircle(ProgramData.TrackSelection.Track.Position, 16, brush, false);
            }

            for (int i = 0; i < TrackDatabase.LiveTracks.Count; i++)
            {
                TrackFile track = TrackDatabase.LiveTracks[i];

                //Base symbol
                DrawTrack(track, i, 6);
                //Velocity leader
                DrawLine(track.Position, track.Position + track.Velocity * 60, SKColors.White, 1);
            }
        }

        public void DrawDatum(TrackDatum datum)
        {
            Vector64 screenPos = CameraMatrix.PointToTangentSpace(datum.Position);
            var canvasPos = GetScreenPoint(screenPos);

            if (Math.Abs(screenPos.x) <= MathL.Conversions.EarthRadius && Canvas.LocalClipBounds.Contains(canvasPos))
            {
                var path = new SKPath(datum.Origin.GetSKPath) ?? new SKPath();
                var paint = datum.Origin.GetSKPaint ?? new SKPaint { Color = SKColors.Coral };
                path.Transform(SKMatrix.CreateScaleTranslation(1, 1, canvasPos.X, canvasPos.Y));

                Canvas.DrawPath(path, paint);
            }
        }

        public void DrawTrack(TrackFile track, int index, float size = 4)
        {
            Vector64 screenPos = CameraMatrix.PointToTangentSpace(track.Position);
            SKPoint canvasPos = GetScreenPoint(screenPos);

            if (Math.Abs(screenPos.x) <= MathL.Conversions.EarthRadius && Canvas.LocalClipBounds.Contains(canvasPos))
            {
                // TODO : find a spectype path first, then a category symbol, then a general symbol
                var originalPath = ProgramData.TrackSymbols[track.Category][track.FFS]?.SKPath ?? ProgramData.TrackSymbols[TrackCategory.None][track.FFS].SKPath;
                var clonedPath = new SKPath(originalPath);

                var fillPaint = TrackDatabase.FillByFFS[track.FFS];
                var strokePaint = TrackDatabase.StrokeByFFS[track.FFS];

                clonedPath.Transform(SKMatrix.CreateScaleTranslation(0.5f, 0.5f, canvasPos.X, canvasPos.Y));

                TrackHotspot hotSpot = new TrackHotspot { Bounds = clonedPath.Bounds, Index = index };
                TrackClickHotspots.Add(hotSpot);

                Canvas.DrawPath(clonedPath, fillPaint);
                Canvas.DrawPath(clonedPath, strokePaint);
            }
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
            var surfaceData = MathL.Conversions.GetSurfaceMotion(from, to - from);
            double heading = Math.Round(surfaceData.heading * MathL.Conversions.ToDegrees, 0);

            SKPaint paint = new SKPaint(new SKFont(SKTypeface.Default, 16))
            {
                Color = SKColors.White,
                Style = SKPaintStyle.StrokeAndFill,

            };

            Canvas.DrawText($"{heading:000}/{Math.Round(arcLength * MathL.Conversions.MetersToNM,0)} NM", textPoint, paint);
        }

        public void DrawCircle(Vector64 center, double radius, SKPaint brush, bool isRadiusInWorldUnits = true)
        {
            if (isRadiusInWorldUnits)
            {
                radius *= PixelsPerUnit;
            }

            Vector64 localCenter = CameraMatrix.PointToTangentSpace(center);
            SKPoint canvasCenter = GetScreenPoint(localCenter);

            Canvas.DrawCircle(canvasCenter, (float)radius, brush);
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

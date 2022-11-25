using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using loki_bms_csharp.Database;
using loki_bms_csharp.UserInterface;
using loki_bms_csharp.Geometry;
using loki_bms_csharp.Settings;

namespace loki_bms_csharp.UserInterface
{
    public class ScopeRenderer
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

        protected GeometrySettings Geometries
        {
            get => ProgramData.GeometrySettings;
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

            DrawGeometry();
            DrawBullseye();
            DrawFromDatabase();

            if (ScopeMouseInput.ClickState == MouseClickState.Left)
            {
                try
                {
                    DrawMeasureLine(ScopeMouseInput.clickStartPoint, ScopeMouseInput.currentMousePoint, SKColors.White, 1);
                }
                catch { }
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
                Color = SKColor.Parse(Geometries.OceanColor),
            };

            DrawCircle((0, 0, 0), MathL.Conversions.EarthRadius, brush);

            SKPaint landPaint = new SKPaint { Color = SKColor.Parse(Geometries.LandmassColor), Style = SKPaintStyle.Fill };

            DrawWorldGeometry(Geometries.Landmasses, landPaint);
        }

        public void DrawWorldGeometry(MapGeometry mapData, SKPaint paint)
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

        public void DrawGeometry()
        {
            SKMatrix screenMatrix = SKMatrix.CreateTranslation(Width / 2, Height / 2);
            screenMatrix.ScaleX = (float)(MathL.Conversions.EarthRadius * PixelsPerUnit);
            screenMatrix.ScaleY = screenMatrix.ScaleX;

            lock (Geometries.Geometries)
            {
                foreach (MapGeometry geom in Geometries.Geometries)
                {
                    if (!geom.Visible) continue;

                    SKPaint fill = geom.GetFillBrush;
                    SKPaint stroke = geom.GetStrokeBrush;

                    foreach (SKPath path in geom.CachedPaths)
                    {
                        if (Canvas.QuickReject(path)) continue;

                        using (SKPath clone = new SKPath(path))
                        {
                            clone.Transform(screenMatrix);

                            Canvas.DrawPath(clone, fill);
                            Canvas.DrawPath(clone, stroke);
                        }
                    }
                }
            }
        }

        private void DrawBullseye()
        { 
            Vector64 BEPos = ProgramData.BullseyeCartesian;
            SKPaint BEBlue = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Blue,
                StrokeWidth = 3
            };

            DrawCircle(BEPos, 8, BEBlue, false);
            DrawCircle(BEPos, 2, BEBlue, false);

            var relToBE = ProgramData.GetPositionRelativeToBullseye(ScopeMouseInput.currentMousePoint);

            SKPoint bottomRight = new SKPoint(Width - 100, Height - 20);
            Canvas.DrawText($"BE: {relToBE.heading_rads * MathL.Conversions.ToDegrees:000},{relToBE.dist * MathL.Conversions.MetersToNM:0}",
                bottomRight, new SKPaint { TextSize = 16, Style = SKPaintStyle.Fill, Color = SKColors.White });
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
                DrawTrack(track, i, 16);
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

        public void DrawTrack(TrackFile track, int index, float size = 16)
        {
            Vector64 screenPos = CameraMatrix.PointToTangentSpace(track.Position);
            SKPoint canvasPos = GetScreenPoint(screenPos);

            if (Math.Abs(screenPos.x) <= MathL.Conversions.EarthRadius && Canvas.LocalClipBounds.Contains(canvasPos))
            {
                var originalPath = ProgramData.SpecTypeSymbols[track.SpecType]?.SKPath ?? null;
                float rotation = 0;
                float extraScale = 1;

                if(originalPath == null)
                {
                    originalPath =
                        ProgramData.TrackSymbols[track.Category][track.FFS]?.SKPath
                        ?? ProgramData.TrackSymbols[TrackCategory.None][track.FFS].SKPath;
                }
                else
                {
                    rotation = (float)track.Heading_Rads;
                    extraScale = 1.5f;
                }

                var clonedPath = new SKPath(originalPath);
                float width = clonedPath.Bounds.Width;
                float scale = size / width * extraScale;

                var fillPaint = TrackDatabase.FillByFFS[track.FFS];
                var strokePaint = TrackDatabase.StrokeByFFS[track.FFS];

                clonedPath.Transform(SKMatrix.CreateScaleTranslation(scale, scale, canvasPos.X, canvasPos.Y));
                clonedPath.Transform(SKMatrix.CreateRotation(rotation, clonedPath.Bounds.MidX, clonedPath.Bounds.MidY));

                SKRect bounds = clonedPath.Bounds;
                bounds.Inflate(4, 4);
                TrackHotspot hotSpot = new TrackHotspot { Bounds = bounds, Index = index };
                TrackClickHotspots.Add(hotSpot);

                Canvas.DrawPath(clonedPath, fillPaint);
                Canvas.DrawPath(clonedPath, strokePaint);

                Canvas.DrawRect(bounds.Right-2, bounds.Bottom - 12, 24, 16, new SKPaint { Color = SKColor.Parse("#84000000"), Style = SKPaintStyle.Fill });
                Canvas.DrawText($"{track.Altitude * MathL.Conversions.MetersToFeet / 100:F0}", new SKPoint(bounds.Right, bounds.Bottom), new SKPaint {  Color = SKColors.White, Style = SKPaintStyle.Fill });
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
    }
}

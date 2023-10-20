using loki_bms_common.Database;
using loki_bms_csharp.Extensions;
using loki_bms_csharp.Geometry;
using loki_bms_csharp.Geometry.SVG;
using loki_bms_csharp.Settings;
using loki_bms_csharp.UserInterface.Labels;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls.Primitives;

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

        protected TrackDatabase DB => ProgramData.Database;
        protected ColorSettings Colors => ProgramData.ColorSettings;

        public SKSurface Surface { get; private set; }
        public SKCanvas Canvas { get; private set; }
        public TangentMatrix CameraMatrix;
        public double VerticalSize = 2;
        public double PixelsPerUnit
        {
            get { return Height / VerticalSize; }
        }

        public List<TrackHotspot> TrackClickHotspots = new List<TrackHotspot>();
        public int ClickThrough = -1;

        public Dictionary<LokiDataSource, SKPaint> DatumPaintCache = new Dictionary<LokiDataSource, SKPaint>();

        public ScopeLabel<TacticalElement> BullseyeLabel = new ScopeLabel<TacticalElement>()
        {
            Margins = 3,
            LineSpacing = 3,
            LabelItems = new List<LabelItem> { new LabelItem.NameLabel() }
        };
        public ScopeLabel<TrackFile> TrackLabel = new ScopeLabel<TrackFile>()
        {
            Margins = 3,
            LineSpacing = 3,
            LabelItems = new List<LabelItem>
            {
                new LabelItem.TNLabel(), new LabelItem.LabelSeparator(), new LabelItem.AltitudeLabel(), new LabelItem.LabelNewLine(),
                new LabelItem.NameLabel(),
            }
        };
        internal SKPaint LabelBG = new SKPaint { Color = SKColor.Parse("#C5000000"), Style = SKPaintStyle.Fill };
        internal SKPaint LabelText = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Fill };

        public ScopeRenderer() { }

        public ISelectableObject GetObjectAtPosition(SKPoint ScreenPoint)
        {
            lock (TrackClickHotspots)
            {
                List<TrackHotspot> hotspots = TrackClickHotspots.FindAll(x => x.Bounds.Contains(ScreenPoint));

                ClickThrough = hotspots.Count > 1 ? (ClickThrough + 1) % hotspots.Count : 0;
                TrackHotspot hotspot = hotspots.Count > 0 ? hotspots[ClickThrough] : null;

                return hotspot?.Target;
            }
        }

        /// <summary>
        /// DEPRECATED
        /// </summary>
        /// <param name="ScreenPoint"></param>
        /// <returns></returns>
        public int GetTrackAtPosition(SKPoint ScreenPoint)
        {
            lock (TrackClickHotspots)
            {
                List<TrackHotspot> hotspots = TrackClickHotspots.FindAll(x => x.Bounds.Contains(ScreenPoint));

                ClickThrough = hotspots.Count > 1 ? (ClickThrough + 1) % hotspots.Count : 0;
                TrackHotspot hotspot = hotspots.Count > 0 ? hotspots[ClickThrough] : null;

                int index = hotspot?.Index ?? -1;
                return index;
            }
        }

        public void Redraw(SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs args, TangentMatrix cameraMatrix, double VFov)
        {
            Info = args.Info;
            if (Surface == null || Surface != args.Surface)
            {
                Surface = args.Surface;
            }
            if (Canvas == null || Canvas != Surface.Canvas)
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
                Color = SKColor.Parse(Colors.OceanColor),
            };

            DrawCircle((0, 0, 0), Conversions.EarthRadius, brush);

            SKPaint landPaint = new SKPaint { Color = SKColor.Parse(Colors.LandmassColor), Style = SKPaintStyle.Fill };

            DrawWorldGeometry(Geometries.Landmasses, landPaint);
        }

        public void DrawWorldGeometry(MapGeometry mapData, SKPaint paint)
        {
            SKMatrix screenMatrix = SKMatrix.CreateTranslation(Width / 2, Height / 2);
            screenMatrix.ScaleX = (float)(Conversions.EarthRadius * PixelsPerUnit);
            screenMatrix.ScaleY = screenMatrix.ScaleX;

            //System.Diagnostics.Debug.WriteLine($"{DateTime.Now:h:mm:ss:fff} [ScopeRenderer]: Drawing {MapData.CachedPaths.Length} Landmasses...");

            foreach (SKPath path in mapData.CachedPaths)
            {

                using (SKPath clone = new SKPath(path))
                {
                    clone.Transform(screenMatrix);

                    if (Canvas.QuickReject(clone)) continue;

                    Canvas.DrawPath(clone, paint);
                }
            }

            //System.Diagnostics.Debug.WriteLine($"{DateTime.Now:h:mm:ss:fff} [ScopeRenderer]: Done!");
        }

        public void DrawGeometry()
        {
            SKMatrix screenMatrix = SKMatrix.CreateTranslation(Width / 2, Height / 2);
            screenMatrix.ScaleX = (float)(Conversions.EarthRadius * PixelsPerUnit);
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
                        using (SKPath clone = new SKPath(path))
                        {
                            clone.Transform(screenMatrix);

                            if (Canvas.QuickReject(clone)) continue;

                            Canvas.DrawPath(clone, fill);
                            Canvas.DrawPath(clone, stroke);
                        }
                    }
                }
            }
        }

        private void DrawBullseye()
        {
            Vector64 mousePos = ScopeMouseInput.currentMousePoint;
            Vector64 BEPos = ProgramData.BullseyeCartesian;

            int count = ProgramData.Bullseyes.Count;

            for (int i = 0; i < count; i++)
            {
                ISelectableObject BE = ProgramData.Bullseyes[i];
                SKPoint bottomRight = new SKPoint(Width - 100, Height - (20 * (count - i)));

                double dist = Conversions.GetGreatCircleDistance(BE.Position, mousePos);
                double heading = Conversions.GetBearing(BE.Position, mousePos);
                string label = $"BE{i}";

                string BullseyeText = $"BE{i}: {heading * Conversions.ToDegrees:000},{dist * Conversions.MetersToNM:0}";

                Canvas.DrawText(BullseyeText, bottomRight, new SKPaint { TextSize = 16, Style = SKPaintStyle.Fill, Color = SKColors.White });

                // Drawing a tag next to the bullseye for clarity!
                Vector64 screenPos = CameraMatrix.PointToTangentSpace(BE.Position);

                if (CheckVisible(screenPos, out SKPoint canvasPos))
                {
                    DrawLabel(BullseyeLabel, BE, canvasPos);
                }
            }
        }

        public void DrawFromDatabase()
        {
            if (DB is null) return;

            if (ProgramData.ViewSettings.ZoomIncrement <= 11)
            {
                foreach (LokiDataSource ds in DB.DataSources)
                {
                    DatumPaintCache[ds] = ds.GetPaint();
                }

                List<TrackDatum> dataSymbols = new List<TrackDatum>(DB.ProcessedData);

                foreach (var datum in dataSymbols)
                {
                    DrawDatum(datum);
                }
            }

            TrackClickHotspots.Clear();

            if (ProgramData.SelectedObject != null)
            {
                SKPaint brush = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColor.FromHsl(0, 0, 255, 196),
                    StrokeWidth = 1,
                };

                DrawCircle(ProgramData.SelectedObject.Position, 16, brush, false);
            }

            foreach (TacticalElement TE in DB.TEs)
            {
                //Base symbol
                DrawTE(TE, 16);
            }

            int clickIndex = 0;

            lock(DB.LiveTracks)
            {
                foreach (TrackFile track in DB.LiveTracks)
                {
                    //Base symbol
                    DrawTrack(track, clickIndex++, 16);
                    //Velocity leader
                    DrawLine(track.Position, track.Position + track.Velocity * 60, SKColors.White, 1);
                }
            }
        }

        public void DrawDatum(TrackDatum datum)
        {
            Vector64 screenPos = CameraMatrix.PointToTangentSpace(datum.Position);

            if (CheckVisible(screenPos, out SKPoint canvasPos))
            {
                var path = new SKPath(ProgramData.DataSymbols.First(x => x.name == datum.Origin.DataSymbol)?.SKPath) ?? new SKPath();
                SKPaint paint = DatumPaintCache[datum.Origin];
                path.Transform(SKMatrix.CreateScaleTranslation(1, 1, canvasPos.X, canvasPos.Y));

                Canvas.DrawPath(path, paint);
            }
        }

        public void DrawTrack(TrackFile track, int index, float size = 16)
        {
            Vector64 screenPos = CameraMatrix.PointToTangentSpace(track.Position);

            if (CheckVisible(screenPos, out SKPoint canvasPos))
            {
                SKPath originalPath;

                float rotation = 0;
                float extraScale = 1;

                if (ProgramData.SpecTypeSymbols.TryGetValue(track.SpecType, out var svgPath) && svgPath is SVGPath)
                {
                    originalPath = svgPath.SKPath;
                    rotation = (float)track.Heading_Rads;
                    extraScale = 1.5f;
                }
                else
                {
                    originalPath =
                        ProgramData.TrackSymbols[track.Category][track.FFS]?.SKPath
                        ?? ProgramData.TrackSymbols[TrackCategory.None][track.FFS].SKPath;
                }

                var clonedPath = new SKPath(originalPath);
                float width = clonedPath.Bounds.Width;
                float scale = size / width * extraScale;

                var paints = ProgramData.ColorSettings.GetPaint(track);

                clonedPath.Transform(SKMatrix.CreateScaleTranslation(scale, scale, canvasPos.X, canvasPos.Y));
                clonedPath.Transform(SKMatrix.CreateRotation(rotation, clonedPath.Bounds.MidX, clonedPath.Bounds.MidY));

                SKRect bounds = clonedPath.Bounds;
                AddClickHotSpot(clonedPath.Bounds, track);

                Canvas.DrawPath(clonedPath, paints.fill);
                Canvas.DrawPath(clonedPath, paints.stroke);

                DrawLabel(TrackLabel, track, canvasPos);
            }
        }

        public void DrawTE(TacticalElement TE, float size = 16)
        {
            Vector64 screenPos = CameraMatrix.PointToTangentSpace(TE.Position);

            if (CheckVisible(screenPos, out SKPoint canvasPos))
            {
                SKPath path = new SKPath(ProgramData.TrackSymbols[TrackCategory.None][TE.FFS].SKPath);

                var paints = ProgramData.ColorSettings.GetPaint(TE);

                float width = path.Bounds.Width;
                float scale = size / width;
                path.Transform(SKMatrix.CreateScaleTranslation(scale, scale, canvasPos.X, canvasPos.Y));

                AddClickHotSpot(path.Bounds, TE);

                Canvas.DrawPath(path, paints.fill);
                Canvas.DrawPath(path, paints.stroke);
            }
        }

        private bool CheckVisible(Vector64 point, out SKPoint canvasPos)
        {
            canvasPos = GetScreenPoint(point);
            return Math.Abs(point.x) <= Conversions.EarthRadius && Canvas.LocalClipBounds.Contains(canvasPos);
        }

        private void AddClickHotSpot(SKRect bounds, ISelectableObject target)
        {
            bounds.Inflate(4, 4);
            TrackHotspot hotSpot = new TrackHotspot { Bounds = bounds, Target = target };
            TrackClickHotspots.Add(hotSpot);
        }

        private void DrawLabel<T> (ScopeLabel<T> label, ISelectableObject target, SKPoint canvasPos) where T: ISelectableObject
        {
            //Canvas.DrawText(label, new SKPoint(canvasPos.X + 12, canvasPos.Y+20),new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Fill });
            string[] labelVals = label.Evaluate(target, out var text, out var border);

            Canvas.DrawRect(canvasPos.X + 10 + border.Left, canvasPos.Y + 8 + border.Top, border.Width, border.Height, LabelBG);
            int heightOffset = 0;
            foreach (string val in labelVals)
            {
                Canvas.DrawText(val, canvasPos.X + 9 + text.Left, canvasPos.Y + 8 + text.Top + heightOffset, BullseyeLabel.Font, LabelText);
                heightOffset += label.charHeight + label.LineSpacing;
            }
        }

        public void DrawAxisLines()
        {
            DrawRay((0, 0, 0), (0, 0, 1), 0.25, SKColors.Blue, 3, true);
            DrawRay((0, 0, 0), (0, 1, 0), 0.25, SKColors.Green, 3, true);
            DrawRay((0, 0, 0), (1, 0, 0), 6378137, SKColors.Red, 3, false);
        }

        public void DrawLine(Vector64 from, Vector64 to, SKColor color, float width)
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

        public void DrawRay(Vector64 from, Vector64 towards, double length, SKColor color, float width = 3, bool isRelativeToScreen = false)
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

        public void DrawMeasureLine(Vector64 from, Vector64 to, SKColor color, float width)
        {
            double angle = Vector64.AngleBetween(from, to);
            double arcLength = Conversions.EarthRadius * angle;

            if (arcLength < 100) return;

            double circumferenceFraction = Math.Round(arcLength / Conversions.EarthCircumference * 72);

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
            var surfaceData = Conversions.GetSurfaceMotion(from, to - from);
            double heading = Math.Round(surfaceData.heading * Conversions.ToDegrees, 0);

            SKPaint paint = new SKPaint(new SKFont(SKTypeface.Default, 16))
            {
                Color = SKColors.White,
                Style = SKPaintStyle.StrokeAndFill,

            };

            Canvas.DrawText($"{heading:000}/{Math.Round(arcLength * Conversions.MetersToNM, 0)} NM", textPoint, paint);
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

        public SKPoint GetScreenPoint(Vector64 screenPos)
        {
            Vector64 scaled = screenPos * PixelsPerUnit;

            return new SKPoint((float)scaled.y + Width / 2, (float)scaled.z + Height / 2);
        }
    }
}

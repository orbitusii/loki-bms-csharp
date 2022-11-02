using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;
using SkiaSharp.Views.WPF;

namespace loki_bms_csharp.UserInterface
{
    class ScopeRenderer
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
            Vector64 localFrom = CameraMatrix.ToLocal(from) * PixelsPerUnit;
            Vector64 localTo = CameraMatrix.ToLocal(to) * PixelsPerUnit;

            SKPoint ptFrom = new SKPoint((float)localFrom.y + Width / 2, (float)localFrom.z + Height/2);
            SKPoint ptTo = new SKPoint((float)localTo.y + Width / 2, (float)localTo.z + Height / 2);

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
            Vector64 localFrom = CameraMatrix.ToLocal(from) * PixelsPerUnit;
            Vector64 localTo = CameraMatrix.ToLocal(towards) * length * (isRelativeToScreen ? Height : PixelsPerUnit);

            SKPoint ptFrom = new SKPoint((float)localFrom.y + Width / 2, (float)localFrom.z + Height / 2);
            SKPoint ptTo = new SKPoint((float)localTo.y + Width / 2, (float)localTo.z + Height / 2);

            SKPaint stroke = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = color,
                StrokeWidth = width,
            };

            Canvas.DrawLine(ptFrom, ptTo, stroke);
        }

        public void DrawCircle (Vector64 center, double radius, SKColor color, bool isRadiusInWorldUnits = true)
        {
            if(isRadiusInWorldUnits)
            {
                radius *= PixelsPerUnit;
            }

            Vector64 localCenter = CameraMatrix.ToLocal(center);

            SKPoint centerPoint = new SKPoint((float)localCenter.y + Width / 2, (float)localCenter.z + Height / 2);

            SKPaint brush = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = color,
            };

            Canvas.DrawCircle(centerPoint, (float)radius, brush);
        }

    }
}

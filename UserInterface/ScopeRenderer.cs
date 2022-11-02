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

    }
}

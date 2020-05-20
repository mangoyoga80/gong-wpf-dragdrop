namespace Showcase.WPF.DragDrop.Models
{
    using System.Windows;
    using System.Windows.Media;
    using GongSolutions.Wpf.DragDrop;

    public class DropTargetInactiveHintAdorner : DropTargetHintAdorner
    {
        private readonly Pen pen;
        private readonly Brush brush;

        public DropTargetInactiveHintAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            this.pen = new Pen(Brushes.DarkGreen, 0.5);
            this.pen.Freeze();
            this.brush = new SolidColorBrush(Colors.DarkGreen) { Opacity = 0.2 };
            this.brush.Freeze();

            this.SetValue(SnapsToDevicePixelsProperty, true);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var visualTarget = this.AdornedElement;
            var translatePoint = visualTarget.TranslatePoint(new Point(), this.AdornedElement);
            var bounds = new Rect(translatePoint,
                                  new Size(visualTarget.RenderSize.Width, visualTarget.RenderSize.Height));
            drawingContext.DrawRectangle(this.brush, this.pen, bounds);
        }
    }

    public class DropTargetActiveHintAdorner : DropTargetAdorner
    {
        private readonly Pen pen;
        private readonly Brush brush;

        public DropTargetActiveHintAdorner(UIElement adornedElement, DropInfo dropInfo)
            : base(adornedElement, dropInfo)
        {
            this.pen = new Pen(SystemColors.ActiveBorderBrush, 2);
            this.pen.Freeze();
            this.brush = new SolidColorBrush(SystemColors.HighlightColor) { Opacity = 0.2 };
            this.brush.Freeze();

            this.SetValue(SnapsToDevicePixelsProperty, true);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var visualTarget = this.DropInfo.VisualTarget;
            if (visualTarget != null)
            {
                var translatePoint = visualTarget.TranslatePoint(new Point(), this.AdornedElement);
                translatePoint.Offset(1, 1);
                var bounds = new Rect(translatePoint,
                                      new Size(visualTarget.RenderSize.Width - 2, visualTarget.RenderSize.Height - 2));
                drawingContext.DrawRectangle(this.brush, this.pen, bounds);
            }
        }
    }
}
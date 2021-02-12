namespace GongSolutions.Wpf.DragDrop
{
    using System;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;

    public abstract class DropTargetHintAdorner : Adorner
    {
        public DropTargetHintAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            this.IsHitTestVisible = false;
            this.AllowDrop = false;
            this.SnapsToDevicePixels = true;
            this.m_AdornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
            this.m_AdornerLayer.Add(this);
        }

        /// <summary>
        /// Gets or Sets the pen which can be used for the render process.
        /// </summary>
        public Pen Pen { get; set; } = new Pen(Brushes.Gray, 2);

        public void Detatch()
        {
            this.m_AdornerLayer.Remove(this);
        }

        internal static DropTargetHintAdorner Create(Type type, UIElement adornedElement)
        {
            if (!typeof(DropTargetHintAdorner).IsAssignableFrom(type))
            {
                throw new InvalidOperationException("The requested adorner class does not derive from DropTargetHintAdorner.");
            }
            return type.GetConstructor(new[] { typeof(UIElement) })?.Invoke(new object[] { adornedElement}) as DropTargetHintAdorner;
        }

        private readonly AdornerLayer m_AdornerLayer;
    }
}
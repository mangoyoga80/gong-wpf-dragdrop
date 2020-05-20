namespace GongSolutions.Wpf.DragDrop
{
    using System;
    using System.Windows;
    using GongSolutions.WPF.DragDrop.Utilities;

    /// <summary>
    /// Holds weak references for UIElements with <see cref="IHintDropTarget"/> handlers and visibility handlers to them.
    /// </summary>
    internal class HintTargetElementWrapper : IDisposable
    {
        private readonly EventHandler onValueChanged;

        #region Private members
        private readonly WeakReference<UIElement> _element;
        private readonly PropertyChangeNotifier<UIElement> _propertyChangedNotifier;

        #endregion
        /// <summary>
        /// Creates a new element wrapper for observing visibility changes and hold weak references. 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="onValueChanged">The event handler for <see cref="UIElement.IsVisible"/> changes.</param>
        /// <remarks>
        /// In some situations such as when using drag & drop in docked settings, such as ActiproSoftware docking, the user can start
        /// dragging an element to the tab of a Docking/Tool window and drop in a new window. Without handling the changes to visibility,
        /// the hint adorner will not be displayed when the element becomes visible. Likewise, drop hint adorners for elements that gets
        /// hidden should no longer be visible.
        /// </remarks>
        public HintTargetElementWrapper(UIElement element, EventHandler onValueChanged)
        {
            this.onValueChanged = onValueChanged;
            this._element = new WeakReference<UIElement>(element);
            this._propertyChangedNotifier = new PropertyChangeNotifier<UIElement>(element, UIElement.IsVisibleProperty);
            this._propertyChangedNotifier.ValueChanged += this.onValueChanged;
        }

        public bool IsAlive => this._element.TryGetTarget(out var target) && this._propertyChangedNotifier.PropertySource != null;
        public bool TryGetTarget(out UIElement target)
        {
            return this._element.TryGetTarget(out target);

        }
        public void Dispose()
        {
            this._propertyChangedNotifier.ValueChanged -= this.onValueChanged;
            this._propertyChangedNotifier.Dispose();
        }
    }
}
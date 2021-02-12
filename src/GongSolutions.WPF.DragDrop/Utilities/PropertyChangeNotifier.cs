using System;

namespace GongSolutions.WPF.DragDrop.Utilities
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Listen to changes to dependency properties. Gotten from https://agsmith.wordpress.com/2008/04/07/propertydescriptor-addvaluechanged-alternative/, trimmed to it's essensials, and slightly modified.
    /// </summary>
    internal sealed class PropertyChangeNotifier<T> : DependencyObject, IDisposable where T : DependencyObject
    {
        #region Member Variables

        private readonly WeakReference _propertySource;

        #endregion // Member Variables

        #region Constructor
        public PropertyChangeNotifier(T propertySource, DependencyProperty property)
            : this(propertySource, new PropertyPath(property))
        {
        }

        public PropertyChangeNotifier(T propertySource, PropertyPath property)
        {
            if (null == propertySource)
            {
                throw new ArgumentNullException(nameof(propertySource));
            }
            if (null == property)
            {
                throw new ArgumentNullException(nameof(property));
            }
            _propertySource = new WeakReference(propertySource);
            var binding = new Binding { Path = property, Mode = BindingMode.OneWay, Source = propertySource };
            BindingOperations.SetBinding(this, ValueProperty, binding);
        }

        #endregion // Constructor

        #region PropertySource

        public T PropertySource
        {
            get
            {
                try
                {
                    // note, it is possible that accessing the target property
                    // will result in an exception so i’ve wrapped this check
                    // in a try catch
                    return _propertySource.IsAlive ? _propertySource.Target as T : null;
                }
                catch
                {
                    return null;
                }
            }
        }

        #endregion // PropertySource


        #region Value

        /// <summary>
        ///     Identifies the <see cref="Value" /> dependency property
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(PropertyChangeNotifier<T>), new FrameworkPropertyMetadata(null, OnValueChanged));

        /// <summary>
        ///     Returns/sets the value of the property
        /// </summary>
        /// <seealso cref="ValueProperty" />
        [Description("Returns/sets the value of the property")]
        [Category("Behavior")]
        [Bindable(true)]
        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var notifier = (PropertyChangeNotifier<T>)d;
            notifier.ValueChanged?.Invoke(notifier.PropertySource, EventArgs.Empty);
        }

        #endregion //Value

        #region Events

        public event EventHandler ValueChanged;

        #endregion // Events

        #region IDisposable Members

        public void Dispose()
        {
            BindingOperations.ClearBinding(this, ValueProperty);
        }

        #endregion
    }
}

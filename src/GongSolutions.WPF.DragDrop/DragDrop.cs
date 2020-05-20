using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GongSolutions.Wpf.DragDrop.Icons;
using GongSolutions.Wpf.DragDrop.Utilities;

namespace GongSolutions.Wpf.DragDrop
{
    using System.Collections.Generic;
    using System.Windows.Documents;

    public static partial class DragDrop
    {
        private static void CreateDragAdorner(DropInfo dropInfo)
        {
            var dragInfo = dropInfo.DragInfo;
            var template = GetDropAdornerTemplate(dropInfo.VisualTarget) ?? GetDragAdornerTemplate(dragInfo.VisualSource);
            var templateSelector = GetDropAdornerTemplateSelector(dropInfo.VisualTarget) ?? GetDragAdornerTemplateSelector(dragInfo.VisualSource);

            UIElement adornment = null;

            var useDefaultDragAdorner = template == null && templateSelector == null && GetUseDefaultDragAdorner(dragInfo.VisualSource);
            var useVisualSourceItemSizeForDragAdorner = GetUseVisualSourceItemSizeForDragAdorner(dragInfo.VisualSource);

            if (useDefaultDragAdorner)
            {
                template = dragInfo.VisualSourceItem.GetCaptureScreenDataTemplate(dragInfo.VisualSourceFlowDirection);
            }

            if (template != null || templateSelector != null)
            {
                if (dragInfo.Data is IEnumerable && !(dragInfo.Data is string))
                {
                    if (!useDefaultDragAdorner && ((IEnumerable)dragInfo.Data).Cast<object>().Count() <= 10)
                    {
                        var itemsControl = new ItemsControl();
                        itemsControl.ItemsSource = (IEnumerable)dragInfo.Data;
                        itemsControl.ItemTemplate = template;
                        itemsControl.ItemTemplateSelector = templateSelector;
                        itemsControl.Tag = dragInfo;

                        if (useVisualSourceItemSizeForDragAdorner)
                        {
                            var bounds = VisualTreeHelper.GetDescendantBounds(dragInfo.VisualSourceItem);
                            itemsControl.SetValue(FrameworkElement.MinWidthProperty, bounds.Width);
                        }

                        // The ItemsControl doesn't display unless we create a grid to contain it.
                        // Not quite sure why we need this...
                        var grid = new Grid();
                        grid.Children.Add(itemsControl);
                        adornment = grid;
                    }
                }
                else
                {
                    var contentPresenter = new ContentPresenter();
                    contentPresenter.Content = dragInfo.Data;
                    contentPresenter.ContentTemplate = template;
                    contentPresenter.ContentTemplateSelector = templateSelector;
                    contentPresenter.Tag = dragInfo;

                    if (useVisualSourceItemSizeForDragAdorner)
                    {
                        var bounds = VisualTreeHelper.GetDescendantBounds(dragInfo.VisualSourceItem);
                        contentPresenter.SetValue(FrameworkElement.MinWidthProperty, bounds.Width);
                        contentPresenter.SetValue(FrameworkElement.MinHeightProperty, bounds.Height);
                    }

                    adornment = contentPresenter;
                }
            }

            if (adornment != null)
            {
                if (useDefaultDragAdorner)
                {
                    adornment.Opacity = GetDefaultDragAdornerOpacity(dragInfo.VisualSource);
                }

                var rootElement = RootElementFinder.FindRoot(dropInfo.VisualTarget ?? dragInfo.VisualSource);
                DragAdorner = new DragAdorner(rootElement, adornment, GetDragAdornerTranslation(dragInfo.VisualSource));
            }
        }

        private static void CreateEffectAdorner(DropInfo dropInfo)
        {
            var dragInfo = m_DragInfo;
            var template = GetEffectAdornerTemplate(dragInfo.VisualSource, dropInfo.Effects, dropInfo.DestinationText, dropInfo.EffectText);

            if (template != null)
            {
                var rootElement = RootElementFinder.FindRoot(dropInfo.VisualTarget ?? dragInfo.VisualSource);

                var adornment = new ContentPresenter();
                adornment.Content = dragInfo.Data;
                adornment.ContentTemplate = template;

                EffectAdorner = new DragAdorner(rootElement, adornment, GetEffectAdornerTranslation(dragInfo.VisualSource), dropInfo.Effects);
            }
        }

        private static DataTemplate GetEffectAdornerTemplate(UIElement target, DragDropEffects effect, string destinationText, string effectText = null)
        {
            switch (effect)
            {
                case DragDropEffects.All:
                    // TODO: Add default template for EffectAll
                    return GetEffectAllAdornerTemplate(target);
                case DragDropEffects.Copy:
                    return GetEffectCopyAdornerTemplate(target) ?? CreateDefaultEffectDataTemplate(target, IconFactory.EffectCopy, effectText == null ? "Copy to" : effectText, destinationText);
                case DragDropEffects.Link:
                    return GetEffectLinkAdornerTemplate(target) ?? CreateDefaultEffectDataTemplate(target, IconFactory.EffectLink, effectText == null ? "Link to" : effectText, destinationText);
                case DragDropEffects.Move:
                    return GetEffectMoveAdornerTemplate(target) ?? CreateDefaultEffectDataTemplate(target, IconFactory.EffectMove, effectText == null ? "Move to" : effectText, destinationText);
                case DragDropEffects.None:
                    return GetEffectNoneAdornerTemplate(target) ?? CreateDefaultEffectDataTemplate(target, IconFactory.EffectNone, effectText == null ? "None" : effectText, destinationText);
                case DragDropEffects.Scroll:
                    // TODO: Add default template EffectScroll
                    return GetEffectScrollAdornerTemplate(target);
                default:
                    return null;
            }
        }

        private static DataTemplate CreateDefaultEffectDataTemplate(UIElement target, BitmapImage effectIcon, string effectText, string destinationText)
        {
            if (!GetUseDefaultEffectDataTemplate(target))
            {
                return null;
            }

            var fontSize = SystemFonts.MessageFontSize; // before 11d

            // The icon
            var imageFactory = new FrameworkElementFactory(typeof(Image));
            imageFactory.SetValue(Image.SourceProperty, effectIcon);
            imageFactory.SetValue(FrameworkElement.HeightProperty, 12d);
            imageFactory.SetValue(FrameworkElement.WidthProperty, 12d);

            // Only the icon for no effect
            if (Equals(effectIcon, GongSolutions.Wpf.DragDrop.Icons.IconFactory.EffectNone))
            {
                return new DataTemplate { VisualTree = imageFactory };
            }

            // Some margin for the icon
            imageFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 3, 0));
            imageFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            // Add effect text
            var effectTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            effectTextBlockFactory.SetValue(TextBlock.TextProperty, effectText);
            effectTextBlockFactory.SetValue(TextBlock.FontSizeProperty, fontSize);
            effectTextBlockFactory.SetValue(TextBlock.ForegroundProperty, Brushes.Blue);
            effectTextBlockFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            // Add destination text
            var destinationTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            destinationTextBlockFactory.SetValue(TextBlock.TextProperty, destinationText);
            destinationTextBlockFactory.SetValue(TextBlock.FontSizeProperty, fontSize);
            destinationTextBlockFactory.SetValue(TextBlock.ForegroundProperty, Brushes.DarkBlue);
            destinationTextBlockFactory.SetValue(TextBlock.MarginProperty, new Thickness(3, 0, 0, 0));
            destinationTextBlockFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.DemiBold);
            destinationTextBlockFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            // Create containing panel
            var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
            stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            stackPanelFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(2));
            stackPanelFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            stackPanelFactory.AppendChild(imageFactory);
            stackPanelFactory.AppendChild(effectTextBlockFactory);
            stackPanelFactory.AppendChild(destinationTextBlockFactory);

            // Add border
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            var stopCollection = new GradientStopCollection
                                 {
                                     new GradientStop(Colors.White, 0.0),
                                     new GradientStop(Colors.AliceBlue, 1.0)
                                 };
            var gradientBrush = new LinearGradientBrush(stopCollection)
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };
            borderFactory.SetValue(Panel.BackgroundProperty, gradientBrush);
            borderFactory.SetValue(Border.BorderBrushProperty, Brushes.DimGray);
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
            borderFactory.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            borderFactory.SetValue(Border.SnapsToDevicePixelsProperty, true);
            borderFactory.SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);
            borderFactory.SetValue(TextOptions.TextRenderingModeProperty, TextRenderingMode.ClearType);
            borderFactory.SetValue(TextOptions.TextHintingModeProperty, TextHintingMode.Fixed);
            borderFactory.AppendChild(stackPanelFactory);

            // Finally add content to template
            return new DataTemplate { VisualTree = borderFactory };
        }

        private static void Scroll(DropInfo dropInfo, DragEventArgs e)
        {
            if (dropInfo == null || dropInfo.TargetScrollViewer == null)
            {
                return;
            }

            var scrollViewer = dropInfo.TargetScrollViewer;
            var scrollingMode = dropInfo.TargetScrollingMode;

            var position = e.GetPosition(scrollViewer);
            var scrollMargin = Math.Min(scrollViewer.FontSize * 2, scrollViewer.ActualHeight / 2);

            if (scrollingMode == ScrollingMode.Both || scrollingMode == ScrollingMode.HorizontalOnly)
            {
                if (position.X >= scrollViewer.ActualWidth - scrollMargin && scrollViewer.HorizontalOffset < scrollViewer.ExtentWidth - scrollViewer.ViewportWidth)
                {
                    scrollViewer.LineRight();
                }
                else if (position.X < scrollMargin && scrollViewer.HorizontalOffset > 0)
                {
                    scrollViewer.LineLeft();
                }
            }

            if (scrollingMode == ScrollingMode.Both || scrollingMode == ScrollingMode.VerticalOnly)
            {
                if (position.Y >= scrollViewer.ActualHeight - scrollMargin && scrollViewer.VerticalOffset < scrollViewer.ExtentHeight - scrollViewer.ViewportHeight)
                {
                    scrollViewer.LineDown();
                }
                else if (position.Y < scrollMargin && scrollViewer.VerticalOffset > 0)
                {
                    scrollViewer.LineUp();
                }
            }
        }

        /// <summary>
        /// Gets the drag handler from the drag info or from the sender, if the drag info is null
        /// </summary>
        /// <param name="dragInfo">the drag info object</param>
        /// <param name="sender">the sender from an event, e.g. mouse down, mouse move</param>
        /// <returns></returns>
        private static IDragSource TryGetDragHandler(DragInfo dragInfo, UIElement sender)
        {
            IDragSource dragHandler = null;
            if (dragInfo != null && dragInfo.VisualSource != null)
            {
                dragHandler = GetDragHandler(dragInfo.VisualSource);
            }
            if (dragHandler == null && sender != null)
            {
                dragHandler = GetDragHandler(sender);
            }
            return dragHandler ?? DefaultDragHandler;
        }

        /// <summary>
        /// Gets the drop handler from the drop info or from the sender, if the drop info is null
        /// </summary>
        /// <param name="dropInfo">the drop info object</param>
        /// <param name="sender">the sender from an event, e.g. drag over</param>
        /// <returns></returns>
        private static IDropTarget TryGetDropHandler(DropInfo dropInfo, UIElement sender)
        {
            IDropTarget dropHandler = null;
            if (dropInfo != null && dropInfo.VisualTarget != null)
            {
                dropHandler = GetDropHandler(dropInfo.VisualTarget);
            }
            if (dropHandler == null && sender != null)
            {
                dropHandler = GetDropHandler(sender);
            }
            return dropHandler ?? DefaultDropHandler;
        }

        private static IHintDropTarget TryGetHintDropHandler(DropInfo dropInfo, UIElement sender)
        {
            return TryGetDropHandler(dropInfo, sender) as IHintDropTarget;
        }

        private static void DragSourceOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DoMouseButtonDown(sender, e);
        }

        private static void DragSourceOnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DoMouseButtonDown(sender, e);
        }

        private static void DoMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_DragInfo = null;

            // Ignore the click if clickCount != 1 or the user has clicked on a scrollbar.
            var elementPosition = e.GetPosition((IInputElement)sender);
            if (e.ClickCount != 1
                || (sender as UIElement).IsDragSourceIgnored()
                || (e.Source as UIElement).IsDragSourceIgnored()
                || (e.OriginalSource as UIElement).IsDragSourceIgnored()
                || (sender is TabControl) && !HitTestUtilities.HitTest4Type<TabPanel>(sender, elementPosition)
                || HitTestUtilities.HitTest4Type<RangeBase>(sender, elementPosition)
                || HitTestUtilities.HitTest4Type<TextBoxBase>(sender, elementPosition)
                || HitTestUtilities.HitTest4Type<PasswordBox>(sender, elementPosition)
                || HitTestUtilities.HitTest4Type<ComboBox>(sender, elementPosition)
                || HitTestUtilities.HitTest4GridViewColumnHeader(sender, elementPosition)
                || HitTestUtilities.HitTest4DataGridTypes(sender, elementPosition)
                || HitTestUtilities.IsNotPartOfSender(sender, e))
            {
                return;
            }

            var dragInfo = new DragInfo(sender, e);

            if (dragInfo.VisualSource is ItemsControl control && control.CanSelectMultipleItems())
            {
                control.Focus();
            }

            if (dragInfo.VisualSourceItem == null)
            {
                return;
            }

            var dragHandler = TryGetDragHandler(dragInfo, sender as UIElement);
            if (!dragHandler.CanStartDrag(dragInfo))
            {
                return;
            }

            // If the sender is a list box that allows multiple selections, ensure that clicking on an 
            // already selected item does not change the selection, otherwise dragging multiple items 
            // is made impossible.
            var itemsControl = sender as ItemsControl;
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0 && (Keyboard.Modifiers & ModifierKeys.Control) == 0 && dragInfo.VisualSourceItem != null && itemsControl != null && itemsControl.CanSelectMultipleItems())
            {
                var selectedItems = itemsControl.GetSelectedItems().OfType<object>().ToList();
                if (selectedItems.Count > 1 && selectedItems.Contains(dragInfo.SourceItem))
                {
                    m_ClickSupressItem = dragInfo.SourceItem;
                    e.Handled = true;
                }
            }

            m_DragInfo = dragInfo;
        }

        private static void DragSourceOnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DoMouseButtonUp(sender, e);
        }

        private static void DragSourceOnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DoMouseButtonUp(sender, e);
        }

        private static void DoMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            var elementPosition = e.GetPosition((IInputElement)sender);
            if ((sender is TabControl) && !HitTestUtilities.HitTest4Type<TabPanel>(sender, elementPosition))
            {
                m_DragInfo = null;
                m_ClickSupressItem = null;
                return;
            }

            var dragInfo = m_DragInfo;

            // If we prevented the control's default selection handling in DragSource_PreviewMouseLeftButtonDown
            // by setting 'e.Handled = true' and a drag was not initiated, manually set the selection here.
            var itemsControl = sender as ItemsControl;
            if (itemsControl != null && dragInfo != null && m_ClickSupressItem != null && m_ClickSupressItem == dragInfo.SourceItem)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
                {
                    itemsControl.SetItemSelected(dragInfo.SourceItem, false);
                }
                else if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0)
                {
                    itemsControl.SetSelectedItem(dragInfo.SourceItem);
                }
            }

            m_DragInfo = null;
            m_ClickSupressItem = null;
        }

        private static void DragSourceOnMouseMove(object sender, MouseEventArgs e)
        {
            var dragInfo = m_DragInfo;
            if (dragInfo != null && !m_DragInProgress)
            {
                // the start from the source
                var dragStart = dragInfo.DragStartPosition;

                // do nothing if mouse left/right button is released or the pointer is captured
                if (dragInfo.MouseButton == MouseButton.Left && e.LeftButton == MouseButtonState.Released)
                {
                    m_DragInfo = null;
                    return;
                }
                if (DragDrop.GetCanDragWithMouseRightButton(dragInfo.VisualSource) && dragInfo.MouseButton == MouseButton.Right && e.RightButton == MouseButtonState.Released)
                {
                    m_DragInfo = null;
                    return;
                }

                // current mouse position
                var position = e.GetPosition((IInputElement)sender);

                // prevent selection changing while drag operation
                dragInfo.VisualSource?.ReleaseMouseCapture();

                // only if the sender is the source control and the mouse point differs from an offset
                if (dragInfo.VisualSource == sender
                    && (Math.Abs(position.X - dragStart.X) > DragDrop.GetMinimumHorizontalDragDistance(dragInfo.VisualSource) ||
                        Math.Abs(position.Y - dragStart.Y) > DragDrop.GetMinimumVerticalDragDistance(dragInfo.VisualSource)))
                {
                    dragInfo.RefreshSelectedItems(sender, e);

                    var dragHandler = TryGetDragHandler(dragInfo, sender as UIElement);
                    if (dragHandler.CanStartDrag(dragInfo))
                    {
                        dragHandler.StartDrag(dragInfo);

                        if (dragInfo.Effects != DragDropEffects.None)
                        {
                            var dataObject = dragInfo.DataObject;

                            if (dataObject == null)
                            {
                                if (dragInfo.Data == null)
                                {
                                    // it's bad if the Data is null, cause the DataObject constructor will raise an ArgumentNullException
                                    m_DragInfo = null; // maybe not necessary or should not set here to null
                                    return;
                                }
                                dataObject = new DataObject(dragInfo.DataFormat.Name, dragInfo.Data);
                            }

                            try
                            {
                                m_DragInProgress = true;
                                CreateHintAdorners(sender, dragInfo);
                                var dragDropHandler = dragInfo.DragDropHandler ?? System.Windows.DragDrop.DoDragDrop;
                                var dragDropEffects = dragDropHandler(dragInfo.VisualSource, dataObject, dragInfo.Effects);
                                if (dragDropEffects == DragDropEffects.None)
                                {
                                    dragHandler.DragCancelled();
                                }
                                dragHandler.DragDropOperationFinished(dragDropEffects, dragInfo);
                                DestroyHintAdorners();
                            }
                            catch (Exception ex)
                            {
                                if (!dragHandler.TryCatchOccurredException(ex))
                                {
                                    throw;
                                }
                            }
                            finally
                            {
                                m_DragInProgress = false;
                                m_DragInfo = null;
                            }
                        }
                    }
                }
            }
        }

        private static readonly List<DropTargetHintAdorner> _hintDropTargetAdorners = new List<DropTargetHintAdorner>();

        /// <summary>
        /// Create hint adorners for all visible and potential drop targets upon drag start.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dragInfo"></param>
        private static void CreateHintAdorners(object sender, DragInfo dragInfo)
        {
            UIElement excludeElement = null;
            if (sender is UIElement exclude)
            {
                excludeElement = GetIsDropTarget(exclude) ? exclude : exclude.TryGetNextAncestorDropTargetElement();
            }

            foreach (var potentialTarget in _hintDropZones.ToList())
            {
                if(!potentialTarget.TryGetTarget(out var target))
                {
                    // Remove dead references
                    _hintDropZones.Remove(potentialTarget);
                }
                if (excludeElement != null && ReferenceEquals(excludeElement, target))
                {
                    // Dp not highlight element from sender
                    continue;
                }

                AddHintAdorner(dragInfo, target);
            }
        }

        private static void AddHintAdorner(DragInfo dragInfo, UIElement target)
        {
            DropTargetHintAdorner adorner = null;
            if (target == null)
            {
                return;
            }

            var hintDropHandler = TryGetHintDropHandler(null, target);
            if (hintDropHandler == null)
            {
                return;
            }

            var hintAdornerType = hintDropHandler.GetHintAdorner(dragInfo);
            if (hintAdornerType == null)
            {
                return;
            }
            var adornedElement = GetAdornedElement(target);
            
            if (!hintAdornerType.IsInstanceOfType(typeof(DropTargetHintAdorner)) && adornedElement.IsVisible)
            {
                adorner = DropTargetHintAdorner.Create(hintAdornerType, adornedElement);
            }
            if (adorner != null)
            {
                _hintDropTargetAdorners.Add(adorner);
            }
        }

        private static void RemoveHintAdorner(UIElement element)
        {
            var adornedElement = GetAdornedElement(element);
            if (adornedElement == null)
            {
                return;
            }

            var adorner = AdornerLayer.GetAdornerLayer(adornedElement)?
                                      .GetAdorners(adornedElement)?
                                      .OfType<DropTargetHintAdorner>()
                                      .FirstOrDefault();
            adorner?.Detatch();
            _hintDropTargetAdorners.Remove(adorner);
        }

        private static UIElement GetAdornedElement(UIElement itemsControl)
        {
            UIElement adornedElement;
            if (itemsControl is TabControl)
            {
                adornedElement = itemsControl.GetVisualDescendent<TabPanel>();
            }
            else if (itemsControl is ItemsControl)
            {
                adornedElement = itemsControl.GetVisualDescendent<ScrollContentPresenter>() as UIElement ?? itemsControl.GetVisualDescendent<ItemsPresenter>() as UIElement ?? itemsControl;
            }
            else
            {
                adornedElement = itemsControl;
            }

            return adornedElement;
        }

        private static void DestroyHintAdorners()
        {
            var adorners = _hintDropTargetAdorners.ToList();
            _hintDropTargetAdorners.Clear();
            foreach (var hintAdorner in adorners)
            {
                hintAdorner.Detatch();
            }
        }

        private static void DragSourceOnQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (e.Action == DragAction.Cancel || e.EscapePressed)
            {
                DragAdorner = null;
                EffectAdorner = null;
                DropTargetAdorner = null;
                Mouse.OverrideCursor = null;
            }
        }

        private static void DropTargetOnDragEnter(object sender, DragEventArgs e)
        {
            OnDropTargetEnter(sender, e);
            DropTargetOnDragOver(sender, e, EventType.Bubbled);
        }

        private static void DropTargetOnPreviewDragEnter(object sender, DragEventArgs e)
        {
            OnDropTargetEnter(sender, e);
            DropTargetOnDragOver(sender, e, EventType.Tunneled);
        }
        private static void OnDropTargetEnter(object sender, DragEventArgs e)
        {
            var element = sender as UIElement;
            var dropHandler = TryGetHintDropHandler(null, element);
            if(dropHandler == null)
            {
                return;
            }

            RemoveHintAdorner(element);
        }

        private static void DropTargetOnDragLeave(object sender, DragEventArgs e)
        {
            DragAdorner = null;
            EffectAdorner = null;
            DropTargetAdorner = null;
            if (TryGetHintDropHandler(null, sender as UIElement) == null)
            {
                return;
            }
            AddHintAdorner(m_DragInfo, sender as UIElement);
        }

        private static void DropTargetOnDragOver(object sender, DragEventArgs e)
        {
            DropTargetOnDragOver(sender, e, EventType.Bubbled);
        }

        private static void DropTargetOnPreviewDragOver(object sender, DragEventArgs e)
        {
            DropTargetOnDragOver(sender, e, EventType.Tunneled);
        }

        private static void DropTargetOnDragOver(object sender, DragEventArgs e, EventType eventType)
        {
            var elementPosition = e.GetPosition((IInputElement)sender);

            var dragInfo = m_DragInfo;
            var dropInfo = new DropInfo(sender, e, dragInfo, eventType);
            var dropHandler = TryGetDropHandler(dropInfo, sender as UIElement);
            var itemsControl = dropInfo.VisualTarget;

            dropHandler.DragOver(dropInfo);

            if (DragAdorner == null && dragInfo != null)
            {
                CreateDragAdorner(dropInfo);
            }

            DragAdorner?.Move(e.GetPosition(DragAdorner.AdornedElement), dragInfo != null ? GetDragMouseAnchorPoint(dragInfo.VisualSource) : default(Point), ref _adornerMousePosition, ref _adornerSize);

            Scroll(dropInfo, e);

            if (HitTestUtilities.HitTest4Type<ScrollBar>(sender, elementPosition)
                || HitTestUtilities.HitTest4GridViewColumnHeader(sender, elementPosition)
                || HitTestUtilities.HitTest4DataGridTypesOnDragOver(sender, elementPosition))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            // If the target is an ItemsControl then update the drop target adorner.
            if (itemsControl != null)
            {
                // Display the adorner in the control's ItemsPresenter. If there is no 
                // ItemsPresenter provided by the style, try getting hold of a
                // ScrollContentPresenter and using that.
                UIElement adornedElement = null;
                if (itemsControl is TabControl)
                {
                    adornedElement = itemsControl.GetVisualDescendent<TabPanel>();
                }
                else if (itemsControl is DataGrid || (itemsControl as ListView)?.View is GridView)
                {
                    adornedElement = itemsControl.GetVisualDescendent<ScrollContentPresenter>() as UIElement ?? itemsControl.GetVisualDescendent<ItemsPresenter>() as UIElement ?? itemsControl;
                }
                else
                {
                    adornedElement = itemsControl.GetVisualDescendent<ItemsPresenter>() as UIElement ?? itemsControl.GetVisualDescendent<ScrollContentPresenter>() as UIElement ?? itemsControl;
                }

                if (adornedElement != null)
                {
                    if (dropInfo.DropTargetAdorner == null)
                    {
                        DropTargetAdorner = null;
                    }
                    else if (!dropInfo.DropTargetAdorner.IsInstanceOfType(DropTargetAdorner))
                    {
                        DropTargetAdorner = DropTargetAdorner.Create(dropInfo.DropTargetAdorner, adornedElement, dropInfo);
                    }

                    var adorner = DropTargetAdorner;
                    if (adorner != null)
                    {
                        var adornerBrush = GetDropTargetAdornerBrush(dropInfo.VisualTarget);
                        if (adornerBrush != null)
                        {
                            adorner.Pen.SetCurrentValue(Pen.BrushProperty, adornerBrush);
                        }
                        adorner.DropInfo = dropInfo;
                        adorner.InvalidateVisual();
                    }
                }
            }

            // Set the drag effect adorner if there is one
            if (dragInfo != null && (EffectAdorner == null || EffectAdorner.Effects != dropInfo.Effects))
            {
                CreateEffectAdorner(dropInfo);
            }

            EffectAdorner?.Move(e.GetPosition(EffectAdorner.AdornedElement), default(Point), ref _effectAdornerMousePosition, ref _effectAdornerSize);

            e.Effects = dropInfo.Effects;
            e.Handled = !dropInfo.NotHandled;

            if (!dropInfo.IsSameDragDropContextAsSource)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private static void DropTargetOnDrop(object sender, DragEventArgs e)
        {
            DropTargetOnDrop(sender, e, EventType.Bubbled);
        }

        private static void DropTargetOnPreviewDrop(object sender, DragEventArgs e)
        {
            DropTargetOnDrop(sender, e, EventType.Tunneled);
        }

        private static void DropTargetOnDrop(object sender, DragEventArgs e, EventType eventType)
        {
            var dragInfo = m_DragInfo;
            var dropInfo = new DropInfo(sender, e, dragInfo, eventType);
            var dropHandler = TryGetDropHandler(dropInfo, sender as UIElement);
            var dragHandler = TryGetDragHandler(dragInfo, sender as UIElement);

            DragAdorner = null;
            EffectAdorner = null;
            DropTargetAdorner = null;

            dropHandler.DragOver(dropInfo);
            dropHandler.Drop(dropInfo);
            dragHandler.Dropped(dropInfo);

            e.Effects = dropInfo.Effects;
            e.Handled = !dropInfo.NotHandled;

            Mouse.OverrideCursor = null;
        }

        private static void DropTargetOnGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (EffectAdorner != null)
            {
                e.UseDefaultCursors = false;
                e.Handled = true;
                if (Mouse.OverrideCursor != Cursors.Arrow)
                {
                    Mouse.OverrideCursor = Cursors.Arrow;
                }
            }
            else
            {
                e.UseDefaultCursors = true;
                e.Handled = true;
                if (Mouse.OverrideCursor != null)
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private static DragAdorner _DragAdorner;

        private static DragAdorner DragAdorner
        {
            get { return _DragAdorner; }
            set
            {
                _DragAdorner?.Detatch();
                _DragAdorner = value;
            }
        }

        private static DragAdorner _EffectAdorner;

        private static DragAdorner EffectAdorner
        {
            get { return _EffectAdorner; }
            set
            {
                _EffectAdorner?.Detatch();
                _EffectAdorner = value;
            }
        }

        private static DropTargetAdorner _DropTargetAdorner;

        private static DropTargetAdorner DropTargetAdorner
        {
            get { return _DropTargetAdorner; }
            set
            {
                _DropTargetAdorner?.Detatch();
                _DropTargetAdorner = value;
            }
        }

        private static DragInfo m_DragInfo;
        private static bool m_DragInProgress;
        private static object m_ClickSupressItem;

        private static Point _adornerMousePosition;
        private static Size _adornerSize;

        private static Point _effectAdornerMousePosition;
        private static Size _effectAdornerSize;
    }
}

using System.Windows;

namespace GongSolutions.Wpf.DragDrop
{
    using System;

    /// <summary>
    /// Interface implemented by Drop Handlers.
    /// </summary>
    public interface IDropTarget
    {
        /// <summary>
        /// Notifies the drop handler about the current drag operation state.
        /// </summary>
        /// <param name="dropInfo">Object which contains several drop information.</param>
        /// <remarks>
        /// To allow a drop at the current drag position, the <see cref="DropInfo.Effects"/> property on 
        /// <paramref name="dropInfo"/> should be set to a value other than <see cref="DragDropEffects.None"/>
        /// and <see cref="DropInfo.Data"/> should be set to a non-null value.
        /// </remarks>
        void DragOver(IDropInfo dropInfo);

        /// <summary>
        /// Performs a drop.
        /// </summary>
        /// <param name="dropInfo">Object which contains several drop information.</param>
        void Drop(IDropInfo dropInfo);
    }

    /// <summary>
    /// Extension interface for <see cref="IDropTarget"/> to allow for hinting the user where data can be dropped.
    /// </summary>
    public interface IHintDropTarget : IDropTarget
    {
        /// <summary>
        /// Ask a drop handler if it is a viable target for <paramref name="dragInfo"/> in order to display hint
        /// adorner for helping user identify available targets.
        /// </summary>
        /// <param name="dragInfo">Object which contains several drag information.</param>
        /// <remarks>
        /// To allow for drop hint, assign the <see cref="IDropInfo.DropTargetHintAdorner"/>
        /// </remarks>
        Type GetHintAdorner(IDragInfo dragInfo);
        // TODO: Should this accept a new IDropHintInfo where additional properties can be set, similar to DragOver(IDropInfo), rather than IDragInfo? DragInfo can't be constructed without DragEventArgs
        // TODO: If so, perhaps rename method to DropHint(IDropHintInfo)
    }
}
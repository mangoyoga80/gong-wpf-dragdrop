namespace GongSolutions.Wpf.DragDrop
{
    using System;

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
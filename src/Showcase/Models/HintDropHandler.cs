namespace Showcase.WPF.DragDrop.Models
{
    using System;
    using System.Linq;
    using GongSolutions.Wpf.DragDrop;

    public abstract class HintDropHandler : IHintDropTarget
    {
        public virtual void DragOver(IDropInfo dropInfo)
        {
            if (!this.CanHandleData(dropInfo.Data))
            {
                return;
            }
            dropInfo.Effects = System.Windows.DragDropEffects.Copy;
            dropInfo.EffectText = "Give gift";
            dropInfo.DropTargetAdorner = typeof(DropTargetActiveHintAdorner);
        }

        protected virtual bool CanHandleData(object data)
        {
            return true;
        }
        
        public abstract void Drop(IDropInfo dropInfo);

        public Type GetHintAdorner(IDragInfo dragInfo)
        {
            if (!CanHandleData(dragInfo.Data))
            {
                return null;
            }
            
            return typeof(DropTargetInactiveHintAdorner);
        }
    }

    public class DefaultHintDropHandler : HintDropHandler
    {
        public override void Drop(IDropInfo dropInfo)
        {
        }
    }

    public class RecipientHintDropHandler : HintDropHandler
    {
        private readonly RecipientItemModel recipient;

        public RecipientHintDropHandler(RecipientItemModel recipient)
        {
            this.recipient = recipient;
        }

        protected override bool CanHandleData(object data)
        {
            var items = DefaultDropHandler.ExtractData(data)?.OfType<GiftItemModel>();
            return items?.Any() == true && this.recipient.Gifts == null;
            //return base.CanHandle(dropInfo);
        }

        public override void Drop(IDropInfo dropInfo)
        {
            if (this.CanHandleData(dropInfo.Data))
            {
                this.recipient.Gifts = DefaultDropHandler.ExtractData(dropInfo.Data)?
                                                         .OfType<GiftItemModel>()
                                                         .ToList();
            }
        }
    }
}
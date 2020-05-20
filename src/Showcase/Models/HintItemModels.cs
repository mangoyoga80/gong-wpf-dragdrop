using System;
using System.ComponentModel;
namespace Showcase.WPF.DragDrop.Models
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using GongSolutions.Wpf.DragDrop;
    using JetBrains.Annotations;


    public class ItemModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value)
                    return;
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    public class GiftItemModel : ItemModelBase
    {
        public GiftItemModel(int i)
        {
            Name = $"Gift {i}";
        }
    }
    [DebuggerDisplay("{Name}")]
    public class RecipientItemModel : ItemModelBase
    {
        private List<GiftItemModel> gifts;

        public RecipientItemModel(int i)
        {
            Name = $"Recipient {i}";
            DropHandler = new RecipientHintDropHandler(this);
        }

        public IDropTarget DropHandler { get; }

        public List<GiftItemModel> Gifts
        {
            get => this.gifts;
            set
            {
                if (Equals(value, this.gifts)) return;
                this.gifts = value;
                this.OnPropertyChanged();
            }
        }
        public override string ToString()
        {
            return Name + string.Join(", ", Gifts.Select(m => m.Name));
        }
    }
}

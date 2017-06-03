using System.ComponentModel;

namespace SharpMessage.WPF
{
    public class ItemViewModel<T> : INotifyPropertyChanged
    {
        public int Index { get; }
        public T Item { get; set; }

        // as there is no change management WITHIN a Bin, this would probably only be raised (iteratively) from the outside
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class StringViewModel : ItemViewModel<string> { }
}

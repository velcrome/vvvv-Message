using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using VVVV.Packs.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;

namespace SharpMessage.WPF
{
    public class BinStringViewModel : BinViewModel<string>
    {
        public BinStringViewModel(Bin<string> source) : base(source)
        {
        }
    }

    public abstract class BinViewModel<T> : ObservableCollection<T>
    {
        protected Bin<T> Source;

        public BinViewModel(Bin<T> source)
        {
            if (source != null) Source = BinFactory.New<T>();
            else Source = source;


//            Source.PropertyChanged += Update;

        }

        private void Update(object sender, PropertyChangedEventArgs e)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>Access to individual elements</summary>
        /// <exception cref="ArgumentNullException">Thrown if null is to be written into a bin.</exception>
        /// <exception cref="TypeNotSupportedException">Thrown if an item is attempting to be written to a bin without a proper registration in TypeIdentity</exception>
        /// <exception cref="InvalidCastException">Thrown if an item is attempting to be written to a bin without the type matching.</exception>
        public new object this[int slice] {
            get
            {
                return Source[slice];
            }
            set
            {
                Source[slice] = (T)value;
                // this will raise PropertyChanged, if successful
            }
        }





        //private bool _IsValid;
        //public bool IsValid
        //{
        //    get { return _IsValid; }
        //    set { this.RaiseAndSetIfChanged(ref _IsValid, value); }
        //}

        //[ValidatesViaMethod(AllowBlanks = false, AllowNull = false, Name = "IsAgeValid", ErrorMessage = "Please enter a valid age 0..120")]
        //public int Age
        //{
        //    get { return _Age; }
        //    set { this.RaiseAndSetIfChanged(ref _Age, value); }
        //}

        //public bool IsAgeValid(int age)
        //{
        //    return ((age >= 0) && (age <= 120));
        //}
    }
}
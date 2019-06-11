using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassLibrary.Annotations;

namespace ClassLibrary.Classes
{
    public sealed class Tag : INotifyPropertyChanged
    {
        private long _id;

        public long Id
        {
            get => _id;
            set
            {
                if (_id == value) return;

                _id = value;
                OnPropertyChanged();
            }
        }

        private string _text;

        public string Text
        {
            get => _text;
            set
            {
                if (_text == value) return;

                _text = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool Equals(Tag other)
        {
            return Id == other.Id && string.Equals(Text, other.Text);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is Tag other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id.GetHashCode() * 397) ^ (Text != null ? Text.GetHashCode() : 0);
            }
        }
    }
}
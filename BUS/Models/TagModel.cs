using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BUS.Models
{
    public class TagModel : INotifyPropertyChanged
    {
        private int _id;

        public int Id
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

        public int? ProjectId { get; set; }
        public string TagTextColor { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool Equals(TagModel other)
        {
            return Id == other.Id && string.Equals(Text, other.Text);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is TagModel other && Equals(other);
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
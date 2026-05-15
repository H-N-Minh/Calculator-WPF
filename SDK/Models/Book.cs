using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SDK.Models
{
    public class Book : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private string? _author;
        public string? Author
        {
            get => _author;
            set { _author = value; OnPropertyChanged(); }
        }

        private DateTime? _publicationDate;
        public DateTime? PublicationDate
        {
            get => _publicationDate;
            set { _publicationDate = value; OnPropertyChanged(); }
        }

        private int? _pages;
        public int? Pages
        {
            get => _pages;
            set { _pages = value; OnPropertyChanged(); }
        }

        private decimal? _price;
        public decimal? Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        private double? _rating;
        public double? Rating
        {
            get => _rating;
            set { _rating = value; OnPropertyChanged(); }
        }

        private string? _description;
        public string? Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
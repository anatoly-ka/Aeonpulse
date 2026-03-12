using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Aeonpulse.Models
{
    public class TickerData : INotifyPropertyChanged
    {
        private string _briefText = string.Empty;
        public string BriefText
        {
            get => _briefText;
            set { _briefText = value; OnPropertyChanged(); }
        }

        private string _fullText = string.Empty;
        public string FullText
        {
            get => _fullText;
            set { _fullText = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

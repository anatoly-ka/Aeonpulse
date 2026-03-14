using Aeonpulse.Views;

namespace Aeonpulse
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new Views.MainPage();
        }
    }
}

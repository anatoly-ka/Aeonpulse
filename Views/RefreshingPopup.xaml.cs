namespace Aeonpulse.Views
{
    public partial class RefreshingPopup : ContentPage
    {
        private readonly Action _onDismissed;

        public RefreshingPopup(Action onDismissed)
        {
            InitializeComponent();
            _onDismissed = onDismissed;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Auto-dismiss after 3 seconds, then invoke the callback
            await Task.Delay(3000);
            await Navigation.PopModalAsync();
            _onDismissed();
        }
    }
}
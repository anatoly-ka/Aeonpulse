using Microsoft.Maui.Controls;

namespace Aeonpulse.Views
{
    public partial class DeepDivePopup : ContentPage
    {
        public DeepDivePopup(
            string title,
            string section1Title,
            string section1Text,
            string section2Title,
            string section2Text,
            double topOffset = 0)
        {
            InitializeComponent();

            TitleLabel.Text = title;
            Section1TitleLabel.Text = section1Title;
            Section1TextLabel.Text = section1Text;
            Section2TitleLabel.Text = section2Title;
            Section2TextLabel.Text = section2Text;

            // Push the Frame down so its top edge aligns with the bottom of the
            // Timeline Heading. The left/right/bottom margins stay as defined in XAML.
            PopupFrame.Margin = new Thickness(24, topOffset, 24, 24);
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}
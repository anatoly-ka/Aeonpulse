using Aeonpulse.Attributes;
using Microsoft.Maui.Controls;

namespace Aeonpulse.Views
{
    /// <summary>
    /// Code-behind for the reusable Deep Dive info popup.
    /// Receives all displayable content as constructor arguments, making this
    /// a generic, stateless shell reused across all ten ticker card info buttons.
    ///
    /// <para>
    /// <b>Layout trick:</b> <paramref name="topOffset"/> is injected as the top
    /// component of <c>PopupFrame.Margin</c> so that the panel's top edge visually
    /// aligns with the bottom of the Timeline Heading, giving a "slide down from heading"
    /// appearance without requiring absolute positioning or a custom layout.
    /// </para>
    /// </summary>
    [AIContext("ModalViewController")]
    public partial class DeepDivePopup : ContentPage
    {
        /// <summary>
        /// Creates a Deep Dive popup populated with the supplied content.
        /// </summary>
        /// <param name="title">Card title shown in the popup heading (e.g., "Time Jubilees").</param>
        /// <param name="section1Title">Label for the first content section (typically "The Method").</param>
        /// <param name="section1Text">Body text for section 1 — methodology explanation.</param>
        /// <param name="section2Title">Label for the second content section (typically "Sources").</param>
        /// <param name="section2Text">Body text for section 2 — data attribution and links.</param>
        /// <param name="topOffset">
        /// Vertical offset in device-independent units equal to
        /// <c>NavBar.Height + TimelineHeading.Height</c>, measured by <c>MainPage</c>
        /// at the moment of opening. Positions the panel below the sticky header area.
        /// </param>
        public DeepDivePopup(
            string title,
            string section1Title,
            string section1Text,
            string section2Title,
            string section2Text,
            double topOffset = 0)
        {
            InitializeComponent();

            TitleLabel.Text       = title;
            Section1TitleLabel.Text = section1Title;
            Section1TextLabel.Text  = section1Text;
            Section2TitleLabel.Text = section2Title;
            Section2TextLabel.Text  = section2Text;

            // Override only the top Margin; preserve XAML-defined left/right/bottom values.
            PopupFrame.Margin = new Thickness(24, topOffset, 24, 24);
        }

        /// <summary>Dismisses the popup.</summary>
        private void OnCloseClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}
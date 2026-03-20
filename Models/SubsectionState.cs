namespace Aeonpulse.Models
{
    /// <summary>
    /// Lightweight snapshot of a collapsible section's display state.
    /// Used to persist and restore which sections the user has expanded
    /// across navigation events (e.g., Lab, Cosmos, Mirror, Eco Echoes).
    /// </summary>
    public class SubsectionState
    {
        /// <summary>
        /// Gets or sets the localised title of the section, used as a stable key.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the section is currently expanded in the UI.
        /// </summary>
        public bool IsExpanded { get; set; }
    }
}

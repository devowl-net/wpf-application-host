using System.Windows.Controls;

namespace WpfApplicationHost.Models
{
    /// <summary>
    /// Tab model for <see cref="TabItem"/>.
    /// </summary>
    public class TabModel
    {
        /// <summary>
        /// <see cref="TabModel"/> Constructor.
        /// </summary>
        public TabModel(string applicationName, string path)
        {
            Path = path;
            ApplicationName = applicationName;
        }

        /// <summary>
        /// Path to application.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Application name.
        /// </summary>
        public string ApplicationName { get; private set; }
    }
}
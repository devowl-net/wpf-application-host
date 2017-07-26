using System.Collections.Generic;

using WpfApplicationHost.Models;
using WpfApplicationHost.Views;

namespace WpfApplicationHost.ViewModels
{
    /// <summary>
    /// <see cref="MainWindow"/> view model.
    /// </summary>
    public class MainWindowViewModel
    {
        /// <summary>
        /// Example application tabs.
        /// </summary>
        public IEnumerable<TabModel> TabModels { get; private set; } = new[]
        {
            new TabModel("Notepad", @"C:\Windows\System32\notepad.exe"),
            new TabModel("Calculator", @"C:\Windows\System32\calc.exe")
        };
    }
}
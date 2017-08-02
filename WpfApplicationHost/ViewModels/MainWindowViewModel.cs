using System;
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
            new TabModel("Notepad", $@"{Environment.GetFolderPath(Environment.SpecialFolder.Windows)}\System32\notepad.exe"),
            new TabModel("Regedit", $@"{Environment.GetFolderPath(Environment.SpecialFolder.Windows)}\regedit.exe"),
            new TabModel("Write", $@"{Environment.GetFolderPath(Environment.SpecialFolder.Windows)}\write.exe")
        };
    }
}
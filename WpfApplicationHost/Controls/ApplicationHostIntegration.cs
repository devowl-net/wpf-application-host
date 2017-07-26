using System;
using System.Diagnostics;
using System.Windows.Forms.Integration;

using WpfApplicationHost.Windows;
using WpfApplicationHost.Windows.Structs;

namespace WpfApplicationHost.Controls
{
    /// <summary>
    /// <see cref="WindowsFormsHost"/> integration with external application.
    /// </summary>
    public class ApplicationHostIntegration
    {
        /// <summary>
        /// <see cref="ApplicationHostIntegration"/> Constructor.
        /// </summary>
        public ApplicationHostIntegration(WindowsFormsHost formsHost)
        {
            FormsHost = formsHost;
        }

        private WindowsFormsHost FormsHost { get; }

        /// <summary>
        /// Attach to process.
        /// </summary>
        /// <param name="process">Process instance.</param>
        public void AttachToProcess(Process process)
        {
            var windowHandle = process.MainWindowHandle;

            // Set application PARENT to this control
            Win32Api.SetParent(windowHandle, FormsHost.Handle);

            // Move window to left top corner. Set Width and Height equals to control size
            Win32Api.MoveWindow(windowHandle, 0, 0, (int)FormsHost.Width, (int)FormsHost.Height, true);

            // Set hook function on window events
            Win32Api.SetWinEventHook(
                WindowsEvents.WINEVENT_OUTOFCONTEXT,
                WindowsEvents.EVENT_AIA_END,
                IntPtr.Zero,
                Hook,
                0,
                0,
                WindowsEvents.WINEVENT_OUTOFCONTEXT);
        }

        private void Hook(
            IntPtr hWinEventHook,
            uint eventType,
            IntPtr hwnd,
            int idObject,
            int idChild,
            uint dwEventThread,
            uint dwmsEventTime)
        {
            Trace.WriteLine($"-> {hWinEventHook} : {eventType} : idObject={idObject}: idChild={idChild}");
        }
    }
}
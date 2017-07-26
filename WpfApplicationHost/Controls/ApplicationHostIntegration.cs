using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms.Integration;

using WpfApplicationHost.Windows;
using WpfApplicationHost.Windows.Structs;

namespace WpfApplicationHost.Controls
{
    /// <summary>
    /// <see cref="WindowsFormsHost"/> integration with external application.
    /// </summary>
    internal sealed class ApplicationHostIntegration
    {
        private IntPtr _currentHook = IntPtr.Zero;

        private IntPtr _windowHandle = IntPtr.Zero;

        /// <summary>
        /// <see cref="ApplicationHostIntegration"/> Constructor.
        /// </summary>
        public ApplicationHostIntegration(WindowsFormsHost formsHost)
        {
            FormsHost = formsHost;
            FormsHost.SizeChanged += (a, e) => ResizeChild();
        }

        private WindowsFormsHost FormsHost { get; }

        /// <summary>
        /// Attach to process.
        /// </summary>
        /// <param name="process">Process instance.</param>
        public void AttachToProcess(Process process)
        {
            Detach();
            _windowHandle = process.MainWindowHandle;

            // Set application PARENT to this control
            Win32Api.SetParent(_windowHandle, FormsHost.Handle);

            // Move window to left top corner. Set Width and Height equals to control size
            ResizeChild();

            // Prevent delegate to be collected by GC
            var hookDelegate = new WinEventDelegate(Hook);
            GCHandle.Alloc(hookDelegate);

            // Set hook function on window events
            _currentHook = Win32Api.SetWinEventHook(
                WindowsEvents.WINEVENT_OUTOFCONTEXT,
                WindowsEvents.EVENT_AIA_END,
                IntPtr.Zero,
                hookDelegate,
                0,
                0,
                WindowsEvents.WINEVENT_OUTOFCONTEXT);
        }

        private void ResizeChild()
        {
            Win32Api.MoveWindow(_windowHandle, 0, 0, (int)FormsHost.ActualWidth, (int)FormsHost.ActualHeight, true);
        }

        private void Detach()
        {
            Win32Api.UnhookWinEvent(_currentHook);
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
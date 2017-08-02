using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Threading;

using HostControlLibary.Data;
using HostControlLibary.Windows;
using HostControlLibary.Windows.Structs;

using Application = System.Windows.Application;

namespace HostControlLibary.Controls
{
    /// <summary>
    /// <see cref="WindowsFormsHost"/> integration with external application.
    /// </summary>
    internal sealed class ApplicationHostIntegration
    {
        private const string ExecutableFile = ".exe";

        private IntPtr _currentHook = IntPtr.Zero;

        private IntPtr _windowHandle = IntPtr.Zero;
        
        /// <summary>
        /// <see cref="ApplicationHostIntegration"/> Constructor.
        /// </summary>
        public ApplicationHostIntegration(WindowsFormsHost formsHost)
        {
            FormsHost = formsHost;
            FormsHost.SizeChanged += (a, e) => ResizeChild();
            FormsHost.Child = FormsHostUserControl = new UserControl(); 
            Application.Current.Exit += OnApplicationExit;
        }

        private UserControl FormsHostUserControl { get; set; }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            CurrentProcess?.Kill();
        }

        /// <summary>
        /// Application state changed event.
        /// </summary>
        public event EventHandler<ApplicationStateEventArgs> OnApplicationStateChanged;
        
        internal Process CurrentProcess { get; set; }

        private WindowsFormsHost FormsHost { get; }
        
        public bool Validate(string applicationPath, out string errorText)
        {
            errorText = null;
            if (string.IsNullOrEmpty(applicationPath))
            {
                errorText = $"{nameof(applicationPath)} is null";
                return false;
            }

            if (!File.Exists(applicationPath))
            {
                errorText = $"{nameof(applicationPath)} file not exists";
                return false;
            }

            if (Path.GetExtension(applicationPath) != ExecutableFile)
            {
                errorText = $"{nameof(applicationPath)} file is not executable ({ExecutableFile}).";
                return false;
            }

            return true;
        }

        private ProcessStartInfo CreateProcessInfo(string applicationPath)
        {
            return new ProcessStartInfo(applicationPath)
            {
                WindowStyle = ProcessWindowStyle.Normal,
                UseShellExecute = false
            };
        }

        /// <summary>
        /// Start application.
        /// </summary>
        /// <param name="applicationPath">Application path.</param>
        /// <param name="errorText">Error text.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool TryStartApplication(string applicationPath, out string errorText)
        {
            errorText = string.Empty;
            try
            {
                if (CurrentProcess != null && !CurrentProcess.HasExited)
                {
                    CurrentProcess.Kill();
                }

                var startInfo = CreateProcessInfo(applicationPath);

                CurrentProcess = new Process() { StartInfo = startInfo, EnableRaisingEvents = true };

                CurrentProcess.Exited += OnCurrentProcessExited;
                CurrentProcess.Start();
                CurrentProcess.WaitForInputIdle(30000);

                if (CurrentProcess.HasExited)
                {
                    Process childProcess;
                    
                    // Process terminated before idle state. The reason can be start of another process.
                    if (!TryGetChildProcess(CurrentProcess.Id, out childProcess, out errorText))
                    {
                        return false;
                    }

                    CurrentProcess = childProcess;
                }

                if (CurrentProcess.MainWindowHandle == IntPtr.Zero)
                {
                    errorText = $"{nameof(Process.MainWindowHandle)} not founded";
                    CurrentProcess.Kill();
                    return false;
                }

                AttachToProcess(CurrentProcess);
                OnApplicationStateChanged?.Invoke(this, new ApplicationStateEventArgs(ApplicationState.Started));
                return true;
            }
            catch (Exception exception)
            {
                errorText = exception.Message;
            }

            return false;
        }

        private static bool TryGetChildProcess(int parentId, out Process childProcess, out string error)
        {
            childProcess = null;
            error = null;
            var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_Process WHERE ParentProcessID={parentId}");
            var childrenProcessesIds = new List<int>();
            
            foreach (var managementObject in searcher.Get().OfType<ManagementObject>())
            {
                childrenProcessesIds.Add(Convert.ToInt32(managementObject["ProcessID"]));
            }

            if (!childrenProcessesIds.Any())
            {
                return false;
            }

            if (childrenProcessesIds.Count > 1)
            {
                error = $"Process owner {parentId} have {childrenProcessesIds.Count} children processes";
                return false;
            }

            var processes = Process.GetProcesses();
            childProcess = processes.First(process => process.Id == childrenProcessesIds.Single());
            childProcess.WaitForInputIdle(30000);
            return true;
        }

        private void OnCurrentProcessExited(object sender, EventArgs e)
        {
            OnApplicationStateChanged?.Invoke(this, new ApplicationStateEventArgs(ApplicationState.Terminated));
        }

        /// <summary>
        /// Attach to process.
        /// </summary>
        /// <param name="process">Process instance.</param>
        private void AttachToProcess(Process process)
        {
            Detach();
            _windowHandle = process.MainWindowHandle;

            // Set application PARENT to this control
            Win32Api.SetParent(_windowHandle, FormsHostUserControl.Handle);

            // WS_VISIBLE - This style can be turned on and off by using the ShowWindow or SetWindowPos function.
            Win32Api.SetWindowLong(_windowHandle, WindowsIndexStyle.GWL_STYLE, WindowStyles.WS_VISIBLE);

            // Move window to left top corner. Set Width and Height equals to control size
            ResizeChild();

            // Prevent delegate to be collected by GC
            var hookDelegate = new WinEventDelegate(Hook);
            GCHandle.Alloc(hookDelegate);

            // Window thread id
            int threadId = Win32Api.GetWindowThreadProcessId(_windowHandle, IntPtr.Zero);

            // Set hook function on window events
            _currentHook = Win32Api.SetWinEventHook(
                WindowsEvents.WINEVENT_SKIPOWNTHREAD,
                WindowsEvents.EVENT_MAX,
                IntPtr.Zero,
                hookDelegate,
                0,
                threadId,
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
            WindowsEvents winEvent,
            uint eventType,
            IntPtr hwnd,
            int idObject,
            int idChild,
            uint dwEventThread,
            uint dwmsEventTime)
        {
            Trace.WriteLine($"-> {winEvent} : {eventType} : idObject={idObject}: idChild={idChild}");
        }
    }
}
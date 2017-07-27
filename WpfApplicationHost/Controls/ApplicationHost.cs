using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Threading;

namespace WpfApplicationHost.Controls
{
    /// <summary>
    /// Application host control.
    /// </summary>
    public sealed class ApplicationHost : WindowsFormsHost
    {
        private const string ExecutableFile = ".exe";

        /// <summary>
        /// Dependency property for <see cref="ApplicationPath"/>.
        /// </summary>
        public static readonly DependencyProperty ApplicationPathProperty =
            DependencyProperty.Register(
                nameof(ApplicationPath),
                typeof(string),
                typeof(ApplicationHost),
                new PropertyMetadata(null, OnApplicationPathChanged));

        /// <summary>
        /// Dependency property for <see cref="ApplicationRunning"/>.
        /// </summary>
        public static readonly DependencyProperty ApplicationRunningProperty =
            DependencyProperty.Register(
                nameof(ApplicationRunning),
                typeof(bool),
                typeof(ApplicationHost),
                new PropertyMetadata(default(bool)));

        /// <summary>
        /// Dependency property for <see cref="ErrorText"/>.
        /// </summary>
        public static readonly DependencyProperty ErrorTextProperty = DependencyProperty.Register(
            "ErrorText",
            typeof(string),
            typeof(ApplicationHost),
            new PropertyMetadata(default(string)));

        private readonly ApplicationHostIntegration _applicationHostIntegration;

        /// <summary>
        /// <see cref="ApplicationHost"/> Constructor.
        /// </summary>
        public ApplicationHost()
        {
            _applicationHostIntegration = new ApplicationHostIntegration(this);
            Application.Current.Exit += OnApplicationExit;
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            CurrentProcess?.Kill();
        }

        /// <summary>
        /// Current error text.
        /// </summary>
        public string ErrorText
        {
            get
            {
                return (string)GetValue(ErrorTextProperty);
            }
            set
            {
                SetValue(ErrorTextProperty, value);
            }
        }

        /// <summary>
        /// Is application running at this moment.
        /// </summary>
        public bool ApplicationRunning
        {
            get
            {
                return (bool)GetValue(ApplicationRunningProperty);
            }
            set
            {
                SetValue(ApplicationRunningProperty, value);
            }
        }

        /// <summary>
        /// Path to application.
        /// </summary>
        public string ApplicationPath
        {
            get
            {
                return (string)GetValue(ApplicationPathProperty);
            }
            set
            {
                SetValue(ApplicationPathProperty, value);
            }
        }

        internal Process CurrentProcess { get; set; }

        private static void OnApplicationPathChanged(
            DependencyObject depObject,
            DependencyPropertyChangedEventArgs args)
        {
            var appHost = (ApplicationHost)depObject;
            appHost.ApplicationPathChanged();
        }

        private void ApplicationPathChanged()
        {
            try
            {
                if (!Validate())
                {
                    return;
                }

                if (CurrentProcess != null)
                {
                    CurrentProcess.Kill();
                    CurrentProcess.Exited -= OnCurrentProcessExited;
                }

                var startInfo = new ProcessStartInfo(ApplicationPath)
                {
                    WindowStyle = ProcessWindowStyle.Normal,
                    UseShellExecute = false
                };

                CurrentProcess = new Process() { StartInfo = startInfo, EnableRaisingEvents = true };

                CurrentProcess.Exited += OnCurrentProcessExited;
                CurrentProcess.Start();
                CurrentProcess.WaitForInputIdle(30000);

                if (CurrentProcess.HasExited)
                {
                    ErrorText = "Unexpected application termination";
                    return;
                }

                if (CurrentProcess.MainWindowHandle == IntPtr.Zero)
                {
                    ErrorText = $"{nameof(Process.MainWindowHandle)} not founded";
                    CurrentProcess.Kill();
                    return;
                }
                
                _applicationHostIntegration.AttachToProcess(CurrentProcess);
                ApplicationRunning = true;
            }
            catch (Exception exception)
            {
                ErrorText = exception.Message;
            }
        }
        
        private void OnCurrentProcessExited(object sender, EventArgs args)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                ApplicationRunning = false;
            }));
        }

        private bool Validate()
        {
            if (string.IsNullOrEmpty(ApplicationPath))
            {
                ErrorText = $"{nameof(ApplicationPath)} is null";
                return false;
            }

            if (!File.Exists(ApplicationPath))
            {
                ErrorText = $"{nameof(ApplicationPath)} file not exists";
                return false;
            }

            if (Path.GetExtension(ApplicationPath) != ExecutableFile)
            {
                ErrorText = $"{nameof(ApplicationPath)} file is not executable ({ExecutableFile}).";
                return false;
            }

            return true;
        }
    }
}
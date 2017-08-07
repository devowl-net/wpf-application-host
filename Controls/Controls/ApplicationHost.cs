using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;

using HostControlLibary.Data;

namespace HostControlLibary.Controls
{
    /// <summary>
    /// Application host control.
    /// </summary>
    public sealed class ApplicationHost : UserControl
    {
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

        /// <summary>
        /// Dependency property for <see cref="MainWindowShowTimeoutSeconds"/>.
        /// </summary>
        public static readonly DependencyProperty MainWindowShowTimeoutSecondsProperty =
            DependencyProperty.Register(
                "MainWindowShowTimeoutSeconds",
                typeof(int),
                typeof(ApplicationHost),
                new PropertyMetadata(10));

        /// <summary>
        /// <see cref="ApplicationHost"/> Constructor.
        /// </summary>
        public ApplicationHost()
        {
            var formHost = new WindowsFormsHost();
            AddChild(formHost);
            ApplicationHostIntegration = new ApplicationHostIntegration(formHost, MainWindowShowTimeoutSeconds);
            ApplicationHostIntegration.OnApplicationStateChanged += OnExternalApplicationStateChanged;
        }

        /// <summary>
        /// Await time for process main window shown. Default value is 10 seconds.
        /// </summary>
        /// <remarks>In a seconds.</remarks>
        public int MainWindowShowTimeoutSeconds
        {
            get
            {
                return (int)GetValue(MainWindowShowTimeoutSecondsProperty);
            }
            set
            {
                SetValue(MainWindowShowTimeoutSecondsProperty, value);
            }
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

        internal ApplicationHostIntegration ApplicationHostIntegration { get; private set; }

        private static void OnApplicationPathChanged(
            DependencyObject depObject,
            DependencyPropertyChangedEventArgs args)
        {
            var appHost = (ApplicationHost)depObject;
            appHost.ApplicationPathChanged();
        }

        private void OnExternalApplicationStateChanged(object sender, ApplicationStateEventArgs args)
        {
            if (Dispatcher.CheckAccess())
            {
                ProcessStateChanged(args);
            }
            else
            {
                Dispatcher.Invoke(new Action(() => ProcessStateChanged(args)));
            }
        }

        private void ProcessStateChanged(ApplicationStateEventArgs args)
        {
            switch (args.State)
            {
                case ApplicationState.Started:
                    ApplicationRunning = true;
                    break;
                case ApplicationState.Terminated:
                    ApplicationRunning = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ApplicationPathChanged()
        {
            string error;
            if (!ApplicationHostIntegration.Validate(ApplicationPath, out error) ||
                !ApplicationHostIntegration.TryStartApplication(ApplicationPath, out error))
            {
                ErrorText = error;
                return;
            }

            ErrorText = string.Empty;
        }
    }
}
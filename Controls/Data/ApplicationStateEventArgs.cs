using System;

namespace HostControlLibary.Data
{
    /// <summary>
    /// Application state changed.
    /// </summary>
    internal class ApplicationStateEventArgs : EventArgs
    {
        /// <summary>
        /// <see cref="ApplicationStateEventArgs"/> Constructor.
        /// </summary>
        public ApplicationStateEventArgs(ApplicationState state, string description = "")
        {
            State = state;
            Description = description;
        }

        /// <summary>
        /// Application state.
        /// </summary>
        public ApplicationState State { get; private set; }

        /// <summary>
        /// State description.
        /// </summary>
        public string Description { get; private set; }
    }
}

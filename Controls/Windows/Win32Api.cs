using System;
using System.Runtime.InteropServices;

using HostControlLibary.Windows.Structs;

namespace HostControlLibary.Windows
{
    /// <summary>
    /// Callback used by SetWinEventHook.
    /// </summary>
    /// <remarks>http://www.pinvoke.net/default.aspx/user32/WinEventDelegate.html</remarks>
    internal delegate void WinEventDelegate(WindowsEvents winEvent, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    /// <summary>
    /// Contains Windows API methods.
    /// </summary>
    internal class Win32Api
    {
        /// <summary>
        /// Retrieves the dimensions of the bounding rectangle of the specified window. The dimensions are given in screen coordinates that are relative to the upper-left corner of the screen.
        /// </summary>
        /// <param name="hwnd">A handle to the window. </param>
        /// <param name="lpRect">A pointer to a <see cref="RECT"/> structure that receives the screen coordinates of the upper-left and lower-right corners of the window. </param>
        /// <returns></returns>
        /// <remarks>https://msdn.microsoft.com/en-us/library/windows/desktop/ms633519(v=vs.85).aspx</remarks>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        /// <summary>
        /// Changes the parent window of the specified child window. 
        /// </summary>
        /// <param name="hWndChild">A handle to the child window.</param>
        /// <param name="hWndNewParent">A handle to the new parent window. If this parameter is NULL, the desktop window becomes the new parent window. If this parameter is HWND_MESSAGE, the child window becomes a message-only window. </param>
        /// <returns>If the function succeeds, the return value is a handle to the previous parent window. If the function fails, the return value is NULL.To get extended error information, call GetLastError. </returns>
        /// <remarks>https://msdn.microsoft.com/en-us/library/windows/desktop/ms633541(v=vs.85).aspx</remarks>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        /// <summary>
        /// Changes an attribute of the specified window. The function also sets the 32-bit (long) value at the specified offset into the extra window memory.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs. The SetWindowLongPtr function fails if the process that owns the window specified by the hWnd parameter is at a higher process privilege in the UIPI hierarchy than the process the calling thread resides in.</param>
        /// <param name="nIndex">The zero-based offset to the value to be set. Valid values are in the range zero through the number of bytes of extra window memory, minus the size of a LONG_PTR.</param>
        /// <param name="dwNewLong"></param>
        /// <returns>
        /// If the function succeeds, the return value is the previous value of the specified offset.
        /// If the function fails, the return value is zero.To get extended error information, call GetLastError.
        /// If the previous value is zero and the function succeeds, the return value is zero, but the function does not clear the last error information. To determine success or failure, clear the last error information by calling SetLastError with 0, then call SetWindowLongPtr.Function failure will be indicated by a return value of zero and a GetLastError result that is nonzero.       
        /// </returns>
        /// <remarks>https://msdn.microsoft.com/en-us/library/windows/desktop/ms644898(v=vs.85).aspx</remarks>
        [DllImport("user32.dll")]
        public static extern uint SetWindowLong(IntPtr hWnd, WindowsIndexStyle nIndex, WindowStyles dwNewLong);

        /// <summary>
        ///     The MoveWindow function changes the position and dimensions of the specified window. For a top-level window, the
        ///     position and dimensions are relative to the upper-left corner of the screen. For a child window, they are relative
        ///     to the upper-left corner of the parent window's client area.
        ///     <para>
        ///     Go to https://msdn.microsoft.com/en-us/library/windows/desktop/ms633534%28v=vs.85%29.aspx for more
        ///     information
        ///     </para>
        /// </summary>
        /// <param name="hWnd">C++ ( hWnd [in]. Type: HWND )<br /> Handle to the window.</param>
        /// <param name="x">C++ ( X [in]. Type: int )<br />Specifies the new position of the left side of the window.</param>
        /// <param name="y">C++ ( Y [in]. Type: int )<br /> Specifies the new position of the top of the window.</param>
        /// <param name="nWidth">C++ ( nWidth [in]. Type: int )<br />Specifies the new width of the window.</param>
        /// <param name="nHeight">C++ ( nHeight [in]. Type: int )<br />Specifies the new height of the window.</param>
        /// <param name="bRepaint">
        ///     C++ ( bRepaint [in]. Type: bool )<br />Specifies whether the window is to be repainted. If this
        ///     parameter is TRUE, the window receives a message. If the parameter is FALSE, no repainting of any kind occurs. This
        ///     applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the
        ///     parent window uncovered as a result of moving a child window.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is nonzero.<br /> If the function fails, the return value is zero.
        ///     <br />To get extended error information, call GetLastError.
        /// </returns>
        /// <remarks>http://www.pinvoke.net/default.aspx/user32.movewindow</remarks>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        /// <summary>
        /// Sets an event hook function for a range of events.
        /// </summary>
        /// <param name="eventMin">Specifies the event constant for the lowest event value in the range of events that are handled by the hook function. This parameter can be set to EVENT_MIN to indicate the lowest possible event value.</param>
        /// <param name="eventMax">Specifies the event constant for the highest event value in the range of events that are handled by the hook function. This parameter can be set to EVENT_MAX to indicate the highest possible event value.</param>
        /// <param name="hmodWinEventProc">Handle to the DLL that contains the hook function at lpfnWinEventProc, if the WINEVENT_INCONTEXT flag is specified in the dwFlags parameter. If the hook function is not located in a DLL, or if the WINEVENT_OUTOFCONTEXT flag is specified, this parameter is NULL.</param>
        /// <param name="lpfnWinEventProc">Pointer to the event hook function. For more information about this function, see WinEventProc.</param>
        /// <param name="idProcess">Specifies the ID of the process from which the hook function receives events. Specify zero (0) to receive events from all processes on the current desktop.</param>
        /// <param name="idThread">Specifies the ID of the thread from which the hook function receives events. If this parameter is zero, the hook function is associated with all existing threads on the current desktop.</param>
        /// <param name="dwFlags">Flag values that specify the location of the hook function and of the events to be skipped.</param>
        /// <returns>
        /// If successful, returns an HWINEVENTHOOK value that identifies this event hook instance. Applications save this return value to use it with the UnhookWinEvent function.
        /// If unsuccessful, returns zero.
        /// </returns>
        /// <remarks>https://msdn.microsoft.com/ru-ru/library/windows/desktop/dd373640%28v=vs.85%29.aspx?f=255&MSPPError=-2147217396</remarks>
        /// <remarks>http://www.pinvoke.net/default.aspx/user32.SetWinEventHook</remarks>
        [DllImport("user32.dll")]
        public static extern IntPtr SetWinEventHook(WindowsEvents eventMin, WindowsEvents eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, int idProcess, int idThread, WindowsEvents dwFlags);

        /// <summary>
        /// Removes an event hook function created by a previous call to SetWinEventHook.
        /// </summary>
        /// <param name="hWinEventHook">Handle to the event hook returned in the previous call to SetWinEventHook.</param>
        /// <returns>If successful, returns TRUE; otherwise, returns FALSE.</returns>
        /// <remarks>http://www.pinvoke.net/default.aspx/user32/UnhookWinEvent.html</remarks>
        /// <remarks>https://msdn.microsoft.com/en-us/library/windows/desktop/dd373671(v=vs.85).aspx</remarks>
        [DllImport("user32.dll")]
        public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        /// <summary>
        /// Retrieves the identifier of the thread that created the specified window and, optionally, the identifier of the process that created the window. 
        /// </summary>
        /// <param name="hWnd">A handle to the window. </param>
        /// <param name="ProcessId">A pointer to a variable that receives the process identifier. If this parameter is not NULL, GetWindowThreadProcessId copies the identifier of the process to the variable; otherwise, it does not.</param>
        /// <returns>The return value is the identifier of the thread that created the window. </returns>
        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);
    }
}
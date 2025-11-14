
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

public static class WindowManager
{
    // --- Win32 API Signatures ---
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);
    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
    [DllImport("user32.dll")]
    static extern bool TileWindows(IntPtr hwndParent, int wHow, Rectangle rc, int cKids, IntPtr[] lpKids);
    [DllImport("user32.dll")]
    static extern bool CascadeWindows(IntPtr hwndParent, int wHow, Rectangle rc, int cKids, IntPtr[] lpKids);


    // --- Window State Constants ---
    public const int SW_MAXIMIZE = 3;
    public const int SW_MINIMIZE = 6;
    public const int SW_RESTORE = 9;
    public const uint WM_CLOSE = 0x0010;
    const byte VK_CONTROL = 0x11;
    const byte VK_TAB = 0x09;
    const uint KEYEVENTF_KEYUP = 0x0002;

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }


    // --- State Tracking ---
    private static readonly Dictionary<string, Process> _profileProcesses = new Dictionary<string, Process>();
    private static readonly Dictionary<string, IntPtr> _profileWindowHandles = new Dictionary<string, IntPtr>();

    // --- Public Methods ---

    public static void RegisterProfileProcess(string profileName, Process process)
    {
        if (string.IsNullOrEmpty(profileName) || process == null) return;
        _profileProcesses[profileName] = process;
        // Use a background thread to find the handle to avoid blocking UI
        ThreadPool.QueueUserWorkItem(_ =>
        {
            Thread.Sleep(500); // Give window time to appear
            FindAndCacheWindowHandle(profileName);
        });
    }

    public static void UnregisterProfile(string profileName)
    {
        if (string.IsNullOrEmpty(profileName)) return;
        _profileProcesses.Remove(profileName);
        _profileWindowHandles.Remove(profileName);
    }
    public static void MaximizeWindow(string profileName) => PerformActionOnProfileWindow(profileName, SW_MAXIMIZE);
    public static void MinimizeWindow(string profileName) => PerformActionOnProfileWindow(profileName, SW_MINIMIZE);
    public static void RestoreWindow(string profileName) => PerformActionOnProfileWindow(profileName, SW_RESTORE);

    public static void PerformActionOnProfileWindow(string profileName, int command)
    {
        IntPtr targetHWnd = GetWindowHandle(profileName);
        if (targetHWnd != IntPtr.Zero) ShowWindow(targetHWnd, command);
    }

    public static void CloseProfileWindow(string profileName)
    {
        IntPtr targetHWnd = GetWindowHandle(profileName, false);
        if (targetHWnd != IntPtr.Zero)
        {
            SendMessage(targetHWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }
        UnregisterProfile(profileName);
    }
    public static string GetActiveTabTitle(string profileName)
    {
        IntPtr hWnd = GetWindowHandle(profileName, false);
        if (hWnd != IntPtr.Zero)
        {
            int length = GetWindowTextLength(hWnd);
            if (length == 0) return "N/A";
            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }
        return "N/A";
    }

    public static void SwitchToNextTab(string profileName)
    {
        IntPtr hWnd = GetWindowHandle(profileName, false);
        if (hWnd != IntPtr.Zero)
        {
            SetForegroundWindow(hWnd);
            Thread.Sleep(100); // Small delay
            // Simulate Ctrl+Tab
            keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero); // Press Ctrl
            keybd_event(VK_TAB, 0, 0, UIntPtr.Zero); // Press Tab
            keybd_event(VK_TAB, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // Release Tab
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // Release Ctrl
        }
    }
    public static void ArrangeWindows(bool cascade)
    {
        var handles = _profileWindowHandles.Values.Where(h => IsWindow(h) && IsWindowVisible(h)).ToArray();
        if (handles.Length < 1) return;

        Rectangle screen = Screen.PrimaryScreen.WorkingArea;

        if (cascade)
        {
            // The 'rc' parameter is ignored for this command
            CascadeWindows(IntPtr.Zero, 0 /*MDITILE_ZORDER*/, Rectangle.Empty, handles.Length, handles);
        }
        else // Tile
        {
            TileWindows(IntPtr.Zero, 1/*MDITILE_HORIZONTAL*/, screen, handles.Length, handles);
        }
    }


    // --- Private Helper Methods ---
    private static IntPtr GetWindowHandle(string profileName, bool unregisterOnFailure = true)
    {
        if (string.IsNullOrEmpty(profileName)) return IntPtr.Zero;

        // Check cache first
        if (_profileWindowHandles.TryGetValue(profileName, out IntPtr handle) && IsWindow(handle))
        {
            return handle;
        }

        // If not in cache or invalid, find it
        IntPtr foundHandle = FindAndCacheWindowHandle(profileName);
        if (foundHandle != IntPtr.Zero)
        {
            return foundHandle;
        }

        // If still not found, unregister if requested
        if (unregisterOnFailure) UnregisterProfile(profileName);
        return IntPtr.Zero;
    }

    private static IntPtr FindAndCacheWindowHandle(string profileName)
    {
        if (!_profileProcesses.TryGetValue(profileName, out Process proc) || proc.HasExited) return IntPtr.Zero;

        IntPtr foundHandle = IntPtr.Zero;

        // Attempt to get the main window handle directly. This is faster.
        proc.Refresh();
        if (proc.MainWindowHandle != IntPtr.Zero && IsWindowVisible(proc.MainWindowHandle))
        {
            foundHandle = proc.MainWindowHandle;
        }
        else // Fallback to enumerating all windows for the process
        {
            EnumWindows((hWnd, lParam) =>
            {
                GetWindowThreadProcessId(hWnd, out uint pid);
                if (pid == proc.Id && IsWindowVisible(hWnd) && GetWindowTextLength(hWnd) > 0)
                {
                    foundHandle = hWnd;
                    return false; // Stop enumerating
                }
                return true; // Continue enumerating
            }, IntPtr.Zero);
        }

        if (foundHandle != IntPtr.Zero) _profileWindowHandles[profileName] = foundHandle;

        return foundHandle;
    }
}

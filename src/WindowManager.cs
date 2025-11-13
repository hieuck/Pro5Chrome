
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

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

    // --- Window State Constants ---
    public const int SW_MAXIMIZE = 3;
    public const int SW_MINIMIZE = 6;
    public const int SW_RESTORE = 9;
    public const uint WM_CLOSE = 0x0010;

    // --- State Tracking ---
    private static readonly Dictionary<string, Process> _profileProcesses = new Dictionary<string, Process>();
    private static readonly Dictionary<string, IntPtr> _profileWindowHandles = new Dictionary<string, IntPtr>();

    // --- Public Methods ---

    /// <summary>
    /// Registers a process associated with a profile name for window tracking.
    /// </summary>
    public static void RegisterProfileProcess(string profileName, Process process)
    {
        if (string.IsNullOrEmpty(profileName) || process == null) return;
        _profileProcesses[profileName] = process;
        // Attempt to find the handle proactively. It might take a moment to appear.
        ThreadPool.QueueUserWorkItem(_ => 
        {
            Thread.Sleep(500); // Wait for the window to likely be created
            FindAndCacheWindowHandle(profileName);
        });
    }

    /// <summary>
    /// Removes a profile from tracking.
    /// </summary>
    public static void UnregisterProfile(string profileName)
    {
        if (string.IsNullOrEmpty(profileName)) return;
        _profileProcesses.Remove(profileName);
        _profileWindowHandles.Remove(profileName);
    }

    /// <summary>
    /// Performs a window state change action on a specific Chrome profile window.
    /// </summary>
    public static void PerformActionOnProfileWindow(string profileName, int command)
    {
        IntPtr targetHWnd = GetWindowHandle(profileName);
        if (targetHWnd != IntPtr.Zero) ShowWindow(targetHWnd, command);
    }

    /// <summary>
    /// Sends a close message to a specific Chrome profile window.
    /// </summary>
    public static void CloseProfileWindow(string profileName)
    {
        IntPtr targetHWnd = GetWindowHandle(profileName, false); // Get handle without unregistering yet
        if (targetHWnd != IntPtr.Zero)
        {
            SendMessage(targetHWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }
        // Always unregister the profile after a close attempt.
        UnregisterProfile(profileName);
    }

    // --- Private Helper Methods ---

    /// <summary>
    /// Gets the window handle for a profile. Tries cache, then live process, then unregisters.
    /// </summary>
    private static IntPtr GetWindowHandle(string profileName, bool unregisterOnFailure = true)
    {
        if (string.IsNullOrEmpty(profileName)) return IntPtr.Zero;

        // 1. Check cached handle first
        if (_profileWindowHandles.TryGetValue(profileName, out IntPtr handle) && IsWindow(handle))
        {
            return handle;
        }

        // 2. If cache fails, try to find it via the tracked process
        IntPtr foundHandle = FindAndCacheWindowHandle(profileName);
        if (foundHandle != IntPtr.Zero)
        {
            return foundHandle;
        }

        // 3. If not found, the process/window is gone. Clean up.
        if(unregisterOnFailure) UnregisterProfile(profileName);
        return IntPtr.Zero;
    }

    /// <summary>
    /// Finds the window handle for a tracked process and updates the cache.
    /// </summary>
    private static IntPtr FindAndCacheWindowHandle(string profileName)
    {
        if (!_profileProcesses.TryGetValue(profileName, out Process proc) || proc.HasExited) return IntPtr.Zero;

        IntPtr foundHandle = IntPtr.Zero;

        // Try the fast MainWIndowHandle first, with a refresh
        proc.Refresh();
        if (proc.MainWindowHandle != IntPtr.Zero && IsWindowVisible(proc.MainWindowHandle)) 
        {
            foundHandle = proc.MainWindowHandle;
        } 
        else // Fallback to enumerating all windows for that process ID
        {
             EnumWindows((hWnd, lParam) =>
            {
                GetWindowThreadProcessId(hWnd, out uint pid);
                if (pid == proc.Id && IsWindowVisible(hWnd) && GetWindowTextLength(hWnd) > 0)
                {
                    foundHandle = hWnd;
                    return false; // Stop searching
                }
                return true; // Continue
            }, IntPtr.Zero);
        }
       
        if (foundHandle != IntPtr.Zero) _profileWindowHandles[profileName] = foundHandle;
        
        return foundHandle;
    }
}


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

public static class WindowManager
{
    #region P/Invoke Signatures

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
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    // Window state constants
    private const uint SWP_NOZORDER = 0x0004;
    private const int SW_MAXIMIZE = 3;
    private const int SW_MINIMIZE = 6;
    private const int SW_RESTORE = 9;
    private const uint WM_CLOSE = 0x0010;

    #endregion

    private static int _nextWindowIndex = 0;

    private static List<IntPtr> GetBrowserWindowHandles(string browserExecutablePath)
    {
        string processName = "chrome"; // Default fallback
        if (!string.IsNullOrEmpty(browserExecutablePath) && File.Exists(browserExecutablePath))
        {
            processName = Path.GetFileNameWithoutExtension(browserExecutablePath);
        }

        var browserProcessIds = Process.GetProcessesByName(processName).Select(p => (uint)p.Id).ToHashSet();
        var browserWindows = new List<IntPtr>();

        if (browserProcessIds.Count > 0)
        {
            EnumWindows((hWnd, lParam) =>
            {
                GetWindowThreadProcessId(hWnd, out uint pid);
                if (browserProcessIds.Contains(pid) && IsWindowVisible(hWnd) && GetWindowTextLength(hWnd) > 0)
                {
                    browserWindows.Add(hWnd);
                }
                return true; // Continue enumeration
            }, IntPtr.Zero);
        }

        return browserWindows;
    }

    private static void ModifyWindowStateByProfile(string profileName, string browserExecutablePath, int command)
    {
        var browserWindows = GetBrowserWindowHandles(browserExecutablePath);
        string searchString1 = $" - {profileName} - "; 
        string searchString2 = $"{profileName} - "; 

        foreach (var hWnd in browserWindows)
        {
            int length = GetWindowTextLength(hWnd);
            if (length == 0) continue;

            StringBuilder titleBuilder = new StringBuilder(length + 1);
            GetWindowText(hWnd, titleBuilder, titleBuilder.Capacity);
            string windowTitle = titleBuilder.ToString();

            bool titleMatches = windowTitle.IndexOf(searchString1, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                windowTitle.StartsWith(searchString2, StringComparison.OrdinalIgnoreCase);

            if (titleMatches)
            {
                if(command == WM_CLOSE)
                {
                    SendMessage(hWnd, (uint)command, IntPtr.Zero, IntPtr.Zero);
                }
                else
                {
                    ShowWindow(hWnd, command);
                }
                break; 
            }
        }
    }

    public static void CloseWindowByProfileName(string profileName, string browserExecutablePath) => ModifyWindowStateByProfile(profileName, browserExecutablePath, (int)WM_CLOSE);
    public static void MaximizeWindowByProfileName(string profileName, string browserExecutablePath) => ModifyWindowStateByProfile(profileName, browserExecutablePath, SW_MAXIMIZE);
    public static void MinimizeWindowByProfileName(string profileName, string browserExecutablePath) => ModifyWindowStateByProfile(profileName, browserExecutablePath, SW_MINIMIZE);
    public static void RestoreWindowByProfileName(string profileName, string browserExecutablePath) => ModifyWindowStateByProfile(profileName, browserExecutablePath, SW_RESTORE);


    public static (string ActiveWindowTitle, List<string> InactiveWindowTitles) GetChromeWindowStates(string browserExecutablePath)
    {
        var activeTitle = string.Empty;
        var inactiveTitles = new List<string>();
        var allBrowserHandles = GetBrowserWindowHandles(browserExecutablePath);
        IntPtr foregroundWindowHandle = GetForegroundWindow();

        foreach (var hWnd in allBrowserHandles)
        {
            int length = GetWindowTextLength(hWnd);
            if (length == 0) continue;

            StringBuilder titleBuilder = new StringBuilder(length + 1);
            GetWindowText(hWnd, titleBuilder, titleBuilder.Capacity);
            string windowTitle = titleBuilder.ToString();

            if (hWnd == foregroundWindowHandle)
            {
                activeTitle = windowTitle;
            }
            else
            {
                inactiveTitles.Add(windowTitle);
            }
        }
        return (activeTitle, inactiveTitles);
    }

    public static void CycleToNextChromeWindow(string browserExecutablePath)
    {
        var chromeWindows = GetBrowserWindowHandles(browserExecutablePath);
        if (chromeWindows.Count == 0) return;

        if (_nextWindowIndex >= chromeWindows.Count)
        {
            _nextWindowIndex = 0;
        }

        IntPtr hWnd = chromeWindows[_nextWindowIndex];
        ShowWindow(hWnd, SW_RESTORE);
        SetForegroundWindow(hWnd);
        _nextWindowIndex++;
    }

    public static void ArrangeChromeWindows(int cols, int gap, string browserExecutablePath)
    {
        var chromeWindows = GetBrowserWindowHandles(browserExecutablePath);
        if (chromeWindows.Count == 0 || cols <= 0) return;

        PerformGlobalAction(browserExecutablePath, SW_RESTORE);

        Rectangle screen = Screen.PrimaryScreen.WorkingArea;
        int rows = (int)Math.Ceiling((double)chromeWindows.Count / cols);
        if (rows == 0) return;

        int width = (screen.Width - (cols + 1) * gap) / cols;
        int height = (screen.Height - (rows + 1) * gap) / rows;

        for (int i = 0; i < chromeWindows.Count; i++)
        {
            IntPtr hWnd = chromeWindows[i];
            int row = i / cols;
            int col = i % cols;
            int x = screen.Left + gap + col * (width + gap);
            int y = screen.Top + gap + row * (height + gap);
            SetWindowPos(hWnd, IntPtr.Zero, x, y, width, height, SWP_NOZORDER);
        }
    }

    public static void PerformGlobalAction(string browserExecutablePath, int command)
    {
        var chromeWindows = GetBrowserWindowHandles(browserExecutablePath);
        foreach (var hWnd in chromeWindows)
        {
            ShowWindow(hWnd, command);
        }
    }
}

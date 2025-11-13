
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

/// <summary>
/// A simple class to hold the titles of active and inactive Chrome windows.
/// </summary>
public class ChromeWindowStates
{
    public string ActiveWindowTitle { get; set; }
    public List<string> InactiveWindowTitles { get; set; }

    public ChromeWindowStates()
    {
        ActiveWindowTitle = string.Empty;
        InactiveWindowTitles = new List<string>();
    }
}

public static class WindowManager
{
    #region P/Invoke Signatures

    // Delegate and P/Invoke signatures for interacting with the Windows API
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

    // Flags for SetWindowPos
    private const uint SWP_NOZORDER = 0x0004;

    // Constants for ShowWindow
    private const int SW_MAXIMIZE = 3;
    private const int SW_MINIMIZE = 6;
    private const int SW_RESTORE = 9; // Also known as SW_NORMAL

    #endregion

    private static int _nextWindowIndex = 0;

    private static List<IntPtr> GetChromeWindowHandles()
    {
        var chromeProcessIds = Process.GetProcessesByName("chrome").Select(p => (uint)p.Id).ToHashSet();
        var chromeWindows = new List<IntPtr>();

        if (chromeProcessIds.Count > 0)
        {
            EnumWindows((hWnd, lParam) =>
            {
                GetWindowThreadProcessId(hWnd, out uint pid);
                if (chromeProcessIds.Contains(pid) && IsWindowVisible(hWnd) && GetWindowTextLength(hWnd) > 0)
                {
                    StringBuilder title = new StringBuilder(256);
                    GetWindowText(hWnd, title, title.Capacity);
                    if (!string.IsNullOrWhiteSpace(title.ToString()) && title.ToString().Contains(" - Google Chrome"))
                    {
                        chromeWindows.Add(hWnd);
                    }
                }
                return true; // Continue enumeration
            }, IntPtr.Zero);
        }

        return chromeWindows;
    }

    public static ChromeWindowStates GetChromeWindowStates()
    {
        var states = new ChromeWindowStates();
        var allChromeHandles = GetChromeWindowHandles();
        IntPtr foregroundWindowHandle = GetForegroundWindow();

        foreach (var hWnd in allChromeHandles)
        {
            int length = GetWindowTextLength(hWnd);
            if (length == 0) continue;

            StringBuilder titleBuilder = new StringBuilder(length + 1);
            GetWindowText(hWnd, titleBuilder, titleBuilder.Capacity);
            string windowTitle = titleBuilder.ToString();

            if (hWnd == foregroundWindowHandle)
            {
                states.ActiveWindowTitle = windowTitle;
            }
            else
            {
                states.InactiveWindowTitles.Add(windowTitle);
            }
        }
        return states;
    }

    public static void CycleToNextChromeWindow()
    {
        var chromeWindows = GetChromeWindowHandles();
        if (chromeWindows.Count == 0) return;

        // Reset index if it goes out of bounds
        if (_nextWindowIndex >= chromeWindows.Count)
        {
            _nextWindowIndex = 0;
        }

        IntPtr hWnd = chromeWindows[_nextWindowIndex];

        // Restore the window if it's minimized before activating
        ShowWindow(hWnd, SW_RESTORE);
        
        // Bring the window to the foreground
        SetForegroundWindow(hWnd);

        // Increment index for the next button press
        _nextWindowIndex++;
    }

    public static void ArrangeChromeWindows(int cols, int gap)
    {
        var chromeWindows = GetChromeWindowHandles();
        if (chromeWindows.Count == 0 || cols <= 0) return;

        RestoreAllWindows();

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

    public static void MaximizeAllWindows()
    {
        var chromeWindows = GetChromeWindowHandles();
        foreach (var hWnd in chromeWindows)
        {
            ShowWindow(hWnd, SW_MAXIMIZE);
        }
    }

    public static void MinimizeAllWindows()
    {
        var chromeWindows = GetChromeWindowHandles();
        foreach (var hWnd in chromeWindows)
        {
            ShowWindow(hWnd, SW_MINIMIZE);
        }
    }

    public static void RestoreAllWindows()
    {
        var chromeWindows = GetChromeWindowHandles();
        foreach (var hWnd in chromeWindows)
        {
            ShowWindow(hWnd, SW_RESTORE);
        }
    }
}

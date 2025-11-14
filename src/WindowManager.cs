
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
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
    [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    // --- Window State Constants ---
    public const int SW_MAXIMIZE = 3;
    public const int SW_MINIMIZE = 6;
    public const int SW_RESTORE = 9;
    public const uint WM_CLOSE = 0x0010;

    // --- State Tracking ---
    private static readonly Dictionary<string, IntPtr> _profileWindowHandles = new Dictionary<string, IntPtr>();
    private static List<IntPtr> _windowCycleList = new List<IntPtr>();
    private static int _currentWindowIndex = 0;

    #region Public Window Actions

    public static void MaximizeWindow(string profileName) => PerformActionOnProfileWindow(profileName, SW_MAXIMIZE);
    public static void MinimizeWindow(string profileName) => PerformActionOnProfileWindow(profileName, SW_MINIMIZE);
    public static void RestoreWindow(string profileName) => PerformActionOnProfileWindow(profileName, SW_RESTORE);
    public static void CloseProfileWindow(string profileName) => CloseSpecificProfileWindow(profileName);

    private static void PerformActionOnProfileWindow(string profileName, int command)
    {
        IntPtr targetHWnd = GetWindowHandle(profileName);
        if (targetHWnd != IntPtr.Zero) 
        {
            ShowWindow(targetHWnd, command);
        }
    }

    public static void ArrangeWindows(int columns, int margin, bool hideTaskbar)
    {
        var chromeWindows = GetAllChromeWindows();
        if (chromeWindows.Count == 0) return;

        var screen = Screen.PrimaryScreen.WorkingArea;
        // If hideTaskbar is checked, use the full screen bounds instead of the working area.
        if (hideTaskbar) { screen = Screen.PrimaryScreen.Bounds; }

        if (columns <= 0) columns = 1;
        int rows = (int)Math.Ceiling((double)chromeWindows.Count / columns);
        if (rows <= 0) rows = 1;

        int windowWidth = (screen.Width - (columns - 1) * margin) / columns;
        int windowHeight = (screen.Height - (rows - 1) * margin) / rows;

        for (int i = 0; i < chromeWindows.Count; i++)
        {
            IntPtr hWnd = chromeWindows[i];
            int row = i / columns;
            int col = i % columns;

            int x = screen.Left + col * (windowWidth + margin);
            int y = screen.Top + row * (windowHeight + margin);

            ShowWindow(hWnd, SW_RESTORE); // Restore if maximized/minimized before moving
            MoveWindow(hWnd, x, y, windowWidth, windowHeight, true);
        }
    }

    public static void SwitchToNextWindow()
    {
        _windowCycleList = GetAllChromeWindows(); // Refresh the list on each call
        if (_windowCycleList.Count == 0) return;

        _currentWindowIndex++;
        if (_currentWindowIndex >= _windowCycleList.Count) 
        { 
            _currentWindowIndex = 0; 
        }

        IntPtr nextWindow = _windowCycleList[_currentWindowIndex];
        ShowWindow(nextWindow, SW_RESTORE);
        SetForegroundWindow(nextWindow);
    }

    public static string GetActiveTabTitle(string profileName)
    {
        IntPtr hWnd = GetWindowHandle(profileName);
        if (hWnd != IntPtr.Zero)
        {
            int length = GetWindowTextLength(hWnd);
            if (length == 0) return "N/A";
            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }
        return "Không tìm thấy cửa sổ";
    }

    #endregion

    #region Window Finding and Management

    public static List<IntPtr> GetAllChromeWindows()
    {
        List<IntPtr> windowHandles = new List<IntPtr>();
        EnumWindows((hWnd, lParam) =>
        {
            if (!IsWindowVisible(hWnd) || GetWindowTextLength(hWnd) == 0) 
                return true; 

            uint processId;
            GetWindowThreadProcessId(hWnd, out processId);
            try
            {
                Process p = Process.GetProcessById((int)processId);
                if (p.ProcessName.ToLower() == "chrome" || p.ProcessName.ToLower() == "msedge")
                { 
                    // Extra check to avoid including background/utility windows
                    if (GetWindowRect(hWnd, out RECT r) && r.Right - r.Left > 100) // Basic check for a reasonably sized window
                    {
                         windowHandles.Add(hWnd);
                    }
                }
            }
            catch { /* Ignore processes that can't be accessed */ }

            return true; 
        }, IntPtr.Zero);

        return windowHandles;
    }

    private static IntPtr GetWindowHandle(string profileName)
    {
        if (string.IsNullOrEmpty(profileName)) return IntPtr.Zero;
        if (_profileWindowHandles.TryGetValue(profileName, out IntPtr handle) && IsWindow(handle) && IsWindowVisible(handle))
        {
            return handle;
        }

        IntPtr foundHandle = FindWindowByProfileName(profileName);
        if (foundHandle != IntPtr.Zero)
        {
            _profileWindowHandles[profileName] = foundHandle;
        }
        else
        {
            _profileWindowHandles.Remove(profileName);
        }
        return foundHandle;
    }

    private static IntPtr FindWindowByProfileName(string profileName)
    {
        IntPtr foundHandle = IntPtr.Zero;
        string searchPattern = $" - {profileName}";
        string defaultProfilePattern = "Google Chrome"; // Or Edge, etc.

        var allWindows = GetAllChromeWindows();

        foreach (var hWnd in allWindows)
        {
            StringBuilder titleBuilder = new StringBuilder(1024);
            GetWindowText(hWnd, titleBuilder, titleBuilder.Capacity);
            string windowTitle = titleBuilder.ToString();

            // Special handling for the Default profile which might not have a "- Profile" indicator
            if (profileName.Equals("Default", StringComparison.OrdinalIgnoreCase))
            {
                bool isDefault = true;
                for (int i=1; i < 100; i++) // Check if it is another numbered profile
                {
                    if(windowTitle.Contains($" - Profile {i}")) {
                         isDefault = false;
                         break;
                    }
                }
                if (isDefault) { foundHandle = hWnd; break; }
            }
            else if (windowTitle.Contains(searchPattern))
            {
                foundHandle = hWnd;
                break; 
            }
        }
        return foundHandle;
    }
    
    private static void CloseSpecificProfileWindow(string profileName)
    {
        IntPtr targetHWnd = GetWindowHandle(profileName);
        if (targetHWnd != IntPtr.Zero)
        {
            SendMessage(targetHWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            _profileWindowHandles.Remove(profileName);
        }
    }

    #endregion
}

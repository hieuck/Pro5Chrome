
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq; // Added to use LINQ extension methods like .Select()
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

public static class WindowManager
{
    // Delegate và P/Invoke signatures để tương tác với Windows API
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    
    [DllImport("user32.dll")]
    static extern int GetWindowTextLength(IntPtr hWnd);

    // Cờ cho hàm SetWindowPos
    private const uint SWP_NOZORDER = 0x0004;

    public static void ArrangeChromeWindows()
    {
        var chromeProcessIds = Process.GetProcessesByName("chrome").Select(p => (uint)p.Id).ToHashSet();
        if (chromeProcessIds.Count == 0) return;

        var chromeWindows = new List<IntPtr>();
        EnumWindows((hWnd, lParam) =>
        {
            GetWindowThreadProcessId(hWnd, out uint pid);
            if (chromeProcessIds.Contains(pid) && IsWindowVisible(hWnd) && GetWindowTextLength(hWnd) > 0)
            {
                chromeWindows.Add(hWnd);
            }
            return true;
        }, IntPtr.Zero);

        if (chromeWindows.Count == 0) return;

        // Sắp xếp các cửa sổ theo một lưới đơn giản trên màn hình chính
        Rectangle screen = Screen.PrimaryScreen.WorkingArea;
        int count = chromeWindows.Count;
        int cols = (int)Math.Ceiling(Math.Sqrt(count));
        int rows = (int)Math.Ceiling((double)count / cols);

        int width = screen.Width / cols;
        int height = screen.Height / rows;

        for (int i = 0; i < count; i++)
        {
            IntPtr hWnd = chromeWindows[i];
            int row = i / cols;
            int col = i % cols;
            int x = screen.Left + col * width;
            int y = screen.Top + row * height;
            SetWindowPos(hWnd, IntPtr.Zero, x, y, width, height, SWP_NOZORDER);
        }
    }
}

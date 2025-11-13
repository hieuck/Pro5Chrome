
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
    // P/Invoke Signatures
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
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    // Window state constants
    public const int SW_MAXIMIZE = 3;
    public const int SW_MINIMIZE = 6;
    public const int SW_RESTORE = 9;
    public const uint WM_CLOSE = 0x0010;


    /// <summary>
    /// Finds the handle of a Chrome window based on its profile name in the title.
    /// </summary>
    /// <param name="profileName">The name of the profile to find.</param>
    /// <returns>The window handle (IntPtr) if found; otherwise, IntPtr.Zero.</returns>
    private static IntPtr FindWindowForProfile(string profileName)
    {
        if (string.IsNullOrEmpty(profileName)) return IntPtr.Zero;

        IntPtr foundHandle = IntPtr.Zero;
        string searchString1 = $" - {profileName} - "; // e.g., "... - MyProfile - Google Chrome"
        string searchString2 = $"{profileName} - "; // e.g., "MyProfile - Google Chrome"

        EnumWindows((hWnd, lParam) =>
        {
            if (!IsWindowVisible(hWnd) || GetWindowTextLength(hWnd) == 0) return true; // Skip invisible windows

            GetWindowThreadProcessId(hWnd, out uint pid);
            try
            {
                Process proc = Process.GetProcessById((int)pid);
                // Ensure it's a chrome process
                if (!proc.ProcessName.Equals("chrome", StringComparison.OrdinalIgnoreCase)) return true;
            }
            catch (ArgumentException) { /* Process likely exited */ return true; }

            StringBuilder titleBuilder = new StringBuilder(GetWindowTextLength(hWnd) + 1);
            GetWindowText(hWnd, titleBuilder, titleBuilder.Capacity);
            string windowTitle = titleBuilder.ToString();

            // Check if the title matches the profile name pattern
            if (windowTitle.Contains(searchString1) || windowTitle.StartsWith(searchString2))
            {
                foundHandle = hWnd;
                return false; // Stop enumeration, we found it
            }

            return true; // Continue enumeration
        }, IntPtr.Zero);

        return foundHandle;
    }

    /// <summary>
    /// Performs a window state change action on a specific Chrome profile window.
    /// </summary>
    /// <param name="profileName">The name of the profile.</param>
    /// <param name="command">The window state command (e.g., SW_MINIMIZE, SW_MAXIMIZE).</param>
    public static void PerformActionOnProfileWindow(string profileName, int command)
    {
        if (string.IsNullOrEmpty(profileName)) return;

        IntPtr targetHWnd = FindWindowForProfile(profileName);

        if (targetHWnd != IntPtr.Zero)
        {
            ShowWindow(targetHWnd, command);
        }
        else
        {
            // Optional: Provide feedback if the window wasn't found
            // MessageBox.Show($"Không tìm thấy cửa sổ cho profile '{profileName}'.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

     /// <summary>
    /// Sends a close message to a specific Chrome profile window.
    /// </summary>
    /// <param name="profileName">The name of the profile.</param>
    public static void CloseProfileWindow(string profileName)
    {
        if (string.IsNullOrEmpty(profileName)) return;

        IntPtr targetHWnd = FindWindowForProfile(profileName);

        if (targetHWnd != IntPtr.Zero)
        {
            SendMessage(targetHWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }
    }
}

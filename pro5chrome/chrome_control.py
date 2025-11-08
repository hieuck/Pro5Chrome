"""Window / Chrome control helpers.
These functions operate on pygetwindow windows and psutil processes.
"""
import os
import logging
from typing import Optional, Sequence

import pygetwindow as gw
import psutil
import screeninfo

logger = logging.getLogger('Pro5Chrome')


def find_chrome_window_by_profile(profile: str, use_chrome_path: Optional[str] = None, default_chrome_path: str = None):
    """Find a pygetwindow Window by matching the Chrome process cmdline containing the profile directory.
    Returns a pygetwindow Window or None.
    """
    profile_directory = f"--profile-directory=Profile {profile}"

    for proc in psutil.process_iter(attrs=['pid', 'name', 'cmdline']):
        name = proc.info.get('name', '')
        if name.lower() in ('chrome.exe', 'centbrowser.exe', 'chrome'):
            try:
                cmdline = proc.info.get('cmdline') or []
                if any(profile_directory in str(arg) for arg in cmdline):
                    logger.debug(f"Found process with profile: {cmdline}")
                    # Look for windows that belong to Chrome instances
                    windows = []
                    try:
                        windows = gw.getWindowsWithTitle('Google Chrome') + gw.getWindowsWithTitle('Cent Browser')
                    except Exception:
                        windows = gw.getAllWindows()

                    for win in windows:
                        try:
                            if proc.pid == getattr(win, '_hWnd', None):
                                return win
                        except Exception:
                            continue
            except (psutil.NoSuchProcess, psutil.AccessDenied, psutil.ZombieProcess):
                continue
    return None


def find_chrome_window(profile_name: str, main_window_title: Optional[str] = None):
    """Naive search for windows whose title contains the profile name or Chrome markers.
    Returns the top-most matching pygetwindow Window or None.
    """
    all_titles = gw.getAllTitles()
    filtered = []
    for title in all_titles:
        if profile_name in title or '- Google Chrome' in title or '- Cent Browser' in title:
            windows = gw.getWindowsWithTitle(title)
            filtered.extend(windows)

    if main_window_title:
        filtered = [w for w in filtered if w.title != main_window_title]

    if filtered:
        filtered.sort(key=lambda x: getattr(x, '_hWnd', 0), reverse=True)
        return filtered[0]
    return None


def arrange_chrome_windows(num_columns: int = 2, margin: int = 0, hide_taskbar: bool = False, main_window_title: Optional[str] = None):
    """Arrange Chrome/CentBrowser windows on the primary monitor in a grid.
    This function finds windows itself.
    """
    screen = screeninfo.get_monitors()[0]
    screen_width = screen.width
    screen_height = screen.height

    taskbar_height = 0 if hide_taskbar else 40
    effective_height = screen_height - taskbar_height

    chrome_windows = gw.getWindowsWithTitle('Google Chrome') + gw.getWindowsWithTitle('Cent Browser')
    if main_window_title:
        chrome_windows = [w for w in chrome_windows if w.title != main_window_title]

    if not chrome_windows:
        logger.info('No Chrome windows found to arrange')
        return

    for w in chrome_windows:
        try:
            if w.isMaximized:
                w.restore()
        except Exception:
            pass

    chrome_windows.sort(key=lambda x: getattr(x, '_hWnd', 0), reverse=True)

    if num_columns < 1:
        num_columns = 1
    num_rows = (len(chrome_windows) + num_columns - 1) // num_columns
    if num_rows == 0:
        logger.info('No windows to arrange after computing rows')
        return

    window_width = (screen_width - (num_columns - 1) * margin) // num_columns
    window_height = (effective_height - (num_rows - 1) * margin) // num_rows

    max_windows = num_columns * num_rows
    chrome_windows = chrome_windows[:max_windows]

    for index, win in enumerate(chrome_windows):
        row = index // num_columns
        col = index % num_columns
        x = col * (window_width + margin)
        y = row * (window_height + margin)
        try:
            win.moveTo(x, y)
            win.resizeTo(window_width, window_height)
            logger.info(f"Arranged window {win.title} at ({x},{y}) size ({window_width},{window_height})")
        except Exception as e:
            logger.exception(f"Failed to move/resize window {getattr(win,'title',str(win))}: {e}")


__all__ = [
    'find_chrome_window_by_profile',
    'find_chrome_window',
    'arrange_chrome_windows',
]

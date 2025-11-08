import os

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
PROJECT_ROOT = os.path.dirname(BASE_DIR)

# Files located at project root
PROFILE_FILE = os.path.join(PROJECT_ROOT, 'profiles.json')
CONFIG_FILE = os.path.join(PROJECT_ROOT, 'config.json')
URL_FILE = os.path.join(PROJECT_ROOT, 'URL.json')

# Default chrome path if not found in config
default_chrome_path = 'C:/Program Files/Google/Chrome/Application/chrome.exe'

DEFAULT_CONFIG = {
    "always_on_top": False,
    "chrome_paths": [default_chrome_path],
    "chrome_path": default_chrome_path
}

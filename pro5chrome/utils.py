import json
import os
from typing import Any, Optional


def read_json(file_path: str, default_value: Optional[Any] = None) -> Any:
    try:
        if os.path.exists(file_path):
            with open(file_path, 'r', encoding='utf-8') as f:
                return json.load(f)
        else:
            return default_value
    except json.JSONDecodeError:
        # caller may handle
        return default_value
    except Exception:
        return default_value


def write_json(file_path: str, data: Any) -> None:
    try:
        with open(file_path, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=4, ensure_ascii=False)
    except Exception:
        pass


def normalize_paths(config: dict) -> dict:
    if 'chrome_paths' in config:
        config['chrome_paths'] = [path.replace('\\', '/') for path in config['chrome_paths']]
    return config

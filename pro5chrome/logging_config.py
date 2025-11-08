import sys
import io
import logging
from logging import Logger

# Configure UTF-8 console stream and a file handler
logger: Logger = logging.getLogger('Pro5Chrome')
logger.setLevel(logging.DEBUG)

if not logger.handlers:
    # UTF-8 console handler (replace unencodable chars)
    stream = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
    stream_handler = logging.StreamHandler(stream)
    stream_handler.setFormatter(logging.Formatter('%(asctime)s - %(levelname)s - %(message)s'))
    logger.addHandler(stream_handler)

    # File handler to persist logs in UTF-8
    file_handler = logging.FileHandler('pro5chrome.log', encoding='utf-8')
    file_handler.setFormatter(logging.Formatter('%(asctime)s - %(levelname)s - %(message)s'))
    logger.addHandler(file_handler)

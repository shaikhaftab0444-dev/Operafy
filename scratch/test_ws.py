import subprocess
import time
import urllib.request
import json
import websocket # let's check if websocket is available or urllib

print("Testing websocket import...")
try:
    import websockets
    print("websockets available")
except ImportError:
    print("websockets not installed")

try:
    import websocket
    print("websocket-client available")
except ImportError:
    print("websocket-client not installed")

import http.server
import socketserver
import threading
import subprocess
import os

PORT = 8998
DIRECTORY = os.path.abspath('scratch')

class Handler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=DIRECTORY, **kwargs)
    def log_message(self, format, *args):
        pass

httpd = socketserver.TCPServer(("", PORT), Handler)
server_thread = threading.Thread(target=httpd.serve_forever)
server_thread.daemon = True
server_thread.start()

edge_path = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"

# Take screenshot at 390px, 430px, 768px
for w, h, name in [(390, 844, "mobile_390"), (430, 932, "mobile_430"), (768, 1024, "tablet_768"), (1366, 768, "laptop_1366")]:
    out_file = os.path.abspath(f"scratch/{name}.png")
    cmd = [
        edge_path,
        "--headless=new",
        "--disable-gpu",
        f"--window-size={w},{h}",
        f"--screenshot={out_file}",
        f"http://localhost:{PORT}/test_page.html"
    ]
    subprocess.run(cmd)
    print(f"Captured {name}.png at {w}x{h}")

httpd.shutdown()

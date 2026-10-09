import os
import subprocess
import time
import json
import urllib.request
import websocket

edge_path = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
port = 9223
user_data_dir = os.path.abspath("scratch/edge_debug_profile_2")

cmd = [
    edge_path,
    "--headless=new",
    "--disable-gpu",
    "--remote-allow-origins=*",
    f"--remote-debugging-port={port}",
    f"--user-data-dir={user_data_dir}",
    "about:blank"
]

proc = subprocess.Popen(cmd)
time.sleep(2)

try:
    resp = urllib.request.urlopen(f"http://127.0.0.1:{port}/json").read().decode()
    tabs = json.loads(resp)
    ws_url = tabs[0]["webSocketDebuggerUrl"]
    
    ws = websocket.create_connection(ws_url)
    msg_id = 0

    def send(method, params=None):
        global msg_id
        msg_id += 1
        payload = {"id": msg_id, "method": method}
        if params:
            payload["params"] = params
        ws.send(json.dumps(payload))
        while True:
            r = json.loads(ws.recv())
            if r.get("id") == msg_id:
                return r.get("result", {})

    send("Page.enable")
    send("Runtime.enable")
    send("Network.enable")
    
    file_url = f"file:///{os.path.abspath('scratch/test_page.html').replace(os.sep, '/')}"

    for target_w in [320, 360, 375, 390, 430, 768, 1024, 1366, 1920]:
        is_mob = target_w < 1000
        send("Emulation.setDeviceMetricsOverride", {
            "width": target_w,
            "height": 844,
            "deviceScaleFactor": 2 if is_mob else 1,
            "mobile": is_mob
        })
        if is_mob:
            send("Emulation.setTouchEmulationEnabled", {"enabled": True, "maxTouchPoints": 5})
        
        send("Page.navigate", {"url": file_url})
        time.sleep(0.5)
        
        res = send("Runtime.evaluate", {
            "expression": "JSON.stringify({ winW: window.innerWidth, docScroll: document.documentElement.scrollWidth, bodyScroll: document.body.scrollWidth })",
            "returnByValue": True
        })
        data = json.loads(res.get("result", {}).get("value", "{}"))
        print(f"Target: {target_w}px -> InnerWidth: {data.get('winW')}px | DocScroll: {data.get('docScroll')}px | BodyScroll: {data.get('bodyScroll')}px")
    
    ws.close()
finally:
    proc.terminate()

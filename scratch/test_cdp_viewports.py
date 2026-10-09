import os
import subprocess
import time
import json
import urllib.request
import websocket

edge_path = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
port = 9222
user_data_dir = os.path.abspath("scratch/edge_debug_profile")

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

    viewports = [
        {"name": "320px (Compact phone)", "w": 320, "h": 640, "mobile": True},
        {"name": "360px (Small Android)", "w": 360, "h": 740, "mobile": True},
        {"name": "375px (Standard iPhone)", "w": 375, "h": 667, "mobile": True},
        {"name": "390px (Modern iPhone)", "w": 390, "h": 844, "mobile": True},
        {"name": "430px (iPhone Pro Max)", "w": 430, "h": 932, "mobile": True},
        {"name": "768px (Tablet portrait)", "w": 768, "h": 1024, "mobile": True},
        {"name": "1024px (Tablet landscape / Laptop)", "w": 1024, "h": 768, "mobile": False},
        {"name": "1366px (Standard Laptop)", "w": 1366, "h": 768, "mobile": False},
        {"name": "1920px (Desktop Full HD)", "w": 1920, "h": 1080, "mobile": False},
    ]

    print("\n================ VIEWPORT RESPONSIVE VERIFICATION ================")
    all_passed = True
    for vp in viewports:
        ua = "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1" if vp["mobile"] else ""
        if ua:
            send("Network.setUserAgentOverride", {"userAgent": ua})
            
        send("Emulation.setDeviceMetricsOverride", {
            "width": vp["w"],
            "height": vp["h"],
            "deviceScaleFactor": 2 if vp["mobile"] else 1,
            "mobile": vp["mobile"],
            "screenWidth": vp["w"],
            "screenHeight": vp["h"]
        })
        
        send("Page.navigate", {"url": file_url})
        time.sleep(1.0)
        
        js = """
        (() => {
            const winW = window.innerWidth;
            const docScroll = document.documentElement.scrollWidth;
            const docClient = document.documentElement.clientWidth;
            const bodyScroll = document.body.scrollWidth;
            
            const badElements = [];
            document.querySelectorAll('*').forEach(el => {
                const tag = el.tagName.toLowerCase();
                if (['script', 'style', 'defs', 'lineargradient', 'stop', 'path', 'circle', 'g', 'svg'].includes(tag)) return;
                
                if (el.closest('.table-responsive, .role-table-wrap') && el !== el.closest('.table-responsive, .role-table-wrap')) return;
                
                const rect = el.getBoundingClientRect();
                if (rect.right > winW + 1.5) {
                    badElements.push({
                        tag: el.tagName,
                        className: (typeof el.className === 'string') ? el.className.slice(0, 40) : '',
                        right: Math.round(rect.right),
                        width: Math.round(rect.width)
                    });
                }
            });
            
            return {
                winW: winW,
                docScroll: docScroll,
                docClient: docClient,
                bodyScroll: bodyScroll,
                hasOverflow: docScroll > winW,
                badCount: badElements.length,
                bad: badElements.slice(0, 5)
            };
        })()
        """
        
        res = send("Runtime.evaluate", {"expression": js, "returnByValue": True})
        val = res.get("result", {}).get("value", {})
        
        has_overflow = val.get("hasOverflow", False)
        doc_scroll = val.get("docScroll", 0)
        win_w = val.get("winW", 0)
        bad_count = val.get("badCount", 0)
        
        status = "PASSED (Zero Overflow)" if (doc_scroll <= win_w and bad_count == 0) else "FAILED"
        if status != "PASSED (Zero Overflow)":
            all_passed = False
            
        print(f"[{vp['name']}]")
        print(f"   Target: {vp['w']}px | Window: {win_w}px | DocScroll: {doc_scroll}px | Bad elements: {bad_count}")
        print(f"   Status: {status}")
        if bad_count > 0:
            for b in val.get("bad", []):
                print(f"      -> {b['tag']}.{b['className']} right={b['right']} w={b['width']}")
        print()

    print(f"OVERALL RESULT: {'ALL 9 VIEWPORTS PASSED WITH ZERO OVERFLOW!' if all_passed else 'SOME VIEWPORTS FAILED'}")

    ws.close()
finally:
    proc.terminate()

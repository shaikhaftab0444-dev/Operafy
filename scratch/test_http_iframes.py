import http.server
import socketserver
import threading
import subprocess
import json
import re
import os
import time

PORT = 8999
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

# Now create iframe tester requesting http://localhost:8999/test_page.html
iframe_tester = f"""<!DOCTYPE html>
<html>
<body>
<div id="results"></div>
<script>
const viewports = [
    {{ name: "320px (Compact phone)", w: 320, h: 640 }},
    {{ name: "360px (Small Android)", w: 360, h: 740 }},
    {{ name: "375px (Standard iPhone)", w: 375, h: 667 }},
    {{ name: "390px (Modern iPhone)", w: 390, h: 844 }},
    {{ name: "430px (iPhone Pro Max)", w: 430, h: 932 }},
    {{ name: "768px (Tablet portrait)", w: 768, h: 1024 }},
    {{ name: "1024px (Tablet landscape / Laptop)", w: 1024, h: 768 }},
    {{ name: "1366px (Standard Laptop)", w: 1366, h: 768 }},
    {{ name: "1920px (Desktop Full HD)", w: 1920, h: 1080 }}
];

const results = [];

async function run() {{
    for (const vp of viewports) {{
        await new Promise(res => {{
            const f = document.createElement('iframe');
            f.style.width = vp.w + 'px';
            f.style.height = vp.h + 'px';
            f.src = 'http://localhost:{PORT}/test_page.html';
            f.onload = () => {{
                setTimeout(() => {{
                    const doc = f.contentDocument;
                    const win = f.contentWindow;
                    const winW = win.innerWidth;
                    const docW = doc.documentElement.scrollWidth;
                    const bodyW = doc.body.scrollWidth;
                    
                    const bad = [];
                    doc.querySelectorAll('*').forEach(el => {{
                        const tag = el.tagName.toLowerCase();
                        if (['script', 'style', 'defs', 'lineargradient', 'stop', 'path', 'circle', 'g', 'svg'].includes(tag)) return;
                        if (el.closest('.table-responsive, .role-table-wrap') && el !== el.closest('.table-responsive, .role-table-wrap')) return;
                        const r = el.getBoundingClientRect();
                        if (r.right > winW + 1.5) {{
                            bad.push({{ tag: el.tagName, cls: (typeof el.className === 'string') ? el.className.slice(0, 30) : '', r: Math.round(r.right), w: Math.round(r.width) }});
                        }}
                    }});

                    results.push({{
                        name: vp.name,
                        target: vp.w,
                        winW: winW,
                        docW: docW,
                        bodyW: bodyW,
                        hasOverflow: docW > winW,
                        badCount: bad.length,
                        bad: bad.slice(0, 5)
                    }});
                    document.body.removeChild(f);
                    res();
                }}, 300);
            }};
            document.body.appendChild(f);
        }});
    }}
    const p = document.createElement('pre');
    p.id = 'FINAL_RESULTS';
    p.textContent = JSON.stringify(results, null, 2);
    document.body.appendChild(p);
}}
run();
</script>
</body>
</html>
"""

with open('scratch/run_http_iframes.html', 'w', encoding='utf-8') as f:
    f.write(iframe_tester)

cmd = [
    r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
    "--headless",
    "--disable-gpu",
    "--window-size=2000,1200",
    "--virtual-time-budget=12000",
    "--dump-dom",
    f"http://localhost:{PORT}/run_http_iframes.html"
]

res = subprocess.run(cmd, capture_output=True, text=True, encoding='utf-8')
m = re.search(r'<pre id="FINAL_RESULTS">(.*?)</pre>', res.stdout, re.DOTALL)
if m:
    data = json.loads(m.group(1))
    print("================ EXACT LOCAL SERVER IFRAME VERIFICATION ================")
    all_pass = True
    for r in data:
        passed = (r['docW'] <= r['winW'] and r['badCount'] == 0)
        if not passed:
            all_pass = False
        print(f"[{r['name']}]")
        print(f"   Target: {r['target']}px | Window: {r['winW']}px | DocScroll: {r['docW']}px | BodyScroll: {r['bodyW']}px")
        print(f"   Overflow: {'YES' if r['hasOverflow'] else 'NO'} | Bad Elements: {r['badCount']}")
        print(f"   Status: {'PASSED (Zero Overflow)' if passed else 'FAILED'}")
        if r['badCount'] > 0:
            for b in r['bad']:
                print(f"      -> {b['tag']}.{b['cls']} right={b['r']} w={b['w']}")
        print()
    print(f"ALL VIEWPORTS RESULT: {'ALL 9 VIEWPORTS PASSED!' if all_pass else 'SOME FAILED'}")
else:
    print("No FINAL_RESULTS found")
    print("Output tail:", res.stdout[-500:])

httpd.shutdown()

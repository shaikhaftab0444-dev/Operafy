import os
import subprocess
import json
import re

def test_size(w, h):
    diag = f"""
    <script>
    window.addEventListener('load', () => {{
        setTimeout(() => {{
            const res = document.createElement('div');
            res.id = 'RESULT';
            res.textContent = JSON.stringify({{
                w: window.innerWidth,
                h: window.innerHeight,
                docW: document.documentElement.scrollWidth,
                bodyW: document.body.scrollWidth
            }});
            document.body.appendChild(res);
        }}, 300);
    }});
    </script>
    """
    with open('scratch/test_page.html', 'r', encoding='utf-8') as f:
        html = f.read()
    test_html = html.replace('</body>', diag + '</body>')
    with open('scratch/test_tmp.html', 'w', encoding='utf-8') as f:
        f.write(test_html)
        
    cmd = [
        r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
        "--headless",
        "--disable-gpu",
        f"--window-size={w},{h}",
        '--user-agent=Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1',
        "--virtual-time-budget=1000",
        "--dump-dom",
        f"file:///{os.path.abspath('scratch/test_tmp.html').replace(os.sep, '/')}"
    ]
    res = subprocess.run(cmd, capture_output=True, text=True, encoding='utf-8')
    m = re.search(r'<div id="RESULT">(.*?)</div>', res.stdout)
    if m:
        data = json.loads(m.group(1))
        print(f"Target: {w}x{h} -> Window: {data['w']}x{data['h']}, DocScroll: {data['docW']}, BodyScroll: {data['bodyW']}")
    else:
        print(f"Target {w}: No result")

for w in [320, 360, 375, 390, 430, 768, 1024, 1366, 1920]:
    test_size(w, 800)

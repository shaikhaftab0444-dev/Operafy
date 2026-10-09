import os
import subprocess
import json
import re

def test_viewport(width, height):
    # Read test_page.html
    html_path = os.path.abspath('scratch/test_page.html')
    with open(html_path, 'r', encoding='utf-8') as f:
        html = f.read()

    # Inject diagnostics script at the end of body
    diag_script = f"""
    <script>
    window.addEventListener('load', () => {{
        setTimeout(() => {{
            const winW = window.innerWidth;
            const docW = document.documentElement.scrollWidth;
            const bodyW = document.body.scrollWidth;
            const overflowing = [];
            
            document.querySelectorAll('*').forEach(el => {{
                const r = el.getBoundingClientRect();
                if (r.right > winW + 1 || r.width > winW + 1) {{
                    overflowing.push({{
                        tag: el.tagName,
                        id: el.id,
                        className: (el.className && typeof el.className === 'string') ? el.className.trim() : '',
                        width: Math.round(r.width),
                        right: Math.round(r.right),
                        left: Math.round(r.left),
                        html: el.outerHTML.slice(0, 80)
                    }});
                }}
            }});
            
            const res = document.createElement('pre');
            res.id = 'DIAG_RESULT';
            res.textContent = JSON.stringify({{
                viewportWidth: winW,
                viewportHeight: window.innerHeight,
                docScrollWidth: docW,
                bodyScrollWidth: bodyW,
                overflowingCount: overflowing.length,
                overflowing: overflowing.slice(0, 20)
            }}, null, 2);
            document.body.appendChild(res);
        }}, 500);
    }});
    </script>
    """
    
    test_run_html = html.replace('</body>', diag_script + '\n</body>')
    run_path = os.path.abspath(f'scratch/run_{width}.html')
    with open(run_path, 'w', encoding='utf-8') as f:
        f.write(test_run_html)

    # Run Edge headless with dump-dom
    cmd = [
        r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
        "--headless",
        "--disable-gpu",
        f"--window-size={width},{height}",
        "--virtual-time-budget=2000",
        "--dump-dom",
        f"file:///{run_path.replace(os.sep, '/')}"
    ]
    
    result = subprocess.run(cmd, capture_output=True, text=True, encoding='utf-8', errors='ignore')
    match = re.search(r'<pre id="DIAG_RESULT">(.*?)</pre>', result.stdout, re.DOTALL)
    if match:
        data = json.loads(match.group(1))
        print(f"=== VIEWPORT {width}x{height} ===")
        print(f"Viewport: {data['viewportWidth']}, Doc Scroll: {data['docScrollWidth']}, Body Scroll: {data['bodyScrollWidth']}")
        print(f"Overflowing elements count: {data['overflowingCount']}")
        for item in data['overflowing']:
            print(f"  [{item['tag']}] class='{item['className']}' id='{item['id']}' | width={item['width']}, right={item['right']}")
    else:
        print(f"=== VIEWPORT {width}x{height} === No DIAG_RESULT found in dump-dom")
        if "DIAG_RESULT" in result.stdout:
            print("Found tag in stdout but regex failed")

for w in [320, 360, 375, 390, 430, 768, 1024, 1366, 1920]:
    test_viewport(w, 844)

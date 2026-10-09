import os
import subprocess
import json
import re

with open('scratch/test_page.html', 'r', encoding='utf-8') as f:
    html = f.read()

# 1. Remove bg-glow divs
html = html.replace('<div class="bg-glow"></div>', '')
html = html.replace('<div class="bg-glow-2"></div>', '')

# 2. Fix body background in landing.css / style
html = re.sub(r'background-color:\s*var\(--dark-bg\);', 'background-color: #f8fafc;', html)

# 3. Fix role-table-wrap overflow: hidden -> overflow-x: auto
html = html.replace('.ops-landing .role-table-wrap {\n        overflow: hidden;', '.ops-landing .role-table-wrap {\n        overflow-x: auto;')
html = html.replace('overflow: hidden;\n        background: #ffffff;\n        border: 1px solid var(--border-color);\n        border-radius: 20px;', 'overflow-x: auto;\n        background: #ffffff;\n        border: 1px solid var(--border-color);\n        border-radius: 20px;')

# 4. Fix row g-5
html = html.replace('row align-items-start g-5', 'row align-items-start gx-3 gx-lg-5 gy-4 gy-lg-5')
html = html.replace('row align-items-center g-5', 'row align-items-center gx-3 gx-lg-5 gy-4 gy-lg-5')

# 5. Fix hero-orb-two right: -150px
html = html.replace('right: -150px;', 'right: -40px; max-width: 50vw;')
html = html.replace('width: 400px;', 'width: min(400px, 80vw);')

# 6. Fix architecture card cols
html = re.sub(r'<div class="col-md-4">\s*<div class="architecture-card">', '<div class="col-12 col-md-4">\n                    <div class="architecture-card">', html)

# 7. Fix hero title inline style
html = html.replace('style="font-size: 4rem; font-weight: 800;"', 'style="font-weight: 800;"')

# Save updated test html
with open('scratch/test_page_fixed.html', 'w', encoding='utf-8') as f:
    f.write(html)

def test_fixed_viewport(width, height):
    with open('scratch/test_page_fixed.html', 'r', encoding='utf-8') as f:
        page = f.read()

    diag_script = f"""
    <script>
    window.addEventListener('load', () => {{
        setTimeout(() => {{
            const winW = window.innerWidth;
            const docW = document.documentElement.scrollWidth;
            const bodyW = document.body.scrollWidth;
            const overflowing = [];
            
            document.querySelectorAll('*').forEach(el => {{
                // Skip SVG internal elements
                if (['defs', 'linearGradient', 'stop', 'path', 'circle', 'g'].includes(el.tagName.toLowerCase())) return;
                const r = el.getBoundingClientRect();
                if (r.right > winW + 1) {{
                    overflowing.push({{
                        tag: el.tagName,
                        className: (el.className && typeof el.className === 'string') ? el.className.trim() : '',
                        width: Math.round(r.width),
                        right: Math.round(r.right)
                    }});
                }}
            }});
            
            const res = document.createElement('pre');
            res.id = 'DIAG_RESULT';
            res.textContent = JSON.stringify({{
                viewportWidth: winW,
                docScrollWidth: docW,
                bodyScrollWidth: bodyW,
                overflowingCount: overflowing.length,
                overflowing: overflowing.slice(0, 10)
            }}, null, 2);
            document.body.appendChild(res);
        }}, 500);
    }});
    </script>
    """
    
    test_run_html = page.replace('</body>', diag_script + '\n</body>')
    run_path = os.path.abspath(f'scratch/run_fixed_{width}.html')
    with open(run_path, 'w', encoding='utf-8') as f:
        f.write(test_run_html)

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
        print(f"[{width}px] Viewport: {data['viewportWidth']}, DocScroll: {data['docScrollWidth']}, BodyScroll: {data['bodyScrollWidth']}, OverflowCount: {data['overflowingCount']}")
        if data['overflowingCount'] > 0:
            for item in data['overflowing']:
                print(f"   -> [{item['tag']}] class='{item['className']}' w={item['width']} r={item['right']}")
    else:
        print(f"[{width}px] No DIAG_RESULT")

print("Testing fixed viewports:")
for w in [320, 360, 375, 390, 430, 768, 1024, 1366, 1920]:
    test_fixed_viewport(w, 844)

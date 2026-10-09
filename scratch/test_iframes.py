import os
import subprocess
import json
import re

# Create iframe tester html
iframe_tester = """
<!DOCTYPE html>
<html>
<head>
    <title>Viewport Test</title>
</head>
<body>
    <div id="results"></div>
    <script>
    const viewports = [
        { name: "320px (Compact phone)", w: 320, h: 640 },
        { name: "360px (Small Android)", w: 360, h: 740 },
        { name: "375px (Standard iPhone)", w: 375, h: 667 },
        { name: "390px (Modern iPhone)", w: 390, h: 844 },
        { name: "430px (iPhone Pro Max)", w: 430, h: 932 },
        { name: "768px (Tablet portrait)", w: 768, h: 1024 },
        { name: "1024px (Tablet landscape / Laptop)", w: 1024, h: 768 },
        { name: "1366px (Standard Laptop)", w: 1366, h: 768 },
        { name: "1920px (Desktop Full HD)", w: 1920, h: 1080 }
    ];

    const results = [];

    async function runTests() {
        for (const vp of viewports) {
            await new Promise(resolve => {
                const iframe = document.createElement('iframe');
                iframe.style.width = vp.w + 'px';
                iframe.style.height = vp.h + 'px';
                iframe.style.border = 'none';
                iframe.src = 'test_page.html';
                
                iframe.onload = () => {
                    setTimeout(() => {
                        const iDoc = iframe.contentDocument;
                        const iWin = iframe.contentWindow;
                        const winW = iWin.innerWidth;
                        const scrollW = iDoc.documentElement.scrollWidth;
                        const bodyScrollW = iDoc.body.scrollWidth;

                        // Check for overflowing elements
                        const badElements = [];
                        iDoc.querySelectorAll('*').forEach(el => {
                            const tag = el.tagName.toLowerCase();
                            if (['script', 'style', 'defs', 'lineargradient', 'stop', 'path', 'circle', 'g', 'svg'].includes(tag)) return;
                            if (el.closest('.table-responsive, .role-table-wrap') && el !== el.closest('.table-responsive, .role-table-wrap')) return;

                            const r = el.getBoundingClientRect();
                            if (r.right > winW + 1.5) {
                                badElements.push({
                                    tag: el.tagName,
                                    cls: (typeof el.className === 'string') ? el.className.slice(0, 30) : '',
                                    right: Math.round(r.right),
                                    w: Math.round(r.width)
                                });
                            }
                        });

                        results.push({
                            name: vp.name,
                            targetWidth: vp.w,
                            actualInnerWidth: winW,
                            scrollWidth: scrollW,
                            bodyScrollWidth: bodyScrollW,
                            hasOverflow: scrollW > winW,
                            badElementsCount: badElements.length,
                            badElements: badElements.slice(0, 5)
                        });

                        document.body.removeChild(iframe);
                        resolve();
                    }, 400);
                };
                document.body.appendChild(iframe);
            });
        }

        const out = document.createElement('pre');
        out.id = 'FINAL_RESULTS';
        out.textContent = JSON.stringify(results, null, 2);
        document.body.appendChild(out);
    }

    runTests();
    </script>
</body>
</html>
"""

with open('scratch/run_iframe_tests.html', 'w', encoding='utf-8') as f:
    f.write(iframe_tester)

cmd = [
    r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
    "--headless",
    "--disable-gpu",
    "--allow-file-access-from-files",
    "--window-size=2000,1200",
    "--virtual-time-budget=10000",
    "--dump-dom",
    f"file:///{os.path.abspath('scratch/run_iframe_tests.html').replace(os.sep, '/')}"
]

res = subprocess.run(cmd, capture_output=True, text=True, encoding='utf-8')
m = re.search(r'<pre id="FINAL_RESULTS">(.*?)</pre>', res.stdout, re.DOTALL)
if m:
    data = json.loads(m.group(1))
    print("================ EXACT IFRAME VIEWPORT VERIFICATION ================")
    all_pass = True
    for r in data:
        passed = (r['scrollWidth'] <= r['actualInnerWidth'] and r['badElementsCount'] == 0)
        if not passed:
            all_pass = False
        print(f"[{r['name']}]")
        print(f"   Target: {r['targetWidth']}px | Window: {r['actualInnerWidth']}px | DocScroll: {r['scrollWidth']}px | BodyScroll: {r['bodyScrollWidth']}px")
        print(f"   Overflow: {'YES' if r['hasOverflow'] else 'NO'} | Bad Elements: {r['badElementsCount']}")
        print(f"   Status: {'PASSED (Zero Overflow)' if passed else 'FAILED'}")
        if r['badElementsCount'] > 0:
            for b in r['badElements']:
                print(f"      -> {b['tag']}.{b['cls']} right={b['right']} w={b['w']}")
        print()
    print(f"ALL VIEWPORTS RESULT: {'ALL PASSED!' if all_pass else 'SOME FAILED'}")
else:
    print("No FINAL_RESULTS found")

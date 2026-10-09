import subprocess
import os

edge_path = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
html_path = os.path.abspath("scratch/test_page.html")

for w, h, name in [(390, 844, "real_mobile_390"), (430, 932, "real_mobile_430"), (1366, 768, "real_laptop_1366")]:
    out_file = os.path.abspath(f"scratch/{name}.png")
    cmd = [
        edge_path,
        "--headless=new",
        "--disable-gpu",
        "--allow-file-access-from-files",
        f"--window-size={w},{h}",
        f"--screenshot={out_file}",
        f"file:///{html_path.replace(os.sep, '/')}"
    ]
    subprocess.run(cmd)
    print(f"Captured {name}.png")

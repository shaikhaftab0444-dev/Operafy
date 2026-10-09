import re

with open('Views/Home/Index.cshtml', 'r', encoding='utf-8') as f:
    lines = f.readlines()

print("--- AUDITING ALL COLUMNS IN INDEX.CSHTML ---")
for i, line in enumerate(lines):
    # find all class attributes with col-
    m = re.findall(r'class="([^"]*col-[^"]*)"', line)
    if m:
        for cls in m:
            print(f"Line {i+1}: {cls}")

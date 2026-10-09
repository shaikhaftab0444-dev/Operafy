import re

with open('Views/Home/Index.cshtml', 'r', encoding='utf-8') as f:
    lines = f.readlines()

print(f"Total lines: {len(lines)}")

# Let's inspect sections:
current_section = None
for i, line in enumerate(lines):
    if '<section' in line or '<footer' in line or '<nav' in line:
        print(f"Line {i+1}: {line.strip()[:100]}")

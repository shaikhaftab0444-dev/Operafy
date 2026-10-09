import re

with open('Views/Home/Index.cshtml', 'r', encoding='utf-8') as f:
    content = f.read()

# Let's check div balance
# Remove script and style tags first
cleaned = re.sub(r'<style>.*?</style>', '', content, flags=re.DOTALL)
cleaned = re.sub(r'<script>.*?</script>', '', cleaned, flags=re.DOTALL)

open_divs = len(re.findall(r'<div\b', cleaned))
close_divs = len(re.findall(r'</div>', cleaned))
print(f"Open divs: {open_divs}, Close divs: {close_divs}, Diff: {open_divs - close_divs}")

open_sections = len(re.findall(r'<section\b', cleaned))
close_sections = len(re.findall(r'</section>', cleaned))
print(f"Open sections: {open_sections}, Close sections: {close_sections}, Diff: {open_sections - close_sections}")

# Now let's trace unclosed tags if diff != 0
tags = re.findall(r'<(/)?([a-zA-Z0-9]+)(?:\s+[^>]*?)?(?<!/)>', cleaned)
stack = []
void_tags = {'area', 'base', 'br', 'col', 'embed', 'hr', 'img', 'input', 'link', 'meta', 'param', 'source', 'track', 'wbr', 'path', 'circle', 'line', 'polyline', 'rect', 'use'}

for is_close, tag in tags:
    tag = tag.lower()
    if tag in void_tags:
        continue
    if not is_close:
        stack.append(tag)
    else:
        if stack and stack[-1] == tag:
            stack.pop()
        else:
            print(f"Mismatch: found </{tag}>, but top of stack was {stack[-1] if stack else 'EMPTY'}")

print(f"Remaining unclosed tags on stack: {len(stack)}")
if stack:
    print(stack[-20:])

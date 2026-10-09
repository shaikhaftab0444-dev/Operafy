import os
import re

with open('Views/Shared/_LandingLayout.cshtml', 'r', encoding='utf-8') as f:
    layout = f.read()

with open('Views/Home/Index.cshtml', 'r', encoding='utf-8') as f:
    index = f.read()

# Strip Razor layout declaration from Index.cshtml
index_body = re.sub(r'@\{[^}]*Layout\s*=\s*"_LandingLayout"[^}]*\}', '', index, count=1)

# Index has <style>...</style> at top and then HTML.
# Merge index_body into layout at @RenderBody()
full_html = layout.replace('@RenderBody()', index_body)

# Replace ~/ with / or local path
# Replace ~/lib/bootstrap/dist/css/bootstrap.min.css with c:/Users/hamid/OneDrive/Desktop/ERP/Operafy/wwwroot/lib/bootstrap/dist/css/bootstrap.min.css
base_dir = os.path.abspath('wwwroot').replace('\\', '/')
full_html = full_html.replace('~/lib/', f'file:///{base_dir}/lib/')
full_html = full_html.replace('~/css/', f'file:///{base_dir}/css/')
full_html = full_html.replace('~/js/', f'file:///{base_dir}/js/')
full_html = full_html.replace('~/videos/', f'file:///{base_dir}/videos/')
full_html = full_html.replace('~/images/', f'file:///{base_dir}/images/')
full_html = full_html.replace('~/', f'file:///{base_dir}/')

# Also remove asp-append-version
full_html = re.sub(r'asp-append-version="[^"]*"', '', full_html)

output_path = os.path.abspath('scratch/test_page.html')
with open(output_path, 'w', encoding='utf-8') as f:
    f.write(full_html)

print(f"Generated test HTML: {output_path}")

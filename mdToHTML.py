import os
import re
import base64
import markdown
from pathlib import Path
from mimetypes import guess_type

# Path to markdown file
# md_path = Path(r"C:\Users\DTPC\Documents\My Docs\Blog\Posts\Posted\8 bit computer\Github Repo\README.md")
md_path = Path(r"C:\Users\DTPC\Documents\My Docs\Blog\Posts\Posted\8 bit computer\Github Repo\MyLangCompiler\MyLangCompiler\README.md")

# IMPORTANT: base folder for resolving images
base_dir = md_path.parent

# Read markdown
md_content = md_path.read_text(encoding="utf-8")


# ---------------------------
# IMAGE ENCODING FUNCTION
# ---------------------------
def encode_image_to_base64(img_path):
    img_path = img_path.split("?")[0].strip()

    # Resolve relative to markdown file
    full_path = (base_dir / img_path).resolve()

    if not full_path.exists():
        print(f"[MISSING IMAGE] {full_path}")
        return img_path

    mime_type, _ = guess_type(full_path)
    if not mime_type:
        mime_type = "application/octet-stream"

    with open(full_path, "rb") as f:
        encoded = base64.b64encode(f.read()).decode("utf-8")

    return f"data:{mime_type};base64,{encoded}"


# ---------------------------
# 1. HANDLE MARKDOWN IMAGES
# ---------------------------
img_pattern = r'!\[(.*?)\]\((.*?)\)'

def replace_md_images(match):
    alt_text = match.group(1)
    img_src = match.group(2)
    return f'![{alt_text}]({encode_image_to_base64(img_src)})'

md_content = re.sub(img_pattern, replace_md_images, md_content)


# ---------------------------
# 2. HANDLE HTML <img> TAGS (IMPORTANT FIX)
# ---------------------------
html_img_pattern = r'<img\s+[^>]*src="([^"]+)"[^>]*>'

def replace_html_images(match):
    src = match.group(1)
    encoded = encode_image_to_base64(src)
    return match.group(0).replace(src, encoded)

md_content = re.sub(html_img_pattern, replace_html_images, md_content)


# ---------------------------
# CONVERT MARKDOWN → HTML
# ---------------------------
html = markdown.markdown(md_content, extensions=["fenced_code", "tables"])


# ---------------------------
# WRAP IN HTML TEMPLATE
# ---------------------------
full_html = f"""
<!DOCTYPE html>
<html>
<head>
<meta charset="UTF-8">
<title>MyLangCompiler README</title>
<style>
body {{
    font-family: Arial, sans-serif;
    max-width: 900px;
    margin: auto;
    padding: 20px;
    line-height: 1.6;
}}
img {{
    max-width: 100%;
    height: auto;
}}
pre {{
    background: #f4f4f4;
    padding: 10px;
    overflow-x: auto;
}}
code {{
    background: #f4f4f4;
    padding: 2px 4px;
}}
</style>
</head>
<body>
{html}
</body>
</html>
"""


# ---------------------------
# SAVE OUTPUT
# ---------------------------
output_path = Path("MyLangCompiler_README_embedded.html")
output_path.write_text(full_html, encoding="utf-8")

print(f"Created: {output_path}")
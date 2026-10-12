# -*- coding: utf-8 -*-
"""把 translations/ 下的所有翻译分片应用到源码。

特性：
  - 字符级精确定位（复用 csharp_lexer 状态机），不碰注释与字符字面量
  - 二进制读写，保持原有 BOM 与行尾，避免无谓的全文 diff
  - 幂等：已是中文的值不在字典里，重复运行不会二次改动
"""
import os
import re
import sys
import json

sys.stdout.reconfigure(encoding='utf-8')
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from csharp_lexer import iter_string_literals, escape_like

HERE = os.path.dirname(os.path.abspath(__file__))   # <repo>/localization
REPO_ROOT = os.path.dirname(HERE)                    # <repo>
SRC_ROOT = os.path.join(REPO_ROOT, "RenoDXCommander")
TRANS_DIR = os.path.join(HERE, "translations")

ATTRS = ('Text', 'Content', 'Header', 'PlaceholderText', 'ToolTip', 'Title',
         'Description', 'Subtitle', 'Watermark', 'AutomationProperties.Name')
XAML_ATTR = re.compile(r'\b(?:' + '|'.join(a.replace('.', r'\.') for a in ATTRS) +
                       r')\s*=\s*"[^"]*"')
XAML_VALUE = re.compile(r'=\s*"([^"]*)"')


def load_table():
    table = {}
    if not os.path.isdir(TRANS_DIR):
        return table
    for fn in sorted(os.listdir(TRANS_DIR)):
        if fn.endswith('.json'):
            with open(os.path.join(TRANS_DIR, fn), encoding='utf-8') as f:
                table.update(json.load(f))
    return table


def unescape_xml(s):
    return (s.replace('&lt;', '<').replace('&gt;', '>').replace('&quot;', '"')
             .replace('&apos;', "'").replace('&amp;', '&'))


def escape_xml(s):
    return (s.replace('&', '&amp;').replace('<', '&lt;')
             .replace('>', '&gt;').replace('"', '&quot;'))


def apply_cs(text, table):
    out, last, changed = [], 0, 0
    for lit in iter_string_literals(text):
        new = table.get(lit.value)
        if new is None or new == lit.value or lit.kind == 'raw':
            continue
        m = re.match(r'^[\$@]*"', lit.source)
        if not m:
            continue
        out.append(text[last:lit.start])
        out.append(m.group(0) + escape_like(new, lit.kind) + '"')
        last = lit.end
        changed += 1
    out.append(text[last:])
    return ''.join(out), changed


def apply_xaml(text, table):
    changed = [0]

    def repl(m):
        raw = XAML_VALUE.search(m.group(0)).group(1)
        dec = unescape_xml(raw)
        new = table.get(dec)
        if new is None or new == dec:
            return m.group(0)
        changed[0] += 1
        return m.group(0).replace('"' + raw + '"', '"' + escape_xml(new) + '"')

    return XAML_ATTR.sub(repl, text), changed[0]


def main():
    table = load_table()
    print(f'已加载翻译条目: {len(table)}')
    total, files = 0, []
    for dirpath, dirnames, filenames in os.walk(SRC_ROOT):
        dirnames[:] = [d for d in dirnames if d not in ('bin', 'obj', '.git')]
        for fn in filenames:
            full = os.path.join(dirpath, fn)
            is_cs, is_xaml = fn.endswith('.cs'), fn.endswith('.xaml')
            if not (is_cs or is_xaml):
                continue
            with open(full, 'rb') as f:
                raw = f.read()
            bom = raw.startswith(b'\xef\xbb\xbf')
            text = raw.decode('utf-8-sig')
            new, ch = (apply_cs if is_cs else apply_xaml)(text, table)
            if ch and new != text:
                with open(full, 'wb') as f:
                    f.write((b'\xef\xbb\xbf' if bom else b'') + new.encode('utf-8'))
                files.append((os.path.relpath(full, SRC_ROOT), ch))
                total += ch
    print(f'共替换 {total} 处，涉及 {len(files)} 个文件')
    for f, c in sorted(files, key=lambda x: -x[1])[:15]:
        print(f'  {c:>5}  {f}')


if __name__ == '__main__':
    main()

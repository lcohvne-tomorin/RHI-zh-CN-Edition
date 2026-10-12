# -*- coding: utf-8 -*-
"""扫描 RHI 源码，提取字符串字面量并分级。

输出：
    strings_raw.json   全部字面量明细（文件 / 行号 / 值 / 类型 / 分类）
    Console            统计摘要
"""
import os
import re
import sys
import json
from collections import Counter, defaultdict

sys.stdout.reconfigure(encoding='utf-8')

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from csharp_lexer import iter_string_literals

HERE = os.path.dirname(os.path.abspath(__file__))   # <repo>/localization
REPO_ROOT = os.path.dirname(HERE)                    # <repo>
SRC_ROOT = os.path.join(REPO_ROOT, "RenoDXCommander")
OUT_DIR = HERE

# 命中即认为这段字面量是给用户看的
UI_HINT = re.compile(
    r'(\bText\s*=|\bContent\s*=|\bHeader\s*=|\bSubtitle\s*=|\bPlaceholderText\s*=|'
    r'\bTitle\s*=|\bToolTip\s*=|\bFlyout\s*=|\bPrimaryButtonText\s*=|\bSecondaryButtonText\s*=|'
    r'\bCloseButtonText\s*=|ContentDialog|MessageBox|AutomationProperties|'
    r'\bDescription\s*=|\bCaption\s*=|StatusText|LabelText|HeaderText|BodyText|'
    r'new TextBlock|new Button|new ComboBoxItem|new MenuFlyoutItem|new ToggleSwitch|'
    r'new CheckBox|new RadioButton|new TextBox|new Expander|new NavigationViewItem|'
    r'BuildSectionHeader|BuildInfoRow|BuildWarning|BuildNote|'
    r'ShowError|ShowInfo|ShowWarning|ShowConfirm|NotificationTitle|NotificationBody)')

# 命中即认为这段字面量是技术性的（路径 / 参数 / 资源名），只保留强信号
TECH_HINT = re.compile(
    r'(Path\.Combine|Path\.Get|File\.(Exists|Read|Write|Copy|Move|Delete)|Directory\.|'
    r'Registry|Process\.Start|new Uri|HttpClient|RegistryKey|'
    r'Environment\.|\.exe|\.dll|\.ini|\.json|\.xml|\.log|Assembly\.|'
    r'CommandLine|Arguments|JsonPropertyName|'
    r'GetEnvironmentVariable|UserAgent|ContentType|'
    r'wscript|cmd\.exe|powershell|reg\.exe|taskkill)')


def is_tech_value(v):
    """仅凭值本身就能判定为技术字符串。返回原因，或 None 表示可能是文案。"""
    if not v.strip():
        return 'empty'
    if not re.search(r'[A-Za-z\u4e00-\u9fff]', v):    # 纯符号 / emoji / 分隔线
        return 'symbol'
    # 含汉字或全角标点（：、（）等）—— 已汉化或本就无需翻译
    if re.search(r'[\u4e00-\u9fff\u3000-\u303f\uff00-\uffef]', v):
        return 'localized'
    # 纯装饰分隔线（去掉装饰符后没有实质文案）—— —— ——
    if re.match(r'^\s*[—–=]{2,}', v) and len(re.sub(r'[—–=·\s]+', '', v)) < 2:
        return 'decoration'
    # 插值表达式被引号截断留下的代码碎片，无法安全翻译
    if (re.search(r'(string\.Join\(|EscapeDataString\(|\.Select\(|\?\s*$)', v)
            or v.endswith(')}]') or v.endswith(')}"') or re.match(r'^[,|]\s', v)
            or re.match(r'^(--|/)', v)):
        return 'code-fragment'
    if '{' in v:                                      # 纯插值/绑定，去掉 {..} 后没有文案
        bare = re.sub(r'\{[^}]*\}', '', v)
        if len(re.sub(r'[^A-Za-z]', '', bare)) < 2:
            return 'no-text'
    if len(v.strip()) <= 2:                           # 单字母/双字符按钮提示
        return 'too-short'
    if re.match(r'^\.[a-z0-9]{1,5}$', v):             # .7z / .gz 这类纯扩展名
        return 'extension'
    if '*' in v:                                      # relimiter_*.log 通配模式
        return 'wildcard'
    if re.match(r'^/[a-z0-9_\-/.]*$', v):             # /releases 路径片段
        return 'path-fragment'
    if re.fullmatch(r'[A-Z0-9+%]{2,6}', v):           # 全大写缩写 DLSS/DXVK/RR/FG
        return 'acronym'
    if re.search(r'[{}]', v) and len(v) <= 12:        # 插值拼接残留的碎片
        return 'fragment'
    if re.fullmatch(r'#[0-9A-Fa-f]{3,8}', v):         # 颜色码 #RRGGBB
        return 'color'
    if re.fullmatch(r'[Xx*#?\-_\.\s]{2,}', v):        # XXXX-XXXX 形式的占位提示
        return 'placeholder'
    if re.fullmatch(r'[a-z]{2,6}_+\.{2,}', v):        # ghp_... 之类的水印占位
        return 'placeholder'
    if re.fullmatch(r'[A-Z][a-z0-9]*(?:[A-Z][a-z0-9]*)+', v):   # 驼峰标识符 Is32Bit
        return 'identifier'
    if re.fullmatch(r'[a-z][a-z0-9]*(?:[A-Z][a-z0-9]*)+', v):   # 小写开头驼峰 gameName
        return 'identifier'
    if not re.search(r'[a-z]', v) and re.search(r'[A-Z]', v):    # 纯大写缩写串 FSR 2.1 / DX12, VK
        return 'acronym-phrase'
    if re.search(r'https?://', v):
        return 'url'
    if re.search(r'^\\\\|^[A-Za-z]:\\', v):          # UNC / 盘符路径
        return 'path'
    if re.search(r'^(HKEY_|SOFTWARE\\|SYSTEM\\)', v):
        return 'registry'
    if re.search(r'%\w+%', v):                        # 环境变量
        return 'envvar'
    if re.match(r'^[\w\-\.]+\s*=\s*', v) and '\n' not in v:
        return 'kv'
    if re.search(r'^\[[^\]]+\]$', v.strip()):         # INI section
        return 'ini'
    if re.match(r'^[\w\-\.]+\.(exe|dll|ini|json|xml|log|txt|md|py|ps1|bat|cmd|zip|7z|pack|rdb|cfg|config)$', v, re.I):
        return 'filename'
    if re.match(r'^[a-z0-9_\-]+$', v) and len(v) <= 24:   # 全小写 token，多为键名/参数
        return 'token'
    if re.match(r'^(Microsoft|Windows|NVIDIA|SOFTWARE|HKEY)', v):
        return 'tech'
    if re.match(r'^[A-Za-z0-9_\-]+(\.[A-Za-z0-9_\-]+)+$', v) and ' ' not in v:  # A.B.C 命名
        return 'dotted'
    return None


def is_script_or_config(v):
    """脚本正文 / 配置文件 / 代码块 / 日志，不是给用户看的界面文案。"""
    if re.match(r'^\[\{?[A-Za-z_]', v):        # [ClassName.Method] 形式的日志前缀
        return 'log'
    if re.match(r'^(//|\.//|/html|@media|\.\w+\s*\{)', v):
        return 'selector'
    if re.match(r'^\s*[{}\[\]]+\s*$', v):
        return 'punct'
    if '\n' not in v:
        if re.search(r'(^|[^A-Za-z])[A-Za-z_]\w*\s*\([^)]*\)\s*;?\s*$', v) and ('{' in v or ';' in v):
            return 'code-line'
        if v.count('\\') >= 2 and ' ' not in v:
            return 'escaped-path'
        return None
    if re.search(r'^\s*(param\s*\(|\$ErrorActionPreference|Add-Type|using System|function |#region|#requires|@[\'"]|namespace |public class|private static)', v, re.M):
        return 'script'
    lines = [l for l in v.split('\n') if l.strip()]
    if not lines:
        return 'empty'
    eq = sum(1 for l in lines if re.match(r'^\s*[\w\.\-\[\]"\']+\s*=', l))
    if eq >= max(2, len(lines) * 0.5):
        return 'config'
    brace = sum(1 for l in lines if re.search(r'[{};]', l))
    if brace >= max(2, len(lines) * 0.5):
        return 'code'
    if len(v) > 600:
        return 'long-blob'
    return None


def classify(value, before, after):
    tech = is_tech_value(value) or is_script_or_config(value)
    if tech:
        return 'skip', tech
    ctx = before[-220:] + ' || ' + after[:120]
    if UI_HINT.search(ctx):
        return 'ui', 'ui-context'
    if TECH_HINT.search(before[-120:]):
        return 'maybe', 'tech-context'
    if ' ' in value.strip() or re.search(r"[a-z][A-Z]", value):
        return 'text', 'sentence-like'
    # 无空格、无驼峰、又不在 UI 语境里的孤立词，基本都是目录名/扩展名/键名，
    # 宁可漏翻（保持英文）也不能误翻（破坏功能）。
    return 'skip', 'bare-token'


def scan_file(path, rel):
    try:
        text = open(path, encoding='utf-8-sig').read()
    except Exception as e:
        print(f'!! 读取失败 {rel}: {e}')
        return []
    items = []
    for lit in iter_string_literals(text):
        line = text.count('\n', 0, lit.start) + 1
        before = text[max(0, lit.start - 260):lit.start]
        after = text[lit.end:lit.end + 140]
        cls, reason = classify(lit.value, before, after)
        items.append({
            'file': rel, 'line': line, 'value': lit.value, 'kind': lit.kind,
            'source': lit.source, 'cls': cls, 'reason': reason,
        })
    return items


XAML_ATTR = re.compile(
    r'\b(Text|Content|Header|PlaceholderText|ToolTip|Title|Description|Subtitle|Watermark|AutomationProperties\.Name)\s*=\s*"([^"]*)"')


def unescape_xml(s):
    """与 apply.py 保持一致：字典一律用解码后的文本作 key。"""
    return (s.replace('&lt;', '<').replace('&gt;', '>').replace('&quot;', '"')
             .replace('&apos;', "'").replace('&amp;', '&'))


def scan_xaml(path, rel):
    text = open(path, encoding='utf-8-sig').read()
    items = []
    for m in XAML_ATTR.finditer(text):
        val = unescape_xml(m.group(2))
        if not val.strip():
            continue
        line = text.count('\n', 0, m.start()) + 1
        tech = is_tech_value(val)
        cls = 'skip' if tech else 'ui'
        items.append({
            'file': rel, 'line': line, 'value': val, 'kind': 'xaml',
            'source': m.group(0), 'cls': cls, 'reason': tech or 'xaml-attr',
        })
    return items


def main():
    all_items = []
    for dirpath, dirnames, filenames in os.walk(SRC_ROOT):
        dirnames[:] = [d for d in dirnames if d not in ('bin', 'obj', '.git')]
        for fn in filenames:
            full = os.path.join(dirpath, fn)
            rel = os.path.relpath(full, SRC_ROOT)
            if fn.endswith('.cs'):
                all_items += scan_file(full, rel)
            elif fn.endswith('.xaml'):
                all_items += scan_xaml(full, rel)

    with open(os.path.join(OUT_DIR, 'strings_raw.json'), 'w', encoding='utf-8') as f:
        json.dump(all_items, f, ensure_ascii=False, indent=1)

    # ---- 唯一值清单：供翻译使用 ----
    order = {'ui': 0, 'maybe': 1, 'text': 2, 'token': 3}
    todo = {}
    for it in all_items:
        if it['cls'] == 'skip':
            continue
        v = it['value']
        rec = todo.setdefault(v, {'cls': it['cls'], 'chars': len(v), 'hits': 0, 'kinds': set()})
        rec['hits'] += 1
        rec['kinds'].add(it['kind'])
        if order[it['cls']] < order[rec['cls']]:
            rec['cls'] = it['cls']
    for rec in todo.values():
        rec['kinds'] = sorted(rec['kinds'])
    todo_sorted = dict(sorted(todo.items(),
                              key=lambda kv: (order[kv[1]['cls']], kv[1]['chars'])))
    with open(os.path.join(OUT_DIR, 'todo.json'), 'w', encoding='utf-8') as f:
        json.dump(todo_sorted, f, ensure_ascii=False, indent=1)

    occ = Counter(i['cls'] for i in all_items)
    uniq = {k: len({i['value'] for i in all_items if i['cls'] == k})
            for k in ('ui', 'text', 'token', 'maybe', 'skip')}

    print('=' * 52)
    print(f'字面量出现总数: {len(all_items)}')
    print(f'{"分类":<8}{"出现次数":>10}{"唯一值":>10}')
    for k in ('ui', 'text', 'token', 'maybe', 'skip'):
        print(f'{k:<8}{occ.get(k, 0):>10}{uniq.get(k, 0):>10}')
    print('--- 待翻译文本长度分布（唯一值）---')
    buckets = [(0, 40), (40, 80), (80, 150), (150, 300), (300, 99999)]
    head = '分类    ' + ''.join(f'{("<" + str(hi)):>9}' for lo, hi in buckets)
    print(head)
    for k in ('ui', 'text', 'token', 'maybe'):
        vals = {i['value'] for i in all_items if i['cls'] == k}
        row = ''.join(f'{sum(1 for v in vals if lo <= len(v) < hi):>9}' for lo, hi in buckets)
        print(f'{k:<8}{row}')
    print('=' * 52)
    for k in ('ui', 'text', 'token', 'maybe'):
        vals = sorted({i['value'] for i in all_items if i['cls'] == k}, key=len, reverse=True)[:3]
        for v in vals:
            print(f'[{k}] {v[:100]!r}')


if __name__ == '__main__':
    main()

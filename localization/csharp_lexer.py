# -*- coding: utf-8 -*-
"""C# 字符串字面量扫描器。

只做一件事：把源码里的字符串字面量连同它们的精确字符区间找出来，
跳过注释、字符字面量、预处理指令。供提取和替换共用。

支持的形态：普通、逐字(@)、插值($)、插值逐字、原始字符串(C# 11 三引号起)。
"""
import re

ESCAPE_MAP = {'n': '\n', 'r': '\r', 't': '\t', '"': '"', '\\': '\\',
              '0': '\0', "'": "'", 'a': '\a', 'b': '\b', 'f': '\f', 'v': '\v'}


class Literal:
    __slots__ = ('start', 'end', 'source', 'value', 'kind')

    def __init__(self, start, end, source, value, kind):
        self.start, self.end = start, end
        self.source, self.value, self.kind = source, value, kind


def iter_string_literals(text):
    """按出现顺序产出 Literal。位置是字符偏移量（不是字节）。"""
    i, n = 0, len(text)
    while i < n:
        c = text[i]

        # ---- 注释 ----
        if c == '/':
            if text.startswith('//', i):
                j = text.find('\n', i)
                i = n if j < 0 else j + 1
                continue
            if text.startswith('/*', i):
                j = text.find('*/', i + 2)
                i = n if j < 0 else j + 2
                continue

        # ---- 字符字面量 ----
        if c == "'":
            j = i + 1
            while j < n:
                if text[j] == '\\':
                    j += 2
                    continue
                if text[j] == "'":
                    j += 1
                    break
                j += 1
            i = j
            continue

        # ---- 字符串（可能带 $ / @ 前缀）----
        j = i
        dollar = at = False
        while j < n and text[j] in '$@':
            if text[j] == '$':
                dollar = True
            else:
                at = True
            j += 1

        if j < n and text[j] == '"':
            # 原始字符串 """..."""（不能带 @，可以带 $）
            if not at:
                k, q = j, 0
                while k < n and text[k] == '"':
                    q += 1
                    k += 1
                if q >= 3:
                    close = '"' * q
                    end = text.find(close, k)
                    if end < 0:
                        i = n
                        continue
                    yield Literal(i, end + q, text[i:end + q], text[k:end], 'raw')
                    i = end + q
                    continue

            if at:  # 逐字：只有 "" 是转义
                k, buf = j + 1, []
                while k < n:
                    if text[k] == '"':
                        if k + 1 < n and text[k + 1] == '"':
                            buf.append('"')
                            k += 2
                            continue
                        break
                    buf.append(text[k])
                    k += 1
                end = k + 1
                kind = 'interp-verbatim' if dollar else 'verbatim'
                yield Literal(i, end, text[i:end], ''.join(buf), kind)
                i = end
                continue

            k, buf = j + 1, []  # 普通 / 插值
            while k < n:
                ch = text[k]
                if ch == '\\' and k + 1 < n:
                    nxt = text[k + 1]
                    if nxt in ESCAPE_MAP:
                        buf.append(ESCAPE_MAP[nxt])
                        k += 2
                        continue
                    if nxt == 'u' and k + 6 <= n:
                        try:
                            buf.append(chr(int(text[k + 2:k + 6], 16)))
                            k += 6
                            continue
                        except ValueError:
                            pass
                    if nxt == 'x':
                        m = re.match(r'[0-9a-fA-F]{1,4}', text[k + 2:k + 6])
                        if m:
                            buf.append(chr(int(m.group(0), 16)))
                            k += 2 + len(m.group(0))
                            continue
                    buf.append(nxt)
                    k += 2
                    continue
                if ch == '"':
                    break
                if ch == '\n':  # 未闭合，保险起见放弃
                    break
                buf.append(ch)
                k += 1
            end = k + 1
            if k >= n or text[k] != '"':
                i = end
                continue
            kind = 'interp' if dollar else 'regular'
            yield Literal(i, end, text[i:end], ''.join(buf), kind)
            i = end
            continue

        i += 1


def escape_like(value, kind):
    """把中文译文转成能塞进同类字面量的源码形式。"""
    if kind in ('verbatim', 'interp-verbatim'):
        return value.replace('"', '""')
    if kind == 'raw':
        return value
    out = []
    for ch in value:
        if ch == '\\':
            out.append('\\\\')
        elif ch == '"':
            out.append('\\"')
        elif ch == '\n':
            out.append('\\n')
        elif ch == '\r':
            out.append('\\r')
        elif ch == '\t':
            out.append('\\t')
        else:
            out.append(ch)
    return ''.join(out)

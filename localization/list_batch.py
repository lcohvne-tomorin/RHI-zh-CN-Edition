# -*- coding: utf-8 -*-
"""列出尚未翻译的条目，按批次输出，供人工翻译。

用法：
    python list_batch.py [本批条数] [跳过条数] [最大字符数]
"""
import os
import re
import sys
import json

sys.stdout.reconfigure(encoding='utf-8')

HERE = os.path.dirname(os.path.abspath(__file__))
OUT_DIR = HERE
TRANS_DIR = os.path.join(OUT_DIR, 'translations')


def load_done():
    done = {}
    if os.path.isdir(TRANS_DIR):
        for fn in sorted(os.listdir(TRANS_DIR)):
            if fn.endswith('.json'):
                with open(os.path.join(TRANS_DIR, fn), encoding='utf-8') as f:
                    done.update(json.load(f))
    return done


def load_skips():
    path = os.path.join(OUT_DIR, 'skiplist.json')
    if not os.path.exists(path):
        return set()
    with open(path, encoding='utf-8') as f:
        data = json.load(f)
    out = set()
    for k, v in data.items():
        if not k.startswith('_') and isinstance(v, list):
            out.update(v)
    return out


def main():
    limit = int(sys.argv[1]) if len(sys.argv) > 1 else 200
    skip = int(sys.argv[2]) if len(sys.argv) > 2 else 0
    maxchars = int(sys.argv[3]) if len(sys.argv) > 3 else 10 ** 9

    include_maybe = '--maybe' in sys.argv
    todo = json.load(open(os.path.join(OUT_DIR, 'todo.json'), encoding='utf-8'))
    done = load_done()
    skips = load_skips()

    # maybe = 技术上下文里的字符串，多为 JSON 键名/目录名/正则，默认不翻（风险高于收益）
    pending = [(k, v) for k, v in todo.items()
               if k not in done and k not in skips
               and (include_maybe or v['cls'] != 'maybe')
               and len(k) <= maxchars and '\n' not in k]
    batch = pending[skip:skip + limit]

    for k, v in batch:
        print(json.dumps(k, ensure_ascii=False))
    print(f'--- 待翻译 {len(pending)} 条 | 本批 {len(batch)} 条 | 已完成 {len(done)} 条 ---',
          file=sys.stderr)


if __name__ == '__main__':
    main()

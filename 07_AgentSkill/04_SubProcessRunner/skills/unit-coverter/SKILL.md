---
name: unit-converter
description: 使用乘法换算系数在常见单位之间进行转换。在需要将英里/公里、磅/千克等单位互相换算时使用。
---

## 使用方法

当用户请求单位换算时：
1. 先查看 `references/conversion-table.md`，找到正确的换算系数
2. 使用 `--value <数值> --factor <系数>` 运行 `scripts/convert.py` 脚本（例如：`--value 26.2 --factor 1.60934`）
3. 输出内容需要清晰地展示换算系数、换算过程和换算结果，并同时标明换算前后的两个单位
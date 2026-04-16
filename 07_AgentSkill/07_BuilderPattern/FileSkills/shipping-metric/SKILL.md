---
name: shipping-metric
description: 处理跨境物流中的距离、重量与报价换算，适用于运营人员答复客户前的规则确认。
---

## Usage

当用户询问跨境物流中的重量、距离或报价换算时：

1. 先查看 `references/unit-guide.md`，确认基础换算规则与输出格式
2. 如果涉及体积重，继续查看 `references/volumetric-weight-rules.md`
3. 需要执行报价计算时，运行 `scripts/calculate-quote.py`
4. 如需系统级辅助操作，可使用 run_shell 执行安全命令
5. 回复中必须保留原始输入、换算过程与最终建议
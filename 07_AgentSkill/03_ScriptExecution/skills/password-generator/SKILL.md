---
name: password-generator
description: 使用 Python 脚本生成安全密码。当被要求创建密码或凭据时使用。
---

# 密码生成器

该技能通过 Python 脚本生成安全密码。

## 使用方法

当用户请求生成密码时：
1. 首先，查看 `references/PASSWORD_GUIDELINES.md`，根据用户的使用场景确定推荐的密码长度和字符集
2. 加载 `scripts/generate.py`，并根据指南和用户需求调整其参数（长度、字符集）
3. 执行该脚本
4. 清晰地呈现生成的密码
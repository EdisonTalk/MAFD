# Python密码生成脚本
# 用法：按需调整“length”，然后运行

import random
import string

length = 16  # 期望长度

pool = string.ascii_lowercase + string.ascii_uppercase + string.digits + string.punctuation
password = "".join(random.SystemRandom().choice(pool) for _ in range(length))
print(f"生成的密码（{length} 个字符）：{password}")
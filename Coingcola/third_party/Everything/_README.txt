Coingcola 搜索模块采用 Everything 作为本地文件 / 文件夹搜索底层引擎。

如果本机未安装 Everything，可将以下文件放到本目录：
1. Everything.exe
2. es.exe

推荐目录：
Coingcola/third_party/Everything/

当前 MVP 的接入策略：
- 优先检测本目录下的 portable Everything
- 若本目录不存在，再检测系统已安装的 Everything
- Coingcola 只负责上层搜索框、最佳匹配、分组、排序和动作执行
- 不自研全盘文件索引替代 Everything

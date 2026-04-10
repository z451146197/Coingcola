# Coingcola / Everything 便携集成说明

## 本脚本已完成的内容
1. 将 `es.exe`、`Everything.exe` / `Everything64.exe` 等运行时文件复制到：
   - `Coingcola\Resources\Everything`
2. 自动为 `csproj` 增加复制到输出目录配置。
3. 自动生成以下基础服务类：
   - `Models\Search\LocalSearchHit.cs`
   - `Services\Search\EverythingRuntimeLocator.cs`
   - `Services\Search\EverythingRuntimeHost.cs`
   - `Services\Search\EverythingQueryService.cs`
4. 尝试在 `MainWindow.xaml.cs` 中补一个启动挂钩，确保程序启动时尽量拉起 Everything。

## 当前落地口径
- 这是“保守补丁”，先把 **便携运行时 + 搜索能力底座** 补上。
- 它不会强行重写你现有的页面结构、ViewModel 或搜索交互。
- 适合你后续继续把搜索框、结果面板、最近使用、应用启动器等接到这套能力上。

## 其他电脑没安装 Everything，能不能用？
可以，但有几个前提：
1. 你的发布包里必须带上 `Resources\Everything`。
2. 程序启动后要能拉起 `Everything.exe / Everything64.exe`。
3. 第一次运行时，Everything 需要完成本地索引初始化。
4. 如果目标机器权限较严、磁盘策略特殊、不是常规 NTFS 环境，索引能力可能受限，此时会自动退回到文件系统兜底检索，速度会慢很多。

## 建议的下一步
1. 将首页统一搜索框接到 `EverythingQueryService.SearchAsync(...)`
2. 将结果分为：
   - 应用
   - 文件
   - 文件夹
   - 最近使用
   - 常用网站
3. 点击结果时，按类型执行：
   - 文件：`Process.Start`
   - 文件夹：资源管理器打开并选中
   - 应用：直接启动
   - 网站：浏览器打开
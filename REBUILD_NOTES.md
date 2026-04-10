# Coingcola 重制版交付说明

## 交付目标
- 基于用户提供的真实源码压缩包重建
- 不主动改业务功能
- 不主动改现有 UI 与交互
- 清理历史补丁、备份、构建产物和仓库污染文件
- 收口为可重新开始的一套干净项目包

## 本次完成
1. 还原多卷 ZIP 并提取完整工程
2. 以实际解决方案 `Coingcola.slnx` + 项目目录 `Coingcola/` 为主干收口
3. 删除 `.git`、`.vs`、`.github`
4. 删除所有 `_backup_home_*`、`_coingcola_backup`、`_repo_snapshot_*`
5. 删除根目录历史修补脚本与上下文快照文件
6. 删除项目内 `bin/`、`obj/`、`*.user`、`*.bak`
7. 删除仓库根目录未接入构建的空壳目录
8. 保留当前实际源码、Resources、Everything 运行时和 Web 首页资源

## 清理后结构
- `Coingcola.slnx`
- `Coingcola/`
- `docs/cleanup_report.md`

## 统计
- 目录数：30
- 文件数：139

## 说明
当前执行环境不包含 `dotnet`，因此无法在此处完成本地编译验证；本包的处理重点是“项目树收口与污染清理”。

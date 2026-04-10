# Coingcola 重制版清理说明

本包基于用户提供的真实源码压缩包重建，目标是：

1. 保留现有功能
2. 保留现有 UI 与交互
3. 清理构建产物、历史补丁、备份目录与仓库污染文件
4. 收口到可重新开始的干净项目包

## 已执行清理

- 删除 `.git`、`.vs`、`.github`
- 删除根目录历史修补脚本 `coingcola_phase1_full_pack*.ps1`
- 删除根目录上下文快照与迁移辅助文件
- 删除 `_backup_home_*`、`_coingcola_backup`、`_repo_snapshot_*`
- 删除项目内 `bin/`、`obj/`
- 删除 `*.user`、`*.bak`
- 删除仓库根目录未接入构建的空壳目录 `App/ Infrastructure/ Models/ Resources/ Services/ Shell/ ThirdParty/ Views/`

## 保留原则

- `Coingcola.slnx`
- `Coingcola/` 项目目录
- Everything 运行时资源与 Web 首页资源
- 当前所有实际源码文件

## 说明

这次重制版的重点是“项目树收口与污染清理”，不是改业务逻辑，因此不会主动改动现有 UI 行为与交互逻辑。

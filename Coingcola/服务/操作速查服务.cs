using Coingcola.模型;
using Coingcola.系统工具;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Coingcola.服务
{
    /// <summary>
    /// 操作速查服务。
    /// 
    /// 当前先聚焦 WPS 快捷键知识库：
    /// 1. 提供本地常见快捷键列表
    /// 2. 提供本地检索
    /// 3. 未命中时提供网页搜索地址
    /// 
    /// 后续可继续扩展为：
    /// - Windows 系统快捷键
    /// - 浏览器快捷键
    /// - 常见软件操作速查
    /// </summary>
    public class 操作速查服务
    {
        /// <summary>
        /// 当前内置的 WPS 快捷键知识库。
        /// 这一版按：通用 / 文字 / 表格 / 演示 四类组织。
        /// </summary>
        private readonly List<快捷键知识项> _wps快捷键列表 = new()
        {
            // =========================
            // 通用
            // =========================
            new 快捷键知识项 { Id = "wps_common_help", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "帮助与问答", 快捷键 = "F1", 说明 = "打开帮助与问答。", 关键字 = "帮助 问答 f1 通用" },
            new 快捷键知识项 { Id = "wps_common_taskpane", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "开启/关闭任务窗格", 快捷键 = "Ctrl + F1", 说明 = "开启或关闭任务窗格。", 关键字 = "任务窗格 ctrl+f1 通用" },
            new 快捷键知识项 { Id = "wps_common_new", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "新建空白文档", 快捷键 = "Ctrl + N", 说明 = "新建空白文档。", 关键字 = "新建 空白 文档 ctrl+n 通用" },
            new 快捷键知识项 { Id = "wps_common_open", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "打开文件", 快捷键 = "Ctrl + O", 说明 = "打开本地文件。", 关键字 = "打开 文件 ctrl+o 通用" },
            new 快捷键知识项 { Id = "wps_common_nexttab", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "向右切换文档标签", 快捷键 = "Ctrl + Tab", 说明 = "向右切换文档标签。", 关键字 = "切换 标签 ctrl+tab 通用" },
            new 快捷键知识项 { Id = "wps_common_prevtab", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "向左切换文档标签", 快捷键 = "Ctrl + Shift + Tab", 说明 = "向左切换文档标签。", 关键字 = "切换 标签 ctrl+shift+tab 通用" },
            new 快捷键知识项 { Id = "wps_common_close_doc", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "关闭文档窗口", 快捷键 = "Ctrl + W / Ctrl + F4", 说明 = "关闭当前文档窗口。", 关键字 = "关闭 文档 ctrl+w ctrl+f4 通用" },
            new 快捷键知识项 { Id = "wps_common_min", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "最小化窗口", 快捷键 = "Alt + Space + N", 说明 = "最小化 WPS 窗口。", 关键字 = "最小化 alt+space+n 通用" },
            new 快捷键知识项 { Id = "wps_common_max", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "最大化窗口", 快捷键 = "Alt + Space + X", 说明 = "最大化 WPS 窗口。", 关键字 = "最大化 alt+space+x 通用" },
            new 快捷键知识项 { Id = "wps_common_restore", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "还原窗口", 快捷键 = "Alt + Space + R", 说明 = "向下还原 WPS 窗口。", 关键字 = "还原 alt+space+r 通用" },
            new 快捷键知识项 { Id = "wps_common_close_app", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "关闭 WPS", 快捷键 = "Alt + F4 / Alt + Space + C", 说明 = "关闭 WPS 窗口。", 关键字 = "关闭 程序 alt+f4 通用" },
            new 快捷键知识项 { Id = "wps_common_copy", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "复制", 快捷键 = "Ctrl + C", 说明 = "复制选中的内容。", 关键字 = "复制 ctrl+c 通用" },
            new 快捷键知识项 { Id = "wps_common_cut", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "剪切", 快捷键 = "Ctrl + X", 说明 = "剪切选中的内容。", 关键字 = "剪切 ctrl+x 通用" },
            new 快捷键知识项 { Id = "wps_common_paste", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "粘贴", 快捷键 = "Ctrl + V", 说明 = "粘贴内容。", 关键字 = "粘贴 ctrl+v 通用" },
            new 快捷键知识项 { Id = "wps_common_save", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "保存", 快捷键 = "Ctrl + S", 说明 = "保存文件，未保存文件会弹出另存为。", 关键字 = "保存 ctrl+s 通用" },
            new 快捷键知识项 { Id = "wps_common_saveas", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "另存为", 快捷键 = "F12", 说明 = "另存文件。", 关键字 = "另存为 f12 通用" },
            new 快捷键知识项 { Id = "wps_common_find", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "查找", 快捷键 = "Ctrl + F", 说明 = "打开查找功能。", 关键字 = "查找 搜索 ctrl+f 通用" },
            new 快捷键知识项 { Id = "wps_common_replace", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "替换", 快捷键 = "Ctrl + H", 说明 = "打开替换功能。", 关键字 = "替换 ctrl+h 通用" },
            new 快捷键知识项 { Id = "wps_common_select_all", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "全选", 快捷键 = "Ctrl + A", 说明 = "全选当前内容。", 关键字 = "全选 ctrl+a 通用" },
            new 快捷键知识项 { Id = "wps_common_head", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "定位到文档首", 快捷键 = "Ctrl + Home", 说明 = "快速定位到文档开头。", 关键字 = "文档首 开头 ctrl+home 通用" },
            new 快捷键知识项 { Id = "wps_common_tail", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "定位到文档尾", 快捷键 = "Ctrl + End", 说明 = "快速定位到文档末尾。", 关键字 = "文档尾 结尾 ctrl+end 通用" },
            new 快捷键知识项 { Id = "wps_common_bold", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "文本加粗", 快捷键 = "Ctrl + B", 说明 = "将文本设置为加粗。", 关键字 = "加粗 粗体 ctrl+b 通用" },
            new 快捷键知识项 { Id = "wps_common_italic", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "文本倾斜", 快捷键 = "Ctrl + I", 说明 = "将文本设置为倾斜。", 关键字 = "倾斜 斜体 ctrl+i 通用" },
            new 快捷键知识项 { Id = "wps_common_underline", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "下划线", 快捷键 = "Ctrl + U", 说明 = "快速添加下划线。", 关键字 = "下划线 ctrl+u 通用" },
            new 快捷键知识项 { Id = "wps_common_font_minus", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "减小字号", 快捷键 = "Ctrl + Shift + ,", 说明 = "快速减小字号。", 关键字 = "减小字号 ctrl+shift+, 通用" },
            new 快捷键知识项 { Id = "wps_common_font_plus", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "增大字号", 快捷键 = "Ctrl + Shift + .", 说明 = "快速增大字号。", 关键字 = "增大字号 ctrl+shift+. 通用" },
            new 快捷键知识项 { Id = "wps_common_macro", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "打开 VB 宏", 快捷键 = "Alt + F8", 说明 = "打开 VB 宏对话框。", 关键字 = "vb 宏 alt+f8 通用" },
            new 快捷键知识项 { Id = "wps_common_vb", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "打开 VB 编辑器", 快捷键 = "Alt + F11", 说明 = "打开 VB 编辑器。", 关键字 = "vb 编辑器 alt+f11 通用" },
            new 快捷键知识项 { Id = "wps_common_undo", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "撤销", 快捷键 = "Ctrl + Z", 说明 = "撤销上一步操作。", 关键字 = "撤销 ctrl+z 通用" },
            new 快捷键知识项 { Id = "wps_common_redo", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "恢复撤销", 快捷键 = "Ctrl + Y", 说明 = "恢复刚刚撤销的内容。", 关键字 = "重做 恢复 ctrl+y 通用" },
            new 快捷键知识项 { Id = "wps_common_shot", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "截屏", 快捷键 = "Ctrl + Alt + X", 说明 = "打开截屏。", 关键字 = "截屏 截图 ctrl+alt+x 通用" },
            new 快捷键知识项 { Id = "wps_common_shot_hide", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "截屏时隐藏当前窗口", 快捷键 = "Ctrl + Alt + C", 说明 = "截屏时隐藏当前窗口。", 关键字 = "截屏 隐藏窗口 ctrl+alt+c 通用" },
            new 快捷键知识项 { Id = "wps_common_ocr", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "截图取字", 快捷键 = "Ctrl + Alt + S", 说明 = "直接截图并取字。", 关键字 = "截图取字 ocr ctrl+alt+s 通用" },
            new 快捷键知识项 { Id = "wps_common_link", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "超链接", 快捷键 = "Ctrl + K", 说明 = "打开超链接对话框。", 关键字 = "超链接 ctrl+k 通用" },
            new 快捷键知识项 { Id = "wps_common_print", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "打印", 快捷键 = "Ctrl + P", 说明 = "打开打印。", 关键字 = "打印 ctrl+p 通用" },
            new 快捷键知识项 { Id = "wps_common_spell", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "拼写检查", 快捷键 = "F7", 说明 = "执行拼写检查。", 关键字 = "拼写 检查 f7 通用" },
            new 快捷键知识项 { Id = "wps_common_updatefield", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "更新域", 快捷键 = "F9", 说明 = "更新域内容。", 关键字 = "更新域 f9 通用" },
            new 快捷键知识项 { Id = "wps_common_repeat", 软件名称 = "WPS", 分类 = "通用", 功能名称 = "重复上一步操作", 快捷键 = "F4", 说明 = "重复最近一次操作。", 关键字 = "重复 上一步 f4 通用" },

            // =========================
            // 文字
            // =========================
            new 快捷键知识项 { Id = "wps_writer_format_copy", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "复制格式", 快捷键 = "Ctrl + Shift + C", 说明 = "复制文本格式。", 关键字 = "复制格式 ctrl+shift+c 文字" },
            new 快捷键知识项 { Id = "wps_writer_format_paste", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "粘贴格式", 快捷键 = "Ctrl + Shift + V", 说明 = "粘贴文本格式。", 关键字 = "粘贴格式 ctrl+shift+v 文字" },
            new 快捷键知识项 { Id = "wps_writer_style_down", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "样式级别下降", 快捷键 = "Shift + Alt + →", 说明 = "样式级别下降。", 关键字 = "样式 级别 下降 shift+alt+→ 文字" },
            new 快捷键知识项 { Id = "wps_writer_style_up", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "样式级别增加", 快捷键 = "Shift + Alt + ←", 说明 = "样式级别增加。", 关键字 = "样式 级别 增加 shift+alt+← 文字" },
            new 快捷键知识项 { Id = "wps_writer_delete_prev", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "删除前一个单词", 快捷键 = "Ctrl + Backspace", 说明 = "删除前面的一个单词或更多字符。", 关键字 = "删除 前一个 单词 ctrl+backspace 文字" },
            new 快捷键知识项 { Id = "wps_writer_delete_next", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "删除后一个单词", 快捷键 = "Ctrl + Delete", 说明 = "删除后面的一个单词或更多字符。", 关键字 = "删除 后一个 单词 ctrl+delete 文字" },
            new 快捷键知识项 { Id = "wps_writer_line_head", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "定位到行首", 快捷键 = "Home", 说明 = "光标定位到当前行首。", 关键字 = "行首 home 文字" },
            new 快捷键知识项 { Id = "wps_writer_line_end", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "定位到行末", 快捷键 = "End", 说明 = "光标定位到当前行末。", 关键字 = "行末 end 文字" },
            new 快捷键知识项 { Id = "wps_writer_goto", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "定位", 快捷键 = "Ctrl + G", 说明 = "打开定位。", 关键字 = "定位 ctrl+g 文字" },
            new 快捷键知识项 { Id = "wps_writer_last_edit", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "回到上次编辑位置", 快捷键 = "Shift + F5", 说明 = "快速定位到上一次编辑的位置。", 关键字 = "上次编辑 shift+f5 文字" },
            new 快捷键知识项 { Id = "wps_writer_superscript", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "上标", 快捷键 = "Ctrl + Shift + =", 说明 = "设置或取消上标。", 关键字 = "上标 ctrl+shift+= 文字" },
            new 快捷键知识项 { Id = "wps_writer_subscript", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "下标", 快捷键 = "Ctrl + =", 说明 = "设置或取消下标。", 关键字 = "下标 ctrl+= 文字" },
            new 快捷键知识项 { Id = "wps_writer_align_justify", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "两端对齐", 快捷键 = "Ctrl + J", 说明 = "设置两端对齐。", 关键字 = "两端对齐 ctrl+j 文字" },
            new 快捷键知识项 { Id = "wps_writer_align_left", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "左对齐", 快捷键 = "Ctrl + L", 说明 = "设置左对齐。", 关键字 = "左对齐 ctrl+l 文字" },
            new 快捷键知识项 { Id = "wps_writer_align_center", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "居中对齐", 快捷键 = "Ctrl + E", 说明 = "设置居中对齐。", 关键字 = "居中 ctrl+e 文字" },
            new 快捷键知识项 { Id = "wps_writer_align_right", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "右对齐", 快捷键 = "Ctrl + R", 说明 = "设置右对齐。", 关键字 = "右对齐 ctrl+r 文字" },
            new 快捷键知识项 { Id = "wps_writer_bookmark", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "插入书签", 快捷键 = "Ctrl + Shift + F5", 说明 = "插入书签。", 关键字 = "书签 ctrl+shift+f5 文字" },
            new 快捷键知识项 { Id = "wps_writer_pagebreak", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "插入分页符", 快捷键 = "Ctrl + Enter", 说明 = "插入分页符。", 关键字 = "分页符 ctrl+enter 文字" },
            new 快捷键知识项 { Id = "wps_writer_linebreak", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "插入换行符", 快捷键 = "Shift + Enter", 说明 = "插入换行符。", 关键字 = "换行符 shift+enter 文字" },
            new 快捷键知识项 { Id = "wps_writer_wordcount", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "字数统计", 快捷键 = "Ctrl + Shift + G", 说明 = "查看文档字数统计。", 关键字 = "字数统计 ctrl+shift+g 文字" },
            new 快捷键知识项 { Id = "wps_writer_fontdialog", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "字体对话框", 快捷键 = "Ctrl + D", 说明 = "打开字体对话框。", 关键字 = "字体 对话框 ctrl+d 文字" },
            new 快捷键知识项 { Id = "wps_writer_track", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "修订模式", 快捷键 = "Ctrl + Shift + E", 说明 = "开启或关闭修订模式。", 关键字 = "修订 模式 ctrl+shift+e 文字" },
            new 快捷键知识项 { Id = "wps_writer_view_page", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "页面视图", 快捷键 = "Ctrl + Alt + P", 说明 = "切换到页面视图。", 关键字 = "页面视图 ctrl+alt+p 文字" },
            new 快捷键知识项 { Id = "wps_writer_view_full", 软件名称 = "WPS", 分类 = "文字", 功能名称 = "全屏显示", 快捷键 = "Ctrl + Alt + F", 说明 = "切换到全屏显示。", 关键字 = "全屏 ctrl+alt+f 文字" },

            // =========================
            // 表格
            // =========================
            new 快捷键知识项 { Id = "wps_sheet_date", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "输入当前日期", 快捷键 = "Ctrl + ;", 说明 = "输入当前日期。", 关键字 = "日期 ctrl+; 表格" },
            new 快捷键知识项 { Id = "wps_sheet_time", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "输入当前时间", 快捷键 = "Ctrl + Shift + ; / Ctrl + '", 说明 = "输入当前时间。", 关键字 = "时间 ctrl+shift+; ctrl+' 表格" },
            new 快捷键知识项 { Id = "wps_sheet_delete_dialog", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "删除对话框", 快捷键 = "Ctrl + -", 说明 = "弹出删除对话框，可删除行列单元格。", 关键字 = "删除 对话框 ctrl+- 表格" },
            new 快捷键知识项 { Id = "wps_sheet_insert_dialog", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "插入对话框", 快捷键 = "Ctrl + Shift + =", 说明 = "弹出插入对话框，可插入行列单元格。", 关键字 = "插入 对话框 ctrl+shift+= 表格" },
            new 快捷键知识项 { Id = "wps_sheet_delete_value", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "删除单元格内容", 快捷键 = "Delete", 说明 = "删除单元格内容。", 关键字 = "删除 内容 delete 表格" },
            new 快捷键知识项 { Id = "wps_sheet_backspace", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "清空并进入编辑", 快捷键 = "Backspace", 说明 = "删除单元格内容并进入编辑状态。", 关键字 = "backspace 编辑 表格" },
            new 快捷键知识项 { Id = "wps_sheet_esc", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "退出编辑不保存", 快捷键 = "Esc", 说明 = "退出编辑状态，编辑内容不保存。", 关键字 = "esc 退出 编辑 表格" },
            new 快捷键知识项 { Id = "wps_sheet_enter", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "保存并向下移动", 快捷键 = "Enter", 说明 = "退出编辑状态并向下移动。", 关键字 = "enter 保存 向下 表格" },
            new 快捷键知识项 { Id = "wps_sheet_f2", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "进入编辑状态", 快捷键 = "F2", 说明 = "进入当前单元格编辑状态。", 关键字 = "f2 编辑 单元格 表格" },
            new 快捷键知识项 { Id = "wps_sheet_newline", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "单元格内换行", 快捷键 = "Alt + Enter", 说明 = "在单元格内换行。", 关键字 = "换行 alt+enter 表格" },
            new 快捷键知识项 { Id = "wps_sheet_end", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "定位到数据区右下角", 快捷键 = "Ctrl + End", 说明 = "定位到数据区域最右下角单元格。", 关键字 = "定位 右下角 ctrl+end 表格" },
            new 快捷键知识项 { Id = "wps_sheet_home", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "定位到 A1", 快捷键 = "Ctrl + Home", 说明 = "定位到 A1 单元格。", 关键字 = "a1 ctrl+home 表格" },
            new 快捷键知识项 { Id = "wps_sheet_ref", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "定位引用单元格", 快捷键 = "Ctrl + [", 说明 = "定位到引用单元格。", 关键字 = "引用 单元格 ctrl+[ 表格" },
            new 快捷键知识项 { Id = "wps_sheet_goto", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "定位", 快捷键 = "Ctrl + G", 说明 = "打开定位。", 关键字 = "定位 ctrl+g 表格" },
            new 快捷键知识项 { Id = "wps_sheet_left_sheet", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "向左切换工作表", 快捷键 = "Ctrl + PageUp", 说明 = "向左切换活动工作表。", 关键字 = "切换 工作表 ctrl+pageup 表格" },
            new 快捷键知识项 { Id = "wps_sheet_right_sheet", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "向右切换工作表", 快捷键 = "Ctrl + PageDown", 说明 = "向右切换活动工作表。", 关键字 = "切换 工作表 ctrl+pagedown 表格" },
            new 快捷键知识项 { Id = "wps_sheet_format", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "单元格格式", 快捷键 = "Ctrl + 1", 说明 = "打开单元格格式对话框。", 关键字 = "单元格 格式 ctrl+1 表格" },
            new 快捷键知识项 { Id = "wps_sheet_bold", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "加粗", 快捷键 = "Ctrl + B / Ctrl + 2", 说明 = "设置加粗。", 关键字 = "加粗 ctrl+b ctrl+2 表格" },
            new 快捷键知识项 { Id = "wps_sheet_fill_down", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "向下填充", 快捷键 = "Ctrl + D", 说明 = "填充上方单元格内容。", 关键字 = "填充 ctrl+d 表格" },
            new 快捷键知识项 { Id = "wps_sheet_fill_right", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "向右填充", 快捷键 = "Ctrl + R", 说明 = "填充左侧单元格内容。", 关键字 = "填充 ctrl+r 表格" },
            new 快捷键知识项 { Id = "wps_sheet_smartfill", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "智能填充", 快捷键 = "Ctrl + E", 说明 = "执行智能填充。", 关键字 = "智能填充 ctrl+e 表格" },
            new 快捷键知识项 { Id = "wps_sheet_paste_value", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "粘贴为数值", 快捷键 = "Ctrl + Shift + V", 说明 = "以数值形式粘贴。", 关键字 = "粘贴 数值 ctrl+shift+v 表格" },
            new 快捷键知识项 { Id = "wps_sheet_formula", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "显示公式", 快捷键 = "Ctrl + `", 说明 = "显示或隐藏公式。", 关键字 = "公式 ctrl+` 表格" },
            new 快捷键知识项 { Id = "wps_sheet_sum", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "求和", 快捷键 = "Alt + =", 说明 = "快速求和。", 关键字 = "求和 alt+= 表格" },
            new 快捷键知识项 { Id = "wps_sheet_refresh_pivot", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "刷新数据透视表", 快捷键 = "Alt + F5", 说明 = "刷新数据透视表。", 关键字 = "刷新 数据透视表 alt+f5 表格" },
            new 快捷键知识项 { Id = "wps_sheet_merge", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "合并居中", 快捷键 = "Ctrl + M", 说明 = "合并并居中。", 关键字 = "合并居中 ctrl+m 表格" },
            new 快捷键知识项 { Id = "wps_sheet_chart", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "插入图表", 快捷键 = "F11", 说明 = "快速插入图表。", 关键字 = "图表 f11 表格" },
            new 快捷键知识项 { Id = "wps_sheet_newsheet", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "新建工作表", 快捷键 = "Shift + F11", 说明 = "插入一个新工作表。", 关键字 = "新建 工作表 shift+f11 表格" },
            new 快捷键知识项 { Id = "wps_sheet_comment", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "插入批注", 快捷键 = "Shift + F2", 说明 = "插入批注。", 关键字 = "批注 shift+f2 表格" },
            new 快捷键知识项 { Id = "wps_sheet_zoom", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "调整显示比例", 快捷键 = "Ctrl + 鼠标滚轮", 说明 = "调整表格显示比例。", 关键字 = "缩放 比例 ctrl+滚轮 表格" },
            new 快捷键知识项 { Id = "wps_sheet_name", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "名称管理器", 快捷键 = "Ctrl + F3", 说明 = "打开名称管理器。", 关键字 = "名称管理器 ctrl+f3 表格" },
            new 快捷键知识项 { Id = "wps_sheet_table", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "创建表格", 快捷键 = "Ctrl + L / Ctrl + T", 说明 = "创建表格。", 关键字 = "创建表格 ctrl+l ctrl+t 表格" },
            new 快捷键知识项 { Id = "wps_sheet_filter", 软件名称 = "WPS", 分类 = "表格", 功能名称 = "自动筛选", 快捷键 = "Ctrl + Shift + L", 说明 = "创建或取消自动筛选。", 关键字 = "自动筛选 ctrl+shift+l 表格" },

            // =========================
            // 演示
            // =========================
            new 快捷键知识项 { Id = "wps_ppt_play_all", 软件名称 = "WPS", 分类 = "演示", 功能名称 = "从第一页放映", 快捷键 = "F5", 说明 = "从第一页开始放映。", 关键字 = "放映 f5 演示" },
            new 快捷键知识项 { Id = "wps_ppt_play_current", 软件名称 = "WPS", 分类 = "演示", 功能名称 = "从当前页放映", 快捷键 = "Shift + F5", 说明 = "从当前幻灯片开始放映。", 关键字 = "当前 放映 shift+f5 演示" },
            new 快捷键知识项 { Id = "wps_ppt_exit", 软件名称 = "WPS", 分类 = "演示", 功能名称 = "结束放映", 快捷键 = "Esc", 说明 = "结束放映。", 关键字 = "结束 放映 esc 演示" },
            new 快捷键知识项 { Id = "wps_ppt_speaker_current", 软件名称 = "WPS", 分类 = "演示", 功能名称 = "当前页演讲者视图", 快捷键 = "Alt + Shift + F5", 说明 = "从当前页进入演讲者视图放映。", 关键字 = "演讲者 视图 alt+shift+f5 演示" },
            new 快捷键知识项 { Id = "wps_ppt_speaker_all", 软件名称 = "WPS", 分类 = "演示", 功能名称 = "首页演讲者视图", 快捷键 = "Alt + F5", 说明 = "从第一页进入演讲者视图放映。", 关键字 = "演讲者 视图 alt+f5 演示" },
            new 快捷键知识项 { Id = "wps_ppt_black", 软件名称 = "WPS", 分类 = "演示", 功能名称 = "黑屏", 快捷键 = "Ctrl + B", 说明 = "放映状态黑屏。", 关键字 = "黑屏 ctrl+b 演示" },
            new 快捷键知识项 { Id = "wps_ppt_white", 软件名称 = "WPS", 分类 = "演示", 功能名称 = "白屏", 快捷键 = "Ctrl + W", 说明 = "放映状态白屏。", 关键字 = "白屏 ctrl+w 演示" },
            new 快捷键知识项 { Id = "wps_ppt_pen", 软件名称 = "WPS", 分类 = "演示", 功能名称 = "水彩笔", 快捷键 = "Ctrl + P", 说明 = "放映状态切换到水彩笔。", 关键字 = "水彩笔 ctrl+p 演示" },
            new 快捷键知识项 { Id = "wps_ppt_marker", 软件名称 = "WPS", 分类 = "演示", 功能名称 = "荧光笔", 快捷键 = "Ctrl + I", 说明 = "放映状态切换到荧光笔。", 关键字 = "荧光笔 ctrl+i 演示" },
            new 快捷键知识项 { Id = "wps_ppt_eraser", 软件名称 = "WPS", 分类 = "演示", 功能名称 = "橡皮擦", 快捷键 = "Ctrl + E", 说明 = "放映状态切换到橡皮擦。", 关键字 = "橡皮擦 ctrl+e 演示" },
            new 快捷键知识项 { Id = "wps_ppt_pointer_show", 软件名称 = "WPS", 分类 = "演示", 功能名称 = "箭头可见", 快捷键 = "Ctrl + U", 说明 = "显示箭头。", 关键字 = "箭头 可见 ctrl+u 演示" },
            new 快捷键知识项 { Id = "wps_ppt_pointer_hide", 软件名称 = "WPS", 分类 = "演示", 功能名称 = "箭头隐藏", 快捷键 = "Ctrl + H", 说明 = "隐藏箭头。", 关键字 = "箭头 隐藏 ctrl+h 演示" },
            new 快捷键知识项 { Id = "wps_ppt_newslide", 软件名称 = "WPS", 分类 = "演示", 功能名称 = "新建幻灯片", 快捷键 = "Ctrl + M / Enter", 说明 = "新建一页幻灯片。", 关键字 = "新建 幻灯片 ctrl+m enter 演示" },
            new 快捷键知识项 { Id = "wps_ppt_menu", 软件名称 = "WPS", 分类 = "演示", 功能名称 = "右键菜单", 快捷键 = "Shift + F10", 说明 = "打开右键菜单。", 关键字 = "右键 菜单 shift+f10 演示" },
            new 快捷键知识项 { Id = "wps_ppt_grid", 软件名称 = "WPS", 分类 = "演示", 功能名称 = "显示/隐藏网格线", 快捷键 = "Shift + F9", 说明 = "显示或隐藏网格线。", 关键字 = "网格线 shift+f9 演示" },
            new 快捷键知识项 { Id = "wps_ppt_addtext", 软件名称 = "WPS", 分类 = "演示", 功能名称 = "给对象添加文字", 快捷键 = "F2", 说明 = "在选取的对象中添加文字。", 关键字 = "文字 f2 演示" }
        };

        /// <summary>
        /// 获取分类列表。
        /// </summary>
        public List<string> 获取WPS分类列表()
        {
            return new List<string> { "全部", "通用", "文字", "表格", "演示" };
        }

        /// <summary>
        /// 获取全部 WPS 快捷键项。
        /// </summary>
        public List<快捷键知识项> 获取全部WPS快捷键()
        {
            return _wps快捷键列表
                .OrderBy(x => 分类排序值(x.分类))
                .ThenBy(x => x.功能名称)
                .ToList();
        }

        /// <summary>
        /// 在本地知识库中搜索 WPS 快捷键。
        /// 支持分类过滤。
        /// </summary>
        public List<快捷键知识项> 搜索WPS快捷键(string 输入, string 分类 = "全部")
        {
            string 关键词 = (输入 ?? string.Empty).Trim();

            IEnumerable<快捷键知识项> 查询 = _wps快捷键列表;

            if (!string.IsNullOrWhiteSpace(分类) && 分类 != "全部")
            {
                查询 = 查询.Where(x => x.分类 == 分类);
            }

            if (!string.IsNullOrWhiteSpace(关键词))
            {
                查询 = 查询.Where(x =>
                    包含文本(x.分类, 关键词) ||
                    包含文本(x.功能名称, 关键词) ||
                    包含文本(x.快捷键, 关键词) ||
                    包含文本(x.说明, 关键词) ||
                    包含文本(x.关键字, 关键词));
            }

            return 查询
                .OrderBy(x => 分类排序值(x.分类))
                .ThenBy(x => x.功能名称)
                .ToList();
        }

        /// <summary>
        /// 构造 WPS 快捷键的网页搜索地址。
        /// </summary>
        public string 构造WPS网页搜索地址(string 输入)
        {
            string 搜索词 = string.IsNullOrWhiteSpace(输入)
                ? "WPS 快捷键"
                : $"WPS 快捷键 {输入.Trim()}";

            return 网址处理工具.构造搜索引擎地址(搜索词);
        }

        public int 获取WPS快捷键数量()
        {
            return _wps快捷键列表.Count;
        }

        private int 分类排序值(string 分类)
        {
            return 分类 switch
            {
                "通用" => 1,
                "文字" => 2,
                "表格" => 3,
                "演示" => 4,
                _ => 99
            };
        }

        private bool 包含文本(string 原文, string 关键词)
        {
            return (原文 ?? string.Empty)
                .Contains(关键词, StringComparison.OrdinalIgnoreCase);
        }
    }
}
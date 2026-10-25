# -*- coding: utf-8 -*-
"""Generate ArchSkin development document (DOCX) for human readers."""

from docx import Document
from docx.shared import Pt, RGBColor, Inches, Cm
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_ALIGN_VERTICAL
from docx.oxml.ns import qn
from docx.oxml import OxmlElement

OUT = r"A:\Now\ArchSkin\docs\ArchSkin_开发文档.docx"


def set_cell_bg(cell, color_hex: str):
    tc_pr = cell._tc.get_or_add_tcPr()
    shd = OxmlElement('w:shd')
    shd.set(qn('w:val'), 'clear')
    shd.set(qn('w:color'), 'auto')
    shd.set(qn('w:fill'), color_hex)
    tc_pr.append(shd)


def add_heading(doc, text, level=1):
    h = doc.add_heading(text, level=level)
    for run in h.runs:
        run.font.color.rgb = RGBColor(0x0F, 0x1A, 0x2E)
    return h


def add_para(doc, text, bold=False, italic=False, color=None, size=None, align=None):
    p = doc.add_paragraph()
    run = p.add_run(text)
    run.bold = bold
    run.italic = italic
    if color:
        run.font.color.rgb = color
    if size:
        run.font.size = Pt(size)
    if align:
        p.alignment = align
    return p


def add_bullet(doc, text, level=0):
    style = 'List Bullet' if level == 0 else 'List Bullet 2'
    p = doc.add_paragraph(text, style=style)
    return p


def add_code_block(doc, text):
    p = doc.add_paragraph()
    p.paragraph_format.left_indent = Cm(0.8)
    run = p.add_run(text)
    run.font.name = 'Consolas'
    run.font.size = Pt(9)
    run.font.color.rgb = RGBColor(0x0B, 0x3D, 0x20)
    return p


def add_table(doc, headers, rows, col_widths=None):
    table = doc.add_table(rows=1 + len(rows), cols=len(headers))
    table.style = 'Light Grid Accent 1'
    # Header
    hdr = table.rows[0]
    for i, h in enumerate(headers):
        cell = hdr.cells[i]
        cell.text = ''
        p = cell.paragraphs[0]
        run = p.add_run(h)
        run.bold = True
        run.font.size = Pt(10)
        run.font.color.rgb = RGBColor(0xFF, 0xFF, 0xFF)
        set_cell_bg(cell, '0F1A2E')
        cell.vertical_alignment = WD_ALIGN_VERTICAL.CENTER
    # Body
    for r_idx, row in enumerate(rows):
        for c_idx, val in enumerate(row):
            cell = table.rows[r_idx + 1].cells[c_idx]
            cell.text = ''
            p = cell.paragraphs[0]
            run = p.add_run(str(val))
            run.font.size = Pt(9)
            if r_idx % 2 == 1:
                set_cell_bg(cell, 'F4F6FA')
    if col_widths:
        for row in table.rows:
            for i, w in enumerate(col_widths):
                row.cells[i].width = Cm(w)
    return table


def main():
    doc = Document()

    # Page margins
    for section in doc.sections:
        section.left_margin = Cm(2.2)
        section.right_margin = Cm(2.2)
        section.top_margin = Cm(2.0)
        section.bottom_margin = Cm(2.0)

    # Default style
    style = doc.styles['Normal']
    style.font.name = 'Microsoft YaHei'
    style.font.size = Pt(10.5)
    style._element.rPr.rFonts.set(qn('w:eastAsia'), 'Microsoft YaHei')

    # ============== Cover ==============
    title = doc.add_paragraph()
    title.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = title.add_run('ArchSkin 项目开发文档')
    r.bold = True
    r.font.size = Pt(28)
    r.font.color.rgb = RGBColor(0x0F, 0x1A, 0x2E)

    sub = doc.add_paragraph()
    sub.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = sub.add_run('Windows 原生 Arch 风格桌面外壳')
    r.font.size = Pt(14)
    r.font.color.rgb = RGBColor(0x4A, 0x5C, 0x78)

    meta = doc.add_paragraph()
    meta.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = meta.add_run('版本 1.0  ·  2026-10  ·  由 LDE (Linux DE for Windows) 更名而来')
    r.font.size = Pt(10)
    r.italic = True
    r.font.color.rgb = RGBColor(0x6B, 0x7A, 0x8F)

    doc.add_paragraph()

    # Warning box
    warn = doc.add_table(rows=1, cols=1)
    warn.style = 'Table Grid'
    cell = warn.rows[0].cells[0]
    set_cell_bg(cell, 'FFF4E5')
    p = cell.paragraphs[0]
    r = p.add_run('⚠ 重要范围界定')
    r.bold = True
    r.font.color.rgb = RGBColor(0xC7, 0x65, 0x10)
    r.font.size = Pt(11)
    p2 = cell.add_paragraph()
    r = p2.add_run(
        '本项目是 Windows 原生重写，不是 Linux DE 移植。\n'
        '不依赖 WSL、不依赖虚拟机、不运行 Linux 二进制。\n'
        '从 Arch 仓库仅拉取静态资源文件（图标、主题、CSS、字体），绝不执行。'
    )
    r.font.size = Pt(10)

    doc.add_page_break()

    # ============== 目录 ==============
    add_heading(doc, '目录', level=1)
    toc_items = [
        '一、项目概述',
        '二、技术选型',
        '三、系统架构',
        '四、功能模块',
        '五、开发计划',
        '六、风险分析',
        '七、附录',
    ]
    for item in toc_items:
        add_bullet(doc, item)

    doc.add_page_break()

    # ============== 一、项目概述 ==============
    add_heading(doc, '一、项目概述', level=1)

    add_heading(doc, '1.1 项目背景', level=2)
    add_para(doc,
        'ArchSkin 是一个面向 Windows 10/11 用户的桌面外壳工具，目标是为用户提供'
        '"看起来像 Arch Linux 桌面"的视觉与交互体验，同时保留 Windows 原生运行环境。'
        '项目曾用名 "Linux DE for Windows"（简称 LDE），因原命名容易引发技术误读，'
        '正式更名为 ArchSkin，以准确反映"视觉复刻"的本质。')

    add_heading(doc, '1.2 项目目标', level=2)
    add_para(doc, '在 Windows 10/11 上，通过原生 C# + WinUI 3 重写，复刻 Arch Linux（GNOME 风格）的桌面体验：', bold=False)
    add_bullet(doc, '顶部面板（仿 GNOME Shell top bar）')
    add_bullet(doc, '左侧 Dock（仿 Ubuntu/Pop!_OS dash）')
    add_bullet(doc, 'Activities 概览与多工作区')
    add_bullet(doc, '通知中心与日历弹出')
    add_bullet(doc, '自研登录界面（覆盖 Windows LogonUI）')
    add_bullet(doc, '桌面替换（隐藏 Explorer 桌面图标）')
    add_bullet(doc, '从 Arch 仓库同步图标/主题/字体')

    add_heading(doc, '1.3 范围界定（重要）', level=2)
    add_para(doc, '为避免后续期望错位，明确以下"是"与"不是"：', bold=True)
    add_table(
        doc,
        ['维度', 'ArchSkin 是', 'ArchSkin 不是'],
        [
            ['本质', 'Windows 原生重写', 'Linux DE 移植'],
            ['Linux 运行时', '无（零依赖）', '需要 WSL/VM/容器'],
            ['执行 Linux 二进制', '不执行', '执行 GNOME Shell 等'],
            ['Arch 仓库内容', '仅下载静态资源文件', '运行 Arch 包管理器'],
            ['窗口管理', 'Windows DWM（不改）', '替换为 Mutter/KWin'],
            ['终端', 'Windows Terminal', 'Linux 终端模拟器'],
            ['文件浏览', '保留 Windows Explorer', '替换为 Nautilus/Dolphin'],
            ['营销表述', '"Windows 原生 Arch 风"', '"在 Windows 上跑 Linux DE"'],
        ],
        col_widths=[3.0, 5.5, 5.5],
    )

    add_heading(doc, '1.4 硬约束', level=2)
    add_bullet(doc, '平台：Windows 10 21H2+ / Windows 11，仅 x86_64')
    add_bullet(doc, '技术栈：C# (.NET 8 LTS) + WinUI 3 + Windows App SDK')
    add_bullet(doc, '开发平台：Windows 11')
    add_bullet(doc, '不依赖 WSL、不依赖虚拟机、不依赖容器')
    add_bullet(doc, '保留 Windows Explorer 的文件浏览功能')
    add_bullet(doc, '所有拉取的资源必须保留原始许可与署名')

    doc.add_page_break()

    # ============== 二、技术选型 ==============
    add_heading(doc, '二、技术选型', level=1)

    add_heading(doc, '2.1 主要技术栈', level=2)
    add_table(
        doc,
        ['层次', '选型', '版本/说明'],
        [
            ['语言', 'C#', '12 (.NET 8 LTS)'],
            ['UI 框架', 'WinUI 3', '1.6+'],
            ['OS SDK', 'Windows App SDK', '1.6+'],
            ['运行时', 'Windows 10 21H2+ / Win 11', 'x86_64'],
            ['包管理', 'NuGet', '—'],
            ['构建', 'dotnet build', '.NET 8 SDK'],
            ['安装包', 'Inno Setup', '6.x（免费）'],
        ],
        col_widths=[3.0, 5.5, 5.5],
    )

    add_heading(doc, '2.2 关键 NuGet 包', level=2)
    add_code_block(doc,
        '<PackageReference Include="Microsoft.WindowsAppSDK" Version="1.6.x" />\n'
        '<PackageReference Include="Microsoft.Windows.SDK.BuildTools" Version="10.0.26100.x" />\n'
        '<PackageReference Include="CommunityToolkit.WinUI.UI" Version="8.x" />\n'
        '<PackageReference Include="CommunityToolkit.WinUI.Controls" Version="8.x" />\n'
        '<PackageReference Include="System.Text.Json" Version="8.x" />\n'
        '<PackageReference Include="SharpZipLib" Version="1.x" />  <!-- .tar 解析 -->\n'
        '<PackageReference Include="ZstdSharp.Port" Version="8.x" /> <!-- .zst 解析 -->\n'
        '<PackageReference Include="Microsoft.Extensions.Hosting" Version="8.x" />')

    add_heading(doc, '2.3 关键 Windows API', level=2)
    add_bullet(doc, 'WinUI 3：Microsoft.UI.Xaml.*')
    add_bullet(doc, 'WinRT Toast：Windows.UI.Notifications.*')
    add_bullet(doc, 'Shell 钩子：P/Invoke user32.dll、shell32.dll、explorerframe.dll')
    add_bullet(doc, '桌面图标隐藏：注册表 HKCU\\...\\Explorer\\Advanced → ShowDesktopIcons=0')
    add_bullet(doc, '定时任务：Microsoft.Win32.TaskScheduler')
    add_bullet(doc, '壁纸设置：SystemParametersInfo(SPI_SETDESKWALLPAPER)')

    add_heading(doc, '2.4 禁止使用的技术', level=2)
    add_bullet(doc, '❌ 不得调用 wsl.exe 或任何 WSL 命令')
    add_bullet(doc, '❌ 不得依赖 \\wsl$ UNC 路径')
    add_bullet(doc, '❌ 不得引入需要 Linux 运行时的原生库')
    add_bullet(doc, '❌ 不得执行从 Arch 包中提取的任何脚本或二进制')

    doc.add_page_break()

    # ============== 三、系统架构 ==============
    add_heading(doc, '三、系统架构', level=1)

    add_heading(doc, '3.1 架构总览', level=2)
    add_para(doc, 'ArchSkin 由 6 个应用组件 + 2 个共享库组成：', bold=False)
    add_table(
        doc,
        ['组件', '类型', '运行时机', '职责'],
        [
            ['ArchSkin.Shell', 'WinUI 3 桌面应用', '用户登录后常驻', '主界面（面板+Dock+概览）'],
            ['ArchSkin.Login', 'WinUI 3 覆盖层', '开机预登录', 'Arch 风格登录界面'],
            ['ArchSkin.Desktop', 'WinUI 3 后台', '用户登录后', '桌面替换+壁纸'],
            ['ArchSkin.Service', '后台任务（定时）', '开机常驻', '生命周期管理+自启'],
            ['ArchSkin.ThemeSync', '命令行工具', '按需', '从 Arch 同步主题'],
            ['ArchSkin.Settings', 'WinUI 3 应用', '按需', '用户配置界面'],
            ['ArchSkin.Core', '共享 .NET 库', '库', '桥接、钩子、注册表'],
            ['ArchSkin.Assets', '内嵌资源包', '库', '默认主题资源'],
        ],
        col_widths=[3.5, 3.5, 3.5, 4.0],
    )

    add_heading(doc, '3.2 组件交互', level=2)
    add_para(doc, '各组件通过以下机制协作：', bold=False)
    add_bullet(doc, '注册表：所有组件共享配置（HKCU\\Software\\ArchSkin）')
    add_bullet(doc, '本地文件：%LOCALAPPDATA%\\ArchSkin\\ 下存放资产、清单、日志')
    add_bullet(doc, 'WinRT 事件：通知桥订阅 Toast 事件')
    add_bullet(doc, '进程间通信：Service 通过命名管道启动/控制 Shell')

    add_heading(doc, '3.3 数据流', level=2)
    add_para(doc, '主题同步流程（从 Arch 仓库到桌面）：', bold=True)
    steps = [
        '1. 用户执行 archskin install papirus-icon-theme',
        '2. 下载 INDEX.xz 元数据文件',
        '3. 解析 INDEX 定位目标包 .pkg.tar.zst',
        '4. 下载包文件（只读，不执行）',
        '5. 解压到 %LOCALAPPDATA%\\ArchSkin\\staging\\',
        '6. 转换：读取图标/主题/字体，生成 WinUI 3 XAML 资源字典',
        '7. 写入 %LOCALAPPDATA%\\ArchSkin\\assets\\ 与 manifest.json',
        '8. Shell 重新加载主题，用户可在设置中看到新主题',
    ]
    for s in steps:
        add_para(doc, s)

    add_heading(doc, '3.4 启动流程', level=2)
    add_para(doc, '用户登录后的启动顺序：', bold=True)
    boot_steps = [
        '1. Windows 启动 → 定时任务触发 ArchSkin.Service',
        '2. Service 检查注册表，确认桌面已隐藏',
        '3. Service 启动 ArchSkin.Shell（WinUI 3 应用）',
        '4. Shell 在 500ms 内显示面板',
        '5. Shell 加载已安装主题清单',
        '6. Shell 注册全局热键（Win+Tab、Win+Space 等）',
        '7. Shell 订阅系统通知',
        '8. Shell 启动进程监控（用于 Dock）',
    ]
    for s in boot_steps:
        add_para(doc, s)

    doc.add_page_break()

    # ============== 四、功能模块 ==============
    add_heading(doc, '四、功能模块', level=1)

    add_heading(doc, '4.1 主界面 Shell', level=2)
    add_para(doc, 'ArchSkin.Shell 是核心 UI 组件，包含以下子模块：', bold=False)
    add_bullet(doc, '面板（Panel）：顶部栏，含时钟、系统托盘、应用菜单')
    add_bullet(doc, 'Dock：左侧浮动，显示运行中应用')
    add_bullet(doc, '概览（Overview）：仿 GNOME Activities，多工作区切换')
    add_bullet(doc, '启动器（Launcher）：应用搜索与网格')
    add_bullet(doc, '通知中心：右下角浮窗，展示 WinRT Toast')
    add_bullet(doc, '日历弹出：顶部中心点击展开')

    add_para(doc, '性能约束：', bold=True)
    add_bullet(doc, '冷启动 < 500ms')
    add_bullet(doc, '稳态内存 < 100MB')
    add_bullet(doc, '不得阻塞开始菜单（Win 键）')
    add_bullet(doc, '必须支持 Win+L（锁定）、Win+D（显示桌面）、Win+Tab（概览）')

    add_heading(doc, '4.2 登录界面', level=2)
    add_para(doc, 'ArchSkin.Login 通过覆盖 Windows LogonUI 实现 Arch/GDM 风格登录：', bold=False)
    add_bullet(doc, 'WMI 检测 LogonUI.exe 启动，100ms 内显示覆盖层')
    add_bullet(doc, '支持用户名选择、密码输入、壁纸定制')
    add_bullet(doc, '凭据透传给 LogonUI，由 Windows 完成认证')
    add_bullet(doc, '遇到 Windows Hello、BitLocker PIN、域账户时自动隐藏，使用原生登录')
    add_bullet(doc, '失败时静默降级，不影响用户登录')

    add_heading(doc, '4.3 桌面替换', level=2)
    add_para(doc, '通过注册表与 Shell 钩子隐藏 Explorer 桌面图标：', bold=False)
    add_bullet(doc, '修改 HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced → ShowDesktopIcons=0')
    add_bullet(doc, '调用 SHChangeNotify 刷新桌面')
    add_bullet(doc, '卸载时回滚注册表')
    add_bullet(doc, '可选：自定义壁纸引擎与桌面小部件（Phase 2+）')

    add_heading(doc, '4.4 主题同步', level=2)
    add_para(doc, 'ArchSkin.ThemeSync 命令行工具从 Arch 仓库拉取资源：', bold=False)
    add_para(doc, '支持的命令：', bold=True)
    add_code_block(doc,
        'archskin sync [--theme <name>] [--mirror <url>] [--offline]\n'
        'archskin list                          # 列出可用主题\n'
        'archskin install <theme>               # 安装主题\n'
        'archskin remove <theme>                # 卸载主题\n'
        'archskin update                        # 检查更新\n'
        'archskin verify [--theme <name>]       # 验证许可完整')

    add_para(doc, '主要 Arch 主题包：', bold=True)
    add_bullet(doc, 'gnome-themes-extra（GNOME 主题扩展）')
    add_bullet(doc, 'papirus-icon-theme（MIT 图标）')
    add_bullet(doc, 'adwaita-icon-theme（GNOME 默认图标）')
    add_bullet(doc, 'material-icon-theme')
    add_bullet(doc, 'numix-themes-arch（社区仓库）')

    add_para(doc, '安全规则：', bold=True)
    add_bullet(doc, '所有 .tar 内容只读，不设置可执行权限')
    add_bullet(doc, '绝不执行 PKGBUILD、.sh 脚本或二进制')
    add_bullet(doc, '所有提取文件视为数据，不运行')
    add_bullet(doc, '每个主题保留 license.txt 与署名信息')

    add_heading(doc, '4.5 设置应用', level=2)
    add_para(doc, 'ArchSkin.Settings 提供用户配置界面，包含以下分区：', bold=False)
    add_bullet(doc, '主题：选择已安装主题、预览、应用')
    add_bullet(doc, '壁纸：系统壁纸或自定义图片')
    add_bullet(doc, '面板：时钟位置、电池显示、系统托盘图标')
    add_bullet(doc, 'Dock：图标大小、自动隐藏、位置')
    add_bullet(doc, '概览：工作区数量、动画效果')
    add_bullet(doc, '登录：启用/禁用覆盖、壁纸、品牌标识')
    add_bullet(doc, '桌面：图标开关、小部件')
    add_bullet(doc, '更新：自动更新、主题更新频率')
    add_bullet(doc, '关于：版本、致谢、许可证')

    add_heading(doc, '4.6 后台服务', level=2)
    add_para(doc, 'ArchSkin.Service 负责生命周期管理：', bold=False)
    add_bullet(doc, '开机时隐藏 Explorer 桌面')
    add_bullet(doc, '登录后自动启动 Shell')
    add_bullet(doc, '健康检查：Shell 崩溃时重启')
    add_bullet(doc, '更新检查（周期性，不激进）')
    add_bullet(doc, '卸载时回滚注册表')
    add_para(doc, '实现方式：用户级定时任务（无需管理员权限）', bold=False)

    doc.add_page_break()

    # ============== 五、开发计划 ==============
    add_heading(doc, '五、开发计划', level=1)

    add_heading(doc, '5.1 阶段划分', level=2)
    add_table(
        doc,
        ['阶段', '周期', '目标', '关键交付'],
        [
            ['M0 PoC', '2-4 周', '验证基础架构',
             '空解决方案、Shell 启动、ThemeSync 拉包、隐藏桌面'],
            ['M1 MVP Shell', '2-3 个月', '可用桌面外壳',
             '面板、Dock、启动器、通知、设置、安装器'],
            ['M2 完整 Shell', '2-3 个月', '完整 GNOME 体验',
             '概览、多工作区、通知操作、日历、登录覆盖'],
            ['M3 多主题', '1-2 个月', 'KDE、Xfce 主题',
             'XAML 字典、主题切换器、预览'],
            ['M4 GA', '2-3 个月', '公开发布',
             '更新器、文档、本地化、商店发布'],
        ],
        col_widths=[2.5, 2.5, 4.0, 6.0],
    )

    add_heading(doc, '5.2 里程碑详情', level=2)

    add_para(doc, 'M0：概念验证（2-4 周）', bold=True)
    add_bullet(doc, '解决方案脚手架（ArchSkin.sln、空项目）')
    add_bullet(doc, 'ArchSkin.Shell 启动并显示空白面板')
    add_bullet(doc, 'ArchSkin.ThemeSync 从 Arch 拉取 papirus-icon-theme 到本地缓存')
    add_bullet(doc, '转换 1 套图标为 WinUI 资源')
    add_bullet(doc, 'ArchSkin.Core 隐藏桌面图标')
    add_para(doc, '退出标准：能看到通过 WinUI 渲染的 Arch 图标，桌面图标已隐藏。')

    add_para(doc, 'M1：MVP Shell（2-3 个月）', bold=True)
    add_bullet(doc, '面板：时钟、系统托盘、应用菜单')
    add_bullet(doc, 'Dock：显示运行中应用')
    add_bullet(doc, '启动器：搜索+网格')
    add_bullet(doc, '通知中心：只读显示 Toast')
    add_bullet(doc, '设置应用：主题选择、面板开关')
    add_bullet(doc, '定时任务自启')
    add_bullet(doc, '安装器（Inno Setup）')
    add_bullet(doc, '卸载器（回滚注册表、删除定时任务）')
    add_para(doc, '退出标准：用户安装后登录，看到完整 Shell，能启动应用。')

    add_para(doc, 'M2：完整 Shell（2-3 个月）', bold=True)
    add_bullet(doc, '概览（Activities）+ 多工作区')
    add_bullet(doc, '通知操作按钮')
    add_bullet(doc, '日历弹出')
    add_bullet(doc, '快速设置（亮度、音量、网络）')
    add_bullet(doc, '壁纸引擎')
    add_bullet(doc, '桌面小部件')
    add_bullet(doc, '登录覆盖（带降级）')
    add_para(doc, '退出标准：用户能切换工作区、收发通知、登录覆盖可用。')

    add_para(doc, 'M3：多主题（1-2 个月）', bold=True)
    add_bullet(doc, 'KDE Plasma 主题（XAML + 资源）')
    add_bullet(doc, 'Xfce 主题（XAML + 资源）')
    add_bullet(doc, '设置中主题切换器')
    add_bullet(doc, '主题预览')
    add_para(doc, '退出标准：用户能实时切换 GNOME/KDE/Xfce 风格。')

    add_para(doc, 'M4：正式发布（2-3 个月）', bold=True)
    add_bullet(doc, '更新检查与自动更新')
    add_bullet(doc, '崩溃上报（可选）')
    add_bullet(doc, '用户手册、FAQ')
    add_bullet(doc, '本地化（英语、中文、德语）')
    add_bullet(doc, 'Beta 测试')
    add_bullet(doc, '商店发布（Microsoft Store + 直接下载）')
    add_para(doc, '退出标准：1.0 版本，1000+ Beta 用户，无严重缺陷。')

    add_heading(doc, '5.3 总工时估算', level=2)
    add_table(
        doc,
        ['人员配置', '总周期', '说明'],
        [
            ['单人全职', '10-14 个月', '含 PoC、MVP、完整功能、多主题、GA'],
            ['2 人团队', '6-8 个月', 'UI 与桥接并行'],
            ['3 人团队', '4-6 个月', '并行开发 + 测试'],
        ],
        col_widths=[3.5, 3.5, 7.0],
    )

    doc.add_page_break()

    # ============== 六、风险分析 ==============
    add_heading(doc, '六、风险分析', level=1)

    add_heading(doc, '6.1 技术风险', level=2)
    add_table(
        doc,
        ['风险', '严重度', '缓解措施'],
        [
            ['Win11 更新破坏 Explorer 钩子', '高', '多版本测试（23H2/24H2/Insider），保留注册表回退'],
            ['微软禁用 LogonUI 覆盖', '高', '失败时静默降级，使用原生登录'],
            ['EDR/杀软误报', '中', '代码签名、提交 Defender、文档化行为'],
            ['WinUI 3 运行时缺失（Win10）', '中', '内嵌运行时或安装前检查'],
            ['热键冲突', '中', '设置界面允许重新绑定'],
            ['低配机性能', '中', '延迟加载资源、限制动画帧率'],
            ['Arch 仓库 URL 变更', '中', '镜像回退、离线缓存'],
        ],
        col_widths=[4.5, 2.5, 7.0],
    )

    add_heading(doc, '6.2 法务风险', level=2)
    add_table(
        doc,
        ['风险', '严重度', '缓解措施'],
        [
            ['GPL 资源再分发违规', '高', '保留许可头、ATTRIBUTION.md、不修改 GPL 代码'],
            ['商标侵权（Arch、GNOME、KDE）', '中', '声明"未关联、未背书、未赞助"'],
            ['用户误解"在 Windows 跑 Linux"', '低', '清晰的营销话术与 README'],
        ],
        col_widths=[5.5, 2.5, 6.0],
    )

    add_heading(doc, '6.3 市场风险', level=2)
    add_bullet(doc, '差异化定位：一站式"Arch 风"桌面套件，竞品为零散工具')
    add_bullet(doc, '教育用户："不是 Linux，是 Windows 原生 Arch 风"')
    add_bullet(doc, '避免"移植"措辞，统一使用"重写"或"复刻"')

    add_heading(doc, '6.4 团队风险', level=2)
    add_bullet(doc, '单人开发易倦怠 → 模块化设计，允许外部贡献')
    add_bullet(doc, '开源社区反馈是关键 → 早期建立 GitHub Issues + Discord')
    add_bullet(doc, '长期维护 WSL 类替代品可能受微软政策影响 → 持续跟进平台更新')

    doc.add_page_break()

    # ============== 七、附录 ==============
    add_heading(doc, '七、附录', level=1)

    add_heading(doc, '7.1 项目结构', level=2)
    add_code_block(doc,
        'A:\\Now\\ArchSkin\\\n'
        '├── docs/\n'
        '│   ├── ARCHSKIN-AI.md            # AI 参考文档\n'
        '│   └── ArchSkin_开发文档.docx     # 本文档\n'
        '├── src/\n'
        '│   ├── ArchSkin.sln\n'
        '│   ├── ArchSkin.Shell/           # 主 UI 应用\n'
        '│   ├── ArchSkin.Login/           # 登录覆盖\n'
        '│   ├── ArchSkin.Service/         # 后台服务\n'
        '│   ├── ArchSkin.ThemeSync/       # 命令行工具\n'
        '│   ├── ArchSkin.Settings/        # 设置应用\n'
        '│   ├── ArchSkin.Core/            # 共享库\n'
        '│   ├── ArchSkin.Assets/          # 内嵌资源\n'
        '│   └── ArchSkin.Tests/           # 测试\n'
        '├── installer/setup.iss           # Inno Setup\n'
        '├── scripts/                      # 开发脚本\n'
        '├── .github/workflows/            # CI/CD\n'
        '├── README.md\n'
        '├── LICENSE                       # MIT\n'
        '└── ATTRIBUTION.md                # 资源署名')

    add_heading(doc, '7.2 术语表', level=2)
    add_table(
        doc,
        ['术语', '含义'],
        [
            ['DE', 'Desktop Environment（桌面环境，如 GNOME、KDE）'],
            ['WM', 'Window Manager（窗口管理器，如 Mutter、KWin）'],
            ['Panel', '面板（顶部/底部栏）'],
            ['Dock', '浮动应用启动器（Ubuntu dash、Pop!_OS dock）'],
            ['Shell', '桌面 UI 层（GNOME Shell、Plasma Shell）'],
            ['Overview', '概览（工作区切换+窗口卡片）'],
            ['Compositor', '合成器（处理透明度、阴影）'],
            ['Toast', 'Windows 通知 API（WinRT）'],
            ['WSL', 'Windows Subsystem for Linux — 本项目不使用'],
            ['WSLg', 'WSL 图形 — 本项目不使用'],
            ['PKG', 'Arch 包文件（.pkg.tar.zst）'],
            ['PKGBUILD', 'Arch 包构建脚本 — 绝不执行'],
            ['XAML', 'WinUI 3 UI 标记语言'],
            ['MVVM', 'Model-View-ViewModel 模式'],
            ['DWM', 'Desktop Window Manager（Windows 内建合成器）'],
        ],
        col_widths=[3.5, 11.0],
    )

    add_heading(doc, '7.3 快速参考命令', level=2)
    add_code_block(doc,
        '# 首次开发环境\n'
        '.\\scripts\\bootstrap.ps1\n'
        '\n'
        '# 构建\n'
        'dotnet build src/ArchSkin.sln -c Debug\n'
        'dotnet publish src/ArchSkin.Shell/ -c Release -r win-x64\n'
        '\n'
        '# 测试\n'
        'dotnet test src/ArchSkin.Tests/\n'
        '\n'
        '# 主题同步\n'
        'archskin install papirus-icon-theme\n'
        'archskin convert papirus\n'
        'archskin verify\n'
        '\n'
        '# 安装包\n'
        '"isetup installer\\setup.iss"')

    add_heading(doc, '7.4 参考资料', level=2)
    add_bullet(doc, 'Arch Linux 包仓库：https://archlinux.org/packages/')
    add_bullet(doc, 'WinUI 3 文档：https://learn.microsoft.com/windows/apps/winui/')
    add_bullet(doc, 'Windows App SDK：https://learn.microsoft.com/windows/apps/windows-app-sdk/')
    add_bullet(doc, 'Community Toolkit：https://github.com/CommunityToolkit/Windows')
    add_bullet(doc, 'Inno Setup：https://jrsoftware.org/')
    add_bullet(doc, 'GNOME Shell 源码（参考）：https://gitlab.gnome.org/GNOME/gnome-shell')
    add_bullet(doc, 'arch-wsl 项目（仅作 INDEX 格式参考，不运行）：https://github.com/archlinux/arch-wsl')

    # Final note
    doc.add_paragraph()
    note = doc.add_paragraph()
    note.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = note.add_run('— 文档结束 —')
    r.italic = True
    r.font.color.rgb = RGBColor(0x6B, 0x7A, 0x8F)

    # Save
    doc.save(OUT)
    print(f"Saved: {OUT}")


if __name__ == '__main__':
    main()

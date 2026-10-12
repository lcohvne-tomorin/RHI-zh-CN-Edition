// MainWindow.FaqBuilder.cs — Builds the FAQ/Quick Start guide content dynamically.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;

namespace RenoDXCommander;

public sealed partial class MainWindow
{
    private bool _faqBuilt;

    /// <summary>
    /// Builds the FAQ panel content. Called once when FAQ is first opened.
    /// </summary>
    private void BuildFaqContent()
    {
        if (_faqBuilt) return;
        _faqBuilt = true;

        var panel = FaqContentPanel;
        panel.Children.Clear();

        // Welcome section
        panel.Children.Add(BuildFaqSection(
            null, "欢迎使用 RHI", "AccentTealBrush",
            "RHI 会自动检测您的游戏，让您安装 HDR mod、着色器、帧率限制器并管理 NVIDIA 驱动设置 — 全部集中在一处。以下是入门指引。",
            null));

        // Step 1: Select a Game
        panel.Children.Add(BuildFaqStep(1,
            "选择游戏",
            "您的游戏列在左侧边栏中。点击任意游戏可查看其详情和可用操作。使用筛选标签（所有游戏、已安装、Unreal 等）和搜索框查找特定游戏。",
            "提示：双击游戏可直接启动。将游戏的 .exe 文件拖到 RHI 上可添加未自动检测到的游戏。"));

        // Step 2: Install ReShade
        panel.Children.Add(BuildFaqStep(2,
            "安装 ReShade",
            "RenoDX HDR mod 需要 ReShade 才能工作。请点击游戏详情面板上的「安装 ReShade」。RHI 会自动下载并安装具备完整插件支持的正确版本。",
            "ReShade version can be changed per-game via the game overrides section — choose Stable, Nightly, Legacy, or a custom ReShade DLL.\nVulkan Games: Vulkan games (like Doom Eternal) require admin privileges. RHI will prompt for elevation when needed.\nDrag and drop ReShade preset files (.ini) onto a game to install them automatically."));

        // Step 3a: RenoDX
        panel.Children.Add(BuildFaqSection(
            "3a", "RenoDX", "AccentTealBrush",
            "RenoDX is RHI's primary HDR mod framework. The RenoDX row on each game shows what's available:\n\n• Named mods — game-specific HDR mods from the RenoDX wiki, made by the community.\n• UE-Extended — for Unreal Engine games without a named mod. Provides native HDR output via a generic UE addon.\n• Unity addon — same idea for Unity Engine games. Provides HDR output for Unity games without a named mod.\n• RTX HDR — if no mod is available, you can enable NVIDIA's driver-level SDR-to-HDR conversion via the RenoDX ⚙ cog.",
            "The cog icon next to RenoDX opens advanced settings: Peak Nits, UE-Extended toggle, and RTX HDR configuration.\nEngine.ini Settings (Unreal Engine games only): toggle HDR keys and LUT update frequency written to the game's Engine.ini for accurate HDR rendering.\nFor games not on the wiki, drag and drop an .addon64 file from the RenoDX Discord directly onto the game in RHI."));

        // Step 3b: Luma
        panel.Children.Add(BuildFaqSection(
            "3b", "Luma", "AccentTealBrush",
            "Luma is an alternative mod framework developed by Pumbo (HDR Den). Depending on the game, a Luma mod may add HDR, DLAA (Deep Learning Anti-Aliasing), game-specific rendering fixes, or a combination of these — check the Info button for details on what each mod offers.\n\n• Completed mods — named Luma mods for supported games, shown on the Luma row.\n• Generic Luma — available for all DX11 Unreal Engine games. RHI installs it automatically and applies any game-specific Engine.ini tweaks or launch arguments listed on the Luma wiki.\n\nYou can install RenoDX and Luma on the same game — but there's no guarantee they'll work together on every title. RHI will warn you the first time you try to install both.",
            "Luma requires ReShade — it will be greyed out until ReShade is installed.\nThe Luma ⚙ cog lets you toggle TAA Engine.ini settings for games that need them.\nIf a game needs a specific launch argument (e.g. -dx11), RHI sets it automatically on install and removes it on uninstall. Launch arguments only apply when the game is launched through RHI.\nCheck the Info button on the Luma row for game-specific notes — completed mods often include details on what the mod adds or any in-game settings required."));

        // Step 4: Choose Shaders
        panel.Children.Add(BuildFaqStep(4,
            "选择着色器（可选）",
            "Click the 'Shaders/Addons' button in the toolbar, then 'Global Shaders' to select shader packs. Lilium's HDR shader pack is selected by default. These apply to all games with ReShade installed.\n\nExpand any pack to pick individual shaders — the pack shows a dash when only some files are selected. Use the Profiles panel on the right to save, load, rename, and share named shader selections. Export a profile as a zip to share via Discord.",
            "Tip: Per-game shaders can be set using the Shaders button on each game's detail card (when ReShade is installed).\nTip: Use Expand All / Collapse All to browse all packs at once. Deselect All clears the whole selection. Export copies a zip of your selected shaders to the clipboard — paste directly into Discord to share."));

        // Step 5: DOF Fix
        panel.Children.Add(BuildFaqStep(5,
            "景深修复（推荐用于 UE5）",
            "景深修复将虚幻引擎 5.7 的景深渲染修复移植到运行 UE 5.0–5.6 的游戏。受支持时，它会出现在游戏详情面板的「推荐」区块中。与 RenoDX 一同安装可获得最佳效果。",
            "DOF Fix only shows on eligible UE 5.0–5.6 games — it won't appear on UE4 or UE 5.7+ titles.\nInstall and uninstall work the same as any other component."));

        // Step 6: Frame Limiters
        panel.Children.Add(BuildFaqStep(6,
            "帧率限制器（可选）",
            "ReLimiter 和 Display Commander 是为 VRR 显示器提供精确帧率限制的 ReShade 插件。可从游戏的详情面板安装。推荐 ReLimiter，因为它与 RHI 由同一团队开发。您可以在「设置」中设定目标 FPS，通过齿轮图标按游戏设定，或直接在游戏内设定。",
            "VRR cap presets by refresh rate (leave headroom below max for smooth VRR):\n• 60Hz → 59 FPS\n• 120Hz → 116 FPS\n• 144Hz → 138 FPS\n• 165Hz → 157 FPS\n• 240Hz → 224 FPS\n• 360Hz → 324 FPS\nThese values are pre-configured in RHI's FPS dropdown menus."));

        // Step 7: DLSS/Streamline
        panel.Children.Add(BuildFaqStep(7,
            "更新 DLSS / Streamline（可选）",
            "带有 DLSS 或 Streamline DLL 的游戏在详情面板上有专门的区块显示版本信息。点击即可更新到最新版本。推荐使用最新版本以获得最佳性能和画质。RHI 会自动备份原始文件，您可随时恢复。",
            "当新的 DLSS 或 Streamline 版本发布时，它们会自动出现在 RHI 中。在「设置」中设定默认 DLSS 预设。各游戏的预设可在每个游戏详情面板的 DLSS 区块中更改。"));

        // OptiScaler
        panel.Children.Add(BuildFaqSpecialSection("⚙", "AccentAmberBrush",
            "OptiScaler（可选）",
            "OptiScaler replaces DLSS/XeSS with alternative upscalers (FSR, XeSS, Intel Arc) or adds/patches frame generation on any GPU. Install it from the game's detail panel when a game has OptiScaler support.\n\nThe ⚙ cog on the OptiScaler row opens per-game settings. For the Nightly build channel these include:\n• Streamline/DLSS Enabler — deploys Streamline and DLSS Enabler to the game folder for DLSS Frame Generation support.\n• Frame Generation — set FG Input, FG Output, FG Nvngx Override, and HUD Fix.\n• Additional Settings — DLSS SR/RR preset, render scale, and flip metering.\n• Presets — save and apply named setting presets across games.\n• Engine.ini Settings (Unreal Engine games) — Dilated Motion Vectors, FSR Crash Fix, FSR-FG Swapchain, Upscaler Plugin.",
            "Switch between Stable and Nightly channels per game in the cog — Nightly adds frame generation and additional settings.\nOptiScaler and ReShade can coexist. If you see crashes with both installed, try renaming ReShade to a different DLL name using DLL Naming Overrides in the Game Overrides panel.\nGPU type and DLSS input settings (AMD/Intel only) are configured in Settings → OptiScaler Settings before installing.\nThe 'Deploy OptiScaler.ini' button in the cog redeploys your configured INI template to the game folder."));

        // Settings Overview
        panel.Children.Add(BuildFaqInfoSection("设置",
            "点击工具栏中的「设置」为所有游戏配置默认值：",
            new[]
            {
                "ReLimiter FPS：默认帧率目标",
                "DLSS 预设：默认超分预设",
                "NVIDIA Driver Settings: VSync, Low Latency, Power Mode",
                "峰值亮度：显示器用于 HDR 的峰值亮度",
                "ReShade 热键：自定义覆盖层和截图按键"
            }));

        // NVIDIA Driver Settings
        panel.Children.Add(BuildFaqInfoSection("NVIDIA Driver Settings",
            "RHI 可以管理各游戏的 NVIDIA 驱动配置文件。这些设置可直接在每个游戏的详情面板上使用：",
            new[]
            {
                "垂直同步：开启、关闭或自适应（快速同步）",
                "低延迟模式：Ultra、开启或关闭",
                "平滑运动：多帧生成（仅限单游戏）",
                "ReBAR：Resizable BAR（需要管理员权限）"
            },
            "垂直同步、低延迟和电源模式的全局默认值在「设置」中设定。各游戏的覆盖项直接在对应游戏的详情面板上配置。"));

        // Vulkan Games
        panel.Children.Add(BuildFaqSpecialSection("V", "AccentPurpleBrush",
            "Vulkan 游戏",
            "Vulkan 游戏（带「Vulkan」标记）使用安装到 C:\\ProgramData\\ReShade 的全局 ReShade 层。这需要管理员权限。当您在 Vulkan 游戏上安装 ReShade 时，RHI 会提示提权。",
            "All Vulkan games share the same ReShade installation. Updating ReShade on one Vulkan game updates it for all.\nPer-game RenoDX addons and shaders are still installed individually to each game folder."));

        // Adding Games Manually
        panel.Children.Add(BuildFaqSpecialSection("+", "AccentAmberBrush",
            "手动添加游戏",
            "如果游戏未被自动检测到，请将其 .exe 文件直接拖到 RHI 窗口上。RHI 会将其添加到您的资料库并检测其引擎类型。",
            "You can also drag .addon64 files from the RenoDX Discord onto any game to install mods not yet on the wiki."));

        // Updating Everything
        panel.Children.Add(BuildFaqSpecialSection("↑", "AccentGreenBrush",
            "正在更新全部",
            "点击工具栏中的「全部更新」，一次性更新所有游戏的所有已安装组件。包括 ReShade、RenoDX mod、ReLimiter、Display Commander 等。",
            "有可用更新的游戏会在侧边栏显示绿点。可在「设置」中配置「全部更新」包含哪些组件。"));

        // Troubleshooting - Full Refresh
        panel.Children.Add(BuildFaqSpecialSection("↻", "AccentBlueBrush",
            "疑难解答：完整刷新",
            "如果有游戏缺失、安装位置已更改，或 DLSS/Streamline 文件有增删，请使用「设置」中的「完整刷新」从头重新扫描整个资料库。",
            "完整刷新会清除缓存的游戏列表并重新检测所有内容。当普通「刷新」按钮无法识别变更时使用。"));

        // System Tray
        panel.Children.Add(BuildFaqSpecialSection("◰", "AccentPurpleBrush",
            "系统托盘",
            "RHI 可以最小化到系统托盘而非关闭。右键点击托盘图标即可快速启动最近玩过的游戏，无需打开主窗口。",
            "在「设置」中启用「关闭到系统托盘」可让 RHI 在后台运行。托盘图标可快速访问您最近玩过的游戏。RHI 在运行期间每 4 小时自动检查更新，确保一切保持最新。"));

        // Need More Help
        panel.Children.Add(BuildFaqLinksSection());
    }


    private Border BuildFaqSection(string? badge, string title, string titleBrush, string description, string? tip)
    {
        var stack = new StackPanel { Spacing = 10 };

        var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        if (badge != null)
        {
            var badgeBorder = new Border
            {
                Background = (Brush)Application.Current.Resources[titleBrush],
                CornerRadius = new CornerRadius(12),
                Width = 24,
                Height = 24
            };
            badgeBorder.Child = new TextBlock
            {
                Text = badge,
                FontSize = 12,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 10, 30, 50)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            header.Children.Add(badgeBorder);
        }
        header.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
            VerticalAlignment = VerticalAlignment.Center
        });
        stack.Children.Add(header);

        stack.Children.Add(new TextBlock
        {
            Text = description,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
            LineHeight = 20
        });

        if (tip != null)
        {
            var tipBorder = new Border
            {
                Background = (Brush)Application.Current.Resources["SurfaceToolbarBrush"],
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10),
                BorderBrush = (Brush)Application.Current.Resources["BorderSubtleBrush"],
                BorderThickness = new Thickness(1)
            };
            tipBorder.Child = new TextBlock
            {
                Text = tip,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 11,
                Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                LineHeight = 18
            };
            stack.Children.Add(tipBorder);
        }

        return new Border
        {
            Background = (Brush)Application.Current.Resources["SurfaceRaisedBrush"],
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(20, 16, 20, 16),
            BorderBrush = (Brush)Application.Current.Resources["BorderSubtleBrush"],
            BorderThickness = new Thickness(1),
            Child = stack
        };
    }


    private Border BuildFaqStep(int step, string title, string description, string? tip)
    {
        var stack = new StackPanel { Spacing = 8 };

        var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var badgeBorder = new Border
        {
            Background = (Brush)Application.Current.Resources["AccentTealBrush"],
            CornerRadius = new CornerRadius(12),
            Width = 24,
            Height = 24
        };
        badgeBorder.Child = new TextBlock
        {
            Text = step.ToString(),
            FontSize = 12,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 10, 30, 50)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        header.Children.Add(badgeBorder);
        header.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
            VerticalAlignment = VerticalAlignment.Center
        });
        stack.Children.Add(header);

        stack.Children.Add(new TextBlock
        {
            Text = description,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
            LineHeight = 20,
            Margin = new Thickness(32, 0, 0, 0)
        });

        if (tip != null)
        {
            var tipBorder = new Border
            {
                Background = (Brush)Application.Current.Resources["SurfaceToolbarBrush"],
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(32, 4, 0, 0),
                BorderBrush = (Brush)Application.Current.Resources["BorderSubtleBrush"],
                BorderThickness = new Thickness(1)
            };
            tipBorder.Child = new TextBlock
            {
                Text = tip,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 11,
                Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                LineHeight = 18
            };
            stack.Children.Add(tipBorder);
        }

        return new Border
        {
            Background = (Brush)Application.Current.Resources["SurfaceRaisedBrush"],
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(20, 16, 20, 16),
            BorderBrush = (Brush)Application.Current.Resources["BorderSubtleBrush"],
            BorderThickness = new Thickness(1),
            Child = stack
        };
    }


    private Border BuildFaqInfoSection(string title, string description, string[] bullets, string? tip = null)
    {
        var stack = new StackPanel { Spacing = 8 };

        var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var badgeBorder = new Border
        {
            Background = (Brush)Application.Current.Resources["AccentBlueBrush"],
            CornerRadius = new CornerRadius(12),
            Width = 24,
            Height = 24
        };
        badgeBorder.Child = new TextBlock
        {
            Text = "?",
            FontSize = 12,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 10, 30, 50)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        header.Children.Add(badgeBorder);
        header.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
            VerticalAlignment = VerticalAlignment.Center
        });
        stack.Children.Add(header);

        stack.Children.Add(new TextBlock
        {
            Text = description,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
            LineHeight = 20,
            Margin = new Thickness(32, 0, 0, 0)
        });

        var bulletStack = new StackPanel { Spacing = 6, Margin = new Thickness(32, 4, 0, 0) };
        foreach (var bullet in bullets)
        {
            bulletStack.Children.Add(new TextBlock
            {
                Text = $"• {bullet}",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 11,
                Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                LineHeight = 18
            });
        }
        stack.Children.Add(bulletStack);

        if (tip != null)
        {
            var tipBorder = new Border
            {
                Background = (Brush)Application.Current.Resources["SurfaceToolbarBrush"],
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(32, 4, 0, 0),
                BorderBrush = (Brush)Application.Current.Resources["BorderSubtleBrush"],
                BorderThickness = new Thickness(1)
            };
            tipBorder.Child = new TextBlock
            {
                Text = tip,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 11,
                Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                LineHeight = 18
            };
            stack.Children.Add(tipBorder);
        }

        return new Border
        {
            Background = (Brush)Application.Current.Resources["SurfaceRaisedBrush"],
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(20, 16, 20, 16),
            BorderBrush = (Brush)Application.Current.Resources["BorderSubtleBrush"],
            BorderThickness = new Thickness(1),
            Child = stack
        };
    }


    private Border BuildFaqSpecialSection(string badge, string badgeBrush, string title, string description, string? tip)
    {
        var stack = new StackPanel { Spacing = 8 };

        var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var badgeBorder = new Border
        {
            Background = (Brush)Application.Current.Resources[badgeBrush],
            CornerRadius = new CornerRadius(12),
            Width = 24,
            Height = 24
        };
        var badgeFg = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 10, 30, 50));
        badgeBorder.Child = new TextBlock
        {
            Text = badge,
            FontSize = badge.Length > 1 ? 11 : 14,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = badgeFg,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        header.Children.Add(badgeBorder);
        header.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
            VerticalAlignment = VerticalAlignment.Center
        });
        stack.Children.Add(header);

        stack.Children.Add(new TextBlock
        {
            Text = description,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
            LineHeight = 20,
            Margin = new Thickness(32, 0, 0, 0)
        });

        if (tip != null)
        {
            var tipBorder = new Border
            {
                Background = (Brush)Application.Current.Resources["SurfaceToolbarBrush"],
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(32, 4, 0, 0),
                BorderBrush = (Brush)Application.Current.Resources["BorderSubtleBrush"],
                BorderThickness = new Thickness(1)
            };
            tipBorder.Child = new TextBlock
            {
                Text = tip,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 11,
                Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                LineHeight = 18
            };
            stack.Children.Add(tipBorder);
        }

        return new Border
        {
            Background = (Brush)Application.Current.Resources["SurfaceRaisedBrush"],
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(20, 16, 20, 16),
            BorderBrush = (Brush)Application.Current.Resources["BorderSubtleBrush"],
            BorderThickness = new Thickness(1),
            Child = stack
        };
    }


    private Border BuildFaqLinksSection()
    {
        var stack = new StackPanel { Spacing = 10 };

        stack.Children.Add(new TextBlock
        {
            Text = "需要更多帮助？",
            FontSize = 14,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = (Brush)Application.Current.Resources["AccentTealBrush"]
        });

        stack.Children.Add(new TextBlock
        {
            Text = "我们提供 Discord 支持 — 加入社区获取帮助、mod 更新和讨论。",
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
            LineHeight = 20
        });

        var linksStack = new StackPanel { Spacing = 6 };

        var discordLink = new HyperlinkButton
        {
            NavigateUri = new Uri("https://discord.gg/ultraplus"),
            Padding = new Thickness(0)
        };
        discordLink.Content = new TextBlock
        {
            Text = "加入 Ultra+ Discord（主社区）",
            Foreground = (Brush)Application.Current.Resources["AccentBlueBrush"],
            FontSize = 12
        };
        linksStack.Children.Add(discordLink);

        var renodxDiscordLink = new HyperlinkButton
        {
            NavigateUri = new Uri("https://discord.gg/renodx"),
            Padding = new Thickness(0)
        };
        renodxDiscordLink.Content = new TextBlock
        {
            Text = "RenoDX Discord（Mod 开发）",
            Foreground = (Brush)Application.Current.Resources["AccentBlueBrush"],
            FontSize = 12
        };
        linksStack.Children.Add(renodxDiscordLink);

        var wikiLink = new HyperlinkButton
        {
            NavigateUri = new Uri("https://github.com/clshortfuse/renodx/wiki/Mods"),
            Padding = new Thickness(0)
        };
        wikiLink.Content = new TextBlock
        {
            Text = "浏览 RenoDX Mod Wiki",
            Foreground = (Brush)Application.Current.Resources["AccentBlueBrush"],
            FontSize = 12
        };
        linksStack.Children.Add(wikiLink);

        var githubLink = new HyperlinkButton
        {
            NavigateUri = new Uri("https://github.com/RankFTW/RHI"),
            Padding = new Thickness(0)
        };
        githubLink.Content = new TextBlock
        {
            Text = "RHI GitHub — 报告问题或请求功能",
            Foreground = (Brush)Application.Current.Resources["AccentBlueBrush"],
            FontSize = 12
        };
        linksStack.Children.Add(githubLink);

        stack.Children.Add(linksStack);

        return new Border
        {
            Background = (Brush)Application.Current.Resources["SurfaceRaisedBrush"],
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(20, 16, 20, 16),
            BorderBrush = (Brush)Application.Current.Resources["AccentTealBorderBrush"],
            BorderThickness = new Thickness(1),
            Child = stack
        };
    }
}

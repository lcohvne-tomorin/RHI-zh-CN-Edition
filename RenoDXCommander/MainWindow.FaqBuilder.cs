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
            "可通过游戏覆盖项按游戏更改 ReShade 版本 — 选择 稳定版、每夜版、旧版或自定义 ReShade DLL。\nVulkan 游戏：Vulkan 游戏（如《毁灭战士永恒》）需要管理员权限。RHI 会在需要时提示提权。\n将 ReShade 预设文件（.ini）拖放到游戏上即可自动安装。"));

        // Step 3a: RenoDX
        panel.Children.Add(BuildFaqSection(
            "3a", "RenoDX", "AccentTealBrush",
            "RenoDX 是 RHI 的主要 HDR mod 框架。每个游戏的 RenoDX 行会显示可用的内容：\n\n• 命名 mod — 来自 RenoDX wiki 的游戏专属 HDR mod，由社区制作。\n• UE-Extended — 面向没有命名 mod 的虚幻引擎游戏。通过通用 UE 插件提供原生 HDR 输出。\n• Unity 附加 — 对 Unity 引擎游戏同理。为没有命名 mod 的 Unity 游戏提供 HDR 输出。\n• RTX HDR — 如果没有可用的 mod，可通过 RenoDX 的 ⚙ 齿轮启用 NVIDIA 驱动级 SDR 转 HDR。",
            "RenoDX 旁的齿轮图标可打开高级设置：峰值亮度、UE-Extended 开关和 RTX HDR 配置。\nEngine.ini 设置（仅虚幻引擎游戏）：切换写入游戏 Engine.ini 的 HDR 键值和 LUT 更新频率，以获得准确的 HDR 渲染。\n对于 wiki 上没有的游戏，可将 RenoDX Discord 中的 .addon64 文件直接拖放到 RHI 里的游戏上。"));

        // Step 3b: Luma
        panel.Children.Add(BuildFaqSection(
            "3b", "Luma", "AccentTealBrush",
            "Luma is an alternative mod framework developed by Pumbo (HDR Den). Depending on the game, a Luma mod may add HDR, DLAA (Deep Learning Anti-Aliasing), game-specific rendering fixes, or a combination of these — check the Info button for details on what each mod offers.\n\n• Completed mods — named Luma mods for supported games, shown on the Luma row.\n• Generic Luma — available for all DX11 Unreal Engine games. RHI installs it automatically and applies any game-specific Engine.ini tweaks or launch arguments listed on the Luma wiki.\n\nYou can install RenoDX and Luma on the same game — but there's no guarantee they'll work together on every title. RHI will warn you the first time you try to install both.",
            "Luma 需要 ReShade — 未安装 ReShade 前该项为灰色不可用。\nLuma 的 ⚙ 齿轮可为其需要的游戏切换 TAA Engine.ini 设置。\n如果游戏需要特定的启动参数（例如 -dx11），RHI 会在安装时自动设置、卸载时移除。启动参数仅在通过 RHI 启动游戏时生效。\n点击 Luma 行的「信息」按钮可查看游戏专属说明 — 完整 mod 通常包含 mod 提供的功能或所需的游戏内设置。"));

        // Step 4: Choose Shaders
        panel.Children.Add(BuildFaqStep(4,
            "选择着色器（可选）",
            "点击工具栏中的「着色器/插件」，然后选择「全局着色器」来挑选着色器包。Lilium 的 HDR 着色器包默认已选中。这些会应用到所有已安装 ReShade 的游戏。\n\n展开任意包可挑选单个着色器 — 只选中部分文件时包会显示为短横线。使用右侧的「配置」面板保存、加载、重命名和分享命名的着色器选择。将配置导出为 zip 即可通过 Discord 分享。",
            "提示：当游戏安装了 ReShade 时，可使用游戏详情卡片上的「着色器」按钮设置该游戏的着色器。\n提示：使用「全部展开」/「全部折叠」可一次浏览所有包。「取消全选」会清除整个选择。「导出」会将所选着色器的 zip 复制到剪贴板 — 直接粘贴到 Discord 即可分享。"));

        // Step 5: DOF Fix
        panel.Children.Add(BuildFaqStep(5,
            "景深修复（推荐用于 UE5）",
            "景深修复将虚幻引擎 5.7 的景深渲染修复移植到运行 UE 5.0–5.6 的游戏。受支持时，它会出现在游戏详情面板的「推荐」区块中。与 RenoDX 一同安装可获得最佳效果。",
            "景深修复只在符合条件的 UE 5.0–5.6 游戏上显示 — 不会出现在 UE4 或 UE 5.7+ 的作品上。\n安装和卸载与其他组件完全相同。"));

        // Step 6: Frame Limiters
        panel.Children.Add(BuildFaqStep(6,
            "帧率限制器（可选）",
            "ReLimiter 和 Display Commander 是为 VRR 显示器提供精确帧率限制的 ReShade 插件。可从游戏的详情面板安装。推荐 ReLimiter，因为它与 RHI 由同一团队开发。您可以在「设置」中设定目标 FPS，通过齿轮图标按游戏设定，或直接在游戏内设定。",
            "按刷新率的 VRR 上限预设（在最大值下方留出余量以保证 VRR 顺畅）：\n• 60Hz → 59 FPS\n• 120Hz → 116 FPS\n• 144Hz → 138 FPS\n• 165Hz → 157 FPS\n• 240Hz → 224 FPS\n• 360Hz → 324 FPS\n这些值已在 RHI 的 FPS 下拉菜单中预先配置。"));

        // Step 7: DLSS/Streamline
        panel.Children.Add(BuildFaqStep(7,
            "更新 DLSS / Streamline（可选）",
            "带有 DLSS 或 Streamline DLL 的游戏在详情面板上有专门的区块显示版本信息。点击即可更新到最新版本。推荐使用最新版本以获得最佳性能和画质。RHI 会自动备份原始文件，您可随时恢复。",
            "当新的 DLSS 或 Streamline 版本发布时，它们会自动出现在 RHI 中。在「设置」中设定默认 DLSS 预设。各游戏的预设可在每个游戏详情面板的 DLSS 区块中更改。"));

        // OptiScaler
        panel.Children.Add(BuildFaqSpecialSection("⚙", "AccentAmberBrush",
            "OptiScaler（可选）",
            "OptiScaler replaces DLSS/XeSS with alternative upscalers (FSR, XeSS, Intel Arc) or adds/patches frame generation on any GPU. Install it from the game's detail panel when a game has OptiScaler support.\n\nThe ⚙ cog on the OptiScaler row opens per-game settings. For the Nightly build channel these include:\n• Streamline/DLSS Enabler — deploys Streamline and DLSS Enabler to the game folder for DLSS Frame Generation support.\n• Frame Generation — set FG Input, FG Output, FG Nvngx Override, and HUD Fix.\n• Additional Settings — DLSS SR/RR preset, render scale, and flip metering.\n• Presets — save and apply named setting presets across games.\n• Engine.ini Settings (Unreal Engine games) — Dilated Motion Vectors, FSR Crash Fix, FSR-FG Swapchain, Upscaler Plugin.",
            "在齿轮中按游戏切换 稳定版 和 每夜版 渠道 — 每夜版增加帧生成和额外设置。\nOptiScaler 和 ReShade 可以共存。如果同时安装后出现崩溃，请尝试使用「游戏覆盖项」面板中的 DLL 命名覆盖，将 ReShade 重命名为其他 DLL 名称。\n显卡类型和 DLSS 输入设置（仅 AMD/Intel）需在安装前于 设置 → OptiScaler 设置 中配置。\n齿轮中的「部署 OptiScaler.ini」按钮会将您配置的 INI 模板重新部署到游戏文件夹。"));

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
            "所有 Vulkan 游戏共用同一份 ReShade 安装。在某个 Vulkan 游戏上更新 ReShade 会为所有游戏更新。\n各游戏的 RenoDX 插件和着色器仍会单独安装到各自的游戏文件夹。"));

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

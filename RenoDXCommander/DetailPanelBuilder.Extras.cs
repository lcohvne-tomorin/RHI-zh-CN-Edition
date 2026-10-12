// DetailPanelBuilder.Extras.cs — Extras section: Ultimate ASI Loader and future extras.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using RenoDXCommander.Models;
using RenoDXCommander.Services;
using RenoDXCommander.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;

namespace RenoDXCommander;

public partial class DetailPanelBuilder
{
    public void BuildExtrasSection(GameCardViewModel card)
    {
        _window.ViewModel.SetLastUiAction($"BuildExtrasSection({card.GameName})");
        var __exSw = System.Diagnostics.Stopwatch.StartNew();
        _window.ExtrasPanel.Children.Clear();
        _window.ExtrasContainer.Visibility = Visibility.Visible;

        // ── Collapsible header ────────────────────────────────────────────────
        const string extrasSectionKey = "附加";
        var exSettings   = _window.ViewModel.Settings;
        bool exCollapsed = exSettings.CollapsedDetailSections.Contains(extrasSectionKey);

        var exArrow = new TextBlock
        {
            Text      = exCollapsed ? "▶" : "▼",
            FontSize  = 10,
            Foreground = UIFactory.Brush(ResourceKeys.TextTertiaryBrush),
            VerticalAlignment = VerticalAlignment.Center,
            Margin    = new Thickness(0, 0, 6, 0),
        };
        var exTitle = new TextBlock
        {
            Text       = "附加",
            FontSize   = 13,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = UIFactory.Brush(ResourceKeys.TextPrimaryBrush),
            VerticalAlignment = VerticalAlignment.Center,
        };
        var exHeaderRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 0 };
        exHeaderRow.Children.Add(MakeDragHandle(_window.ExtrasContainer));
        exHeaderRow.Children.Add(exArrow);
        exHeaderRow.Children.Add(exTitle);
        _window.ExtrasPanel.Children.Add(exHeaderRow);

        var exBody = new StackPanel { Spacing = 10, Visibility = exCollapsed ? Visibility.Collapsed : Visibility.Visible };
        _window.ExtrasPanel.Children.Add(exBody);

        exHeaderRow.PointerEntered += (s, e) => exTitle.Foreground = UIFactory.Brush(ResourceKeys.AccentTealBrush);
        exHeaderRow.PointerExited  += (s, e) => exTitle.Foreground = UIFactory.Brush(ResourceKeys.TextPrimaryBrush);
        var exHandCursor  = Microsoft.UI.Input.InputSystemCursor.Create(Microsoft.UI.Input.InputSystemCursorShape.Hand);
        var exArrowCursor = Microsoft.UI.Input.InputSystemCursor.Create(Microsoft.UI.Input.InputSystemCursorShape.Arrow);
        var exCursorProp  = DetailPanelBuilder.CursorProp;
        exHeaderRow.PointerEntered += (s, e) => exCursorProp?.SetValue(exHeaderRow, exHandCursor);
        exHeaderRow.PointerExited  += (s, e) => exCursorProp?.SetValue(exHeaderRow, exArrowCursor);

        // ── Collapsed summary (dict lookups only — no FS calls) ──────────────
        var vm = _window.ViewModel;
        var gn = card.GameName;
        var gs = card.Source ?? "";
        var installPath = card.InstallPath ?? "";
        var dgSvc = App.Services.GetRequiredService<DgVoodooService>();
        bool dgvInstalled = !string.IsNullOrEmpty(installPath) && dgSvc.IsDeployed(installPath);
        var exSummaryEntries = new List<(string, string?)>();
        if (!string.IsNullOrEmpty(vm.GetUalInstalledAs(gn, gs)))                           exSummaryEntries.Add(("ASI Loader", vm.GetUalInstalledAs(gn, gs)));
        if (vm.GetRtx40MfgInstalled(gn, gs))                                               exSummaryEntries.Add(("RTX 40 MFG", "开"));
        if (card.MfgAdaInstalled)                                                          exSummaryEntries.Add(("MFG Ada", "开"));
        if (vm.GetDlssg2030Installed(gn, gs))                                              exSummaryEntries.Add(("20/30 FG", "开"));
        if (card.IsOsInstalled)                                                             exSummaryEntries.Add(("OptiScaler", card.OsInstalledVersion));
        if (!string.IsNullOrEmpty(vm.GetDeInstalledAs(gn, gs)))                            exSummaryEntries.Add(("DLSS Enabler", "开"));
        if (card.IsDxvkInstalled)                                                           exSummaryEntries.Add(("DXVK", card.DxvkInstalledVersion ?? "开"));
        if (dgvInstalled)                                                                   exSummaryEntries.Add(("dgVoodoo2", vm.GetDgVoodooVersion(gn, gs) ?? _window.ViewModel.Manifest?.DgVoodooVersions?.Keys.FirstOrDefault()));
        var exSummary = DetailPanelBuilder.MakeSectionSummaryInlines(exSummaryEntries);
        if (exSummary != null)
        {
            exSummary.Visibility = exCollapsed ? Visibility.Visible : Visibility.Collapsed;
            exHeaderRow.Children.Add(exSummary);
        }

        exHeaderRow.PointerPressed += (s, e) =>
        {
            bool nowCollapsed = exBody.Visibility == Visibility.Visible;
            exBody.Visibility = nowCollapsed ? Visibility.Collapsed : Visibility.Visible;
            exArrow.Text = nowCollapsed ? "▶" : "▼";
            if (exSummary != null)
                exSummary.Visibility = nowCollapsed ? Visibility.Visible : Visibility.Collapsed;
            if (nowCollapsed) exSettings.CollapsedDetailSections.Add(extrasSectionKey);
            else              exSettings.CollapsedDetailSections.Remove(extrasSectionKey);
            _window.ViewModel.SaveSettingsPublic();
        };

        // ── Ultimate ASI Loader row ───────────────────────────────────────────
        var __t0 = __exSw.ElapsedMilliseconds;
        BuildUalRow(card, exBody);
        CrashReporter.Log($"[BuildExtrasSection] UalRow: {__exSw.ElapsedMilliseconds - __t0}ms '{card.GameName}'");

        // ── OptiScaler row ────────────────────────────────────────────────────
        __t0 = __exSw.ElapsedMilliseconds;
        BuildOsRow(card, exBody);
        CrashReporter.Log($"[BuildExtrasSection] OsRow: {__exSw.ElapsedMilliseconds - __t0}ms '{card.GameName}'");

        // ── MFG Unlocks separator ─────────────────────────────────────────────
        exBody.Children.Add(MakeExtrasSeparator("MFG 解锁"));

        // ── RTX Encore row ────────────────────────────────────────────────────
        __t0 = __exSw.ElapsedMilliseconds;
        BuildRtxEncoreRow(card, exBody);
        CrashReporter.Log($"[BuildExtrasSection] RtxEncoreRow: {__exSw.ElapsedMilliseconds - __t0}ms '{card.GameName}'");

        // ── DLSS Enabler (standalone) row ─────────────────────────────────────
        __t0 = __exSw.ElapsedMilliseconds;
        BuildDlssEnablerRow(card, exBody);
        CrashReporter.Log($"[BuildExtrasSection] DlssEnablerRow: {__exSw.ElapsedMilliseconds - __t0}ms '{card.GameName}'");

        // ── 20/30 FG Unlock row ───────────────────────────────────────────────
        __t0 = __exSw.ElapsedMilliseconds;
        BuildDlssg2030Row(card, exBody);
        CrashReporter.Log($"[BuildExtrasSection] Dlssg2030Row: {__exSw.ElapsedMilliseconds - __t0}ms '{card.GameName}'");

        // ── RTX 40 MFG Unlock row ─────────────────────────────────────────────
        __t0 = __exSw.ElapsedMilliseconds;
        BuildRtx40MfgRow(card, exBody);
        CrashReporter.Log($"[BuildExtrasSection] Rtx40MfgRow: {__exSw.ElapsedMilliseconds - __t0}ms '{card.GameName}'");

        // ── MFG Ada Unlock row ────────────────────────────────────────────────
        __t0 = __exSw.ElapsedMilliseconds;
        BuildMfgAdaUnlockRow(card, exBody);
        CrashReporter.Log($"[BuildExtrasSection] MfgAdaRow: {__exSw.ElapsedMilliseconds - __t0}ms '{card.GameName}'");

        // ── API Upgrades sub-header + DXVK row ────────────────────────────────
        if (card.IsDxvkToggleVisible)
        {
            exBody.Children.Add(MakeExtrasSeparator("API 升级"));
            __t0 = __exSw.ElapsedMilliseconds;
            BuildDxvkRow(card, exBody);
            CrashReporter.Log($"[BuildExtrasSection] DxvkRow: {__exSw.ElapsedMilliseconds - __t0}ms '{card.GameName}'");
        }

        // ── dgVoodoo2 standalone row — DX9 games or already installed ─────────
        bool isDx9ForDgv = card.GraphicsApi == GraphicsApiType.DirectX9;
        // dgvInstalled already computed above for the summary
        if (isDx9ForDgv || dgvInstalled)
        {
            if (!card.IsDxvkToggleVisible) // Show separator only if DXVK row wasn't shown
                exBody.Children.Add(MakeExtrasSeparator("API 升级"));
            __t0 = __exSw.ElapsedMilliseconds;
            BuildDgVoodooRow(card, exBody);
            CrashReporter.Log($"[BuildExtrasSection] DgVoodooRow: {__exSw.ElapsedMilliseconds - __t0}ms '{card.GameName}'");
        }

        UpdateOsFeedback(card);
        __exSw.Stop();
        CrashReporter.Log($"[BuildExtrasSection] Total: {__exSw.ElapsedMilliseconds}ms '{card.GameName}'");
    }

    
    public void OnExtrasCardPropertyChanged(GameCardViewModel card, string? propertyName)
    {
        UpdateOsFeedback(card);

        if (propertyName is "IsOsInstalled" or "OsActionLabel" or "OsStatusText"
            or "OsStatusColor" or "OsDeleteVisibility" or "OsRowVisibility"
            or "OsInstallEnabled" or "OsBtnBackground" or "OsInstalledFile" or "Is32Bit")
        {
            RequestExtrasRebuild(card);
        }
    }

    
    public void UpdateOsFeedback(GameCardViewModel card)
    {
        _window.DetailOsProgress.Visibility = card.OsRowVisibility == Visibility.Visible ? card.OsProgressVisibility : Visibility.Collapsed;
        _window.DetailOsProgress.Value = card.OsProgress;
        _window.DetailOsMessage.Visibility = card.OsRowVisibility == Visibility.Visible ? card.OsMessageVisibility : Visibility.Collapsed;
        _window.DetailOsMessage.Text = card.OsActionMessage;
        _window.DetailOsMessage.Foreground = UIFactory.GetBrush(GetMessageColor(card.OsActionMessage));
    }

    
    private bool _extrasRebuildPending;

    public void RequestExtrasRebuild(GameCardViewModel card)
    {
        if (_extrasRebuildPending) return;
        _extrasRebuildPending = true;
        _window.DispatcherQueue.TryEnqueue(() =>
        {
            _extrasRebuildPending = false;
            if (_currentDetailCard != card) return;
            BuildExtrasSection(card);
        });
    }

    private static Grid MakeExtrasSeparator(string label)
    {
        var grid = new Grid { ColumnSpacing = 8, Margin = new Thickness(0, 4, 0, 2) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(350) }); // fixed — separator text is centred, exact width doesn't matter
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });

        var text = new TextBlock
        {
            Text = $"———  {label}  ———",
            FontSize = 11,
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(text, 3);
        grid.Children.Add(text);
        return grid;
    }

    private void BuildUalRow(GameCardViewModel card, StackPanel body)
    {
        _window.ViewModel.SetLastUiAction($"BuildUalRow({card.GameName})");
        var ualSvc    = _window.ViewModel.UalServiceInstance;
        var gameName  = card.GameName;
        var store     = card.Source ?? "";
        var installPath = card.InstallPath ?? "";

        // Detect current install state
        var ualRecord   = string.IsNullOrEmpty(installPath) ? null
            : _auxInstallService.FindRecord(gameName, installPath, UltimateAsiLoaderService.AddonType);
        bool isInstalled = ualRecord != null;
        string? installedAs = ualRecord?.InstalledAs;

        // Consistency guard: if the dict says installed but FindRecord finds no record,
        // the aux record was lost (e.g. Game Pass path version change). Clear the stale dict entry
        // so the collapsed header doesn't show "ASI Loader winmm.dll" when nothing is actually installed.
        if (!isInstalled && !string.IsNullOrEmpty(_window.ViewModel.GetUalInstalledAs(gameName, store)))
        {
            CrashReporter.Log($"[BuildUalRow] Clearing stale UalInstalledAs entry for '{gameName}' — dict said installed but no aux record found");
            _window.ViewModel.SetUalInstalledAs(gameName, null, store);
        }

        // Status text
        string statusText;
        string statusColor;
        if (isInstalled)
        {
            var staged = card.Is32Bit ? ualSvc.StagedVersion32 : ualSvc.StagedVersion64;
            statusText  = staged ?? "已安装";
            statusColor = "#5ECB7D";
        }
        else
        {
            statusText  = "就绪";
            statusColor = "#A0AABB";
        }

        // ── Row grid matching Components section exactly ───────────────────────
        // Col 0: label (120)  Col 1: status (80)  Col 2: Info (36)
        // Col 3: install (*)  Col 4: cog (36)     Col 5: delete (36)
        var row = new Grid { ColumnSpacing = 8 };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(PanelColW(1, 0, 350, _window.ExtrasContainer.ActualWidth)) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });

        // Col 0 — label
        var label = new TextBlock
        {
            Text = "ASI Loader",
            FontSize = 12,
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            VerticalAlignment = VerticalAlignment.Center,
        };
        ToolTipService.SetToolTip(label, "Ultimate ASI Loader — 将 .asi 插件加载到游戏进程的代理 DLL。");
        Grid.SetColumn(label, 0);
        row.Children.Add(label);

        // Col 1 — status
        var statusBlock = new TextBlock
        {
            Text = statusText,
            FontSize = 12,
            Foreground = UIFactory.GetBrush(statusColor),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalTextAlignment = Microsoft.UI.Xaml.TextAlignment.Center,
            TextDecorations = isInstalled ? Windows.UI.Text.TextDecorations.Underline : Windows.UI.Text.TextDecorations.None,
        };
        if (isInstalled)
        {
            ToolTipService.SetToolTip(statusBlock, $"Installed as: {installedAs}\nClick to open GitHub releases");
            statusBlock.PointerPressed += (s, e) =>
                _ = Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/ThirteenAG/Ultimate-ASI-Loader/releases"));
        }
        Grid.SetColumn(statusBlock, 1);
        row.Children.Add(statusBlock);

        // Col 2 — Info button (matches Components style)
        var infoBtn = new Button
        {
            Content = "信息",
            FontSize = 11,
            Padding = new Thickness(6, 2, 6, 2),
            Width = 36,
            Height = 32,
            Background = UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentBlueBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
        };
        ToolTipService.SetToolTip(infoBtn, "打开 Ultimate ASI Loader GitHub 发行版页面");
        infoBtn.Click += (s, e) =>
            _ = Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/ThirteenAG/Ultimate-ASI-Loader/releases"));
        Grid.SetColumn(infoBtn, 2);
        row.Children.Add(infoBtn);

        // Col 3 — Install button
        var installBtn = new Button
        {
            Content = isInstalled ? "↺  重新安装 ASI Loader" : "⬇  安装 ASI Loader",
            FontSize = 12,
            Height = 32,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            CornerRadius = new CornerRadius(8),
            Background = isInstalled
                ? UIFactory.GetBrush("#182840")
                : UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
            Foreground = isInstalled
                ? UIFactory.GetBrush("#7AACDD")
                : UIFactory.Brush(ResourceKeys.AccentBlueBrush),
            BorderBrush = isInstalled
                ? UIFactory.GetBrush("#2A4468")
                : UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
            BorderThickness = new Thickness(1),
        };
        ToolTipService.SetToolTip(installBtn, isInstalled
            ? $"Reinstall Ultimate ASI Loader (currently '{installedAs}')"
            : "安装 Ultimate ASI Loader — 选择要使用的 DLL 名称");
        installBtn.Click += async (s, e) =>
        {
            if (string.IsNullOrEmpty(installPath)) return;
            var chosen = await ShowUalDllPickerAsync(card, ualRecord?.InstalledAs);
            if (chosen == null) return;

            installBtn.IsEnabled = false;
            installBtn.Content = "正在安装...";
            try
            {
                var (success, hookedOriginal) = await ualSvc.InstallAsync(card, chosen);
                if (success)
                {
                    _window.ViewModel.SetUalInstalledAs(gameName, chosen, store);
                    if (hookedOriginal != null)
                    {
                        _ = DialogService.ShowSafeAsync(new ContentDialog
                        {
                            Title = "原 DLL 已链式加载",
                            Content = $"现有的 '{chosen}' 已重命名为 '{hookedOriginal}'，以便 ASI Loader 自动链式加载。",
                            CloseButtonText = "OK",
                            XamlRoot = _window.Content.XamlRoot,
                        });
                    }
                    RequestExtrasRebuild(card);
                }
                else
                {
                    installBtn.Content = "❌ 安装失败";
                }
            }
            finally { installBtn.IsEnabled = true; }
        };
        Grid.SetColumn(installBtn, 3);
        row.Children.Add(installBtn);

        // Col 4 — Cog (empty for now, matches Components cog size/position)
        var cogBtn = new Button
        {
            Width = 36,
            Height = 32,
            Padding = new Thickness(0),
            Background = UIFactory.Brush(ResourceKeys.SurfaceOverlayBrush),
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.BorderDefaultBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Content = new TextBlock { Text = "⚙", FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center },
            Tag = card,
        };
        ToolTipService.SetToolTip(cogBtn, "ASI Loader 设置（即将推出）");
        cogBtn.Click += async (s, e) =>
        {
            // Placeholder — settings dialog will be added later
            var dlg = new ContentDialog
            {
                Title = "ASI Loader 设置",
                Content = new TextBlock { Text = "暂无可用设置。", FontSize = 12 },
                CloseButtonText = "关闭",
                XamlRoot = _window.Content.XamlRoot,
            };
            await DialogService.ShowSafeAsync(dlg);
        };
        Grid.SetColumn(cogBtn, 4);
        row.Children.Add(cogBtn);

        // Col 5 — Remove button
        var removeBtn = new Button
        {
            Width = 36,
            Height = 32,
            Padding = new Thickness(0),
            Background = UIFactory.Brush(ResourceKeys.AccentRedBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentRedBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentPurpleBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Content = new TextBlock { Text = "✕", FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, Foreground = UIFactory.Brush(ResourceKeys.AccentRedBrush) },
            Opacity = isInstalled ? 1.0 : 0,
            IsHitTestVisible = isInstalled,
        };
        ToolTipService.SetToolTip(removeBtn, "从此游戏移除 Ultimate ASI Loader");
        removeBtn.Click += (s, e) =>
        {
            if (string.IsNullOrEmpty(installPath)) return;
            ualSvc.Uninstall(card);
            _window.ViewModel.SetUalInstalledAs(gameName, null, store);
            RequestExtrasRebuild(card);
        };
        Grid.SetColumn(removeBtn, 5);
        row.Children.Add(removeBtn);

        body.Children.Add(row);
    }

    private void BuildMfgAdaUnlockRow(GameCardViewModel card, StackPanel body)
    {
        _window.ViewModel.SetLastUiAction($"BuildMfgAdaUnlockRow({card.GameName})");
        var gameName    = card.GameName;
        var store       = card.Source ?? "";
        var installPath = card.InstallPath ?? "";

        const string DeployFileName  = "renodx-mfgunlock.addon64";
        var stagedPath  = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RHI", "addons", "MFG Ada Unlock.addon64");

        // Use cached state from card instead of File.Exists on UI thread
        bool isInstalled   = card.MfgAdaInstalled;
        bool rsInstalled   = card.IsRsInstalled;
        bool rtx40Conflict = card.Rtx40MfgInstalled;
        // Use AddonPackService.IsDownloaded instead of File.Exists
        bool staged        = _window.ViewModel.AddonPackServiceInstance.IsDownloaded("MFG Ada 解锁");

        var   addonVersion = AddonPackService.LoadAddonVersion("MFG Ada 解锁");
        string statusText  = isInstalled ? (string.IsNullOrEmpty(addonVersion) ? "已安装" : $"v{addonVersion}") : "就绪";
        string statusColor = isInstalled ? "#5ECB7D" : "#A0AABB";

        var row = new Grid { ColumnSpacing = 8 };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(PanelColW(1, 0, 350, _window.ExtrasContainer.ActualWidth)) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });

        // Col 0 — label
        var label = new TextBlock
        {
            Text = "MFG Ada 解锁",
            FontSize = 12,
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            VerticalAlignment = VerticalAlignment.Center,
        };
        ToolTipService.SetToolTip(label, "MFG Ada 解锁 — 在 RTX 40 系列显卡上解锁 DLSS 多帧生成（3x/4x+）。需要 ReShade。仅在内存中生效，不修改任何文件。");
        Grid.SetColumn(label, 0);
        row.Children.Add(label);

        // Col 1 — status
        var statusBlock = new TextBlock
        {
            Text = statusText,
            FontSize = 12,
            Foreground = UIFactory.GetBrush(statusColor),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalTextAlignment = Microsoft.UI.Xaml.TextAlignment.Center,
            TextDecorations = isInstalled ? Windows.UI.Text.TextDecorations.Underline : Windows.UI.Text.TextDecorations.None,
        };
        if (isInstalled)
        {
            ToolTipService.SetToolTip(statusBlock, "点击打开 GitHub 发行版");
            statusBlock.PointerPressed += (s, e) =>
                _ = Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/mavismmg/MFGAdaUnlock-RenoDx/releases"));
        }
        Grid.SetColumn(statusBlock, 1);
        row.Children.Add(statusBlock);

        // Col 2 — Info button (always blue)
        var infoBtn = new Button
        {
            Content = "信息",
            FontSize = 11,
            Padding = new Thickness(6, 2, 6, 2),
            Width = 36,
            Height = 32,
            Background = UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentBlueBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
        };
        ToolTipService.SetToolTip(infoBtn, "打开 MFG Ada 解锁 GitHub 页面");
        infoBtn.Click += (s, e) =>
            _ = Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/mavismmg/MFGAdaUnlock-RenoDx"));
        Grid.SetColumn(infoBtn, 2);
        row.Children.Add(infoBtn);

        // Col 3 — Install button
        string btnLabel;
        bool btnEnabled = true;
        if (!rsInstalled)
        {
            btnLabel   = "⚠  ReShade 必需";
            btnEnabled = false;
        }
        else if (rtx40Conflict)
        {
            btnLabel   = "⚠  RTX 40 MFG 已安装";
            btnEnabled = false;
        }
        else if (!staged)
        {
            btnLabel   = "⬇  安装 MFG Ada 解锁";
            btnEnabled = true; // will download on demand when clicked
        }
        else
        {
            btnLabel = isInstalled ? "↺  重新安装 MFG Ada 解锁" : "⬇  安装 MFG Ada 解锁";
        }

        var installBtn = new Button
        {
            Content = btnLabel,
            FontSize = 12,
            Height = 32,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            CornerRadius = new CornerRadius(8),
            Background = isInstalled ? UIFactory.GetBrush("#182840") : UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
            Foreground = isInstalled ? UIFactory.GetBrush("#7AACDD") : UIFactory.Brush(ResourceKeys.AccentBlueBrush),
            BorderBrush = isInstalled ? UIFactory.GetBrush("#2A4468") : UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
            BorderThickness = new Thickness(1),
            IsEnabled = btnEnabled,
            Opacity = btnEnabled ? 1.0 : 0.35,
        };

        if (!rsInstalled)
            ToolTipService.SetToolTip(installBtn, "请先安装 ReShade — MFG Ada 解锁需要它");
        else if (rtx40Conflict)
            ToolTipService.SetToolTip(installBtn, "RTX 40 MFG 解锁（ASI 版）已安装且存在冲突。请先移除它。");

        installBtn.Click += async (s, e) =>
        {
            if (string.IsNullOrEmpty(installPath)) return;
            installBtn.IsEnabled = false;
            installBtn.Content   = "正在下载...";
            try
            {
                // Download on demand if not yet staged
                if (!File.Exists(stagedPath))
                {
                    var addonSvc = _window.ViewModel.AddonPackServiceInstance;
                    var entry = addonSvc.AvailablePacks.FirstOrDefault(p =>
                        p.PackageName.Equals("MFG Ada 解锁", StringComparison.OrdinalIgnoreCase));
                    if (entry != null)
                        await addonSvc.DownloadAddonAsync(entry).ConfigureAwait(false);
                }

                if (!File.Exists(stagedPath))
                {
                    CrashReporter.Log("[BuildMfgAdaUnlockRow] Staged file still not found after download attempt");
                    _window.DispatcherQueue?.TryEnqueue(() => installBtn.Content = "下载失败");
                    return;
                }

                var dest = Path.Combine(installPath, DeployFileName);
                File.Copy(stagedPath, dest, overwrite: true);
                CrashReporter.Log($"[BuildMfgAdaUnlockRow] Installed '{DeployFileName}' to '{installPath}'");
                _window.DispatcherQueue?.TryEnqueue(() =>
                {
                    card.SetMfgState(mfgAdaInstalled: true, mfgAdaRtx40Conflict: card.Rtx40MfgInstalled, rtx40MfgInstalled: card.Rtx40MfgInstalled);
                    RequestExtrasRebuild(card);
                });
            }
            catch (Exception ex)
            {
                CrashReporter.Log($"[BuildMfgAdaUnlockRow] Install failed — {ex.Message}");
                _window.DispatcherQueue?.TryEnqueue(() => { installBtn.IsEnabled = true; installBtn.Content = btnLabel; });
            }
        };
        Grid.SetColumn(installBtn, 3);
        row.Children.Add(installBtn);

        // Col 4 — Cog (placeholder for consistency)
        var cogBtn = new Button
        {
            Width = 36,
            Height = 32,
            Padding = new Thickness(0),
            Background = UIFactory.Brush(ResourceKeys.SurfaceOverlayBrush),
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.BorderDefaultBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Content = new TextBlock { Text = "⚙", FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center },
        };
        ToolTipService.SetToolTip(cogBtn, "MFG Ada 解锁设置");
        cogBtn.Click += async (s, e) =>
        {
            var dlg = new ContentDialog
            {
                Title = "MFG Ada 解锁",
                Content = new TextBlock
                {
                    Text = "MFG Ada Unlock unlocks DLSS Multi Frame Generation (3x/4x and above) on RTX 40-series GPUs.\n\n" +
                           "请在游戏内通过 ReShade 覆盖层配置。",
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                },
                PrimaryButtonText = "打开 GitHub",
                CloseButtonText = "关闭",
                XamlRoot = _window.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark,
            };
            var result = await DialogService.ShowSafeAsync(dlg);
            if (result == ContentDialogResult.Primary)
                _ = Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/mavismmg/MFGAdaUnlock-RenoDx"));
        };
        Grid.SetColumn(cogBtn, 4);
        row.Children.Add(cogBtn);

        // Col 5 — Remove button
        var removeBtn = new Button
        {
            Width = 36,
            Height = 32,
            Padding = new Thickness(0),
            Background = UIFactory.Brush(ResourceKeys.AccentRedBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentRedBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentPurpleBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Content = new TextBlock { Text = "✕", FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, Foreground = UIFactory.Brush(ResourceKeys.AccentRedBrush) },
            Opacity = isInstalled ? 1.0 : 0,
            IsHitTestVisible = isInstalled,
        };
        ToolTipService.SetToolTip(removeBtn, "从此游戏移除 MFG Ada 解锁");
        removeBtn.Click += (s, e) =>
        {
            if (string.IsNullOrEmpty(installPath)) return;
            try
            {
                var dest = Path.Combine(installPath, DeployFileName);
                if (File.Exists(dest)) File.Delete(dest);
                CrashReporter.Log($"[BuildMfgAdaUnlockRow] Removed '{DeployFileName}' from '{installPath}'");

                // Also remove from addon selections so SyncGameFolder doesn't redeploy it.
                // MFG Ada Unlock installed via Extras greys out the picker toggle — the user
                // can't deselect it there, so we must clean up the selection here.
                const string PackName = "MFG Ada 解锁";

                // Global selection
                var globalAddons = _window.ViewModel.Settings.EnabledGlobalAddons;
                if (globalAddons.Remove(PackName))
                {
                    _window.ViewModel.SaveSettingsPublic();
                    CrashReporter.Log($"[BuildMfgAdaUnlockRow] Removed '{PackName}' from global addon selection");
                }

                // Per-game selection (composite key first, then legacy name-only)
                var gameNameService = _window.ViewModel.GameNameServiceInstance;
                var compositeKey = Models.GameKey.FromCard(card.GameName, card.Source).ToKey();
                bool perGameChanged = false;
                if (gameNameService.PerGameAddonSelection.TryGetValue(compositeKey, out var perGame))
                    perGameChanged = perGame.Remove(PackName);
                if (!perGameChanged && gameNameService.PerGameAddonSelection.TryGetValue(card.GameName, out var perGameLegacy))
                    perGameChanged = perGameLegacy.Remove(PackName);
                if (perGameChanged)
                {
                    _window.ViewModel.SaveSettingsPublic();
                    CrashReporter.Log($"[BuildMfgAdaUnlockRow] Removed '{PackName}' from per-game addon selection for '{card.GameName}'");
                }

                card.SetMfgState(mfgAdaInstalled: false, mfgAdaRtx40Conflict: card.Rtx40MfgInstalled, rtx40MfgInstalled: card.Rtx40MfgInstalled);
                RequestExtrasRebuild(card);
            }
            catch (Exception ex)
            {
                CrashReporter.Log($"[BuildMfgAdaUnlockRow] Remove failed — {ex.Message}");
            }
        };
        Grid.SetColumn(removeBtn, 5);
        row.Children.Add(removeBtn);

        body.Children.Add(row);
    }

    private void BuildOsRow(GameCardViewModel card, StackPanel body)
    {
        _window.ViewModel.SetLastUiAction($"BuildOsRow({card.GameName})");
        // Only add the row when it should be visible
        if (card.OsRowVisibility != Visibility.Visible) return;

        bool osGreyed = card.Is32Bit;

        // ── Row grid matching Components section exactly ───────────────────────
        // Col 0: label (120)  Col 1: status (80)  Col 2: Info (36)
        // Col 3: install (*)  Col 4: cog (36)     Col 5: delete (36)
        var row = new Grid { ColumnSpacing = 8 };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(PanelColW(1, 0, 350, _window.ExtrasContainer.ActualWidth)) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });

        // Col 0 — label
        var label = new TextBlock
        {
            Text = "OptiScaler",
            FontSize = 12,
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            VerticalAlignment = VerticalAlignment.Center,
            TextDecorations = osGreyed ? Windows.UI.Text.TextDecorations.Strikethrough : Windows.UI.Text.TextDecorations.None,
            Opacity = osGreyed ? 0.35 : 1.0,
            Tag = card,
        };
        Grid.SetColumn(label, 0);
        row.Children.Add(label);

        // Col 1 — status
        var statusBlock = new TextBlock
        {
            Text = card.OsStatusText,
            FontSize = 12,
            Foreground = UIFactory.GetBrush(card.OsStatusColor),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalTextAlignment = Microsoft.UI.Xaml.TextAlignment.Center,
            TextDecorations = osGreyed
                ? Windows.UI.Text.TextDecorations.Strikethrough
                : (card.IsOsInstalled ? Windows.UI.Text.TextDecorations.Underline : Windows.UI.Text.TextDecorations.None),
            Opacity = osGreyed ? 0.35 : 1.0,
        };
        if (card.IsOsInstalled && !osGreyed)
        {
            var osVariant = _window.ViewModel.GetOsVariant(card.GameName, card.Source ?? "");
            var osWikiUrl = osVariant == "DlssNr"
                ? "https://github.com/wilsjo2/OptiScaler-DLSSNR-PreSR-Multipass/releases"
                : osVariant == "F5"
                ? "https://github.com/janblade/OptiScaler-F5-DLSSNR-Multipass/releases"
                : "https://github.com/optiscaler/OptiScaler/wiki";
            ToolTipService.SetToolTip(statusBlock, "点击打开 OptiScaler 发行版");
            statusBlock.PointerPressed += async (s, e) =>
                await Windows.System.Launcher.LaunchUriAsync(new Uri(osWikiUrl));
        }
        Grid.SetColumn(statusBlock, 1);
        row.Children.Add(statusBlock);

        // Col 2 — Info button
        var infoBtn = new Button
        {
            Content = "信息",
            FontSize = 11,
            Padding = new Thickness(6, 2, 6, 2),
            Width = 36,
            Height = 32,
            CornerRadius = new CornerRadius(8),
            Tag = card,
            DataContext = AddonType.OptiScaler,
        };
        ApplyInfoButtonStyle(infoBtn, card, AddonType.OptiScaler);
        var osVariantForInfo = _window.ViewModel.GetOsVariant(card.GameName, card.Source ?? "");
        if (osVariantForInfo == "DlssNr" || osVariantForInfo == "F5")
        {
            var forkUrl = osVariantForInfo == "F5"
                ? "https://github.com/janblade/OptiScaler-F5-DLSSNR-Multipass"
                : "https://github.com/wilsjo2/OptiScaler-DLSSNR-PreSR-Multipass";
            ToolTipService.SetToolTip(infoBtn, $"Open {(osVariantForInfo == "F5" ? "F5 DLSSNR" : "DLSS NR")} GitHub page");
            infoBtn.Click += (s, e) => _ = Windows.System.Launcher.LaunchUriAsync(new Uri(forkUrl));
        }
        else
        {
            infoBtn.Click += (s, e) => _window.InfoButton_Click(s, e);
        }
        Grid.SetColumn(infoBtn, 2);
        row.Children.Add(infoBtn);

        // Col 3 — Install button
        var installBtn = new Button
        {
            Height = 32,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            CornerRadius = new CornerRadius(8),
            FontSize = 12,
            Background = UIFactory.GetBrush(card.OsBtnBackground),
            Foreground = UIFactory.GetBrush(card.OsBtnForeground),
            BorderBrush = UIFactory.GetBrush(card.OsBtnBorderBrush),
            BorderThickness = new Thickness(1),
            Tag = card,
            IsEnabled = card.OsInstallEnabled && !osGreyed,
            Opacity = osGreyed ? 0.35 : 1.0,
            IsHitTestVisible = !osGreyed,
        };
        installBtn.Content = WithInfoArrow(card.OsActionLabel, HasRealInfoContent(card, AddonType.OptiScaler), card.OsStatus == GameStatus.UpdateAvailable, installBtn);
        installBtn.Click += (s, e) => _window.InstallOsButton_Click(s, e);
        Grid.SetColumn(installBtn, 3);
        row.Children.Add(installBtn);

        // Col 4 — Cog (⚙) button
        var cogBtn = new Button
        {
            Width = 36,
            Height = 32,
            Padding = new Thickness(0),
            Background = UIFactory.Brush(ResourceKeys.SurfaceOverlayBrush),
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.BorderStrongBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Content = new TextBlock { Text = "⚙", FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center },
            Tag = card,
            IsEnabled = !osGreyed,
            Opacity = osGreyed ? 0.35 : 1.0,
        };
        ToolTipService.SetToolTip(cogBtn, "OptiScaler 设置");
        cogBtn.Click += (s, e) => _window.OsCogButton_ClickInternal(s, e);
        Grid.SetColumn(cogBtn, 4);
        row.Children.Add(cogBtn);

        // Col 5 — Delete (✕) button
        bool osShow = card.OsDeleteVisibility == Visibility.Visible;
        var deleteBtn = new Button
        {
            Width = 36,
            Height = 32,
            Padding = new Thickness(0),
            Background = UIFactory.Brush(ResourceKeys.AccentRedBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentRedBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentPurpleBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Content = new TextBlock { Text = "✕", FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, Foreground = UIFactory.Brush(ResourceKeys.AccentRedBrush) },
            Tag = card,
            Opacity = (osGreyed || !osShow) ? 0 : 1.0,
            IsHitTestVisible = osShow && !osGreyed,
        };
        ToolTipService.SetToolTip(deleteBtn, "移除 OptiScaler");
        deleteBtn.Click += (s, e) => _window.UninstallOsButton_Click(s, e);
        Grid.SetColumn(deleteBtn, 5);
        row.Children.Add(deleteBtn);

        body.Children.Add(row);
    }

    private async Task<string?> ShowUalDllPickerAsync(GameCardViewModel card, string? currentDllName)
    {
        if (string.IsNullOrEmpty(card.InstallPath)) return null;

        var names = card.Is32Bit
            ? UltimateAsiLoaderService.Win32Names
            : UltimateAsiLoaderService.Win64Names;

        // Collect files already in the game folder
        var existingFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            if (Directory.Exists(card.InstallPath))
                foreach (var f in Directory.GetFiles(card.InstallPath, "*.dll"))
                    existingFiles.Add(Path.GetFileName(f));
        }
        catch { }

        // RHI-managed filenames to flag as conflict
        var rhiOwned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(card.RsInstalledFile))  rhiOwned.Add(card.RsInstalledFile);
        if (!string.IsNullOrEmpty(card.OsInstalledFile))  rhiOwned.Add(card.OsInstalledFile);
        if (!string.IsNullOrEmpty(card.DcInstalledFile))  rhiOwned.Add(card.DcInstalledFile);

        string? chosen = null;

        var listPanel = new StackPanel { Spacing = 4 };
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            MaxHeight = 420,
            Content = listPanel,
        };

        foreach (var name in names)
        {
            bool isRecommended = UltimateAsiLoaderService.RecommendedNames.Contains(name, StringComparer.OrdinalIgnoreCase);
            bool isRhiConflict = UltimateAsiLoaderService.RhiConflictNames.Contains(name, StringComparer.OrdinalIgnoreCase);
            bool isTaken       = existingFiles.Contains(name) && !rhiOwned.Contains(name) && name != currentDllName;
            bool isRhiOwned    = rhiOwned.Contains(name);
            bool isCurrent     = string.Equals(name, currentDllName, StringComparison.OrdinalIgnoreCase);

            var btn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(10, 6, 10, 6),
                CornerRadius = new CornerRadius(6),
                BorderThickness = new Thickness(1),
                IsEnabled = !isRhiOwned,
                Opacity = isRhiOwned ? 0.4 : 1.0,
            };

            // Styling
            if (isCurrent)
            {
                btn.Background   = UIFactory.Brush(ResourceKeys.AccentBlueBgBrush);
                btn.BorderBrush  = UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush);
            }
            else
            {
                btn.Background  = UIFactory.Brush(ResourceKeys.SurfaceOverlayBrush);
                btn.BorderBrush = UIFactory.Brush(ResourceKeys.BorderDefaultBrush);
            }

            // Content: name + badges
            var contentRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            contentRow.Children.Add(new TextBlock
            {
                Text = name,
                FontSize = 12,
                Foreground = isTaken || isRhiOwned
                    ? UIFactory.Brush(ResourceKeys.TextTertiaryBrush)
                    : UIFactory.Brush(ResourceKeys.TextPrimaryBrush),
                VerticalAlignment = VerticalAlignment.Center,
            });

            if (isRecommended)
                contentRow.Children.Add(MakeBadge("Recommended", "#1A3A20", "#6AE87A", "#2A5A30"));
            if (isTaken)
                contentRow.Children.Add(MakeBadge("使用中", "#2A1818", "#CC6666", "#5A2828"));
            if (isRhiOwned)
                contentRow.Children.Add(MakeBadge("由 RHI 使用", "#2A1818", "#CC6666", "#5A2828"));
            if (isRhiConflict && !isRhiOwned)
                contentRow.Children.Add(MakeBadge("可能与 ReShade/系统冲突", "#2A1A10", "#CC9955", "#5A3A18"));
            if (isCurrent)
                contentRow.Children.Add(MakeBadge("当前", "#182840", "#7AACDD", "#2A4468"));

            btn.Content = contentRow;

            // Tooltip for taken files
            if (isTaken)
                ToolTipService.SetToolTip(btn, $"'{name}' 已存在于游戏文件夹中。选择它会把现有文件重命名为 '{Path.GetFileNameWithoutExtension(name)}Hooked.dll'，以便 ASI Loader 链式加载。");
            else if (isRhiOwned)
                ToolTipService.SetToolTip(btn, "此文件名已被 RHI 管理的组件（ReShade、OptiScaler 或 DC）使用。请换一个名称。");

            btn.Tag = name;
            btn.Click += (s, ev) =>
            {
                chosen = (s as Button)?.Tag as string;
                // Close the dialog by finding and closing it
                if (s is FrameworkElement fe)
                {
                    var dialog = FindParentContentDialog(fe);
                    dialog?.Hide();
                }
            };

            listPanel.Children.Add(btn);
        }

        var dialog = new ContentDialog
        {
            Title = "选择 ASI Loader DLL 名称",
            Content = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock
                    {
                        Text = "选择 ASI Loader 的文件名。大多数游戏使用 version.dll 或 winmm.dll 即可。",
                        FontSize = 11,
                        Foreground = UIFactory.Brush(ResourceKeys.TextTertiaryBrush),
                        TextWrapping = TextWrapping.Wrap,
                    },
                    scrollViewer,
                }
            },
            CloseButtonText = "取消",
            XamlRoot = _window.Content.XamlRoot,
        };

        await DialogService.ShowSafeAsync(dialog);
        return chosen;
    }

    /// <summary>Walks up the visual tree to find the parent ContentDialog.</summary>
    private static ContentDialog? FindParentContentDialog(DependencyObject element)
    {
        var parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(element);
        while (parent != null)
        {
            if (parent is ContentDialog d) return d;
            parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(parent);
        }
        return null;
    }

    private static Border MakeBadge(string text, string bg, string fg, string border)
    {
        return new Border
        {
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(5, 1, 5, 1),
            Background = UIFactory.GetBrush(bg),
            BorderBrush = UIFactory.GetBrush(border),
            BorderThickness = new Thickness(1),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                Text = text,
                FontSize = 10,
                Foreground = UIFactory.GetBrush(fg),
            },
        };
    }

    private static readonly string[] MfgDllNames =
    {
        "version.dll", "dinput8.dll", "winmm.dll", "d3d9.dll", "d3d10.dll",
        "d3d11.dll", "d3d12.dll", "dxgi.dll", "dsound.dll", "wininet.dll",
        "winhttp.dll", "binkw64.dll", "bink2w64.dll", "xinput1_1.dll",
        "xinput1_2.dll", "xinput1_3.dll", "xinput1_4.dll", "xinput9_1_0.dll",
        "xinputuap.dll",
    };

    private void BuildDlssg2030Row(GameCardViewModel card, StackPanel body)
    {
        _window.ViewModel.SetLastUiAction($"BuildDlssg2030Row({card.GameName})");
        var svc         = App.Services.GetRequiredService<Dlssg20_30Service>();
        var gameName    = card.GameName;
        var store       = card.Source ?? "";
        var installPath = card.InstallPath ?? "";

        var currentDllName = _window.ViewModel.GetDlssg2030InstalledAs(gameName, store);
        bool isInstalled   = svc.IsInstalledIn(installPath, currentDllName);
        // Use cached state from card instead of File.Exists on UI thread
        bool addonConflict = card.MfgAdaInstalled;

        string statusText  = isInstalled ? (svc.StagedVersion ?? "已安装") : "就绪";
        string statusColor = isInstalled ? "#5ECB7D" : "#A0AABB";

        var row = new Grid { ColumnSpacing = 8 };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(PanelColW(1, 0, 350, _window.ExtrasContainer.ActualWidth)) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });

        // Col 0 — label
        var label = new TextBlock
        {
            Text = "20/30 系帧生成解锁",
            FontSize = 12,
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            VerticalAlignment = VerticalAlignment.Center,
        };
        ToolTipService.SetToolTip(label, "20/30 系帧生成解锁 — 在 RTX 20 和 30 系列显卡上启用 DLSS 帧生成。仅限 D3D12。无需 ASI Loader 或 ReShade。");
        Grid.SetColumn(label, 0);
        row.Children.Add(label);

        // Col 1 — status (version SHA when installed)
        var statusBlock = new TextBlock
        {
            Text = statusText,
            FontSize = 12,
            Foreground = UIFactory.GetBrush(statusColor),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalTextAlignment = Microsoft.UI.Xaml.TextAlignment.Center,
            TextDecorations = isInstalled ? Windows.UI.Text.TextDecorations.Underline : Windows.UI.Text.TextDecorations.None,
        };
        if (isInstalled)
        {
            ToolTipService.SetToolTip(statusBlock, $"Installed as: {currentDllName}\nClick to open GitHub releases");
            statusBlock.PointerPressed += (s, e) =>
                _ = Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/sdli1995/dlssg_for_sm86"));
        }
        Grid.SetColumn(statusBlock, 1);
        row.Children.Add(statusBlock);

        // Col 2 — Info button
        var infoBtn = new Button
        {
            Content = "信息",
            FontSize = 11,
            Padding = new Thickness(6, 2, 6, 2),
            Width = 36,
            Height = 32,
            Background = UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentBlueBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
        };
        ToolTipService.SetToolTip(infoBtn, "打开 20/30 系帧生成解锁 GitHub 页面");
        infoBtn.Click += (s, e) =>
            _ = Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/sdli1995/dlssg_for_sm86"));
        Grid.SetColumn(infoBtn, 2);
        row.Children.Add(infoBtn);

        // Col 3 — Install button
        var installBtn = new Button
        {
            Content = isInstalled ? "↺  重新安装 20/30 系帧生成" : "⬇  安装 20/30 系帧生成",
            FontSize = 12,
            Height = 32,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            CornerRadius = new CornerRadius(8),
            Background = isInstalled ? UIFactory.GetBrush("#182840") : UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
            Foreground = isInstalled ? UIFactory.GetBrush("#7AACDD") : UIFactory.Brush(ResourceKeys.AccentBlueBrush),
            BorderBrush = isInstalled ? UIFactory.GetBrush("#2A4468") : UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
            BorderThickness = new Thickness(1),
        };

        if (addonConflict)
        {
            installBtn.IsEnabled = false;
            installBtn.Opacity   = 0.35;
            installBtn.Content   = "请先卸载 MFG Ada 解锁";
            ToolTipService.SetToolTip(installBtn, "MFG Ada 解锁（插件）已安装且存在冲突。请先从插件选择器中移除它。");
        }
        else
        {
            ToolTipService.SetToolTip(installBtn, isInstalled
                ? $"Reinstall 20/30 FG Unlock (currently deployed as {currentDllName})"
                : "安装 20/30 系帧生成解锁 — 以您选择的名称部署 version.dll");
        }

        installBtn.Click += async (s, e) =>
        {
            if (string.IsNullOrEmpty(installPath)) return;

            var chosen = await ShowDlssg2030DllPickerAsync(card, currentDllName);
            if (chosen == null) return;

            installBtn.IsEnabled = false;
            installBtn.Content   = "正在安装...";
            try
            {
                // If reinstalling with a different name, remove the old one first
                if (isInstalled && !string.IsNullOrEmpty(currentDllName)
                    && !currentDllName.Equals(chosen, StringComparison.OrdinalIgnoreCase))
                    svc.Uninstall(installPath, currentDllName);

                var gpuGen = _window.ViewModel.GetDlssg2030GpuGen(gameName, store);
                bool ok = await Task.Run(async () =>
                {
                    if (!svc.IsStagingReady || svc.HasUpdate)
                        await svc.EnsureStagingAsync().ConfigureAwait(false);
                    return svc.Install(installPath, chosen, gpuGen);
                });

                if (ok)
                {
                    _window.ViewModel.SetDlssg2030InstalledAs(gameName, chosen, store);
                    RequestExtrasRebuild(card);
                }
                else
                {
                    installBtn.Content   = "下载失败 — 请重试";
                    installBtn.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                CrashReporter.Log($"[BuildDlssg2030Row] Install failed — {ex.Message}");
                installBtn.Content   = "安装失败";
                installBtn.IsEnabled = true;
            }
        };
        Grid.SetColumn(installBtn, 3);
        row.Children.Add(installBtn);

        // Col 4 — Cog button
        var cogBtn = new Button
        {
            Width = 36, Height = 32, Padding = new Thickness(0),
            Background = UIFactory.Brush(ResourceKeys.SurfaceOverlayBrush),
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.BorderDefaultBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Content = new TextBlock { Text = "⚙", FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center },
        };
        ToolTipService.SetToolTip(cogBtn, "选择显卡代次");
        cogBtn.Click += async (s, e) =>
        {
            var currentGen = _window.ViewModel.GetDlssg2030GpuGen(gameName, store);
            var combo = new ComboBox
            {
                ItemsSource = new[] { Dlssg20_30Service.GpuGenRtx30, Dlssg20_30Service.GpuGenRtx20 },
                SelectedItem = currentGen,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MaxDropDownHeight = 300,
            };
            var dlg = new ContentDialog
            {
                Title = "20/30 系帧生成解锁 — 显卡代次",
                Content = new StackPanel
                {
                    Spacing = 8,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Select your GPU generation. This controls which rendering path is used.\n\n" +
                                   "RTX 30 Series (SM86) — Ampere\n" +
                                   "RTX 20 Series (SM75) — Turing\n\n" +
                                   "更改后请重新安装以应用新设置。",
                            FontSize = 12,
                            TextWrapping = TextWrapping.Wrap,
                        },
                        combo,
                    },
                },
                PrimaryButtonText = "保存",
                CloseButtonText   = "取消",
                XamlRoot          = _window.Content.XamlRoot,
                RequestedTheme    = ElementTheme.Dark,
            };
            var result = await DialogService.ShowSafeAsync(dlg);
            if (result == ContentDialogResult.Primary && combo.SelectedItem is string selected)
            {
                _window.ViewModel.SetDlssg2030GpuGen(gameName, selected, store);
                // If installed, update the INI immediately
                if (isInstalled && !string.IsNullOrEmpty(currentDllName)
                    && !string.IsNullOrEmpty(installPath))
                {
                    var iniPath = System.IO.Path.Combine(installPath, Dlssg20_30Service.IniFileName);
                    if (System.IO.File.Exists(iniPath))
                        Dlssg20_30Service.ApplyRouterToIniPublic(iniPath, selected);
                }
                RequestExtrasRebuild(card);
            }
        };
        Grid.SetColumn(cogBtn, 4);
        row.Children.Add(cogBtn);

        // Col 5 — Remove button
        var removeBtn = new Button
        {
            Width = 36, Height = 32, Padding = new Thickness(0),
            Background = UIFactory.Brush(ResourceKeys.AccentRedBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentRedBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentPurpleBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Content = new TextBlock { Text = "✕", FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, Foreground = UIFactory.Brush(ResourceKeys.AccentRedBrush) },
            Opacity = isInstalled ? 1.0 : 0,
            IsHitTestVisible = isInstalled,
        };
        ToolTipService.SetToolTip(removeBtn, "从此游戏移除 20/30 系帧生成解锁");
        removeBtn.Click += (s, e) =>
        {
            if (string.IsNullOrEmpty(installPath)) return;
            svc.Uninstall(installPath, currentDllName);
            _window.ViewModel.SetDlssg2030InstalledAs(gameName, null, store);
            RequestExtrasRebuild(card);
        };
        Grid.SetColumn(removeBtn, 5);
        row.Children.Add(removeBtn);

        body.Children.Add(row);
    }

    private async Task<string?> ShowDlssg2030DllPickerAsync(GameCardViewModel card, string? currentDllName)
    {
        if (string.IsNullOrEmpty(card.InstallPath)) return null;

        var rhiOwned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(card.RsInstalledFile))  rhiOwned.Add(card.RsInstalledFile);
        if (!string.IsNullOrEmpty(card.OsInstalledFile))  rhiOwned.Add(card.OsInstalledFile);
        if (!string.IsNullOrEmpty(card.DcInstalledFile))  rhiOwned.Add(card.DcInstalledFile);

        string? chosen = null;

        var listPanel    = new StackPanel { Spacing = 4 };
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            MaxHeight = 360,
            Content   = listPanel,
        };

        foreach (var name in Dlssg20_30Service.ProxyNames)
        {
            bool isRecommended  = string.Equals(name, "version.dll", StringComparison.OrdinalIgnoreCase);
            bool isRhiOwned     = rhiOwned.Contains(name);
            bool isCurrent      = string.Equals(name, currentDllName, StringComparison.OrdinalIgnoreCase);

            var btn = new Button
            {
                HorizontalAlignment        = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding         = new Thickness(10, 6, 10, 6),
                CornerRadius    = new CornerRadius(6),
                BorderThickness = new Thickness(1),
                IsEnabled       = !isRhiOwned,
                Opacity         = isRhiOwned ? 0.4 : 1.0,
                Background  = isCurrent ? UIFactory.Brush(ResourceKeys.AccentBlueBgBrush) : UIFactory.Brush(ResourceKeys.SurfaceOverlayBrush),
                BorderBrush = isCurrent ? UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush) : UIFactory.Brush(ResourceKeys.BorderDefaultBrush),
            };

            var contentRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            contentRow.Children.Add(new TextBlock
            {
                Text = name,
                FontSize = 12,
                Foreground = isRhiOwned
                    ? UIFactory.Brush(ResourceKeys.TextTertiaryBrush)
                    : UIFactory.Brush(ResourceKeys.TextPrimaryBrush),
                VerticalAlignment = VerticalAlignment.Center,
            });
            if (isRecommended)
                contentRow.Children.Add(MakeBadge("Recommended", "#1A3A20", "#6AE87A", "#2A5A30"));
            if (isRhiOwned)
                contentRow.Children.Add(MakeBadge("由 RHI 使用", "#2A1818", "#CC6666", "#5A2828"));
            if (isCurrent)
                contentRow.Children.Add(MakeBadge("当前", "#182840", "#7AACDD", "#2A4468"));

            btn.Content = contentRow;
            if (isRhiOwned)
                ToolTipService.SetToolTip(btn, "此文件名已被 RHI 管理的组件使用。请换一个名称。");

            btn.Tag    = name;
            btn.Click += (s, ev) =>
            {
                chosen = (s as Button)?.Tag as string;
                if (s is FrameworkElement fe)
                {
                    var dialog = FindParentContentDialog(fe);
                    dialog?.Hide();
                }
            };
            listPanel.Children.Add(btn);
        }

        var pickerDialog = new ContentDialog
        {
            Title = "选择 20/30 系帧生成解锁 DLL 名称",
            Content = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock
                    {
                        Text = "安装前请在齿轮（⚙）中选择您的显卡代次。然后选择部署 DLL 时使用的文件名。",
                        FontSize = 11,
                        Foreground = UIFactory.Brush(ResourceKeys.TextTertiaryBrush),
                        TextWrapping = TextWrapping.Wrap,
                    },
                    scrollViewer,
                }
            },
            CloseButtonText = "取消",
            XamlRoot        = _window.Content.XamlRoot,
        };

        await DialogService.ShowSafeAsync(pickerDialog);
        return chosen;
    }

    private void BuildRtx40MfgRow(GameCardViewModel card, StackPanel body)
    {
        _window.ViewModel.SetLastUiAction($"BuildRtx40MfgRow({card.GameName})");
        var mfgSvc       = App.Services.GetRequiredService<Rtx40MfgService>();
        var gameName     = card.GameName;
        var store        = card.Source ?? "";
        var installPath  = card.InstallPath ?? "";

        // Use cached state from card instead of File.Exists on UI thread
        bool isInstalled   = card.Rtx40MfgInstalled;
        bool addonConflict = card.MfgAdaInstalled;
        var currentDllName = _window.ViewModel.GetRtx40MfgInstalledAs(gameName, store);

        // Status
        string statusText  = isInstalled ? (mfgSvc.StagedVersion ?? "已安装") : "就绪";
        string statusColor = isInstalled ? "#5ECB7D" : "#A0AABB";

        var row = new Grid { ColumnSpacing = 8 };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(PanelColW(1, 0, 350, _window.ExtrasContainer.ActualWidth)) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });

        // Col 0 — label
        var label = new TextBlock
        {
            Text = "RTX 40 MFG",
            FontSize = 12,
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            VerticalAlignment = VerticalAlignment.Center,
        };
        ToolTipService.SetToolTip(label, "RTX 40 MFG 解锁 — 在 RTX 40 系列显卡上启用超过 2x（最高 6x）的 DLSS 多帧生成倍率。独立 DLL，无需 ASI Loader。");
        Grid.SetColumn(label, 0);
        row.Children.Add(label);

        // Col 1 — status
        var statusBlock = new TextBlock
        {
            Text = statusText,
            FontSize = 12,
            Foreground = UIFactory.GetBrush(statusColor),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalTextAlignment = Microsoft.UI.Xaml.TextAlignment.Center,
            TextDecorations = isInstalled ? Windows.UI.Text.TextDecorations.Underline : Windows.UI.Text.TextDecorations.None,
        };
        if (isInstalled)
        {
            ToolTipService.SetToolTip(statusBlock, $"Installed as: {currentDllName}\nClick to open GitHub releases page");
            statusBlock.PointerPressed += (s, e) =>
                _ = Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/dashdogy/RTX40MFG-Unlock/releases"));
        }
        Grid.SetColumn(statusBlock, 1);
        row.Children.Add(statusBlock);

        // Col 2 — Info button
        var infoBtn = new Button
        {
            Content = "信息",
            FontSize = 11,
            Padding = new Thickness(6, 2, 6, 2),
            Width = 36,
            Height = 32,
            Background = UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentBlueBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
        };
        ToolTipService.SetToolTip(infoBtn, "打开 RTX 40 MFG 解锁 GitHub 页面");
        infoBtn.Click += (s, e) =>
            _ = Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/dashdogy/RTX40MFG-Unlock"));
        Grid.SetColumn(infoBtn, 2);
        row.Children.Add(infoBtn);

        // Col 3 — Install button
        var installBtn = new Button
        {
            Content = isInstalled ? "↺  重新安装 RTX 40 MFG" : "⬇  安装 RTX 40 MFG",
            FontSize = 12,
            Height = 32,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            CornerRadius = new CornerRadius(8),
            Background = isInstalled
                ? UIFactory.GetBrush("#182840")
                : UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
            Foreground = isInstalled
                ? UIFactory.GetBrush("#7AACDD")
                : UIFactory.Brush(ResourceKeys.AccentBlueBrush),
            BorderBrush = isInstalled
                ? UIFactory.GetBrush("#2A4468")
                : UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
            BorderThickness = new Thickness(1),
        };

        if (addonConflict)
        {
            installBtn.IsEnabled = false;
            installBtn.Opacity   = 0.35;
            installBtn.Content   = "请先卸载 MFG Ada 解锁";
            ToolTipService.SetToolTip(installBtn, "MFG Ada 解锁（插件）已安装，并与 RTX 40 MFG 解锁冲突。请先从插件选择器中移除它。");
        }
        else
        {
            ToolTipService.SetToolTip(installBtn, isInstalled
                ? $"Reinstall RTX 40 MFG Unlock (currently deployed as {currentDllName})"
                : "安装 RTX 40 MFG 解锁 — 以您选择的名称部署 RTXMFG.dll");
        }

        installBtn.Click += async (s, e) =>
        {
            if (string.IsNullOrEmpty(installPath)) return;

            var chosen = await ShowMfgDllPickerAsync(card, currentDllName);
            if (chosen == null) return;

            installBtn.IsEnabled = false;
            installBtn.Content   = "正在安装...";
            try
            {
                // If reinstalling with a different name, remove the old one first
                if (isInstalled && !string.IsNullOrEmpty(currentDllName)
                    && !currentDllName.Equals(chosen, StringComparison.OrdinalIgnoreCase))
                    mfgSvc.Uninstall(installPath, currentDllName);

                bool ok = await Task.Run(async () =>
                {
                    if (!mfgSvc.IsStagingReady || mfgSvc.HasUpdate)
                        await mfgSvc.EnsureStagingAsync().ConfigureAwait(false);
                    return mfgSvc.Install(installPath, chosen);
                });

                if (ok)
                {
                    _window.ViewModel.SetRtx40MfgInstalledAs(gameName, chosen, store);
                    card.SetMfgState(mfgAdaInstalled: card.MfgAdaInstalled, mfgAdaRtx40Conflict: card.MfgAdaInstalled, rtx40MfgInstalled: true);
                    RequestExtrasRebuild(card);
                }
                else
                {
                    installBtn.Content   = "下载失败 — 请重试";
                    installBtn.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                CrashReporter.Log($"[BuildRtx40MfgRow] Install failed — {ex.Message}");
                installBtn.Content   = "安装失败";
                installBtn.IsEnabled = true;
            }
        };
        Grid.SetColumn(installBtn, 3);
        row.Children.Add(installBtn);

        // Col 4 — Cog button
        var cogBtn = new Button
        {
            Width = 36,
            Height = 32,
            Padding = new Thickness(0),
            Background = UIFactory.Brush(ResourceKeys.SurfaceOverlayBrush),
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.BorderDefaultBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Content = new TextBlock { Text = "⚙", FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center },
        };
        ToolTipService.SetToolTip(cogBtn, "RTX 40 MFG 设置 — 配置倍率模式");
        cogBtn.Click += async (s, e) =>
        {
            var installedNote = isInstalled ? $"\n\nCurrently installed as: {currentDllName}" : "";
            var dlg = new ContentDialog
            {
                Title = "RTX 40 MFG 设置",
                Content = new TextBlock
                {
                    Text = "Press Backspace in-game to open the RTX 40 MFG menu.\n\n" +
                           "• Follow game — uses the game's own MFG setting\n" +
                           "• Fixed 2x–6x — forces a specific multiplier\n" +
                           "• Dynamic — targets the display refresh rate or a custom FPS value\n\n" +
                           "如果帧生成超过 2x 时卡住，请尝试在 NVIDIA App 中将帧生成设为预设 B。" +
                           installedNote,
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                },
                PrimaryButtonText = "打开 GitHub",
                CloseButtonText   = "关闭",
                XamlRoot          = _window.Content.XamlRoot,
                RequestedTheme    = ElementTheme.Dark,
            };
            var result = await DialogService.ShowSafeAsync(dlg);
            if (result == ContentDialogResult.Primary)
                _ = Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/dashdogy/RTX40MFG-Unlock"));
        };
        Grid.SetColumn(cogBtn, 4);
        row.Children.Add(cogBtn);

        // Col 5 — Remove button
        var removeBtn = new Button
        {
            Width = 36,
            Height = 32,
            Padding = new Thickness(0),
            Background = UIFactory.Brush(ResourceKeys.AccentRedBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentRedBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentPurpleBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Content = new TextBlock { Text = "✕", FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, Foreground = UIFactory.Brush(ResourceKeys.AccentRedBrush) },
            Opacity = isInstalled ? 1.0 : 0,
            IsHitTestVisible = isInstalled,
        };
        ToolTipService.SetToolTip(removeBtn, "从此游戏移除 RTX 40 MFG 解锁");
        removeBtn.Click += (s, e) =>
        {
            if (string.IsNullOrEmpty(installPath)) return;
            mfgSvc.Uninstall(installPath, currentDllName);
            _window.ViewModel.SetRtx40MfgInstalledAs(gameName, null, store);
            card.SetMfgState(mfgAdaInstalled: card.MfgAdaInstalled, mfgAdaRtx40Conflict: false, rtx40MfgInstalled: false);
            RequestExtrasRebuild(card);
        };
        Grid.SetColumn(removeBtn, 5);
        row.Children.Add(removeBtn);

        body.Children.Add(row);
    }

    private static readonly string[] DeDllNames =
        { "version.dll", "dxgi.dll", "winmm.dll", "dbghelp.dll", "psapi.dll", "winhttp.dll" };

    private void BuildDlssEnablerRow(GameCardViewModel card, StackPanel body)
    {
        _window.ViewModel.SetLastUiAction($"BuildDlssEnablerRow({card.GameName})");
        var deSvc       = App.Services.GetRequiredService<DlssEnablerService>();
        var gameName    = card.GameName;
        var store       = card.Source ?? "";
        var installPath = card.InstallPath ?? "";

        var currentDllName = _window.ViewModel.GetDeInstalledAs(gameName, store);
        _window.ViewModel.SetLastUiAction($"BuildDlssEnablerRow:IsInstalled({card.GameName})");
        bool isInstalled   = deSvc.IsStandaloneInstalledIn(installPath, currentDllName);

        // Mutual exclusivity with OptiScaler
        bool osConflict = card.IsOsInstalled;

        _window.ViewModel.SetLastUiAction($"BuildDlssEnablerRow:StagedVersion({card.GameName})");
        // Status text
        string statusText  = isInstalled ? (deSvc.StagedVersion ?? "已安装") : "就绪";
        string statusColor = isInstalled ? "#5ECB7D" : "#A0AABB";

        _window.ViewModel.SetLastUiAction($"BuildDlssEnablerRow:BuildGrid({card.GameName})");
        // ── Row grid matching Components section exactly ───────────────────────
        // Col 0: label (120)  Col 1: status (80)  Col 2: Info (36)
        // Col 3: install (*)  Col 4: cog (36)     Col 5: delete (36)
        var row = new Grid { ColumnSpacing = 8 };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(PanelColW(1, 0, 350, _window.ExtrasContainer.ActualWidth)) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });

        // Col 0 — label
        var label = new TextBlock
        {
            Text = "DLSS Enabler",
            FontSize = 12,
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            VerticalAlignment = VerticalAlignment.Center,
        };
        ToolTipService.SetToolTip(label, "DLSS Enabler — 独立代理 DLL，可为原生不支持 DLSS 的游戏启用 DLSS。");
        Grid.SetColumn(label, 0);
        row.Children.Add(label);

        // Col 1 — status
        var statusBlock = new TextBlock
        {
            Text = statusText,
            FontSize = 12,
            Foreground = UIFactory.GetBrush(statusColor),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalTextAlignment = Microsoft.UI.Xaml.TextAlignment.Center,
            TextDecorations = isInstalled ? Windows.UI.Text.TextDecorations.Underline : Windows.UI.Text.TextDecorations.None,
        };
        if (isInstalled)
        {
            ToolTipService.SetToolTip(statusBlock, $"Installed as: {currentDllName}\nClick to open GitHub releases");
            statusBlock.PointerPressed += (s, e) =>
                _ = Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/RankFTW/rhi-repo/releases"));
        }
        Grid.SetColumn(statusBlock, 1);
        row.Children.Add(statusBlock);

        // Col 2 — Info button
        var infoBtn = new Button
        {
            Content = "信息",
            FontSize = 11,
            Padding = new Thickness(6, 2, 6, 2),
            Width = 36,
            Height = 32,
            Background = UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentBlueBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
        };
        ToolTipService.SetToolTip(infoBtn, "打开 DLSS Enabler Nexus 页面");
        infoBtn.Click += (s, e) =>
            _ = Windows.System.Launcher.LaunchUriAsync(new Uri("https://www.nexusmods.com/site/mods/757"));
        Grid.SetColumn(infoBtn, 2);
        row.Children.Add(infoBtn);

        // Col 3 — Install button
        string installBtnLabel;
        if (osConflict)
            installBtnLabel = "通过 OptiScaler 安装";
        else
            installBtnLabel = isInstalled ? "↺  重新安装 DLSS Enabler" : "⬇  安装 DLSS Enabler";

        var installBtn = new Button
        {
            Content = installBtnLabel,
            FontSize = 12,
            Height = 32,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            CornerRadius = new CornerRadius(8),
            Background = isInstalled
                ? UIFactory.GetBrush("#182840")
                : UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
            Foreground = isInstalled
                ? UIFactory.GetBrush("#7AACDD")
                : UIFactory.Brush(ResourceKeys.AccentBlueBrush),
            BorderBrush = isInstalled
                ? UIFactory.GetBrush("#2A4468")
                : UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
            BorderThickness = new Thickness(1),
        };

        if (osConflict)
        {
            installBtn.IsEnabled = false;
            installBtn.IsHitTestVisible = false;
            installBtn.Opacity = 0.35;
            ToolTipService.SetToolTip(installBtn, "无法与 OptiScaler 同时安装 — DLSS Enabler 已包含在 OptiScaler 内");
        }
        else
        {
            ToolTipService.SetToolTip(installBtn, isInstalled
                ? $"Reinstall DLSS Enabler (currently '{currentDllName}')"
                : "安装 DLSS Enabler — 选择要使用的 DLL 名称");

            installBtn.Click += async (s, e) =>
            {
                if (string.IsNullOrEmpty(installPath)) return;
                var chosen = await ShowDeDllPickerAsync(card, currentDllName);
                if (chosen == null) return;

                installBtn.IsEnabled = false;
                installBtn.Content   = "正在安装...";
                try
                {
                    bool ok = await deSvc.InstallStandaloneAsync(gameName, installPath, store, chosen, currentDllName);
                    if (ok)
                    {
                        _window.ViewModel.SetDeInstalledAs(gameName, chosen, store);
                        RequestExtrasRebuild(card);
                    }
                    else
                    {
                        installBtn.Content   = "安装失败";
                        installBtn.IsEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    CrashReporter.Log($"[BuildDlssEnablerRow] Install failed — {ex.Message}");
                    installBtn.Content   = "安装失败";
                    installBtn.IsEnabled = true;
                }
            };
        }
        Grid.SetColumn(installBtn, 3);
        row.Children.Add(installBtn);

        // Col 4 — Cog button
        var cogBtn = new Button
        {
            Width = 36,
            Height = 32,
            Padding = new Thickness(0),
            Background = UIFactory.Brush(ResourceKeys.SurfaceOverlayBrush),
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.BorderDefaultBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Content = new TextBlock { Text = "⚙", FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center },
            IsEnabled = !osConflict,
            IsHitTestVisible = !osConflict,
            Opacity = osConflict ? 0.35 : 1.0,
        };
        ToolTipService.SetToolTip(cogBtn, osConflict ? "无法与 OptiScaler 同时安装" : "DLSS Enabler 设置");
        cogBtn.Click += async (s, e) =>
        {
            var dlg = new ContentDialog
            {
                Title   = "DLSS Enabler 设置",
                Content = new TextBlock { Text = "没有可用设置。", FontSize = 12 },
                CloseButtonText   = "关闭",
                XamlRoot          = _window.Content.XamlRoot,
                RequestedTheme    = ElementTheme.Dark,
            };
            await DialogService.ShowSafeAsync(dlg);
        };
        Grid.SetColumn(cogBtn, 4);
        row.Children.Add(cogBtn);

        // Col 5 — Remove button (hidden when not installed)
        var removeBtn = new Button
        {
            Width = 36,
            Height = 32,
            Padding = new Thickness(0),
            Background = UIFactory.Brush(ResourceKeys.AccentRedBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentRedBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentPurpleBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Content = new TextBlock { Text = "✕", FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, Foreground = UIFactory.Brush(ResourceKeys.AccentRedBrush) },
            Opacity = isInstalled ? 1.0 : 0,
            IsHitTestVisible = isInstalled,
        };
        ToolTipService.SetToolTip(removeBtn, "从此游戏移除独立版 DLSS Enabler");
        removeBtn.Click += (s, e) =>
        {
            if (string.IsNullOrEmpty(installPath) || string.IsNullOrEmpty(currentDllName)) return;
            deSvc.UninstallStandalone(installPath, currentDllName);
            _window.ViewModel.SetDeInstalledAs(gameName, null, store);
            RequestExtrasRebuild(card);
        };
        Grid.SetColumn(removeBtn, 5);
        row.Children.Add(removeBtn);

        body.Children.Add(row);
        _window.ViewModel.SetLastUiAction($"BuildDlssEnablerRow:Done({card.GameName})");
    }

    private async Task<string?> ShowMfgDllPickerAsync(GameCardViewModel card, string? currentDllName)
    {
        if (string.IsNullOrEmpty(card.InstallPath)) return null;

        var rhiOwned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(card.RsInstalledFile))  rhiOwned.Add(card.RsInstalledFile);
        if (!string.IsNullOrEmpty(card.OsInstalledFile))  rhiOwned.Add(card.OsInstalledFile);
        if (!string.IsNullOrEmpty(card.DcInstalledFile))  rhiOwned.Add(card.DcInstalledFile);

        string? chosen = null;

        var listPanel    = new StackPanel { Spacing = 4 };
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            MaxHeight = 420,
            Content   = listPanel,
        };

        foreach (var name in MfgDllNames)
        {
            bool isRecommended  = string.Equals(name, "version.dll", StringComparison.OrdinalIgnoreCase);
            bool isRhiOwned     = rhiOwned.Contains(name);
            bool isDxgiConflict = string.Equals(name, "dxgi.dll", StringComparison.OrdinalIgnoreCase)
                               && !string.IsNullOrEmpty(card.RsInstalledFile);
            bool isCurrent      = string.Equals(name, currentDllName, StringComparison.OrdinalIgnoreCase);

            var btn = new Button
            {
                HorizontalAlignment        = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding         = new Thickness(10, 6, 10, 6),
                CornerRadius    = new CornerRadius(6),
                BorderThickness = new Thickness(1),
                IsEnabled       = !isRhiOwned,
                Opacity         = isRhiOwned ? 0.4 : 1.0,
                Background  = isCurrent ? UIFactory.Brush(ResourceKeys.AccentBlueBgBrush) : UIFactory.Brush(ResourceKeys.SurfaceOverlayBrush),
                BorderBrush = isCurrent ? UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush) : UIFactory.Brush(ResourceKeys.BorderDefaultBrush),
            };

            var contentRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            contentRow.Children.Add(new TextBlock
            {
                Text = name,
                FontSize = 12,
                Foreground = isRhiOwned
                    ? UIFactory.Brush(ResourceKeys.TextTertiaryBrush)
                    : UIFactory.Brush(ResourceKeys.TextPrimaryBrush),
                VerticalAlignment = VerticalAlignment.Center,
            });
            if (isRecommended)
                contentRow.Children.Add(MakeBadge("Recommended", "#1A3A20", "#6AE87A", "#2A5A30"));
            if (isRhiOwned)
                contentRow.Children.Add(MakeBadge("由 RHI 使用", "#2A1818", "#CC6666", "#5A2828"));
            else if (isDxgiConflict)
                contentRow.Children.Add(MakeBadge("可能与 ReShade/系统冲突", "#2A1A10", "#CC9955", "#5A3A18"));
            if (isCurrent)
                contentRow.Children.Add(MakeBadge("当前", "#182840", "#7AACDD", "#2A4468"));

            btn.Content = contentRow;

            if (isRhiOwned)
                ToolTipService.SetToolTip(btn, "此文件名已被 RHI 管理的组件使用。请换一个名称。");
            else if (isDxgiConflict)
                ToolTipService.SetToolTip(btn, "如果 ReShade 或 OptiScaler 也使用此名称，dxgi.dll 可能会产生冲突。");

            btn.Tag    = name;
            btn.Click += (s, ev) =>
            {
                chosen = (s as Button)?.Tag as string;
                if (s is FrameworkElement fe)
                {
                    var dialog = FindParentContentDialog(fe);
                    dialog?.Hide();
                }
            };

            listPanel.Children.Add(btn);
        }

        var pickerDialog = new ContentDialog
        {
            Title = "选择 RTX 40 MFG DLL 名称",
            Content = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock
                    {
                        Text = "选择 RTXMFG.dll 部署时使用的文件名。请选择游戏会较早加载的名称，并避开其他 mod 已使用的名称。",
                        FontSize = 11,
                        Foreground = UIFactory.Brush(ResourceKeys.TextTertiaryBrush),
                        TextWrapping = TextWrapping.Wrap,
                    },
                    scrollViewer,
                }
            },
            CloseButtonText = "取消",
            XamlRoot        = _window.Content.XamlRoot,
        };

        await DialogService.ShowSafeAsync(pickerDialog);
        return chosen;
    }

    private async Task<string?> ShowDeDllPickerAsync(GameCardViewModel card, string? currentDllName)
    {
        if (string.IsNullOrEmpty(card.InstallPath)) return null;

        // RHI-managed filenames to flag as conflict
        var rhiOwned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(card.RsInstalledFile))  rhiOwned.Add(card.RsInstalledFile);
        if (!string.IsNullOrEmpty(card.OsInstalledFile))  rhiOwned.Add(card.OsInstalledFile);
        if (!string.IsNullOrEmpty(card.DcInstalledFile))  rhiOwned.Add(card.DcInstalledFile);

        string? chosen = null;

        var listPanel    = new StackPanel { Spacing = 4 };
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            MaxHeight = 420,
            Content   = listPanel,
        };

        foreach (var name in DeDllNames)
        {
            bool isRecommended = string.Equals(name, "version.dll", StringComparison.OrdinalIgnoreCase);
            bool isRhiOwned    = rhiOwned.Contains(name);
            bool isDxgiConflict = string.Equals(name, "dxgi.dll", StringComparison.OrdinalIgnoreCase)
                               && !string.IsNullOrEmpty(card.RsInstalledFile);
            bool isCurrent     = string.Equals(name, currentDllName, StringComparison.OrdinalIgnoreCase);

            var btn = new Button
            {
                HorizontalAlignment        = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding         = new Thickness(10, 6, 10, 6),
                CornerRadius    = new CornerRadius(6),
                BorderThickness = new Thickness(1),
                IsEnabled       = !isRhiOwned,
                Opacity         = isRhiOwned ? 0.4 : 1.0,
            };

            if (isCurrent)
            {
                btn.Background  = UIFactory.Brush(ResourceKeys.AccentBlueBgBrush);
                btn.BorderBrush = UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush);
            }
            else
            {
                btn.Background  = UIFactory.Brush(ResourceKeys.SurfaceOverlayBrush);
                btn.BorderBrush = UIFactory.Brush(ResourceKeys.BorderDefaultBrush);
            }

            var contentRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            contentRow.Children.Add(new TextBlock
            {
                Text = name,
                FontSize = 12,
                Foreground = isRhiOwned
                    ? UIFactory.Brush(ResourceKeys.TextTertiaryBrush)
                    : UIFactory.Brush(ResourceKeys.TextPrimaryBrush),
                VerticalAlignment = VerticalAlignment.Center,
            });

            if (isRecommended)
                contentRow.Children.Add(MakeBadge("Recommended", "#1A3A20", "#6AE87A", "#2A5A30"));
            if (isRhiOwned)
                contentRow.Children.Add(MakeBadge("由 RHI 使用", "#2A1818", "#CC6666", "#5A2828"));
            else if (isDxgiConflict)
                contentRow.Children.Add(MakeBadge("可能与 ReShade/系统冲突", "#2A1A10", "#CC9955", "#5A3A18"));
            if (isCurrent)
                contentRow.Children.Add(MakeBadge("当前", "#182840", "#7AACDD", "#2A4468"));

            btn.Content = contentRow;

            if (isRhiOwned)
                ToolTipService.SetToolTip(btn, "此文件名已被 RHI 管理的组件（ReShade、OptiScaler 或 DC）使用。请换一个名称。");
            else if (isDxgiConflict)
                ToolTipService.SetToolTip(btn, "如果 ReShade 或系统组件也使用此名称，dxgi.dll 可能会产生冲突。");

            btn.Tag    = name;
            btn.Click += (s, ev) =>
            {
                chosen = (s as Button)?.Tag as string;
                if (s is FrameworkElement fe)
                {
                    var dialog = FindParentContentDialog(fe);
                    dialog?.Hide();
                }
            };

            listPanel.Children.Add(btn);
        }

        var pickerDialog = new ContentDialog
        {
            Title = "选择 DLSS Enabler DLL 名称",
            Content = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock
                    {
                        Text = "选择独立 DLSS Enabler DLL 的文件名。大多数游戏使用 version.dll 即可。",
                        FontSize = 11,
                        Foreground = UIFactory.Brush(ResourceKeys.TextTertiaryBrush),
                        TextWrapping = TextWrapping.Wrap,
                    },
                    scrollViewer,
                }
            },
            CloseButtonText = "取消",
            XamlRoot        = _window.Content.XamlRoot,
        };

        await DialogService.ShowSafeAsync(pickerDialog);
        return chosen;
    }

    private void BuildDxvkRow(GameCardViewModel card, StackPanel body)
    {
        _window.ViewModel.SetLastUiAction($"BuildDxvkRow({card.GameName})");
        // Col 0: label (120)  Col 1: status (80)  Col 2: Info (36)
        // Col 3: install (*)  Col 4: cog (36)     Col 5: delete (36)
        var row = new Grid { ColumnSpacing = 8 };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(PanelColW(1, 0, 350, _window.ExtrasContainer.ActualWidth)) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });

        bool isDisabled = !card.IsDxvkToggleEnabled;

        // Col 0 — label
        var label = new TextBlock
        {
            Text = "DXVK",
            FontSize = 12,
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = isDisabled ? 0.35 : 1.0,
            Tag = card,
        };
        if (card.DxvkToggleTooltip != null)
            ToolTipService.SetToolTip(label, card.DxvkToggleTooltip);
        Grid.SetColumn(label, 0);
        row.Children.Add(label);

        // Col 1 — status
        var statusBlock = new TextBlock
        {
            Text = card.DxvkStatusText,
            FontSize = 12,
            Foreground = UIFactory.GetBrush(card.DxvkStatusColor),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalTextAlignment = Microsoft.UI.Xaml.TextAlignment.Center,
            TextDecorations = card.IsDxvkInstalled
                ? Windows.UI.Text.TextDecorations.Underline
                : Windows.UI.Text.TextDecorations.None,
            Opacity = isDisabled ? 0.35 : 1.0,
        };
        if (card.IsDxvkInstalled && !isDisabled)
        {
            ToolTipService.SetToolTip(statusBlock, "点击打开 DXVK 发行版");
            statusBlock.PointerPressed += (s, e) => _window.DetailDxvkStatus_PointerPressed(s, e);
            statusBlock.PointerEntered += (s, e) => _window.LinkText_PointerEntered(s, e);
            statusBlock.PointerExited  += (s, e) => _window.LinkText_PointerExited(s, e);
        }
        Grid.SetColumn(statusBlock, 1);
        row.Children.Add(statusBlock);

        // Col 2 — Info button
        var infoBtn = new Button
        {
            Content = "信息",
            FontSize = 11,
            Padding = new Thickness(6, 2, 6, 2),
            Width = 36,
            Height = 32,
            CornerRadius = new CornerRadius(8),
            Background = UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentBlueBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
            BorderThickness = new Thickness(1),
            Tag = card,
            Opacity = isDisabled ? 0.35 : 1.0,
        };
        infoBtn.Click += (s, e) => _window.DxvkInfoButton_Click(s, e);
        Grid.SetColumn(infoBtn, 2);
        row.Children.Add(infoBtn);

        // Col 3 — Install button
        var installBtn = new Button
        {
            Height = 32,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            CornerRadius = new CornerRadius(8),
            FontSize = 12,
            Background = UIFactory.GetBrush(card.DxvkBtnBackground),
            Foreground = UIFactory.GetBrush(card.DxvkBtnForeground),
            BorderBrush = UIFactory.GetBrush(card.DxvkBtnBorderBrush),
            BorderThickness = new Thickness(1),
            Tag = card,
            IsEnabled = card.DxvkInstallEnabled && !isDisabled,
            Opacity = isDisabled ? 0.35 : 1.0,
            IsHitTestVisible = !isDisabled,
        };
        installBtn.Content = card.DxvkActionLabel;
        installBtn.Click += (s, e) => _window.InstallDxvkButton_Click(s, e);
        Grid.SetColumn(installBtn, 3);
        row.Children.Add(installBtn);

        // Col 4 — Cog button
        var cogBtn = new Button
        {
            Width = 36,
            Height = 32,
            Padding = new Thickness(0),
            Background = UIFactory.Brush(ResourceKeys.SurfaceOverlayBrush),
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.BorderStrongBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Content = new TextBlock { Text = "⚙", FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center },
            Tag = card,
            IsEnabled = !isDisabled,
            Opacity = isDisabled ? 0.35 : 1.0,
        };
        ToolTipService.SetToolTip(cogBtn, "DXVK 设置");
        cogBtn.Click += (s, e) => _window.DxvkCogButton_Click(s, e);
        Grid.SetColumn(cogBtn, 4);
        row.Children.Add(cogBtn);

        // Col 5 — Delete button
        bool showDelete = card.DxvkDeleteVisibility == Visibility.Visible;
        var deleteBtn = new Button
        {
            Width = 36,
            Height = 32,
            Padding = new Thickness(0),
            Background = UIFactory.Brush(ResourceKeys.AccentRedBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentRedBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentPurpleBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Content = new TextBlock { Text = "✕", FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, Foreground = UIFactory.Brush(ResourceKeys.AccentRedBrush) },
            Tag = card,
            Opacity = showDelete ? 1.0 : 0.0,
            IsHitTestVisible = showDelete,
        };
        ToolTipService.SetToolTip(deleteBtn, "移除 DXVK");
        deleteBtn.Click += (s, e) => _window.UninstallDxvkButton_Click(s, e);
        Grid.SetColumn(deleteBtn, 5);
        row.Children.Add(deleteBtn);

        body.Children.Add(row);
    }

    private void BuildDgVoodooRow(GameCardViewModel card, StackPanel body)
    {
        _window.ViewModel.SetLastUiAction($"BuildDgVoodooRow({card.GameName})");
        var gameName    = card.GameName;
        var store       = card.Source ?? "";
        var installPath = card.InstallPath ?? "";

        var dgSvc   = App.Services.GetRequiredService<DgVoodooService>();
        var manifest = _window.ViewModel.Manifest;

        bool isInstalled = dgSvc.IsDeployed(installPath);

        // Status: show deployed version — prefer stored per-game override, else latest from manifest
        string? latestVersion = manifest?.DgVoodooVersions?.Keys.FirstOrDefault();
        var activeVersion = _window.ViewModel.GetDgVoodooVersion(gameName, store) ?? latestVersion;
        string statusText  = isInstalled ? (activeVersion != null ? $"v{activeVersion}" : "已安装") : "就绪";
        string statusColor = isInstalled ? "#5ECB7D" : "#A0AABB";

        // ── Row grid ──────────────────────────────────────────────────────────
        var row = new Grid { ColumnSpacing = 8 };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(PanelColW(1, 0, 350, _window.ExtrasContainer.ActualWidth)) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });

        // Col 0 — label
        var label = new TextBlock
        {
            Text = "dgVoodoo2",
            FontSize = 12,
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            VerticalAlignment = VerticalAlignment.Center,
        };
        ToolTipService.SetToolTip(label,
            "dgVoodoo2 — translates DX9 calls to DX11, enabling ReShade compute shaders and DLSS5 Feeder on DX9 games.\nDeploys D3D9.dll + dgVoodoo.conf to the game folder.");
        Grid.SetColumn(label, 0);
        row.Children.Add(label);

        // Col 1 — status
        var statusBlock = new TextBlock
        {
            Text = statusText,
            FontSize = 12,
            Foreground = UIFactory.GetBrush(statusColor),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalTextAlignment = Microsoft.UI.Xaml.TextAlignment.Center,
            TextDecorations = isInstalled ? Windows.UI.Text.TextDecorations.Underline : Windows.UI.Text.TextDecorations.None,
        };
        if (isInstalled)
        {
            ToolTipService.SetToolTip(statusBlock, "点击打开 dgVoodoo2 发行版页面");
            statusBlock.PointerPressed += (s, e) =>
                _ = Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/dege-diosg/dgVoodoo2/releases"));
            statusBlock.PointerEntered += (s, e) => _window.LinkText_PointerEntered(s, e);
            statusBlock.PointerExited  += (s, e) => _window.LinkText_PointerExited(s, e);
        }
        Grid.SetColumn(statusBlock, 1);
        row.Children.Add(statusBlock);

        // Col 2 — Info button
        var infoBtn = new Button
        {
            Content = "信息",
            FontSize = 11,
            Padding = new Thickness(6, 2, 6, 2),
            Width = 36,
            Height = 32,
            Background = UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentBlueBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
        };
        ToolTipService.SetToolTip(infoBtn, "打开 dgVoodoo2 发行版页面");
        infoBtn.Click += (s, e) =>
            _ = Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/dege-diosg/dgVoodoo2/releases"));
        Grid.SetColumn(infoBtn, 2);
        row.Children.Add(infoBtn);

        // Col 3 — Install button
        var installBtn = new Button
        {
            Content = isInstalled ? "↺  重新部署 dgVoodoo2" : "⬇  安装 dgVoodoo2",
            FontSize = 12,
            Height = 32,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            CornerRadius = new CornerRadius(8),
            Background = isInstalled
                ? UIFactory.GetBrush("#182840")
                : UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
            Foreground = isInstalled
                ? UIFactory.GetBrush("#7AACDD")
                : UIFactory.Brush(ResourceKeys.AccentBlueBrush),
            BorderBrush = isInstalled
                ? UIFactory.GetBrush("#2A4468")
                : UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
            BorderThickness = new Thickness(1),
            IsEnabled = manifest?.DgVoodooVersions?.Count > 0,
        };
        ToolTipService.SetToolTip(installBtn,
            isInstalled ? "重新部署 dgVoodoo2 — 从暂存版本刷新 D3D9.dll 和 dgVoodoo.conf"
                        : "Install dgVoodoo2 — deploys D3D9.dll and dgVoodoo.conf to the game folder");

        installBtn.Click += async (s, ev) =>
        {
            if (string.IsNullOrEmpty(installPath) || manifest?.DgVoodooVersions == null) return;
            installBtn.IsEnabled = false;
            installBtn.Content   = "正在安装...";
            try
            {
                // Pick version: prefer stored per-game override, then latest from manifest
                KeyValuePair<string, string> versionEntry;
                var storedVersion = _window.ViewModel.GetDgVoodooVersion(gameName, store);
                if (!string.IsNullOrEmpty(storedVersion)
                    && manifest.DgVoodooVersions.TryGetValue(storedVersion, out var storedUrl))
                    versionEntry = new KeyValuePair<string, string>(storedVersion, storedUrl);
                else
                    versionEntry = manifest.DgVoodooVersions.First();

                await dgSvc.EnsureStagedAsync(versionEntry.Key, versionEntry.Value).ConfigureAwait(false);
                var deployed = await Task.Run(() =>
                    dgSvc.DeployToGame(installPath, versionEntry.Key, is64Bit: !card.Is32Bit));

                if (deployed.Count > 0)
                {
                    _window.ViewModel.SetDgVoodooStandalone(gameName, true, store);
                    CrashReporter.Log($"[BuildDgVoodooRow] dgVoodoo2 v{versionEntry.Key} deployed standalone for '{gameName}'");

                    // If ReShade was at d3d9.dll, dgVoodoo2's SentinelBackup has already saved it
                    // as d3d9.dll.original. Copy it to dxgi.dll so ReShade hooks dgVoodoo2's DX11 output.
                    // Do NOT move d3d9.dll (that's now dgVoodoo2).
                    if (card.IsRsInstalled
                        && card.RsInstalledFile?.Equals("d3d9.dll", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        CrashReporter.Log($"[BuildDgVoodooRow] Copying ReShade from D3D9.dll.original → dxgi.dll for dgVoodoo2 coexistence on '{gameName}'");
                        try
                        {
                            // D3D9.dll.original is the ReShade DLL backed up by SentinelBackup during deploy.
                            // Copy it to dxgi.dll so ReShade hooks dgVoodoo2's DX11 output.
                            // No sentinel needed on dxgi.dll — this is our file, not a foreign DLL.
                            var sentinelPath = Path.Combine(installPath, "d3d9.dll.original");
                            var dxgiPath     = Path.Combine(installPath, "dxgi.dll");
                            if (File.Exists(sentinelPath) && new FileInfo(sentinelPath).Length > 0)
                            {
                                File.Copy(sentinelPath, dxgiPath, overwrite: true);
                                // Update ReShade tracking record
                                if (card.RsRecord != null)
                                {
                                    card.RsRecord.InstalledAs = "dxgi.dll";
                                    _auxInstallService.SaveAuxRecord(card.RsRecord);
                                }
                                card.RsInstalledFile = "dxgi.dll";
                                CrashReporter.Log($"[BuildDgVoodooRow] ReShade copied to dxgi.dll for '{gameName}'");
                            }
                            else
                            {
                                CrashReporter.Log($"[BuildDgVoodooRow] D3D9.dll.original not found or is empty sentinel — skipping coexistence copy for '{gameName}'");
                            }
                        }
                        catch (Exception rsEx)
                        {
                            CrashReporter.Log($"[BuildDgVoodooRow] Failed to copy ReShade to dxgi.dll for '{gameName}' — {rsEx.Message}");
                        }
                    }

                    // Rebuild after everything (including ReShade reinstall) is complete
                    RequestExtrasRebuild(card);
                }
                else
                {
                    installBtn.Content = "❌ 部署失败";
                    installBtn.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                CrashReporter.Log($"[BuildDgVoodooRow] Install failed for '{gameName}' — {ex.Message}");
                installBtn.Content   = "❌ 失败";
                installBtn.IsEnabled = true;
            }
        };
        Grid.SetColumn(installBtn, 3);
        row.Children.Add(installBtn);

        // Col 4 — Cog
        var cogBtn = new Button
        {
            Width = 36,
            Height = 32,
            Padding = new Thickness(0),
            Background = UIFactory.Brush(ResourceKeys.SurfaceOverlayBrush),
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.BorderDefaultBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Content = new TextBlock { Text = "⚙", FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center },
        };
        ToolTipService.SetToolTip(cogBtn, "dgVoodoo2 设置");
        cogBtn.Click += async (s, ev) =>
        {
            // Build version list from manifest
            var versions = manifest?.DgVoodooVersions?.Keys.ToList() ?? new List<string>();
            if (versions.Count == 0)
            {
                var noVerDlg = new ContentDialog
                {
                    Title = "dgVoodoo2 设置",
                    Content = new TextBlock { Text = "manifest 中没有可用版本。", FontSize = 12, Foreground = UIFactory.Brush(ResourceKeys.TextPrimaryBrush) },
                    CloseButtonText = "关闭",
                    XamlRoot = _window.Content.XamlRoot,
                    RequestedTheme = ElementTheme.Dark,
                };
                await DialogService.ShowSafeAsync(noVerDlg);
                return;
            }

            var currentVersion = _window.ViewModel.GetDgVoodooVersion(gameName, store) ?? versions[0];

            var versionStack = new StackPanel { Spacing = 6 };
            versionStack.Children.Add(new TextBlock
            {
                Text = "版本",
                FontSize = 12,
                Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            });
            var versionCombo = new ComboBox
            {
                ItemsSource = versions,
                SelectedItem = versions.Contains(currentVersion) ? currentVersion : versions[0],
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MaxDropDownHeight = 300,
            };
            ToolTipService.SetToolTip(versionCombo,
                "要部署的 dgVoodoo2 版本。较旧的版本在某些游戏中可能效果更好（例如《质量效应》用 2.87.3）。");
            versionStack.Children.Add(versionCombo);

            versionStack.Children.Add(new TextBlock
            {
                Text = "更改版本会立即将 dgVoodoo2 重新部署到游戏文件夹。",
                FontSize = 11,
                Foreground = UIFactory.Brush(ResourceKeys.TextTertiaryBrush),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 0),
            });

            var cogDlg = new ContentDialog
            {
                Title = "dgVoodoo2 设置",
                Content = versionStack,
                PrimaryButtonText = "应用",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = _window.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark,
            };

            var result = await DialogService.ShowSafeAsync(cogDlg);
            if (result != ContentDialogResult.Primary) return;

            var selectedVersion = versionCombo.SelectedItem as string;
            if (string.IsNullOrEmpty(selectedVersion)) return;

            _window.ViewModel.SetDgVoodooVersion(gameName, selectedVersion, store);

            // Redeploy if already installed
            if (isInstalled && manifest?.DgVoodooVersions?.TryGetValue(selectedVersion, out var versionUrl) == true)
            {
                try
                {
                    await dgSvc.EnsureStagedAsync(selectedVersion, versionUrl).ConfigureAwait(false);
                    var redeployed = await Task.Run(() => dgSvc.DeployToGame(installPath, selectedVersion, is64Bit: !card.Is32Bit));
                    if (redeployed.Count > 0)
                        CrashReporter.Log($"[BuildDgVoodooRow] Redeployed dgVoodoo2 v{selectedVersion} for '{gameName}'");
                }
                catch (Exception ex)
                {
                    CrashReporter.Log($"[BuildDgVoodooRow] Redeploy failed — {ex.Message}");
                }
                RequestExtrasRebuild(card);
            }
        };
        Grid.SetColumn(cogBtn, 4);
        row.Children.Add(cogBtn);

        // Col 5 — Remove button (visible only when installed)
        var removeBtn = new Button
        {
            Width = 36,
            Height = 32,
            Padding = new Thickness(0),
            Background = UIFactory.Brush(ResourceKeys.AccentRedBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentRedBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentPurpleBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Content = new TextBlock { Text = "✕", FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, Foreground = UIFactory.Brush(ResourceKeys.AccentRedBrush) },
            Opacity = isInstalled ? 1.0 : 0.0,
            IsHitTestVisible = isInstalled,
        };
        ToolTipService.SetToolTip(removeBtn, "从此游戏移除 dgVoodoo2");
        removeBtn.Click += async (s, ev) =>
        {
            if (string.IsNullOrEmpty(installPath)) return;
            // Coexistence guard: don't remove if Luma or Feeder also needs dgVoodoo2
            bool lumaNeeds    = card.LumaStatus == GameStatus.Installed;
            bool feederNeeds  = File.Exists(Path.Combine(installPath, "dlss5-feed.addon32"))
                             || File.Exists(Path.Combine(installPath, "dlss5-feed.addon64"));
            if (lumaNeeds || feederNeeds)
            {
                CrashReporter.Log($"[BuildDgVoodooRow] {(lumaNeeds ? "Luma" : "Feeder")} still installed — only clearing standalone flag for '{gameName}'");
                _window.ViewModel.SetDgVoodooStandalone(gameName, false, store);
                RequestExtrasRebuild(card);
                return;
            }
            dgSvc.RemoveFromGame(installPath);
            _window.ViewModel.SetDgVoodooStandalone(gameName, false, store);
            CrashReporter.Log($"[BuildDgVoodooRow] dgVoodoo2 removed for '{gameName}'");

            // If ReShade was coexisting as dxgi.dll, dgVoodoo2's SentinelRestore already put
            // ReShade back at D3D9.dll (from D3D9.dll.original). Just delete the dxgi.dll copy
            // and update the tracking record — no full reinstall needed.
            if (card.IsRsInstalled
                && card.RsInstalledFile?.Equals("dxgi.dll", StringComparison.OrdinalIgnoreCase) == true)
            {
                try
                {
                    var dxgiPath = Path.Combine(installPath, "dxgi.dll");
                    if (File.Exists(dxgiPath)) File.Delete(dxgiPath);
                    // Also clean up any stale dxgi.dll.original left from a previous failed attempt
                    var dxgiOrigPath = Path.Combine(installPath, "dxgi.dll.original");
                    if (File.Exists(dxgiOrigPath)) File.Delete(dxgiOrigPath);
                    if (card.RsRecord != null)
                    {
                        card.RsRecord.InstalledAs = "d3d9.dll";
                        _auxInstallService.SaveAuxRecord(card.RsRecord);
                    }
                    card.RsInstalledFile = "d3d9.dll";
                    CrashReporter.Log($"[BuildDgVoodooRow] Cleaned up coexistence dxgi.dll, updated ReShade record to d3d9.dll for '{gameName}'");
                }
                catch (Exception rsEx)
                {
                    CrashReporter.Log($"[BuildDgVoodooRow] Failed to clean up dxgi.dll for '{gameName}' — {rsEx.Message}");
                }
            }

            RequestExtrasRebuild(card);
        };
        Grid.SetColumn(removeBtn, 5);
        row.Children.Add(removeBtn);

        body.Children.Add(row);
    }

    // ── RTX Encore ────────────────────────────────────────────────────────────

    private void BuildRtxEncoreRow(GameCardViewModel card, StackPanel body)
    {
        _window.ViewModel.SetLastUiAction($"BuildRtxEncoreRow({card.GameName})");
        var svc         = App.Services.GetRequiredService<RtxEncoreService>();
        var gameName    = card.GameName;
        var store       = card.Source ?? "";
        var installPath = card.InstallPath ?? "";

        var currentDllName = _window.ViewModel.GetRtxEncoreInstalledAs(gameName, store);
        bool isInstalled   = svc.IsInstalledIn(installPath, currentDllName);
        bool nrDeployed    = _window.ViewModel.GetRtxEncoreNrDllDeployed(gameName, store);

        string statusText  = isInstalled ? (svc.StagedVersion ?? "已安装") : "就绪";
        string statusColor = isInstalled ? "#5ECB7D" : "#A0AABB";

        _window.ViewModel.SetLastUiAction($"BuildRtxEncoreRow:BuildGrid({card.GameName})");

        var row = new Grid { ColumnSpacing = 8 };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(PanelColW(1, 0, 350, _window.ExtrasContainer.ActualWidth)) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });

        var label = new TextBlock { Text = "RTX Encore (Beta)", FontSize = 12, Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush), VerticalAlignment = VerticalAlignment.Center };
        ToolTipService.SetToolTip(label, "RTX Encore — 为 RTX 20/30/40 提供 X2-X6 帧生成、平滑运动和神经网络渲染。");
        Grid.SetColumn(label, 0); row.Children.Add(label);

        var statusBlock = new TextBlock { Text = statusText, FontSize = 12, Foreground = UIFactory.GetBrush(statusColor), VerticalAlignment = VerticalAlignment.Center, HorizontalTextAlignment = Microsoft.UI.Xaml.TextAlignment.Center, TextDecorations = isInstalled ? Windows.UI.Text.TextDecorations.Underline : Windows.UI.Text.TextDecorations.None };
        if (isInstalled) { ToolTipService.SetToolTip(statusBlock, $"Installed as: {currentDllName}\nClick to open GitHub releases"); statusBlock.PointerPressed += (s, e) => _ = Windows.System.Launcher.LaunchUriAsync(new Uri(RtxEncoreService.RepoUrl + "/releases")); }
        Grid.SetColumn(statusBlock, 1); row.Children.Add(statusBlock);

        var infoBtn = new Button { Content = "信息", FontSize = 11, Padding = new Thickness(6, 2, 6, 2), Width = 36, Height = 32, Background = UIFactory.Brush(ResourceKeys.AccentBlueBgBrush), Foreground = UIFactory.Brush(ResourceKeys.AccentBlueBrush), BorderBrush = UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8) };
        ToolTipService.SetToolTip(infoBtn, "打开 RTX Encore GitHub 页面");
        infoBtn.Click += (s, e) => _ = Windows.System.Launcher.LaunchUriAsync(new Uri(RtxEncoreService.RepoUrl));
        Grid.SetColumn(infoBtn, 2); row.Children.Add(infoBtn);

        string installBtnLabel = isInstalled ? "↺  重新安装 RTX Encore（测试版）" : "⬇  安装 RTX Encore（测试版）";
        var installBtn = new Button { Content = installBtnLabel, FontSize = 12, Height = 32, HorizontalAlignment = HorizontalAlignment.Stretch, CornerRadius = new CornerRadius(8), Background = isInstalled ? UIFactory.GetBrush("#182840") : UIFactory.Brush(ResourceKeys.AccentBlueBgBrush), Foreground = isInstalled ? UIFactory.GetBrush("#7AACDD") : UIFactory.Brush(ResourceKeys.AccentBlueBrush), BorderBrush = isInstalled ? UIFactory.GetBrush("#2A4468") : UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush), BorderThickness = new Thickness(1) };
        ToolTipService.SetToolTip(installBtn, isInstalled ? $"Reinstall RTX Encore (currently '{currentDllName}')" : "安装 RTX Encore — 选择要使用的 DLL 名称");
        installBtn.Click += async (s, e) =>
        {
            if (string.IsNullOrEmpty(installPath)) return;
            var chosen = await ShowRtxEncoreDllPickerAsync(card, currentDllName);
            if (chosen == null) return;
            installBtn.IsEnabled = false; installBtn.Content = "正在安装...";
            try
            {
                bool ok = await svc.InstallAsync(installPath, chosen, currentDllName);
                if (ok)
                {
                    _window.ViewModel.SetRtxEncoreInstalledAs(gameName, chosen, store);

                    // If the user had NR DLL deployment enabled in the cog, deploy it now
                    if (_window.ViewModel.GetRtxEncoreNrDllDeployed(gameName, store))
                    {
                        // Ensure 310.8.0 is cached — try with (50xx) suffix first (manifest key), then bare version
                        var dlssS = App.Services.GetRequiredService<IDlssStreamlineService>();
                        var cachedNr = svc.GetNrDllCachedPath()
                                    ?? await dlssS.EnsureSpecificDlssnrCachedAsync(RtxEncoreService.NrRequiredVer + " (50xx)").ConfigureAwait(false)
                                    ?? await dlssS.EnsureSpecificDlssnrCachedAsync(RtxEncoreService.NrRequiredVer).ConfigureAwait(false);
                        if (cachedNr != null)
                            svc.DeployNrDll(installPath);
                        else
                            CrashReporter.Log("[RtxEncoreInstall] NR DLL 310.8.0 not available — skipping NR deploy");
                    }

                    RequestExtrasRebuild(card);
                }
                else { installBtn.Content = "安装失败"; installBtn.IsEnabled = true; }
            }
            catch (Exception ex) { CrashReporter.Log($"[BuildRtxEncoreRow] Install failed — {ex.Message}"); installBtn.Content = "安装失败"; installBtn.IsEnabled = true; }
        };
        Grid.SetColumn(installBtn, 3); row.Children.Add(installBtn);

        var cogBtn = new Button { Width = 36, Height = 32, Padding = new Thickness(0), Background = UIFactory.Brush(ResourceKeys.SurfaceOverlayBrush), Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush), BorderBrush = UIFactory.Brush(ResourceKeys.BorderDefaultBrush), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8), Content = new TextBlock { Text = "⚙", FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center } };
        ToolTipService.SetToolTip(cogBtn, "RTX Encore 设置");
        cogBtn.Click += async (s, e) =>
        {
            var content = new StackPanel { Spacing = 8 };
            content.Children.Add(new TextBlock { Text = "神经网络渲染 (nvngx_dlssnr.dll)", FontSize = 13, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = UIFactory.Brush(ResourceKeys.TextPrimaryBrush) });
            content.Children.Add(new TextBlock { Text = "RTX Encore 要求 nvngx_dlssnr.dll 精确匹配 310.8.0 版本。其他版本会被拒绝。", FontSize = 11, Foreground = UIFactory.Brush(ResourceKeys.InlineDescriptionBrush), TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap });
            var nrCachedPath = svc.GetNrDllCachedPath();
            bool nrAvailable = nrCachedPath != null;

            var nrRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, Margin = new Thickness(0, 4, 0, 0) };
            nrRow.Children.Add(new TextBlock { Text = "部署 NR DLL", FontSize = 11, Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush), VerticalAlignment = VerticalAlignment.Center });
            var nrCombo = new ComboBox { FontSize = 11, MinWidth = 80 };
            nrCombo.Items.Add("否"); nrCombo.Items.Add("是");
            nrCombo.SelectedIndex = nrDeployed ? 1 : 0;
            ToolTipService.SetToolTip(nrCombo, nrAvailable
                ? "随 RTX Encore 一起部署 nvngx_dlssnr.dll 310.8.0，以支持神经网络渲染。"
                : "nvngx_dlssnr.dll 310.8.0 not yet cached — selecting Yes will download it automatically.");
            nrRow.Children.Add(nrCombo); content.Children.Add(nrRow);
            if (nrAvailable)
                content.Children.Add(new TextBlock { Text = $"来源：{nrCachedPath}", FontSize = 10, Foreground = UIFactory.Brush(ResourceKeys.TextTertiaryBrush), TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap });
            else
                content.Children.Add(new TextBlock { Text = "310.8.0 尚未下载 — 将在应用时下载。", FontSize = 10, Foreground = UIFactory.Brush(ResourceKeys.TextTertiaryBrush), TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap });
            var cogDlg = new ContentDialog { Title = "RTX Encore 设置", Content = new ScrollViewer { Content = content, MaxHeight = 480, Padding = new Thickness(0, 0, 16, 0) }, PrimaryButtonText = "应用", CloseButtonText = "取消", XamlRoot = _window.Content.XamlRoot, RequestedTheme = ElementTheme.Dark };
            var dlgResult = await DialogService.ShowSafeAsync(cogDlg);

            if (dlgResult != ContentDialogResult.Primary) return;
            bool enableNr = nrCombo.SelectedIndex == 1;

            // Always persist the preference, regardless of install state
            if (enableNr != nrDeployed)
                _window.ViewModel.SetRtxEncoreNrDllDeployed(gameName, enableNr, store);

            if (!isInstalled) return; // can't deploy to disk yet — preference is saved, deploy happens at install

            if (enableNr && !nrDeployed)
            {
                // Ensure 310.8.0 is cached — try exact match first, then with (50xx) suffix
                var dlssSvc = App.Services.GetRequiredService<IDlssStreamlineService>();
                var cachedNrPath = svc.GetNrDllCachedPath()
                                ?? await dlssSvc.EnsureSpecificDlssnrCachedAsync(RtxEncoreService.NrRequiredVer + " (50xx)").ConfigureAwait(false)
                                ?? await dlssSvc.EnsureSpecificDlssnrCachedAsync(RtxEncoreService.NrRequiredVer).ConfigureAwait(false);
                if (cachedNrPath == null)
                {
                    CrashReporter.Log("[RtxEncoreCog] 310.8.0 not available and download failed — NR DLL not deployed");
                    return;
                }
                bool ok = svc.DeployNrDll(installPath);
                if (!ok) { _window.ViewModel.SetRtxEncoreNrDllDeployed(gameName, false, store); } // revert if deploy failed
                RequestExtrasRebuild(card);
            }
            else if (!enableNr && nrDeployed) { svc.RemoveNrDll(installPath); RequestExtrasRebuild(card); }
        };
        Grid.SetColumn(cogBtn, 4); row.Children.Add(cogBtn);

        var removeBtn2 = new Button { Width = 36, Height = 32, Padding = new Thickness(0), Background = UIFactory.Brush(ResourceKeys.AccentRedBgBrush), Foreground = UIFactory.Brush(ResourceKeys.AccentRedBrush), BorderBrush = UIFactory.Brush(ResourceKeys.AccentPurpleBorderBrush), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8), Content = new TextBlock { Text = "✕", FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, Foreground = UIFactory.Brush(ResourceKeys.AccentRedBrush) }, Opacity = isInstalled ? 1.0 : 0, IsHitTestVisible = isInstalled };
        ToolTipService.SetToolTip(removeBtn2, "从此游戏移除 RTX Encore");
        removeBtn2.Click += (s, e) =>
        {
            if (string.IsNullOrEmpty(installPath) || string.IsNullOrEmpty(currentDllName)) return;
            svc.Uninstall(installPath, currentDllName, removeNrDll: nrDeployed);
            _window.ViewModel.SetRtxEncoreInstalledAs(gameName, null, store);
            if (nrDeployed) _window.ViewModel.SetRtxEncoreNrDllDeployed(gameName, false, store);
            RequestExtrasRebuild(card);
        };
        Grid.SetColumn(removeBtn2, 5); row.Children.Add(removeBtn2);

        body.Children.Add(row);
        _window.ViewModel.SetLastUiAction($"BuildRtxEncoreRow:Done({card.GameName})");
    }

    private async Task<string?> ShowRtxEncoreDllPickerAsync(GameCardViewModel card, string? currentDllName)
    {
        if (string.IsNullOrEmpty(card.InstallPath)) return null;
        var rhiConflicts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(card.RsInstalledFile)) rhiConflicts[card.RsInstalledFile] = "ReShade";
        if (!string.IsNullOrEmpty(card.OsInstalledFile)) rhiConflicts[card.OsInstalledFile] = "OptiScaler";
        if (!string.IsNullOrEmpty(card.DcInstalledFile)) rhiConflicts[card.DcInstalledFile] = "Display Commander";
        var ualName = _window.ViewModel.GetUalInstalledAs(card.GameName, card.Source ?? "");
        if (!string.IsNullOrEmpty(ualName)) rhiConflicts[ualName] = "ASI Loader";
        var mfgName = _window.ViewModel.GetRtx40MfgInstalledAs(card.GameName, card.Source ?? "");
        if (!string.IsNullOrEmpty(mfgName)) rhiConflicts[mfgName] = "RTX 40 MFG";
        var fgName = _window.ViewModel.GetDlssg2030InstalledAs(card.GameName, card.Source ?? "");
        if (!string.IsNullOrEmpty(fgName)) rhiConflicts[fgName] = "20/30 系帧生成解锁";
        if (!string.IsNullOrEmpty(currentDllName)) rhiConflicts.Remove(currentDllName);

        var onDiskConflicts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (Directory.Exists(card.InstallPath))
        {
            foreach (var name in RtxEncoreService.KnownProxyNames)
            {
                if (rhiConflicts.ContainsKey(name)) continue;
                if (string.Equals(name, currentDllName, StringComparison.OrdinalIgnoreCase)) continue;
                var diskPath = name.EndsWith(".asi", StringComparison.OrdinalIgnoreCase)
                    ? System.IO.Path.Combine(card.InstallPath, "plugins", name)
                    : System.IO.Path.Combine(card.InstallPath, name);
                if (File.Exists(diskPath))
                {
                    try { var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(diskPath); onDiskConflicts[name] = !string.IsNullOrEmpty(fvi.FileDescription) ? fvi.FileDescription : !string.IsNullOrEmpty(fvi.ProductName) ? fvi.ProductName : "existing file"; }
                    catch { onDiskConflicts[name] = "existing file"; }
                }
            }
        }

        string? chosen = null;
        var listPanel = new StackPanel { Spacing = 4 };
        var scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, MaxHeight = 480, Content = listPanel };

        foreach (var name in RtxEncoreService.KnownProxyNames)
        {
            bool isRecommended = string.Equals(name, "version.dll", StringComparison.OrdinalIgnoreCase);
            bool isAsi         = name.EndsWith(".asi", StringComparison.OrdinalIgnoreCase);
            bool isCurrent     = string.Equals(name, currentDllName, StringComparison.OrdinalIgnoreCase);
            bool isRhiOwned    = rhiConflicts.ContainsKey(name);
            bool isOnDisk      = onDiskConflicts.ContainsKey(name);
            var btn = new Button { HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Left, Padding = new Thickness(10, 6, 10, 6), CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1), IsEnabled = !isRhiOwned, Opacity = isRhiOwned ? 0.4 : 1.0, Background = isCurrent ? UIFactory.Brush(ResourceKeys.AccentBlueBgBrush) : UIFactory.Brush(ResourceKeys.SurfaceOverlayBrush), BorderBrush = isCurrent ? UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush) : UIFactory.Brush(ResourceKeys.BorderDefaultBrush) };
            var contentRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            contentRow.Children.Add(new TextBlock { Text = name, FontSize = 12, Foreground = isRhiOwned ? UIFactory.Brush(ResourceKeys.TextTertiaryBrush) : UIFactory.Brush(ResourceKeys.TextPrimaryBrush), VerticalAlignment = VerticalAlignment.Center });
            if (isRecommended) contentRow.Children.Add(MakeBadge("Recommended", "#1A3A20", "#6AE87A", "#2A5A30"));
            if (isAsi)         contentRow.Children.Add(MakeBadge("ASI plugin",  "#1A2A3A", "#7AACDD", "#2A4A6A"));
            if (isRhiOwned)    contentRow.Children.Add(MakeBadge($"正在被 {rhiConflicts[name]} 使用", "#2A1818", "#CC6666", "#5A2828"));
            else if (isOnDisk) contentRow.Children.Add(MakeBadge($"磁盘上：{onDiskConflicts[name]}", "#2A1A10", "#CC9955", "#5A3A18"));
            if (isCurrent)     contentRow.Children.Add(MakeBadge("当前", "#182840", "#7AACDD", "#2A4468"));
            btn.Content = contentRow;
            if (isRhiOwned)    ToolTipService.SetToolTip(btn, $"{rhiConflicts[name]} 正在使用中。请换一个名称。");
            else if (isOnDisk) ToolTipService.SetToolTip(btn, $"已存在 '{name}'（{onDiskConflicts[name]}）。RHI 会将其备份，并在卸载时恢复。");
            btn.Tag = name;
            btn.Click += (s, ev) => { chosen = (s as Button)?.Tag as string; if (s is FrameworkElement fe) FindParentContentDialog(fe)?.Hide(); };
            listPanel.Children.Add(btn);
        }

        var pickerDialog = new ContentDialog
        {
            Title = "选择 RTX Encore DLL 名称",
            Content = new StackPanel { Spacing = 8, Children = { new TextBlock { Text = "选择 RTX Encore 的文件名。已被 RHI 组件占用的名称不可选。琥珀色名称表示磁盘上已有同名文件 — RHI 会将其备份，并在卸载时恢复。", FontSize = 11, Foreground = UIFactory.Brush(ResourceKeys.TextTertiaryBrush), TextWrapping = TextWrapping.Wrap }, scrollViewer } },
            CloseButtonText = "取消", XamlRoot = _window.Content.XamlRoot, RequestedTheme = ElementTheme.Dark,
        };
        await DialogService.ShowSafeAsync(pickerDialog);
        return chosen;
    }
}

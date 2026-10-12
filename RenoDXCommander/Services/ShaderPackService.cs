// ShaderPackService.cs — Class declaration, constructor, path constants, pack definitions, enums, and ShaderPack record

using RenoDXCommander.Models;

namespace RenoDXCommander.Services;

/// <summary>
/// Downloads, extracts and deploys HDR ReShade shader packs from multiple sources.
///
/// All packs are merged into a single shared staging tree:
///   %LocalAppData%\RenoDXCommander\reshade\Shaders\
///   %LocalAppData%\RenoDXCommander\reshade\Textures\
///
/// Each pack's extracted files are tracked individually. If a pack's cache zip is
/// deleted — or extracted files are missing from the staging folder — the pack is
/// re-downloaded and re-extracted on the next launch.
///
/// Source types:
///   GhRelease — GitHub Releases API, picks first matching asset extension
///   DirectUrl — Any static URL; versioned by ETag / Last-Modified header
/// </summary>
public partial class ShaderPackService : IShaderPackService
{
    private readonly HttpClient _http;
    private readonly GitHubETagCache _etagCache;

    public ShaderPackService(HttpClient http, GitHubETagCache etagCache)
    {
        _http = http;
        _etagCache = etagCache;
    }
    // ── Public path constants (used by AuxInstallService) ─────────────────────────
    public static readonly string ShadersDir = Path.Combine(AuxInstallService.RsStagingDir, "着色器");
    public static readonly string TexturesDir = Path.Combine(AuxInstallService.RsStagingDir, "Textures");

    // User-defined custom shaders — placed by the user, never auto-downloaded
    public const string CustomShaderSentinel = "__custom__";
    /// <summary>Virtual pack ID for individual custom shader file selection in the picker.</summary>
    public const string CustomFilePackId = "__custom_files__";
    public static readonly string CustomDir = Path.Combine(AuxInstallService.RsStagingDir, "自定义");
    public static readonly string CustomShadersDir = Path.Combine(CustomDir, "着色器");
    public static readonly string CustomTexturesDir = Path.Combine(CustomDir, "Textures");

    /// <summary>
    /// Returns all shader and texture files from the Custom folder as relative paths
    /// suitable for display in the picker (e.g. "Shaders/MyShader.fx", "Textures/LUT.png").
    /// Returns an empty list if the Custom folder doesn't exist or is empty.
    /// </summary>
    public static IReadOnlyList<string> GetCustomPackFiles()
    {
        var files = new List<string>();
        try
        {
            if (Directory.Exists(CustomShadersDir))
                foreach (var f in Directory.EnumerateFiles(CustomShadersDir, "*", SearchOption.AllDirectories))
                    files.Add(Path.Combine("着色器", Path.GetRelativePath(CustomShadersDir, f)));
            if (Directory.Exists(CustomTexturesDir))
                foreach (var f in Directory.EnumerateFiles(CustomTexturesDir, "*", SearchOption.AllDirectories))
                    files.Add(Path.Combine("Textures", Path.GetRelativePath(CustomTexturesDir, f)));
        }
        catch (Exception ex) { CrashReporter.Log($"[ShaderPackService.GetCustomPackFiles] Failed — {ex.Message}"); }
        return files;
    }

    /// <summary>Returns true when the Custom shader folder contains at least one file.</summary>
    public static bool CustomPackHasFiles() => GetCustomPackFiles().Count > 0;

    public const string GameReShadeShaders = "reshade-shaders";
    public const string GameReShadeOriginal = "reshade-shaders-original";
    internal const string ManagedMarkerFileName = "Managed by RDXC.txt";
    private const string ManagedMarkerContent = "This folder is managed by RenoDXCommander. Do not edit manually.\n"
                                                  + "删除此文件会使 RDXC 将该文件夹视为用户自行管理。";

    // ── Pack definitions ──────────────────────────────────────────────────────────

    private enum SourceKind { GhRelease, DirectUrl }

    /// <summary>
    /// UI grouping for the shader picker dialog.
    /// Essential — always deployed, shown at the top.
    /// Recommended — suggested packs shown in the second group.
    /// Custom — user-placed files, shown between Recommended and Extra.
    /// Extra — everything else.
    /// </summary>
    public enum PackCategory { Essential, Recommended, Custom, Extra }

    /// <summary>
    /// Shader files that fail to compile and should never be extracted or deployed.
    /// Matched against the filename (leaf) of each archive entry, case-insensitive.
    /// </summary>
    private static readonly HashSet<string> ExcludedShaderFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        "BX_XIV_ChromakeyPlus.fx",
        "GrainSpread.fx",
        "NTSCCustom.fx",
        "NTSC_XOT.fx",
    };

    private record ShaderPack(
        string Id,           // unique key — used in settings.json and cache filenames
        string DisplayName,  // shown in progress messages and logs
        SourceKind Kind,
        string Url,          // API url (GhRelease) or direct download url (DirectUrl)
        bool IsMinimum,    // true for packs included in the Lilium/minimum set
        string? AssetExt = null,  // GhRelease: required file extension of the release asset
        string? Description = null, // short description shown in the shader picker dialog
        PackCategory Category = PackCategory.Extra, // UI grouping
        string[]? Requires = null // IDs of packs that must also be selected when this pack is enabled
    );

    // Packs in order of download. IsMinimum=true → included in Minimum mode.
    private static readonly ShaderPack[] DefaultPacks =
    {
        new(
            Id          : "Lilium",
            DisplayName : "Lilium HDR Shaders",
            Kind        : SourceKind.GhRelease,
            Url         : "https://api.github.com/repos/EndlesslyFlowering/ReShade_HDR_shaders/releases/latest",
            IsMinimum   : true,
            AssetExt    : ".7z",
            Description : "HDR 色调映射和逆向色调映射着色器",
            Category    : PackCategory.Essential
        ),
        new(
            Id          : "CrosireMaster",
            DisplayName : "crosire reshade-shaders (master)",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/crosire/reshade-shaders/archive/refs/heads/master.zip",
            IsMinimum   : true,
            Description : "官方 ReShade 标准效果 — 完整 master 分支",
            Category    : PackCategory.Recommended
        ),
        new(
            Id          : "CrosireLegacy",
            DisplayName : "crosire reshade-shaders (legacy)",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/crosire/reshade-shaders/archive/refs/heads/legacy.zip",
            IsMinimum   : false,
            Description : "旧版 ReShade 效果（已从 master 移除的较旧版本）",
            Category    : PackCategory.Extra
        ),
        new(
            Id          : "PumboAutoHDR",
            DisplayName : "PumboAutoHDR",
            Kind        : SourceKind.GhRelease,
            Url         : "https://api.github.com/repos/Filoppi/PumboAutoHDR/releases/latest",
            IsMinimum   : false,
            AssetExt    : ".zip",
            Description : "为 SDR 游戏自动进行 HDR 转换",
            Category    : PackCategory.Recommended
        ),
        new(
            Id          : "SmolbbsoopShaders",
            DisplayName : "smolbbsoop shaders",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/smolbbsoop/smolbbsoopshaders/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "HDR 实用着色器和效果",
            Category    : PackCategory.Extra
        ),
        new(
            Id          : "MaxG2DSimpleHDR",
            DisplayName : "MaxG2D Simple HDR Shaders",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/MaxG2D/ReshadeSimpleHDRShaders/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "简单的 HDR 泛光、镜头光晕和色调映射",
            Category    : PackCategory.Recommended
        ),
        new(
            Id          : "ClshortfuseShaders",
            DisplayName : "clshortfuse ReShade shaders",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/clshortfuse/reshade-shaders/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "用于 RenoDX 的 HDR 和色彩校正着色器",
            Category    : PackCategory.Recommended
        ),
        new(
            Id          : "PotatoFX",
            DisplayName : "potatoFX (CreepySasquatch)",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/CreepySasquatch/potatoFX/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "面向低端硬件的轻量后处理效果",
            Category    : PackCategory.Extra
        ),
        new(
            Id          : "Azen",
            DisplayName : "Azen by Zenteon",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/Zenteon/Azen/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "Zenteon 的休闲着色器合集 — 实验性效果",
            Requires    : new[] { "SmolbbsoopShaders" }
        ),
        new(
            Id          : "SweetFX",
            DisplayName : "SweetFX by CeeJay.dk",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/CeeJayDK/SweetFX/archive/refs/heads/master.zip",
            IsMinimum   : false,
            Description : "经典调色、锐化和泛光效果"
        ),
        new(
            Id          : "OtisFX",
            DisplayName : "OtisFX by Otis_Inf",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/FransBouma/OtisFX/archive/refs/heads/master.zip",
            IsMinimum   : false,
            Description : "电影级景深、光线和摄像机效果"
        ),
        new(
            Id          : "Depth3D",
            DisplayName : "Depth3D by BlueSkyDefender",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/BlueSkyDefender/Depth3D/archive/refs/heads/master.zip",
            IsMinimum   : false,
            Description : "立体 3D 和基于深度的视觉效果"
        ),
        new(
            Id          : "DaodanShaders",
            DisplayName : "reshade-shaders by Daodan",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/Daodan317081/reshade-shaders/archive/refs/heads/master.zip",
            IsMinimum   : false,
            Description : "漫画、交叉阴影和艺术风格效果"
        ),
        new(
            Id          : "BrussellShaders",
            DisplayName : "Shaders by brussell",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/brussell1/Shaders/archive/refs/heads/master.zip",
            IsMinimum   : false,
            Description : "半调、素描和风格化渲染效果"
        ),
        new(
            Id          : "FubaxShaders",
            DisplayName : "fubax-shaders by Fubaxiusz",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/Fubaxiusz/fubax-shaders/archive/refs/heads/master.zip",
            IsMinimum   : false,
            Description : "VR 友好的镜头畸变和色差"
        ),
        new(
            Id          : "qUINT",
            DisplayName : "qUINT by Marty McFly",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/martymcmodding/qUINT/archive/refs/heads/master.zip",
            IsMinimum   : false,
            Description : "MXAO、ADOF、Lightroom 和屏幕空间反射"
        ),
        new(
            Id          : "AlucardDH",
            DisplayName : "dh-reshade-shaders by AlucardDH",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/AlucardDH/dh-reshade-shaders/archive/refs/heads/master.zip",
            IsMinimum   : false,
            Description : "环境光遮蔽、去抖动和色彩增强"
        ),
        new(
            Id          : "WarpFX",
            DisplayName : "Warp-FX by Radegast",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/Radegast-FFXIV/Warp-FX/archive/refs/heads/master.zip",
            IsMinimum   : false,
            Description : "屏幕扭曲、旋涡和变形效果"
        ),
        new(
            Id          : "Prod80",
            DisplayName : "Color effects by prod80",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/prod80/prod80-ReShade-Repository/archive/refs/heads/master.zip",
            IsMinimum   : false,
            Description : "专业调色、曲线和色调工具"
        ),
        new(
            Id          : "CorgiFX",
            DisplayName : "CorgiFX by originalnicodr",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/originalnicodr/CorgiFX/archive/refs/heads/master.zip",
            IsMinimum   : false,
            Description : "截图和虚拟摄影工具"
        ),
        new(
            Id          : "InsaneShaders",
            DisplayName : "Insane-Shaders by Lord of Lunacy",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/LordOfLunacy/Insane-Shaders/archive/refs/heads/master.zip",
            IsMinimum   : false,
            Description : "高级抖动、去雾和边缘检测"
        ),
        new(
            Id          : "CobraFX",
            DisplayName : "CobraFX by SirCobra",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/LordKobra/CobraFX/archive/refs/heads/master.zip",
            IsMinimum   : false,
            Description : "重力、自动对焦和实时光线追踪效果"
        ),
        new(
            Id          : "AstrayFX",
            DisplayName : "AstrayFX by BlueSkyDefender",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/BlueSkyDefender/AstrayFX/archive/refs/heads/master.zip",
            IsMinimum   : false,
            Description : "基于深度的雾、霾和大气效果"
        ),
        new(
            Id          : "CRTRoyale",
            DisplayName : "CRT-Royale-ReShade by akgunter",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/akgunter/crt-royale-reshade/archive/refs/heads/master.zip",
            IsMinimum   : false,
            Description : "带荧光和扫描线模拟的 CRT 显示器仿真"
        ),
        new(
            Id          : "RSRetroArch",
            DisplayName : "RSRetroArch by Matsilagi",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/Matsilagi/RSRetroArch/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "RetroArch 着色器移植 — CRT、LCD 和复古滤镜"
        ),
        new(
            Id          : "VRToolkit",
            DisplayName : "VRToolkit by retroluxfilm",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/retroluxfilm/reshade-vrtoolkit/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "为 VR 头显优化的锐化和清晰度工具"
        ),
        new(
            Id          : "FGFX",
            DisplayName : "FGFX by AlexTuduran",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/AlexTuduran/FGFX/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "胶片颗粒、多 LUT 和电影级后处理"
        ),
        new(
            Id          : "CShade",
            DisplayName : "CShade by papadanku",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/papadanku/CShade/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "光流、运动模糊和卷积效果"
        ),
        new(
            Id          : "iMMERSE",
            DisplayName : "iMMERSE by Marty McFly",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/martymcmodding/iMMERSE/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "下一代 RTGI、MXAO 和抗锯齿套件"
        ),
        new(
            Id          : "VortShaders",
            DisplayName : "vort_Shaders by vortigern11",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/vortigern11/vort_Shaders/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "锐化、色彩校正和深度效果"
        ),
        new(
            Id          : "BXShade",
            DisplayName : "BX-Shade by BarricadeMKXX",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/liuxd17thu/BX-Shade/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "泛光、曝光和色彩增强效果"
        ),
        new(
            Id          : "SHADERDECK",
            DisplayName : "SHADERDECK by TreyM",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/IAmTreyM/SHADERDECK/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "精选的色彩和光照效果合集"
        ),
        new(
            Id          : "METEOR",
            DisplayName : "METEOR by Marty McFly",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/martymcmodding/METEOR/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "高级降噪和图像重建"
        ),
        new(
            Id          : "AnnReShade",
            DisplayName : "Ann-ReShade by Anastasia Bouwsma",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/AnastasiaGals/Ann-ReShade/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "柔和泛光、调色和环境光预设"
        ),
        new(
            Id          : "ZenteonFX",
            DisplayName : "ZenteonFX Shaders by Zenteon",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/Zenteon/ZenteonFX/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "全局光照、屏幕空间反射和路径追踪效果"
        ),
        new(
            Id          : "GShadeShaders",
            DisplayName : "GShade-Shaders by Marot",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/Mortalitas/GShade-Shaders/archive/refs/heads/master.zip",
            IsMinimum   : false,
            Description : "GShade 的社区着色器大型合集"
        ),
        new(
            Id          : "PthoFX",
            DisplayName : "Ptho-FX by PthoEastCoast",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/PthoEastCoast/Ptho-FX/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "电影级调色和胶片模拟"
        ),
        new(
            Id          : "Anagrama",
            DisplayName : "The Anagrama Collection by nullfractal",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/nullfrctl/reshade-shaders/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "艺术与实验性视觉效果"
        ),
        new(
            Id          : "BarbatosShaders",
            DisplayName : "reshade-shaders by Barbatos",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/BarbatosBachiko/Reshade-Shaders/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "环境光遮蔽、泛光和色彩效果"
        ),
        new(
            Id          : "BFBFX",
            DisplayName : "BFBFX by yaboi BFB",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/yplebedev/BFBFX/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "风格化艺术后处理效果"
        ),
        new(
            Id          : "Rendepth",
            DisplayName : "Rendepth by cybereality",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/outmode/rendepth-reshade/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "基于深度的 3D 渲染和立体效果"
        ),
        new(
            Id          : "CropAndResize",
            DisplayName : "Crop and Resize by P0NYSLAYSTATION",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/P0NYSLAYSTATION/Scaling-Shaders/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "屏幕裁剪、缩放和宽高比工具"
        ),
        new(
            Id          : "FXShaders",
            DisplayName : "FXShaders by luluco250",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/luluco250/FXShaders/archive/refs/heads/master.zip",
            IsMinimum   : false,
            Description : "泛光、颗粒、抖动和实用着色器库"
        ),
        new(
            Id          : "LumeniteFX",
            DisplayName : "LumeniteFX by Kaido",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/umar-afzaal/LumeniteFX/archive/refs/heads/mainline.zip",
            IsMinimum   : false,
            Description : "光照、泛光和大气辉光效果"
        ),
        new(
            Id          : "NNShaders",
            DisplayName : "NN-Shaders by Sarenya",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/Sarenya/NN-Shaders/archive/refs/heads/master.zip",
            IsMinimum   : false,
            Description : "基于神经网络的图像处理着色器"
        ),
        new(
            Id          : "QdOledAplFixer",
            DisplayName : "QD-OLED APL Fixer by mspeedo",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/mspeedo/QD-OLED-APL-FIXER/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "HDR 亮度增强，以补偿 QD-OLED 的 ABL 变暗"
        ),
        new(
            Id          : "GlamaryeFX",
            DisplayName : "Glamarye Fast Effects by rj200",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/rj200/Glamarye_Fast_Effects_for_ReShade/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "轻量一体化：单次处理完成锐化、环境光遮蔽、间接光照和色彩校正"
        ),
        new(
            Id          : "LumaBoost",
            DisplayName : "LumaBoost by Valadore",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/Valadore/LumaBoost/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "OLED ABL 补偿 — 动态提升中间调以抵消自动亮度限制"
        ),
        new(
            Id          : "DLSS5Feeder",
            DisplayName : "DLSS5 Feeder shader",
            Kind        : SourceKind.DirectUrl,
            Url         : "",   // no URL — seeded from the Feeder addon zip by RHI, never downloaded
            IsMinimum   : false,
            Description : "DLSS5_Feed.fx — 由 RHI 从 Feeder 插件压缩包中提取。不会显示在着色器选择器中。",
            Category    : PackCategory.Extra
        ),
        new(
            Id          : "RenoFXHDRToolkit",
            DisplayName : "RenoFX HDR Toolkit by OopyDoopy",
            Kind        : SourceKind.DirectUrl,
            Url         : "https://github.com/clshortfuse/renofx/archive/refs/heads/main.zip",
            IsMinimum   : false,
            Description : "为没有 RenoDX mod 的游戏进行 SDR 到 HDR 转换、色调映射和调色",
            Category    : PackCategory.Recommended
        ),
    };

    // Active packs — starts as DefaultPacks, may be overridden by manifest
    private ShaderPack[] _packs = DefaultPacks;

    // ── AvailablePacks ───────────────────────────────────────────────────────────

    /// <summary>
    /// Exposes pack metadata for the picker UI — returns every known pack's Id and DisplayName.
    /// </summary>
    public IReadOnlyList<(string Id, string DisplayName, PackCategory Category)> AvailablePacks { get; private set; } =
        DefaultPacks.OrderBy(p => p.Category).ThenBy(p => p.DisplayName, StringComparer.OrdinalIgnoreCase)
             .Select(p => (p.Id, p.DisplayName, p.Category)).ToList().AsReadOnly();

    /// <summary>
    /// Returns the short description for a pack, or null if none is set.
    /// </summary>
    public string? GetPackDescription(string packId) =>
        _packs.FirstOrDefault(p => p.Id == packId)?.Description;

    /// <summary>
    /// Returns the IDs of packs that the given pack requires (dependencies).
    /// </summary>
    public string[] GetRequiredPacks(string packId) =>
        _packs.FirstOrDefault(p => p.Id == packId)?.Requires ?? Array.Empty<string>();

    // ── Manifest overrides ────────────────────────────────────────────────────────

    /// <summary>
    /// Merges remote manifest shader pack overrides into the active pack list.
    /// Starts from DefaultPacks, applies additions/modifications/removals, then
    /// rebuilds the AvailablePacks property.
    /// </summary>
    public void ApplyManifestOverrides(RemoteManifest? manifest)
    {
        if (manifest?.ShaderPacks == null || manifest.ShaderPacks.Count == 0)
        {
            _packs = DefaultPacks;
            RebuildAvailablePacks();
            return;
        }

        var merged = new List<ShaderPack>(DefaultPacks);

        foreach (var (id, entry) in manifest.ShaderPacks)
        {
            // disabled → remove from list
            if (entry.Disabled == true)
            {
                merged.RemoveAll(p => p.Id == id);
                continue;
            }

            var existingIdx = merged.FindIndex(p => p.Id == id);
            if (existingIdx >= 0)
            {
                // Override non-null fields on existing pack
                var existing = merged[existingIdx];
                merged[existingIdx] = existing with
                {
                    DisplayName = entry.DisplayName ?? existing.DisplayName,
                    Kind        = TryParseKind(entry.Kind) ?? existing.Kind,
                    Url         = entry.Url ?? existing.Url,
                    IsMinimum   = entry.IsMinimum ?? existing.IsMinimum,
                    AssetExt    = entry.AssetExt ?? existing.AssetExt,
                    Description = entry.Description ?? existing.Description,
                    Category    = TryParseCategory(entry.Category) ?? existing.Category,
                    Requires    = entry.Requires ?? existing.Requires,
                };
            }
            else
            {
                // New pack from manifest — requires at minimum a URL and kind
                if (string.IsNullOrEmpty(entry.Url) || string.IsNullOrEmpty(entry.Kind))
                    continue;

                var kind = TryParseKind(entry.Kind);
                if (kind == null) continue;

                merged.Add(new ShaderPack(
                    Id          : id,
                    DisplayName : entry.DisplayName ?? id,
                    Kind        : kind.Value,
                    Url         : entry.Url,
                    IsMinimum   : entry.IsMinimum ?? false,
                    AssetExt    : entry.AssetExt,
                    Description : entry.Description,
                    Category    : TryParseCategory(entry.Category) ?? PackCategory.Extra,
                    Requires    : entry.Requires
                ));
            }
        }

        _packs = merged.ToArray();
        RebuildAvailablePacks();
    }

    private void RebuildAvailablePacks()
    {
        AvailablePacks = _packs
            .OrderBy(p => p.Category)
            .ThenBy(p => p.DisplayName, StringComparer.OrdinalIgnoreCase)
            .Select(p => (p.Id, p.DisplayName, p.Category))
            .ToList()
            .AsReadOnly();
    }

    private static SourceKind? TryParseKind(string? value) =>
        value switch
        {
            "GhRelease" => SourceKind.GhRelease,
            "DirectUrl" => SourceKind.DirectUrl,
            _ => null
        };

    private static PackCategory? TryParseCategory(string? value) =>
        value switch
        {
            "Essential"   => PackCategory.Essential,
            "Recommended" => PackCategory.Recommended,
            "额外"       => PackCategory.Extra,
            _ => null
        };
}


using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;

public class ContentWindow
{
    public string Name { get; set; }
    public double X { get; set; }
    public int ZIndex { get; set; }
    public double Y { get; set; }
    public double ScaleX { get; set; }
    public double ScaleY { get; set; }
    public double CropLeft { get; set; }
    public double CropRight { get; set; }
    public double CropTop { get; set; }
    public double CropBottom { get; set; }
    public string AdditionalFilters { get; set; }

    public override string ToString()
    {
        return "[Content: " + Name + "] ZIndex=" + ZIndex + ",X=" + X + ", Y=" + Y +
               ", Scale=(" + ScaleX + ", " + ScaleY + "), " +
               "Crop(L=" + CropLeft + ", R=" + CropRight + ", T=" + CropTop + ", B=" + CropBottom + "), " +
               "Filters='" + AdditionalFilters + "'";
    }
}

public class ContentLayoutManager
{
    private readonly Dictionary<string, object> args;
    private readonly string scene;
    private readonly string prefix;
    private readonly string HUD_NAMESPACE = "Hud";
    private readonly string GRID_POSITION_ATTRIBUTE = "gridPosition";

    // Delegates injetados para evitar depender do objeto CPH diretamente
    private readonly Action<string, string, ContentWindow> obsSetTransform;
    private readonly Action<string, string> gridPositioningAction;
    private readonly Action<string> logDebug;

    public ContentLayoutManager(
        Dictionary<string, object> args,
        string scene,
        string prefix,
        Action<string, string, ContentWindow> obsSetTransform,
        Action<string, string> gridPositioningAction,
        Action<string> logDebug)
    {
        this.args = args;
        this.scene = scene;
        this.prefix = prefix.EndsWith(".") ? prefix : prefix + ".";
        this.obsSetTransform = obsSetTransform;
        this.gridPositioningAction = gridPositioningAction;
        this.logDebug = logDebug;
    }

    public Dictionary<string, string> GetContents()
    {
        var contents = new Dictionary<string, string>();

        foreach (var kv in args)
        {
            if (kv.Key.StartsWith(prefix + "Contents."))
            {
                var key = kv.Key.Replace(prefix + "Contents.", "");
                contents[key] = kv.Value.ToString();
            }
        }

        return contents;
    }

    public Dictionary<string, ContentWindow> GetLayout(string layoutName)
    {
        var layout = new Dictionary<string, ContentWindow>();
        string layoutPrefix = prefix + layoutName + ".";

        foreach (var kv in args)
        {
            if (!kv.Key.StartsWith(layoutPrefix))
                continue;

            string[] parts = kv.Key.Replace(layoutPrefix, "").Split('.');
            if (parts.Length < 2)
            {
                this.logDebug("[WARN] Parametrizacao para '" + kv.Key + "' FORA DO PADRAO!");
                continue;
            }

            if (parts.Length == 2) // ContentPositioning
            {
                string winKey = parts[0];
                string field = parts[1];

                if (!layout.ContainsKey(winKey))
                    layout[winKey] = new ContentWindow { Name = winKey };

                var win = layout[winKey];

                try
                {
                    switch (field)
                    {
                        case "zIndex": win.ZIndex = Convert.ToInt32(kv.Value); break;
                        case "x": win.X = Convert.ToDouble(kv.Value); break;
                        case "y": win.Y = Convert.ToDouble(kv.Value); break;
                        case "scaleX": win.ScaleX = Convert.ToDouble(kv.Value); break;
                        case "scaleY": win.ScaleY = Convert.ToDouble(kv.Value); break;
                        case "cropLeft": win.CropLeft = Convert.ToDouble(kv.Value); break;
                        case "cropRight": win.CropRight = Convert.ToDouble(kv.Value); break;
                        case "cropTop": win.CropTop = Convert.ToDouble(kv.Value); break;
                        case "cropBottom": win.CropBottom = Convert.ToDouble(kv.Value); break;
                        case "additionalFilters": win.AdditionalFilters = $"{kv.Value}"; break;
                    }
                }
                catch (Exception ex)
                {
                    this.logDebug("[WARN] Erro ao converter valor para '" + field + "' em '" + winKey + "': " + ex.Message);
                }

            }
            else if (parts.Length == 3)  // GridPositioning
            {
                if (HUD_NAMESPACE.Equals(parts[0]))
                {
                    string hud = parts[0];
                    string hudComponent = parts[1];
                    string attribute = parts[2];
                    // this.logDebug($"\n\n\t GridPositioning 0:{hud}, 1:{hudComponent}, 2:{attribute} value[{kv.Value}]");

                    if (GRID_POSITION_ATTRIBUTE.Equals(attribute))
                    {
                        this.gridPositioningAction(hudComponent, kv.Value.ToString());
                    }
                }
                continue;
            }
        }

        return layout;
    }

    public void ApplyLayout(string layoutName)
    {
        var contents = GetContents();
        var layout = GetLayout(layoutName);

        foreach (var kv in layout)
        {
            string winKey = kv.Key;
            ContentWindow window = kv.Value;

            string targetSource;
            if (!contents.TryGetValue(winKey, out targetSource))
            {
                this.logDebug("[WARN] " + winKey + " não encontrado em contents para layout '" + layoutName + "'.");
                continue;
            }

            this.logDebug("[APLICANDO] " + targetSource + " => " + window.ToString());
            this.obsSetTransform(scene, targetSource, window);
        }
    }
}

// Nota: NÃO declaramos herança explícita nem interfaces aqui para evitar conflitos
// que o compilador do Streamer.bot pode gerar (ele pode injetar CPHInlineBase por baixo).
public class CPHInline
{
    // Este é o ponto de entrada exigido pelo Streamer.bot.
    // O ambiente normalmente disponibiliza o objeto 'CPH' neste contexto.
    public bool Execute()
    {
        const string STREAM_ASSET_NAME = "MagSFContentPositioningManager";
        const string SCENE_OBS_NAME = $"{STREAM_ASSET_NAME}.sceneObsName";
        const string PREFIX = $"{STREAM_ASSET_NAME}.prefix";
        const string LAYOUT_TYPE = $"{STREAM_ASSET_NAME}.layoutType";
        const string LAYOUT_URI = $"{STREAM_ASSET_NAME}.layoutUri";

        CPH.TryGetArg(SCENE_OBS_NAME, out string scene);
        CPH.TryGetArg(PREFIX, out string prefix);

        CPH.LogDebug($"{STREAM_ASSET_NAME} parameters");
        CPH.LogDebug($"{SCENE_OBS_NAME}:{scene}");
        CPH.LogDebug($"{PREFIX}:{prefix}");

        if (!CPH.TryGetArg(LAYOUT_TYPE, out string layoutType))
        {
            CPH.TryGetArg(LAYOUT_URI, out string layoutUri);
            layoutType = CPH.GetGlobalVar<string>(layoutUri, true);
            CPH.LogDebug($"{LAYOUT_URI}:{layoutUri}");
        }
        layoutType = layoutType.Replace("%", "");
        CPH.LogDebug($"{LAYOUT_TYPE}:{layoutType}");
        CPH.SetArgument(LAYOUT_TYPE, layoutType);

        System.Action<string> logDebug =
            (message) =>
            {
                CPH.LogDebug(message);
            };

        // Funcao para posicionamento dos componentes da HUD
        System.Action<string, string> gridPositioningAction =
            (hudComponent, gridPosition) =>
            {
                CPH.LogDebug($"\n\n\tAplicando ao [{hudComponent}] o GridPosition [{gridPosition}] no ContentPositioning");
                CPH.SetArgument($"MagSF{hudComponent}GridPositionUpdater.position", gridPosition);
                CPH.RunAction($"MagSFGridPositionUpdater-Hud.{hudComponent}");
            };

        // Funcao para posicionamento de Contents
        System.Action<string, string, ContentWindow> obsSetTransform =
            (scene, targetSource, content) =>
            {
                CPH.LogDebug($"Applying Content Positioning: {scene}|{targetSource}: {content}");

                string filterName = "[MagSF] Content Positioning";
                JObject config = new JObject
                {
                    ["sourceName"] = scene,
                    ["filterName"] = filterName,
                    ["overlay"] = true,
                    ["filterSettings"] = new JObject
                    {
                        ["source"] = targetSource,
                        ["change_order"] = 10, // [Order] 10 - End Absolute
                        ["order_position"] = content.ZIndex,
                        ["pos"] = new JObject
                        {
                            ["x"] = content.X,
                            ["y"] = content.Y,
                        },
                        ["scale"] = new JObject
                        {
                            ["x"] = content.ScaleX,
                            ["y"] = content.ScaleY,
                        },
                        ["crop"] = new JObject
                        {
                            ["left"] = content.CropLeft,
                            // ["left_sign"] = " ",
                            ["top"] = content.CropTop,
                            // ["top_sign"] = " ",
                            ["right"] = content.CropRight,
                            // ["right_sign"] = " ",
                            ["bottom"] = content.CropBottom,
                            // ["bottom_sign"] = " ",
                        }
                    },
                };

                // Output the JSON string
                string request = config.ToString(Formatting.None);
                // CPH.LogDebug($"request:\n {request}");

                CPH.ObsSetSourceVisibility(scene, targetSource, true);
                CPH.ObsSendRaw("SetSourceFilterSettings", request);
                CPH.ObsShowFilter(scene, filterName);

                if (!string.IsNullOrEmpty(targetSource) && !string.IsNullOrEmpty(content.AdditionalFilters))
                {
                    CPH.LogDebug("[FILTROS] Aplicando filtros adicionais: " + content.AdditionalFilters);
                    foreach (string _filter in content.AdditionalFilters.Split(';'))
                    {
                        CPH.LogDebug($"\t applying additionalFilter: {_filter} to {targetSource}");
                        CPH.ObsShowFilter(targetSource, _filter);
                    }
                }
                // Timeout necessário para realizar as movimentações através dos filtros (menos que 100, buga§)
                CPH.Wait(120);
            };

        // Criar manager com os delegates injetados
        var manager = new ContentLayoutManager(args, scene, prefix, obsSetTransform, gridPositioningAction, logDebug);

        // Aplicar layout
        manager.ApplyLayout(layoutType);

        return true;
    }
}

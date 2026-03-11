using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;


public class CPHInline
{
    public bool Execute()
    {
        const string STREAM_ASSET_NAME = "MagSFGridPositionUpdater";
        const string POSITION = $"{STREAM_ASSET_NAME}.position";
        const string SOURCE_NAME = $"{STREAM_ASSET_NAME}.sourceName";
        const string FILTER_NAME = $"{STREAM_ASSET_NAME}.filterName";
        const string ADDITIONAL_SOURCE_NAME = $"{STREAM_ASSET_NAME}.additionalSourceName"; // Nested scene onde serão aplicados os additionalSourceFilters
        const string ADDITIONAL_SOURCE_FILTERS = $"{STREAM_ASSET_NAME}.additionalSourceFilters"; // Filtros separados por ; (ponto-e-vírgula) a serem aplicados em additionalSourceName
        const string ADDITIONAL_SOURCE_FILTERS_FOR_SCENE = $"{STREAM_ASSET_NAME}.additionalSourceFiltersForScene"; // Filtros separados por ; (ponto-e-vírgula) a serem aplicados em additionalSourceName

        const string POS_X = $"{STREAM_ASSET_NAME}.posX";
        const string POS_Y = $"{STREAM_ASSET_NAME}.posY";
        const string SCALE_X = $"{STREAM_ASSET_NAME}.scaleX";
        const string SCALE_Y = $"{STREAM_ASSET_NAME}.scaleY";
        const string CROP_TOP = $"{STREAM_ASSET_NAME}.cropTop";
        const string CROP_LEFT = $"{STREAM_ASSET_NAME}.cropLeft";
        const string CROP_RIGHT = $"{STREAM_ASSET_NAME}.cropRight";
        const string CROP_BOTTOM = $"{STREAM_ASSET_NAME}.cropBottom";

        CPH.TryGetArg(SOURCE_NAME, out string sourceName);
        CPH.TryGetArg(POSITION, out string position);
        CPH.TryGetArg(FILTER_NAME, out string filterName);
        CPH.TryGetArg(ADDITIONAL_SOURCE_FILTERS, out string additionalSourceFilters);
        CPH.TryGetArg(ADDITIONAL_SOURCE_FILTERS_FOR_SCENE, out string additionalSourceFiltersForScene);
        CPH.TryGetArg(ADDITIONAL_SOURCE_NAME, out string additionalSourceName);
        CPH.TryGetArg(POS_X, out string _posX);
        CPH.TryGetArg(POS_Y, out string _posY);
        CPH.TryGetArg(SCALE_X, out string _scaleX);
        CPH.TryGetArg(SCALE_Y, out string _scaleY);
        CPH.TryGetArg(CROP_TOP, out string _cropTop);
        CPH.TryGetArg(CROP_LEFT, out string _cropLeft);
        CPH.TryGetArg(CROP_RIGHT, out string _cropRight);
        CPH.TryGetArg(CROP_BOTTOM, out string _cropBottom);

        // CPH.LogDebug($"antes {CROP_TOP}|{_cropTop}");
        // CPH.LogDebug($"antes {CROP_LEFT}|{_cropLeft}");
        // CPH.LogDebug($"antes {CROP_RIGHT}|{_cropRight}");
        // CPH.LogDebug($"antes {CROP_BOTTOM}|{_cropBottom}");

        // TODO FIXME entender pq double no json gera string com ',' = usar CultureInfo.CurrentCulture resolveu a questão
        // double.Parse("52.8725945");
        // CultureInfo culture = new CultureInfo("pt-BR");

        CultureInfo culture = CultureInfo.CurrentCulture;
        double posX = (string.IsNullOrEmpty(_posX) || _posX.StartsWith("%")) ? 0.0 : double.Parse(_posX, culture);
        double posY = (string.IsNullOrEmpty(_posY) || _posY.StartsWith("%")) ? 0.0 : double.Parse(_posY, culture);
        double scaleX = (string.IsNullOrEmpty(_scaleX) || _scaleX.StartsWith("%")) ? 1.0 : double.Parse(_scaleX, culture);
        double scaleY = (string.IsNullOrEmpty(_scaleY) || _scaleY.StartsWith("%")) ? 1.0 : double.Parse(_scaleY, culture);
        double cropTop = (string.IsNullOrEmpty(_cropTop) || _cropTop.StartsWith("%")) ? 0.0 : double.Parse(_cropTop, culture);
        double cropLeft = (string.IsNullOrEmpty(_cropLeft) || _cropLeft.StartsWith("%")) ? 0.0 : double.Parse(_cropLeft, culture);
        double cropRight = (string.IsNullOrEmpty(_cropRight) || _cropRight.StartsWith("%")) ? 0.0 : double.Parse(_cropRight, culture);
        double cropBottom = (string.IsNullOrEmpty(_cropBottom) || _cropBottom.StartsWith("%")) ? 0.0 : double.Parse(_cropBottom, culture);

        // CPH.LogDebug($"{STREAM_ASSET_NAME} input params:");
        // CPH.LogDebug($"{POSITION}|{position}");
        // CPH.LogDebug($"{SOURCE_NAME}|{sourceName}");
        // CPH.LogDebug($"{FILTER_NAME}|{filterName}");
        // CPH.LogDebug($"{ADDITIONAL_SOURCE_NAME}|{additionalSourceName}");
        // CPH.LogDebug($"{ADDITIONAL_SOURCE_FILTERS}|{additionalSourceFilters}");
        // CPH.LogDebug($"{POS_X}|{posX}");
        // CPH.LogDebug($"{POS_Y}|{posY}");
        // CPH.LogDebug($"{SCALE_X}|{scaleX}");
        // CPH.LogDebug($"{SCALE_Y}|{scaleY}");
        // CPH.LogDebug($"{CROP_TOP}|{cropTop}");
        // CPH.LogDebug($"{CROP_LEFT}|{cropLeft}");
        // CPH.LogDebug($"{CROP_RIGHT}|{cropRight}");
        // CPH.LogDebug($"{CROP_BOTTOM}|{cropBottom}");

        // Filter Parameters for Obs-raw call
        JObject config = new JObject
        {
            ["sourceName"] = sourceName,
            ["filterName"] = filterName,
            ["overlay"] = true,
            ["filterSettings"] = new JObject
            {
                ["pos"] = new JObject
                {
                    ["x"] = posX,
                    ["y"] = posY,
                },
                ["scale"] = new JObject
                {
                    ["x"] = scaleX,
                    ["y"] = scaleY,
                },
                ["crop"] = new JObject
                {
                    ["left"] = cropLeft,
                    // ["left_sign"] = " ",
                    ["top"] = cropTop,
                    // ["top_sign"] = " ",
                    ["right"] = cropRight,
                    // ["right_sign"] = " ",
                    ["bottom"] = cropBottom,
                    // ["bottom_sign"] = " ",
                }
            },
        };

        // Output the JSON string
        string request = config.ToString(Formatting.None);
        // CPH.LogDebug($"request:\n {request}");

        CPH.ObsSendRaw("SetSourceFilterSettings", request);
        CPH.ObsShowFilter(sourceName, filterName);

        int waitAfterFilters = 100;
        bool waitABitLonger = false;
        if (!string.IsNullOrEmpty(additionalSourceName))
        {
            if (!string.IsNullOrEmpty(additionalSourceFilters))
            {
                CPH.LogDebug($"\t additionalSourceFilters to apply: {additionalSourceFilters}");
                foreach (string _filter in additionalSourceFilters.Split(';'))
                {
                    CPH.LogDebug($"\t\t{_filter} to {additionalSourceName}");
                    CPH.ObsShowFilter(additionalSourceName, _filter);
                }
                waitABitLonger = true;
            }

            if (!string.IsNullOrEmpty(additionalSourceFiltersForScene))
            {
                CPH.LogDebug($"\t additionalSourceFiltersForScene to apply: {additionalSourceFiltersForScene}");
                foreach (string _filter in additionalSourceFiltersForScene.Split(';'))
                {
                    CPH.LogDebug($"\t\t{_filter} to {additionalSourceName}");
                    CPH.ObsShowFilter(additionalSourceName, _filter);
                }
                waitABitLonger = true;
            }
        }

        // TODO FIXME colocar como um parametro
        // Wait para que os filtros tenham tempo de serem aplicados
        int _wait = (waitABitLonger) ? waitAfterFilters : waitAfterFilters * 2;
        CPH.LogDebug($"\t\tWAIT GRID POSITION {sourceName}");
        CPH.Wait(_wait);

        return true;
    }
}

// var transitionSettings = new { duration = 500, position = "center_left", zoom = "100" };
// string jsonSettings = Newtonsoft.Json.JsonConvert.SerializeObject(transitionSettings);
// CPH.ObsSendRaw("SetSourceFilterSettings", "{\"sourceName\":\"[TXT] Ad Widget - Box - Timer\",\"filterName\":\"[Move] Timer - Reset\",\"filterSettings\":{\"setting_float\": " + adLength + "},\"overlay\":true}", 0);
// CPH.ObsSendRaw("SetSourceFilterSettings", "{\"sourceName\":\"[W] Ad Widget by Content Delta\",\"filterName\":\"[Move] Ad Bar - Countdown\",\"filterSettings\":{\"duration\": " + adLengthMs + "},\"overlay\":true}", 0);
// CPH.ObsSendRaw("SetSourceFilterSettings", "{\"sourceName\":\"_HUD [MagSF]\",\"filterName\":\"Multicam GridPosition [MagSF]\",\"filterSettings\":{\"crop\":{\"bottom\":0,\"bottom_sign\":\" \",\"left\":500,\"left_sign\":\" \",\"right\":0,\"right_sign\":\" \",\"top\":0,\"top_sign\":\" \"}},\"overlay\":true}", 0);
// CPH.ObsShowFilter("[W] Ad Widget by Content Delta", "[Move] Ad Bar - Reset", 0);

// ["transform_text"] = "pos: x 0.0 y 500.0 rot: 0.0 scale: x 1.000 y 1.000 crop: l 0 t 0 r 0 b 0",
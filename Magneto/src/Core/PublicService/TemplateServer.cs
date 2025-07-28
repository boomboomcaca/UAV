using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Core.Utils;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Protocol.Define;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core.PublicService;

public sealed class TemplateServer
{
    private const string DefaultDevicePath = "Device";
    private const string DefaultDriverPath = "Driver";

    /// <summary>
    ///     参数枚举排序规则
    ///     true: 从大到小
    ///     false:从小到大
    /// </summary>
    private const bool ParamsSortOrdingRule = false;

    private static readonly Lazy<TemplateServer> _server = new(() => new TemplateServer());
    private string _outputPath;

    private TemplateServer()
    {
    }

    public static TemplateServer Instance => _server.Value;

    public Func<string> GetDefaultDevicePath { get; set; } =
        () => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultDevicePath);

    public Func<string> GetDefaultDriverPath { get; set; } =
        () => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultDriverPath);

    public void UpdateOutputPath(string path)
    {
        Console.WriteLine($"导出目录:{path}");
        _outputPath = path;
    }

    #region 加载Json文件

    /// <summary>
    ///     加载Json文件，返回ModuleInfo
    /// </summary>
    /// <param name="jsonpath">接送路径</param>
    public ModuleInfo LoadModuleInfo(string jsonpath)
    {
        ModuleInfo info = null;
        if (File.Exists(jsonpath))
        {
            var jsonString = File.ReadAllText(jsonpath);
            info = JsonConvert.DeserializeObject<ModuleInfo>(jsonString);
        }

        return info;
    }

    #endregion

    /// <summary>
    ///     判断是Device还是Driver，还是都不是
    ///     -1：都不是
    ///     0：Device
    ///     1：Driver
    /// </summary>
    /// <param name="dllPath"></param>
    /// <returns>-1:都不是 0:Device 1：Driver</returns>
    private int IsDriverOrDevice(string dllPath)
    {
        if (Path.GetExtension(dllPath) != ".dll") return -1;
        var temp = Assembly.LoadFile(dllPath);
        var types = temp.GetTypes();
        foreach (var type in types)
            if (TypesFactory.IsBaseTypeExists(type, typeof(DeviceBase)))
                return 0;
            else if (TypesFactory.IsBaseTypeExists(type, typeof(DriverBase))) return 1;
        return -1;
    }

    private void ExportDevice(string path)
    {
        var dir = _outputPath; //$"{AppDomain.CurrentDomain.BaseDirectory}../../../doc/";
        // string fullPath = path.Replace(Path.GetFileName(path), "");
        var fullPath = dir;
        if (!Directory.Exists(fullPath))
            if (fullPath != null)
                Directory.CreateDirectory(fullPath);
        if (fullPath != null)
        {
            fullPath = Path.Combine(fullPath, "Device");
            if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);
            var ass = Assembly.LoadFile(path);
            var types = ass.GetTypes();
            foreach (var type in types)
            {
                if (!TypesFactory.IsBaseTypeExists(type, typeof(DeviceBase))) continue;
                var device = new ModuleInfo();
                var devDescriptions = type.GetCustomAttributes<DeviceDescriptionAttribute>().ToArray();
                if (devDescriptions.Length == 0) continue;
                foreach (var desc in devDescriptions)
                {
                    var fileName = desc.Name;
                    Console.WriteLine($"开始导出模板-Device:{fileName} Version:{desc.Version}");
                    device.DisplayName = desc.Name;
                    device.Class = type.FullName;
                    device.ModuleType = ModuleType.Device;
                    device.Category = desc.DeviceCategory;
                    device.Feature = desc.FeatureType;
                    device.TemplateId = Guid.NewGuid();
                    device.Version = string.IsNullOrEmpty(desc.Version) ? "" : desc.Version;
                    device.Manufacturer = desc.Manufacturer;
                    device.Model = string.IsNullOrEmpty(desc.Model) ? "" : desc.Model;
                    device.Description = string.IsNullOrEmpty(desc.Description) ? "" : desc.Description;
                    device.State = ModuleState.Idle;
                    device.MaxInstance = desc.MaxInstance;
                    device.Sn = string.IsNullOrEmpty(desc.Sn) ? "" : desc.Sn;
                    device.Parameters = GetDeviceParameters(type, desc.FeatureType, desc.Name);
                    device.Capability = desc.DeviceCapability;
                    var constraints = GetEmbeddedConstraints(type, desc.Name);
                    if (constraints != null) device.ConstraintScript = constraints;
                    var json = JsonConvert.SerializeObject(device, Formatting.Indented);
                    var configPath = Path.Combine(fullPath, $"{fileName}.json");
                    if (File.Exists(configPath)) File.Delete(configPath);
                    // continue;
                    File.WriteAllText(configPath, json, Encoding.UTF8);
                    // Console.WriteLine(json);
                    Console.WriteLine($"    导出模板成功-Device:{fileName} Version:{device.Version}");
                }
            }
        }
    }

    private void ExportDriver(string path)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        var dir = _outputPath; //$"{AppDomain.CurrentDomain.BaseDirectory}../../../doc/";
        // string fullPath = path.Replace(Path.GetFileName(path), "");
        var fullPath = dir;
        if (!Directory.Exists(fullPath))
            if (fullPath != null)
                Directory.CreateDirectory(fullPath);
        if (fullPath != null)
        {
            fullPath = Path.Combine(fullPath, "Driver");
            if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);
            var ass = Assembly.LoadFile(path);
            var types = ass.GetTypes();
            foreach (var type in types)
            {
                if (!TypesFactory.IsBaseTypeExists(type, typeof(DriverBase))) continue;
                var driver = new ModuleInfo();
                var funcDescriptions = type.GetCustomAttributes(typeof(DriverDescriptionAttribute), true);
                if (funcDescriptions.Length == 0) continue;
                var desc = funcDescriptions[0] as DriverDescriptionAttribute;
                Console.WriteLine($"开始导出模板-Driver:{fileName} Version:{desc?.Version}");
                driver.TemplateId = Guid.NewGuid();
                driver.DisplayName = desc?.Name;
                if (desc != null)
                {
                    driver.Feature = desc.FeatureType;
                    driver.Class = string.IsNullOrEmpty(type.FullName) ? "" : type.FullName;
                    driver.Description = string.IsNullOrEmpty(desc.Description) ? "" : desc.Description;
                    driver.ModuleType = ModuleType.Driver;
                    driver.Category = desc.Category;
                    driver.Manufacturer = "Aleph";
                    driver.State = ModuleState.Idle;
                    driver.Model = string.IsNullOrEmpty(desc.Model) ? "" : desc.Model;
                    driver.MaxInstance = 0;
                    driver.Version = string.IsNullOrEmpty(desc.Version) ? "" : desc.Version;
                    driver.Sn = "";
                    driver.IsMonopolized = desc.IsMonopoly;
                    driver.Parameters = GetDriverParameters(type, desc.FeatureType);
                }

                var json = JsonConvert.SerializeObject(driver, Formatting.Indented);
                var configPath = Path.Combine(fullPath, $"{fileName}.json");
                if (File.Exists(configPath)) File.Delete(configPath);
                // continue;
                File.WriteAllText(configPath, json, Encoding.UTF8);
                // Console.WriteLine(json);
                Console.WriteLine($"    导出模板成功-Driver:{fileName} Version:{driver.Version}");
            }
        }
    }

    private List<Parameter> GetDeviceParameters(Type type, FeatureType feature, string deviceName)
    {
        var list = new List<Parameter>();
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            var parameter = ConvertPropertyToParameter(property, feature, deviceName);
            if (parameter != null) list.Add(parameter);
        }

        // 设备参数，如果支持scan，需要将所有参数设置为scanSegments的子参数
        if ((feature & FeatureType.Scan) > 0)
        {
            var scanSegments = list.Find(item => item.Name == ParameterNames.ScanSegments) ?? new Parameter
            {
                Name = ParameterNames.ScanSegments,
                DisplayName = "频段信息",
                Description = "频段信息，存放频段扫描的频段信息",
                Category = PropertyCategoryNames.Scan,
                Feature = FeatureType.Scan,
                Owners = new List<string>(),
                Browsable = false
            };
            (scanSegments.Template ??= new List<Parameter>()).Clear();
            // TODO: 这里有问题，ps与list的值会同步修改，后面需要处理
            var ps = list.Where(item => (item.Feature & FeatureType.Scan) > 0).ToList().Select(item => item.Clone());
            list.RemoveAll(item => item.Name == ParameterNames.ScanSegments);
            list.ForEach(item =>
            {
                if (item.Name is ParameterNames.StartFrequency or ParameterNames.StopFrequency
                    or ParameterNames.StepFrequency) item.Browsable = false;
            });
            foreach (var item in ps)
            {
                if (item.Name == ParameterNames.ScanSegments) continue;
                item.Feature = FeatureType.Scan;
                scanSegments.Template.Add(item);
            }

            list.Add(scanSegments);
        }

        // 设备参数，如果支持离散测向，需要构造mfdfPoints参数
        if ((feature & FeatureType.Ffdf) > 0)
        {
            var mfdf = new Parameter
            {
                Name = ParameterNames.MfdfPoints,
                Feature = FeatureType.Ffdf,
                DisplayName = "离散测向频点",
                Description = "设置离散测向频点参数",
                Owners = new List<string>(),
                Browsable = false,
                Template = new List<Parameter>()
            };
            var tps = list.Where(item => item.Name is ParameterNames.Frequency or ParameterNames.DfBandwidth);
            var parameters = tps as Parameter[] ?? tps.ToArray();
            if (parameters.Any())
            {
                mfdf.Template.AddRange(parameters);
                Parameter thd = new()
                {
                    Name = ParameterNames.MeasureThreshold,
                    Category = PropertyCategoryNames.DirectionFinding,
                    Feature = FeatureType.Ffdf,
                    DisplayName = "测量门限",
                    Description = "获取或设置离散扫描进行占用度测量的门限值",
                    Minimum = -40,
                    Maximum = 120,
                    Step = 1,
                    Style = DisplayStyle.Slider,
                    Default = 20,
                    Value = 20,
                    Unit = UnitNames.DBuV,
                    Owners = new List<string>(),
                    Browsable = true,
                    SelectOnly = false,
                    ReadOnly = false,
                    Type = ParameterDataType.Number,
                    Children = new List<string>(),
                    RelatedValue = new List<object>(),
                    Values = new List<object>(),
                    DisplayValues = new List<string>(),
                    IsInstallation = false,
                    Parameters = new List<object>(),
                    Template = new List<Parameter>()
                };
                mfdf.Template.Add(thd);
                list.Add(mfdf);
            }
        }

        return list;
    }

    /// <summary>
    ///     获取参数列表
    /// </summary>
    /// <param name="type"></param>
    /// <param name="feature"></param>
    private List<Parameter> GetDriverParameters(Type type, FeatureType feature)
    {
        var list = new List<Parameter>();
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            var parameter = ConvertPropertyToParameter(property, feature, feature.ToString(), true);
            if (parameter != null) list.Add(parameter);
        }

        return list;
    }

    private List<Parameter> GetChildParameters(Type type, FeatureType feature, string name, bool isFunc = false)
    {
        var list = new List<Parameter>();
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            var parameter = ConvertPropertyToParameter(property, feature, name, isFunc);
            if (parameter != null) list.Add(parameter);
        }

        List<Parameter> parameters;
        // TODO:
        //var repeated = type.GetCustomAttributes(typeof(RepeatedParameterAttribute), true);
        //if (repeated == null || repeated.Length == 0)
        {
            parameters = list; //list.Select(i => (object)i).ToList();
        }
        // else
        // {
        //     parameters.Add(list);
        // }
        return parameters;
    }

    private Parameter ConvertPropertyToParameter(PropertyInfo property, FeatureType feature, string name,
        bool isFunc = false)
    {
        var desc = GetMultipleAttribute<ParameterAttribute>(property, name);
        if (desc == null) return null;
        if (feature != FeatureType.None && desc.AbilitySupport != FeatureType.None &&
            (feature & desc.AbilitySupport) == 0) return null;
        var nameAttribute = GetMultipleAttribute<NameAttribute>(property, name);
        var realFeature = desc.AbilitySupport & feature;
        var parameter = new Parameter
        {
            Feature = isFunc ? feature : realFeature == 0 ? FeatureType.None : realFeature,
            IsInstallation = desc.IsInstallation,
            Name = nameAttribute == null ? property.Name : nameAttribute.Name
        };
        try
        {
            var category = property.GetCustomAttribute<CategoryAttribute>();
            var display = property.GetCustomAttribute<DisplayNameAttribute>();
            var description = property.GetCustomAttribute<DescriptionAttribute>();
            var defaultValue = property.GetCustomAttribute<DefaultValueAttribute>();
            var propertyOrder = property.GetCustomAttribute<PropertyOrderAttribute>();
            var valueRange = GetMultipleAttribute<ValueRangeAttribute>(property, name);
            var unit = GetMultipleAttribute<UnitAttribute>(property, name);
            parameter.Category = category?.Category;
            parameter.DisplayName = display?.DisplayName;
            parameter.Description = description?.Description;
            parameter.Type = GetDataType(property.PropertyType);
            parameter.Default = defaultValue?.Value;
            if (property.PropertyType == typeof(Guid)) parameter.Default = Guid.Empty;
            if (parameter.Default != null && property.PropertyType.IsEnum)
                parameter.Default = ConvertValue(parameter.Default.ToString(), property.PropertyType);
            parameter.Minimum = valueRange?.Minimum;
            parameter.Maximum = valueRange?.Maximum;
            parameter.Step = valueRange?.Step;
            var standardValues = GetMultipleAttribute<StandardValuesAttribute>(property, name);
            if (standardValues != null)
            {
                var strArr = standardValues.DisplayValues;
                var strVal = standardValues.StandardValues;
                parameter.SelectOnly = standardValues.IsSelectOnly;
                var displays = strArr[1..].Split(strArr[0]).ToArray();
                var arr = strVal[1..].Split(strVal[0]);
                var values = arr.Where(item => !string.IsNullOrEmpty(item))
                    .Select(i => ConvertValue(i, property.PropertyType)).ToArray();
                if (values.Length != displays.Length)
                {
                    var msg = $"    !!:参数[{parameter.Name}]的值集合[{strVal}]与显示集合[{strArr}]不匹配";
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(msg);
                    Console.ResetColor();
                    throw new ArgumentException(msg);
                }

                SortArray(ref values, ref displays, ParamsSortOrdingRule);
                parameter.DisplayValues = displays.ToList();
                parameter.Values = values.ToList();
                if (standardValues.DefaultValue != null) parameter.Default = standardValues.DefaultValue;
            }

            parameter.Browsable = property.GetCustomAttribute<BrowsableAttribute>()?.Browsable != false;
            parameter.ReadOnly = property.GetCustomAttribute<ReadOnlyAttribute>()?.IsReadOnly == true;
            parameter.Order = propertyOrder?.Order ?? 0;
            parameter.Value = parameter.Default;
            parameter.Owners = new List<string>();
            parameter.Unit = unit?.Unit ?? "";
            if (Attribute.GetCustomAttribute(property, typeof(ChildrenAttribute)) is ChildrenAttribute children)
            {
                parameter.Children = children.Children;
                parameter.RelatedValue =
                    children.Values.ConvertAll(item => ConvertValue(item.ToString(), property.PropertyType));
            }

            if (Attribute.GetCustomAttribute(property, typeof(ModuleAttribute)) is ModuleAttribute module)
            {
                parameter.NeedFeature = module.NeedFeature;
                parameter.NeedModuleCategory = module.NeedModule;
                parameter.IsPrimaryDevice = module.IsPrimaryDevice;
                parameter.IsRequired = module.NeedEquip;
            }

            if (desc.Template != null)
            {
                var childParams = GetChildParameters(desc.Template, feature, name, isFunc);
                parameter.Template = childParams;
                parameter.Type = ParameterDataType.None;
            }
            else
            {
                parameter.Template = new List<Parameter>();
            }

            var style = GetMultipleAttribute<StyleAttribute>(property, name);
            if (style != null)
            {
                parameter.Style = style.Style;
            }
            else
            {
                parameter.Style = GetDisplayStyle(parameter);
                if (parameter.Style == DisplayStyle.Input && parameter.Step == null) parameter.Step = 100;
            }

            if (Attribute.GetCustomAttribute(property, typeof(ParametersDefaultAttribute)) is ParametersDefaultAttribute
                paramsDefault) parameter.Parameters = paramsDefault.DefaultParameters.ToList();
        }
        catch (Exception ex)
        {
            // Console.ForegroundColor = ConsoleColor.Red;
            var str = $"参数:{parameter.Name}导出失败,{ex.Message}";
            // Console.WriteLine(str);
            // Console.ResetColor();
            throw new Exception(str);
        }

        return parameter;
    }

    private static T GetMultipleAttribute<T>(PropertyInfo property, string name) where T : MultipleAttribute, new()
    {
        var attributes = property.GetCustomAttributes<T>();
        var multipleAttributes = attributes as T[] ?? attributes.ToArray();
        if (multipleAttributes.Any() != true) return null;
        var attribute = multipleAttributes.Where(p => p.IsMatch(name)).MaxBy(p => p.Level);
        return attribute;
    }

    private object ConvertValue(string strValue, Type propertyType)
    {
        if (propertyType.IsEnum)
        {
            if (Enum.TryParse(propertyType, strValue, true, out var value))
                return Magneto.Contract.Utils.ConvertEnumToString(value);
            return strValue;
        }

        return Convert.ChangeType(strValue, propertyType);
    }

    private ParameterDataType GetDataType(Type type)
    {
        if (type == typeof(string)) return ParameterDataType.String;
        if (type == typeof(double) || type == typeof(decimal)) return ParameterDataType.Number;
        if (type == typeof(float)) return ParameterDataType.Number;
        if (type == typeof(long)) return ParameterDataType.Number;
        if (type == typeof(byte) || type == typeof(short) || type == typeof(int))
            return ParameterDataType.Number;
        if (type == typeof(bool)) return ParameterDataType.Bool;
        if (type.IsArray || type.Name.StartsWith("List")) return ParameterDataType.List;
        if (type.IsEnum) return ParameterDataType.String;
        // var att = type.GetCustomAttributes(typeof(FlagsAttribute), true);
        // if (att != null && att.Length > 0)
        // {
        //     return DataType.Long;
        // }
        // else
        // {
        //     return DataType.Int;
        // }
        return ParameterDataType.String;
    }

    /// <summary>
    ///     将数值类型的集合按照从大到小或从小到大进行排序
    /// </summary>
    /// <param name="values"></param>
    /// <param name="displays"></param>
    /// <param name="desc">true:大到小,false:小到大</param>
    private static void SortArray(ref object[] values, ref string[] displays, bool desc)
    {
        var arr1 = new double[values.Length];
        var arr2 = new string[values.Length];
        for (var i = 0; i < values.Length; i++)
        {
            if (!double.TryParse(values[i].ToString(), out var db)) return;
            arr1[i] = db;
            arr2[i] = displays[i];
            for (var j = 0; j <= i; j++)
                if (desc)
                {
                    if (arr1[j] < arr1[i])
                    {
                        (arr1[i], arr1[j]) = (arr1[j], arr1[i]);
                        (arr2[i], arr2[j]) = (arr2[j], arr2[i]);
                    }
                }
                else if (arr1[j] > arr1[i])
                {
                    (arr1[i], arr1[j]) = (arr1[j], arr1[i]);
                    (arr2[i], arr2[j]) = (arr2[j], arr2[i]);
                }
        }

        values = arr1.Select(item => (object)item).ToArray();
        displays = arr2;
    }

    private DisplayStyle GetDisplayStyle(Parameter parameter)
    {
        if (parameter.Type == ParameterDataType.Bool
            && parameter.Values != null
            && parameter.DisplayValues != null
            && parameter.Values.Count > 0
            && parameter.DisplayValues.Count > 0)
            return DisplayStyle.Switch;
        if (parameter.SelectOnly
            && parameter.Values != null
            && parameter.DisplayValues != null
            && parameter.Values.Count > 0
            && parameter.DisplayValues.Count > 0)
        {
            if ((parameter.Minimum == null && parameter.Maximum == null) || parameter.Type == ParameterDataType.String)
                return parameter.Values.Count <= 4 ? DisplayStyle.Radio : DisplayStyle.Dropdown;
            if (parameter.Type == ParameterDataType.Number) return DisplayStyle.Slider;
        }

        if (parameter.Maximum != null
            && parameter.Minimum != null
            && parameter.Step != null)
            return DisplayStyle.Slider;
        if (!parameter.SelectOnly && parameter.Maximum == null && parameter.Minimum == null) return DisplayStyle.Input;
        return DisplayStyle.Default;
    }

    /// <summary>
    ///     获取嵌入的参数约束脚本
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    private JObject GetEmbeddedConstraints(Type type, string name)
    {
        const string constraints = "constraints.json";
        var assembly = type.Assembly;
        var ns = assembly.GetName().Name;
        var nsClass = type.Namespace;
        var names = assembly.GetManifestResourceNames();
        if (names.Any() != true) return null;
        var constraintResources =
            names.Where(p => Regex.IsMatch(p, @"\.\w*constraints\.json$", RegexOptions.IgnoreCase)).ToArray();
        if (constraintResources.Any() != true) return null;
        string resourceName;
        if (constraintResources.Length == 1)
        {
            resourceName = constraintResources[0];
        }
        else
        {
            var pattern =
                $"^(({ns?.Replace(".", "\\.").Replace("-", "\\-")})|({nsClass?.Replace(".", "\\.").Replace("-", "\\-")}))\\.{name}(_?){constraints}$";
            resourceName = Array.Find(constraintResources, p => Regex.IsMatch(p, pattern, RegexOptions.IgnoreCase));
            if (string.IsNullOrWhiteSpace(resourceName))
            {
                pattern =
                    $"(({ns?.Replace(".", "\\.").Replace("-", "\\-")})|({nsClass?.Replace(".", "\\.").Replace("-", "\\-")}))\\.{constraints}$";
                resourceName = Array.Find(names, p => Regex.IsMatch(p, pattern, RegexOptions.IgnoreCase));
                if (string.IsNullOrWhiteSpace(resourceName)) return null;
            }
        }

        var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null) return null;
        using var sr = new StreamReader(stream);
        var str = sr.ReadToEnd();
        stream.Close();
        // return str;
        var ja = JsonConvert.DeserializeObject<JObject>(str);
        return ja;
    }

    #region 生成模板

    public void GenerateTemplate(IEnumerable<string> path)
    {
        foreach (var item in path)
            if (Directory.Exists(item))
                ExportMouldDirectory(item);
            else if (File.Exists(item)) ExportMouldFile(item);
    }

    /// <summary>
    ///     根据目录导出模板
    /// </summary>
    /// <param name="dir"></param>
    public void ExportMouldDirectory(string dir)
    {
        foreach (var path in TypesFactory.FindFiles(dir))
        {
            if (Path.GetExtension(path) != ".dll") continue;
            ExportMouldFile(path);
        }
    }

    /// <summary>
    ///     根据文件路径导出模板
    /// </summary>
    /// <param name="filePath"></param>
    public void ExportMouldFile(string filePath)
    {
        if (Path.GetExtension(filePath) != ".dll") return;
        try
        {
            if (IsDriverOrDevice(filePath) == 0) ExportDevice(filePath);
            if (IsDriverOrDevice(filePath) == 1) ExportDriver(filePath);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }
    }

    #endregion

    #region 上传模板

    public void UploadTemplateToCloud(IEnumerable<string> paths, string url)
    {
        foreach (var path in paths)
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, "*.json");
                foreach (var file in files) Upload(file, url);
            }
            else if (File.Exists(path))
            {
                Upload(path, url);
            }
            else
            {
                Console.WriteLine($"路径{path}不正确，无法正确识别");
            }
    }

    private void Upload(string jsonPath, string url)
    {
        var module = new ModuleInfoForUpload
        {
            Template = LoadModuleInfo(jsonPath),
            Name = Path.GetFileName(jsonPath),
            Remark = "由边缘端上传"
        };
        var json = JsonConvert.SerializeObject(module);
        Console.WriteLine(CloudClient.Instance.PutModules(url, json)
            ? $"{Path.GetFileName(jsonPath)}上传成功"
            : $"{Path.GetFileName(jsonPath)}上传失败");
    }

    #endregion
}
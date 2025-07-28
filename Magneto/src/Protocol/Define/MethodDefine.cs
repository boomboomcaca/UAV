// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="MethodDefine.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using Magneto.Protocol.Extensions;
using MessagePack;
using Newtonsoft.Json;

namespace Magneto.Protocol.Define;

/// <summary>
///     Class MethodDefine.
/// </summary>
public static class MethodDefine
{
    /// <summary>
    ///     The message handler notify
    /// </summary>
    public const string MessageHandlerNotify = "channel.messageHandler.notify";

    /// <summary>
    ///     The data handler notify
    /// </summary>
    public const string DataHandlerNotify = "channel.dataHandler.notify";

    /// <summary>
    ///     进行语音识别的方法
    /// </summary>
    public const string AudioRecognitionMethod = "cloud.audioManager.recognition";
}

#region 定义

#region JsonRpc2.0协议/公共定义

/// <summary>
///     Class JsonRpcRequest.
/// </summary>
/// <typeparam name="T"></typeparam>
[MessagePackObject]
public class JsonRpcRequest<T>
{
    /// <summary>
    ///     Gets the json RPC.
    /// </summary>
    /// <value>The json RPC.</value>
    [Key("jsonrpc")]
    public string JsonRpc => "2.0";

    /// <summary>
    ///     Gets or sets the identifier.
    /// </summary>
    /// <value>The identifier.</value>
    [Key("id")]
    public object Id { get; set; }

    /// <summary>
    ///     Gets or sets the method.
    /// </summary>
    /// <value>The method.</value>
    [Key("method")]
    public string Method { get; set; }

    /// <summary>
    ///     Gets or sets the parameters.
    /// </summary>
    /// <value>The parameters.</value>
    [Key("params")]
    public T Params { get; set; }
}

/// <summary>
///     Class JsonRpcResult.
/// </summary>
[MessagePackObject]
public class JsonRpcResult
{
    /// <summary>
    ///     Gets the json RPC.
    /// </summary>
    /// <value>The json RPC.</value>
    [Key("jsonrpc")]
    public string JsonRpc => "2.0";

    /// <summary>
    ///     Gets or sets the identifier.
    /// </summary>
    /// <value>The identifier.</value>
    [Key("id")]
    public object Id { get; set; }

    /// <summary>
    ///     Gets or sets the result.
    /// </summary>
    /// <value>The result.</value>
    [Key("result")]
    public SharedData Result { get; set; }
}

// [MessagePackObject]
// public class ResultResponse
// {
//     [Key("result")]
//     public bool Result { get; set; }
//     [Key("message")]
//     public string Message { get; set; }
// }
/// <summary>
///     Class SharedData.
/// </summary>
[MessagePackObject]
public class SharedData
{
    /// <summary>
    ///     Gets or sets the edge identifier.
    /// </summary>
    /// <value>The edge identifier.</value>
    [Key("edgeId")]
    public string EdgeId { get; set; }

    /// <summary>
    ///     Gets or sets the task identifier.
    /// </summary>
    /// <value>The task identifier.</value>
    [Key("taskId")]
    public string TaskId { get; set; }

    /// <summary>
    ///     Gets or sets the device identifier.
    /// </summary>
    /// <value>The device identifier.</value>
    [Key("deviceId")]
    public string DeviceId { get; set; }

    /// <summary>
    ///     Gets or sets the timestamp.
    /// </summary>
    /// <value>The timestamp.</value>
    [Key("timestamp")]
    public ulong Timestamp { get; set; }

    /// <summary>
    ///     Gets or sets the data collection.
    /// </summary>
    /// <value>The data collection.</value>
    [Key("dataCollection")]
    public List<object> DataCollection { get; set; }
}

#endregion

#region ISystem

/// <summary>
///     Class HelloResponse.
/// </summary>
[MessagePackObject]
public class HelloResponse
{
    /// <summary>
    ///     Gets or sets the message.
    /// </summary>
    /// <value>The message.</value>
    [JsonProperty("message")]
    [Key("message")]
    public string Message { get; set; }
}

/// <summary>
///     Class QueryStationResponse.
/// </summary>
[MessagePackObject]
public class QueryStationResponse
{
    /// <summary>
    ///     Gets or sets the station.
    /// </summary>
    /// <value>The station.</value>
    [JsonProperty("station")]
    [Key("station")]
    public StationInfo Station { get; set; }
}

/// <summary>
///     Class QueryDevicesResponse.
/// </summary>
[MessagePackObject]
public class QueryDevicesResponse
{
    /// <summary>
    ///     Gets or sets the devices.
    /// </summary>
    /// <value>The devices.</value>
    [JsonProperty("devices")]
    [Key("devices")]
    public List<ModuleInfo> Devices { get; set; }
}

/// <summary>
///     Class QueryDriversResponse.
/// </summary>
[MessagePackObject]
public class QueryDriversResponse
{
    /// <summary>
    ///     Gets or sets the drivers.
    /// </summary>
    /// <value>The drivers.</value>
    [JsonProperty("drivers")]
    [Key("drivers")]
    public List<ModuleInfo> Drivers { get; set; }
}

/// <summary>
///     Class AllInfoResponse.
/// </summary>
[MessagePackObject]
public class AllInfoResponse
{
    /// <summary>
    ///     Gets or sets the station information.
    /// </summary>
    /// <value>The station information.</value>
    [Key("station")]
    public StationInfo StationInfo { get; set; }

    /// <summary>
    ///     Gets or sets the driver information.
    /// </summary>
    /// <value>The driver information.</value>
    [Key("drivers")]
    public List<ModuleInfo> DriverInfo { get; set; }

    /// <summary>
    ///     Gets or sets the device information.
    /// </summary>
    /// <value>The device information.</value>
    [Key("devices")]
    public List<ModuleInfo> DeviceInfo { get; set; }
}

/// <summary>
///     Enum RsyncConfigType
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<RsyncConfigType>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<RsyncConfigType>))]
public enum RsyncConfigType
{
    /// <summary>
    ///     The start
    /// </summary>
    [Key("start")] Start,

    /// <summary>
    ///     The stop
    /// </summary>
    [Key("stop")] Stop,

    /// <summary>
    ///     The update
    /// </summary>
    [Key("update")] Update
}

/// <summary>
///     Class QueryDirectoryResponse.
/// </summary>
[MessagePackObject]
public class QueryDirectoryResponse
{
    /// <summary>
    ///     Gets or sets the directory.
    /// </summary>
    /// <value>The directory.</value>
    [Key("directory")]
    public string Directory { get; set; }

    /// <summary>
    ///     Gets or sets the file list.
    /// </summary>
    /// <value>The file list.</value>
    [Key("list")]
    public List<string> FileList { get; set; }
}

/// <summary>
///     Class QueryAvailableFeaturesResponse.
/// </summary>
[MessagePackObject]
public class QueryAvailableFeaturesResponse
{
    /// <summary>
    ///     Gets or sets the features.
    /// </summary>
    /// <value>The features.</value>
    [Key("features")]
    public List<AvailableFeature> Features { get; set; }
}

/// <summary>
///     Class AvailableFeature.
/// </summary>
[MessagePackObject]
public class AvailableFeature
{
    /// <summary>
    ///     Gets or sets the feature.
    /// </summary>
    /// <value>The feature.</value>
    [Key("feature")]
    public FeatureType Feature { get; set; }

    /// <summary>
    ///     Gets or sets the feature identifier.
    /// </summary>
    /// <value>The feature identifier.</value>
    [Key("id")]
    public Guid FeatureId { get; set; }
}

/*
[MessagePackObject]
public class HelloRequest
{
    [JsonProperty("name")]
    [Key("name")]
    public string Name { get; set; }
    [Key("age")]
    public int Age { get; set; }
}
[MessagePackObject]
public class SystemCallRequest
{
    [Key("type")]
    public SystemCallType Type { get; set; }
    [Key("tag")]
    public string Tag { get; set; }
}
*/
/// <summary>
///     Class FastEmtTaskResponse.
/// </summary>
[MessagePackObject]
public class FastEmtTaskResponse
{
    /// <summary>
    ///     Gets or sets the task identifier.
    /// </summary>
    /// <value>The task identifier.</value>
    [Key("taskId")]
    public Guid TaskId { get; set; }

    /// <summary>
    ///     Gets or sets the URI.
    /// </summary>
    /// <value>The URI.</value>
    [Key("uri")]
    public string Uri { get; set; }

    /// <summary>
    ///     Gets or sets the ip address.
    /// </summary>
    /// <value>The ip address.</value>
    [Key("ipAddress")]
    public string IpAddress { get; set; }
}

#endregion

#region ITask

/// <summary>
///     Class PresetTaskResponse.
/// </summary>
[MessagePackObject]
public class PresetTaskResponse
{
    /// <summary>
    ///     Gets or sets the task identifier.
    /// </summary>
    /// <value>The task identifier.</value>
    [Key("taskId")]
    public Guid TaskId { get; set; }

    /// <summary>
    ///     Gets or sets the URI.
    /// </summary>
    /// <value>The URI.</value>
    [Key("uri")]
    public string Uri { get; set; }

    /// <summary>
    ///     Gets or sets the ip address.
    /// </summary>
    /// <value>The ip address.</value>
    [Key("ipAddress")]
    public string IpAddress { get; set; }
}

/*
[MessagePackObject]
public class PresetRequest
{
    [Key("feature")]
    public FeatureType Feature { get; set; }
    [Key("moduleId")]
    public Guid DriverId { get; set; }
}
[MessagePackObject]
public class StartTaskRequest
{
    [Key("taskId")]
    public Guid TaskId { get; set; }
}
[MessagePackObject]
public class StopTaskRequest
{
    [Key("taskId")]
    //[MessagePackFormatter(typeof(EnumAsStringFormatter<FeatureType>))]
    public Guid TaskId { get; set; }
}
[MessagePackObject]
public class SetParametersRequest
{
    [Key("taskId")]
    public Guid TaskId { get; set; }
    [Key("parameters")]
    public List<Parameter> Parameters { get; set; }
}
*/

#endregion

#region IEnvironment

/*
[MessagePackObject]
public class SetEnvParametersRequest
{
    [Key("moduleId")]
    public Guid ModuleID { get; set; }
    [Key("parameters")]
    public List<Parameter> Parameters { get; set; }
}
*/

#endregion

#region IAudioRecognition

/// <summary>
///     Class AudioRecognitionResponse.
/// </summary>
[MessagePackObject]
public class AudioRecognitionResponse
{
    /// <summary>
    ///     Gets or sets the message.
    /// </summary>
    /// <value>The message.</value>
    [Key("message")]
    public string Message { get; set; }
}

/// <summary>
///     Class AudioRecognitionRequest.
/// </summary>
[MessagePackObject]
public class AudioRecognitionRequest
{
    /// <summary>
    ///     Gets or sets the edge identifier.
    /// </summary>
    /// <value>The edge identifier.</value>
    [Key("edgeId")]
    public string EdgeId { get; set; }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public byte[] Data { get; set; }
}

#endregion

#endregion
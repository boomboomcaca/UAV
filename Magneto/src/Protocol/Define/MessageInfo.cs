// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="MessageInfo.cs" company="锟缴讹拷锟斤拷锟斤拷锟斤拷锟斤拷息锟斤拷锟斤拷锟斤拷锟睫癸拷司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace Magneto.Protocol.Define;

/// <summary>
///     Class MessageInfo.
/// </summary>
public class MessageInfo
{
    /// <summary>
    ///     Gets or sets the type of the message.
    /// </summary>
    /// <value>The type of the message.</value>
    public MessageType MessageType { get; set; }

    /// <summary>
    ///     Gets or sets the content.
    /// </summary>
    /// <value>The content.</value>
    public object Content { get; set; }
}

/// <summary>
///     Enum MessageType
/// </summary>
public enum MessageType
{
    /// <summary>
    ///     The GPS
    /// </summary>
    Gps = 0,

    /// <summary>
    ///     The compass
    /// </summary>
    Compass,

    /// <summary>
    ///     The ads b
    /// </summary>
    AdsB,

    /// <summary>
    ///     The ais
    /// </summary>
    Ais,

    /// <summary>
    ///     The device state
    /// </summary>
    DeviceState = 97,

    /// <summary>
    ///     The driver state
    /// </summary>
    DriverState = 98,

    /// <summary>
    ///     The log
    /// </summary>
    Log = 99
}
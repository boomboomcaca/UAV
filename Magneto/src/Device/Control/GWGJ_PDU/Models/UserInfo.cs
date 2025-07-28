namespace Magneto.Device.GWGJ_PDU.Models;

public class UserInfo
{
    /// <summary>
    ///     用户名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     密码
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    ///     状态
    /// </summary>
    public bool Available { get; set; }

    /// <summary>
    ///     可控制插座口
    /// </summary>
    public byte Port { get; set; }

    /// <summary>
    ///     序号
    /// </summary>
    public byte Id { get; set; }
}
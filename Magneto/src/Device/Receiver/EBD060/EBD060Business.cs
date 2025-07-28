using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.EBD060;

public partial class Ebd060
{
    #region 声卡数据采集

    private void AudioProc(IntPtr lpData, int cbSize)
    {
        if (AudioSwitch)
        {
            var data = new byte[cbSize];
            Marshal.Copy(lpData, data, 0, cbSize);
            var audio = new SDataAudio
            {
                Format = AudioFormat.Pcm,
                Data = data
            };
            var result = new List<object> { audio };
            SendData(result);
        }
    }

    #endregion
}
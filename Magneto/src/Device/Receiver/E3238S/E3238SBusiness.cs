using System;

namespace Magneto.Device.E3238S;

public partial class E3238S
{
    #region 插值采样处理

    public class CommonInterpolit
    {
        /// <summary>
        ///     插值采样
        /// </summary>
        public static float[] InterSample(float[] data, int afterFs, int beforeFs)
        {
            var sampleData = beforeFs <= afterFs ? data : Sample(data, afterFs, beforeFs);
            return sampleData;
        }

        private static float[] Sample(float[] data, int afterFs, int beforeFs)
        {
            float[] sampleData;
            if (beforeFs > afterFs)
            {
                // 采样比
                var ratio = beforeFs / (double)afterFs;
                var picLength = (int)(data.Length / ratio);
                var sampDatas = new float[picLength][];
                sampleData = new float[picLength];
                var picCount = (int)Math.Round(ratio);
                var iNowNum = 0;
                for (var i = 0; i < picLength; i++)
                    if (iNowNum + picCount < data.Length)
                    {
                        sampDatas[i] = new float[picCount];
                        Array.Copy(data, iNowNum, sampDatas[i], 0, picCount);
                        sampleData[i] = sampDatas[i][sampDatas[i].Length / 2];
                        iNowNum += picCount;
                    }
                    else
                    {
                        sampDatas[i] = Array.Empty<float>();
                        sampleData[i] = sampleData[i - 1];
                    }
            }
            else
            {
                sampleData = InterSample(data, afterFs, beforeFs);
            }

            return sampleData;
        }
    }

    #endregion
}
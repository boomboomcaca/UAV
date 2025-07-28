using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magneto.Device.RFeyeGps;

public partial class RFeyeGps
{
    #region 辅助类型定义

    //用以标识FieldName和ParameterName
    internal static class PacketKey
    {
        //brief Retrieve GPS information
        public const int FieldGps = 0x53504753; //BuildKey('S', 'G', 'P', 'S')
        public const int GpsLatitude = 0x4954414C; //BuildKey('L', 'A', 'T', 'I')
        public const int GpsLongitude = 0x474E4F4C; //BuildKey('L', 'O', 'N', 'G')
        public const int GpsSatellites = 0x53544153; //BuildKey('S', 'A', 'T', 'S')
        public const int GpsFix = 0x58494647; //BuildKey('G', 'F', 'I', 'X')
        public const int GpsStatus = 0x54415453; //BuildKey('S', 'T', 'A', 'T')
        public const int GpsUtim = 0x4D495455; //BuildKey('U', 'T', 'I', 'M')
        public const int GpsSpeed = 0x45455053; //BuildKey('S', 'P', 'E', 'E')
        public const int GpsHeading = 0x44414548; //BuildKey('H', 'E', 'A', 'D')
        public const int GpsAltitude = 0x49544C41; //BuildKey('A', 'L', 'T', 'I')

        public const int GpsDatetimeString = 0x52545354; //BuildKey('T', 'S', 'T', 'R')

        //brief Update/Retrieve reference clock source
        public const int FieldRefClock = 0x4B4C4352; //BuildKey('R', 'C', 'L', 'K')
        public const int RefClockSourceDac = 0x43414452; //BuildKey('R', 'D', 'A', 'C')
        public const int RefClockSourceGps = 0x53504752; //BuildKey('R', 'G', 'P', 'S')
        public const int RefClockSourceExp1 = 0x31584552; //BuildKey('R', 'E', 'X', '1')
        public const int RefClockSourceExp2 = 0x32584552; //BuildKey('R', 'E', 'X', '2')
        public const int RefClockOutExp1 = 0x3158454F; //BuildKey('O', 'E', 'X', '1')
        public const int RefClockOutExp2 = 0x3258454F; //BuildKey('O', 'E', 'X', '2')
        public const int RefClockDisableTimeTransmission = 0x54544452; //BuildKey('R', 'D', 'T', 'T')

        public const int RefClockDacSetting = 0x43414453; //BuildKey('S', 'D', 'A', 'C')

        //brief Update/Retrieve RTC settings
        public const int FieldDspRtc = 0x4C435452; //BuildKey('R', 'T', 'C', 'L')
        public const int DspRtcGetTime = 0x4D495447; //BuildKey('G', 'T', 'I', 'M')
        public const int DspRtcGetTimeFutureNs = 0x55465447; //BuildKey('G', 'T', 'F', 'U')
        public const int DspRtcSetTime = 0x4D495453; //BuildKey('S', 'T', 'I', 'M')
        public const int DspRtcNowDate = 0x5441444E; //BuildKey('N', 'D', 'A', 'T')
        public const int DspRtcNowTime = 0x4D49544E; //BuildKey('N', 'T', 'I', 'M')
        public const int DspRtcNowUnixTime = 0x584E554E; //BuildKey('N', 'U', 'N', 'X')
        public const int DspRtcNowNano = 0x4E414E4E; //BuildKey('N', 'N', 'A', 'N')
        public const int DspRtcNowStr = 0x5254534E; //BuildKey('N', 'S', 'T', 'R')
        public const int DspRtcFutureUnixTime = 0x584E5546; //BuildKey('F', 'U', 'N', 'X')
        public const int DspRtcFutureNano = 0x4E414E46; //BuildKey('F', 'N', 'A', 'N')

        public const int DspRtcFutureString = 0x52545346; //BuildKey('F', 'S', 'T', 'R')

        //brief Update/retrieve configuration options of NCPd
        public const int FieldConfigure = 0x464E4F43; //BuildKey('C', 'O', 'N', 'F')

        //Automatically add GPS information to all returned packets
        public const int ConfigureGpsAll = 0x4C535047; //BuildKey('G', 'P', 'S', 'L')

        //"Any" DSP options can be included\\returned in any packet of type DSPC or DSPL
        public const int AnyDspRtcUnixTime = 0x4D495455; //BuildKey('U', 'T', 'I', 'M')

        public const int AnyDspRtcNano = 0x4F4E414E; //BuildKey('N', 'A', 'N', 'O')

        //"Any" options can be returned in any packet to the client
        public const int AnyErrorCode = 0x43525245; //BuildKey('E', 'R', 'R', 'C')
        public const int AnyWarningCode = 0x43524157; //BuildKey('W', 'A', 'R', 'C')
        public const int AnyAcknowledgePacket = 0x4E4B4341; //BuildKey('A', 'C', 'K', 'N')
        public const int AnyDataSegmentNumber = 0x4E474553; //BuildKey('S', 'E', 'G', 'N')
        public const int AnyNumDataSegments = 0x4745534E; //BuildKey('N', 'S', 'E', 'G')

        public const int AnyAntennaUid = 0x44495541; //BuildKey('A', 'U', 'I', 'D')

        //TODO
        public const int LinkFieldServerGreeting = 0x4F4C4548; //BuildKey('H', 'E', 'L', 'O')
        public const int LinkFieldClientConnReq = 0x45524343; //BuildKey('C', 'C', 'R', 'E')
        public const int LinkFieldServerAuthReq = 0x52414353; //BuildKey('S', 'C', 'A', 'R')
        public const int LinkFieldClientAuthResp = 0x45524143; //BuildKey('C', 'A', 'R', 'E')
        public const int LinkFieldServerCobfirm = 0x4E4F4353; //BuildKey('S', 'C', 'O', 'N')
        public const int LinkFieldTermReq = 0x4D524554; //BuildKey('T', 'E', 'R', 'M')
        public const int LinkParamClientId = 0x574943; //BuildKey('C', 'I', 'W', 0)

        public const int LinkParamClientAuth = 0x524143; //BuildKey('C', 'A', 'R', 0);
        //public static int BuildKey(char first, char second, char third, char fourth)
        //{
        //    return (first + (second << 8) + (third << 16) + (fourth << 24));
        //}
    }

    internal static class DataSize
    {
        //以下Size的单位都为4个字节
        public const int HeaderSize = 8;
        public const int FooterSize = 2;
        public const int FieldSize = 3;
        public const int IntParamSize = 3;
    }

    internal enum PacketType
    {
        //Link administration data(i.e. Keep alives)
        Link = 0x4B4E494C, //BuildKey('L', 'I', 'N', 'K')

        //Node status, Once per second
        Status = 0x54415453, //BuildKey('S', 'T', 'A', 'T')

        //Node control instructions
        Node = 0x45444F4E, //BuildKey('N', 'O', 'D', 'E')

        //DSP control instructions & data
        DspControl = 0x43505344, //BuildKey('D', 'S', 'P', 'C')

        //DSP control background instructions & data
        DspLoop = 0x4C505344, //BuildKey('D', 'S', 'P', 'L')

        //deprecated Removed
        Crfs = 0x53465243 //BuildKey('C', 'R', 'F', 'S')
    }

    internal enum ParamType
    {
        Int = 0x00, //32-bit signed integer value (int32_t)
        UnsignedInt = 0x01, //32-bit un-signed integer value (uint32_t)
        String = 0x02, //String (char[])
        DataRaw = 0x80, //Raw data type
        DataUnsigned8 = 0x81, //Array of unsigned 8 bit bytes (uint8_t[])
        DataUnsigned16 = 0x82, //Array of unsigned 16 bit bytes (uint16_t[])
        DataUnsigned32 = 0x83, //Array of unsigned 32 bit bytes (uint32_t[])
        DataSigned8 = 0x84, //Array of signed 8 bit bytes (int8_t[])
        DataSigned16 = 0x85, //Array of signed 16 bit bytes (int16_t[])
        DataSigned32 = 0x86 //Array of signed 32 bit bytes (int32_t[])
    }

    internal enum ClientConnectionState
    {
        TcpConnectionEstablished,
        ReceivedGreeting,
        SentClientConnectionRequest,
        ReceivedAuthenticationRequest,
        SentAuthenticationResponse,
        ReceivedAuthenticationOk,
        ConnectionActive
    }

    #endregion

    #region 数据协议

    internal class Packet
    {
        //
        private int _nextFieldId;

        //包尾信息
        public Footer FooterInfo;

        //包头信息
        public Header HeaderInfo;

        //内容
        public List<Field> ListFieldInfo = new();

        //从接收到的二进制数据中解析数据包
        public static Packet Parse(byte[] value, int startIndex)
        {
            var packet = new Packet
            {
                HeaderInfo = Header.Parse(value, startIndex)
            };
            var fieldStartPos = DataSize.HeaderSize;
            var fieldEndPos = (int)packet.HeaderInfo.PacketSize - DataSize.FooterSize;
            while (fieldStartPos < fieldEndPos)
            {
                var field = Field.Parse(value, startIndex + (fieldStartPos << 2));
                packet.ListFieldInfo.Add(field);
                fieldStartPos += field.GetSize();
            }

            packet.FooterInfo = new Footer();
            return packet;
        }

        //获取序列化后的二进制数据包
        public byte[] GetBytes()
        {
            var bytes = new List<byte>();
            bytes.AddRange(HeaderInfo.GetBytes());
            foreach (var field in ListFieldInfo) bytes.AddRange(field.GetBytes());
            bytes.AddRange(FooterInfo.GetBytes());
            return bytes.ToArray();
        }

        //打包包头
        public void BeginPacket(PacketType packetType, int packetId)
        {
            HeaderInfo = new Header
            {
                PacketTypeData = (int)packetType
            };
            HeaderInfo.PacketId = packetId == -1 ? ++HeaderInfo.PacketId : packetId;
            //_header.PacketSize = DataSize.HeaderSize; //PacketSize以4字节为单位
            HeaderInfo.PacketTimeSecond = 0; //TODO: 暂时写0
            HeaderInfo.PacketTimeNanosecond = 0;
        }

        //添加数据
        public void AddField(int fieldName, int fieldId)
        {
            ListFieldInfo ??= new List<Field>();
            var field = new Field
            {
                FieldName = fieldName,
                Info = 0,
                FieldId = fieldId == -1 ? ++_nextFieldId : fieldId
            };
            ListFieldInfo.Add(field);
        }

        //添加请求参数
        public void AddParamInt(int paramName, int data)
        {
            var field = ListFieldInfo.Last();
            field.AddParamInt(paramName, data);
        }

        public void AddParamString(int paramName, string data)
        {
            var field = ListFieldInfo.Last();
            field.AddParamString(paramName, data);
        }

        //打包包尾
        public void EndPacket()
        {
            //在上一个Field中填充下一个Field的相对位置，单位为4字节
            foreach (var field in ListFieldInfo) field.Info = (uint)field.GetSize();
            //添加数据包尾
            FooterInfo = new Footer
            {
                Checksum = 0,
                FooterCode = 0xDDCCBBAA
            };
            //更新数据包长度
            var packetBuffer = GetBytes();
            HeaderInfo.PacketSize = (uint)(packetBuffer.Length / 4);
        }
    }

    internal class Header
    {
        //Header to allow successful packet sync & decode (0xAABBCCDD)
        public uint HeaderCode = 0xAABBCCDD;

        //Packet format information
        public uint PacketFormat = 1;

        //User definable identifier word
        public int PacketId;

        //Packet size in 32 bit words of the entire packet
        public uint PacketSize;

        public uint PacketTimeNanosecond;

        //Packet time, second / nanoseconds
        public uint PacketTimeSecond;

        //Packet type
        public int PacketTypeData;
        public uint Spare;

        public static Header Parse(byte[] value, int startIndex)
        {
            var header = new Header
            {
                HeaderCode = BitConverter.ToUInt32(value, startIndex),
                PacketTypeData = BitConverter.ToInt32(value, startIndex + 4),
                PacketSize = BitConverter.ToUInt32(value, startIndex + 8),
                PacketId = BitConverter.ToInt32(value, startIndex + 12),
                PacketFormat = BitConverter.ToUInt32(value, startIndex + 16),
                PacketTimeSecond = BitConverter.ToUInt32(value, startIndex + 20),
                PacketTimeNanosecond = BitConverter.ToUInt32(value, startIndex + 24),
                Spare = BitConverter.ToUInt32(value, startIndex + 28)
            };
            return header;
        }

        public byte[] GetBytes()
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(HeaderCode));
            bytes.AddRange(BitConverter.GetBytes(PacketTypeData));
            bytes.AddRange(BitConverter.GetBytes(PacketSize));
            bytes.AddRange(BitConverter.GetBytes(PacketId));
            bytes.AddRange(BitConverter.GetBytes(PacketFormat));
            bytes.AddRange(BitConverter.GetBytes(PacketTimeSecond));
            bytes.AddRange(BitConverter.GetBytes(PacketTimeNanosecond));
            bytes.AddRange(BitConverter.GetBytes(Spare));
            return bytes.ToArray();
        }
    }

    internal class Field
    {
        //all parameters in current field
        public readonly List<Parameter> ListParameter = new();

        //User definable identifier word. Returned in responses
        public int FieldId;

        //Name key
        public int FieldName;

        //Bits 0-23 Next field position, Bits 24-31 data type
        public uint Info;

        //TODO:此成员仅供调试时方便查看
        public string Name;

        //get the serialized binary data
        public byte[] GetBytes()
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(FieldName));
            bytes.AddRange(BitConverter.GetBytes(Info));
            bytes.AddRange(BitConverter.GetBytes(FieldId));
            foreach (var param in ListParameter) bytes.AddRange(param.GetBytes());
            return bytes.ToArray();
        }

        //Size in 32 bit words of the entire field
        public int GetSize()
        {
            var totalSize = DataSize.FieldSize;
            foreach (var param in ListParameter) totalSize += param.GetSize();
            return totalSize;
        }

        //add parameter
        public void AddParamString(int paramName, string data)
        {
            var bytes = Encoding.ASCII.GetBytes(data);
            Array.Resize(ref bytes, bytes.Length + 1); //需要包含最后一位'\0'
            var param = new Parameter
            {
                ParameterName = paramName
            };
            var len32 = bytes.Length >> 2;
            if ((bytes.Length & 0x03) != 0) len32++;
            param.Info = (uint)(2 + len32 + ((int)ParamType.String << 24));
            if (bytes.Length != len32 << 2) Array.Resize(ref bytes, len32 << 2);
            param.Data = bytes;
            ListParameter.Add(param);
        }

        //add parameter
        public void AddParamInt(int paramName, int data)
        {
            var param = new Parameter
            {
                ParameterName = paramName,
                Info = DataSize.IntParamSize,
                Data = BitConverter.GetBytes(data)
            };
            ListParameter.Add(param);
        }

        //parse the field info from the received binary data 
        public static Field Parse(byte[] value, int startIndex)
        {
            var field = new Field
            {
                FieldName = BitConverter.ToInt32(value, startIndex),
                Name = Encoding.ASCII.GetString(value, startIndex, 4), //TODO:此成员仅供调试时方便查看
                Info = BitConverter.ToUInt32(value, startIndex + 4),
                FieldId = BitConverter.ToInt32(value, startIndex + 8)
            };
            var nextFieldOffset = field.Info & 0xFFFFFF; //该值包含了当前的FieldSize
            var paramStartPos = DataSize.FieldSize;
            while (paramStartPos < nextFieldOffset)
            {
                var param = Parameter.Parse(value, startIndex + (paramStartPos << 2));
                field.ListParameter.Add(param);
                paramStartPos += param.GetSize();
            }

            return field;
        }
    }

    internal class Parameter
    {
        //Packet payload of parameter type
        public byte[] Data;

        //Bits 0-23 Length & Offset to next field position, Bits 24-31 data type
        public uint Info;

        //TODO:此成员仅供调试时方便查看
        public string Name;

        //Name key
        public int ParameterName;

        //get the serialized binary data
        public byte[] GetBytes()
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(ParameterName));
            bytes.AddRange(BitConverter.GetBytes(Info));
            if (Data.Length > 0) bytes.AddRange(Data);
            return bytes.ToArray();
        }

        //Size in 32 bit words of the entire parameter
        public int GetSize()
        {
            return (int)(Info & 0xFFFFFF);
        }

        //Get the parameter type
        public ParamType GetParamType()
        {
            var type = (int)(Info >> 24);
            return (ParamType)type;
        }

        //parse the parameter info from the received binary data 
        public static Parameter Parse(byte[] value, int startIndex)
        {
            var param = new Parameter
            {
                ParameterName = BitConverter.ToInt32(value, startIndex),
                Name = Encoding.ASCII.GetString(value, startIndex, 4), //TODO:此成员仅供调试时方便查看
                Info = BitConverter.ToUInt32(value, startIndex + 4)
            };
            var paramLen = (int)(param.Info & 0xFFFFFF); //该值包含了整个Parameter的大小
            param.Data = new byte[(paramLen - 2) * 4];
            Buffer.BlockCopy(value, startIndex + 8, param.Data, 0, param.Data.Length);
            return param;
        }
    }

    internal class Footer
    {
        //Not used. Always 0
        public uint Checksum;

        //Footer to allow successful packet sync & decode (0xDDCCBBAA)
        public uint FooterCode = 0xDDCCBBAA;

        //get the serialized binary data
        public byte[] GetBytes()
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(Checksum));
            bytes.AddRange(BitConverter.GetBytes(FooterCode));
            return bytes.ToArray();
        }
    }

    #endregion
}
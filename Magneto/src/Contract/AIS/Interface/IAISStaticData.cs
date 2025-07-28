using System;

namespace Magneto.Contract.AIS.Interface;

public interface IAisStaticData : IAisMessage
{
    string ShipName { get; }
    string CallSign { get; }
    int DimensionA { get; }
    int DimensionB { get; }
    int DimensionC { get; }
    int DimensionD { get; }
    CountryId CountryId { get; }
}

public interface IAisStaticAVoyage : IAisStaticData
{
    /// <summary>
    ///     IMO�����ʺ�����֯��ţ�
    /// </summary>
    public int Imo { get; }

    /// <summary>
    ///     ��������
    /// </summary>
    public ShipType ShipType { get; }

    /// <summary>
    ///     Ԥ�Ƶ���ʱ��
    /// </summary>
    public DateTime Eta { get; }

    /// <summary>
    ///     ���̬��ˮ���
    /// </summary>
    public double Draught { get; }

    /// <summary>
    ///     Ŀ�ĵ�
    /// </summary>
    public string Destination { get; }

    /// <summary>
    ///     ��ϢԴID
    /// </summary>
    public int MsgSrc { get; }
}
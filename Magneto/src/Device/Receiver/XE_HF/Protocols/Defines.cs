namespace Magneto.Device.XE_HF.Protocols;

internal static class MessageId
{
    //Acknowledgement
    public const int MreAcquittement = 0x01;

    //BB Interception request
    public const int MreIntLb = 0x02;

    //Stop processing
    public const int MreArretTrait = 0x03;

    //External switch configuration request
    public const int MreDemConfigcommut = 0x05;

    //External switch configuration result
    public const int MreResConfigcommut = 0x06;

    //Test request
    public const int MreDemTest = 0x07;

    //Test result
    public const int MreResTest = 0x08;

    //Hardware configuration request
    public const int MreDemConfigmat = 0x09;

    //Hardware configuration result
    public const int MreResConfigmat = 0x0A;

    //IQ recording status
    public const int MreEtatSaveIq = 0x0B;

    //IQ flux request
    public const int MreDemFluxIq = 0x0C;

    //NDDC status
    public const int MreEtatNddc = 0x0E;

    //Manual direction-finding request
    public const int MreDemGonioManual = 0x15;

    //LO test request
    public const int MreDemLoTest = 0x16;

    //Change of dipole request
    public const int MreDemChangeDipole = 0x17;

    //Cable calibration request
    public const int MreDemTarage = 0x18;

    //Cable calibration result, TODO
    public const int MreResTarage = 0x19;

    //FFT results
    public const int MreResFft = 0x20;

    //Extraction results
    public const int MreResExtract = 0x30;

    //Interfero extraction results, TODO:
    public const int MreResExtractInterfero = 0x32;

    //GSM Extraction results
    public const int MreResGsmExtract = 0x33;

    //Direction-finding results
    public const int MreResGonio = 0x40;

    //Maintenance direction-finding results(digit algorithm) TODO:
    public const int MreResGonioMaint = 0x41;

    //Maintenance direction finding result(vector correlation algorithm) TODO:
    public const int MreResGonioMaintCorrelationVect = 0x43;

    // Maintenance direction finding result(interfero algorithm) TODO:
    public const int MreResGonioMaintInterfero = 0x44;

    //Interfero direction finding results TODO:
    public const int MreResGonioInterfero = 0x45;

    // Interfero elevation histogram results TODO:
    public const int MreResElevationHisto = 0x46;

    //IQ production results
    public const int MreResIq = 0x50;

    //Parameter adjustment data
    public const int MreInfoReglageParams = 0x60;

    //Available status request
    public const int MreDemEtatDispo = 0x62;

    //Available status results
    public const int MreResEtatDispo = 0x63;

    //IQ sample request
    public const int MreDemEchiq = 0x64;

    //Update of Time / DF data
    public const int MreMajinfos = 0x66;

    //Receiver programming request
    public const int MreDemProgrx = 0x67;

    //Receiver programming Modification/Result
    public const int MreProgrx = 0x68;

    //Masking Request
    public const int MreDemMask = 0x69;

    //Datation Type
    public const int MreDemTypeDatation = 0x6A;

    //Software configuration request
    public const int MreDemConfiglog = 0x70;

    //Software configuration result
    public const int MreResConfiglog = 0x71;

    //Current state result
    public const int MreResEtatCourant = 0x72;

    //Single shot DF request
    public const int MreDemGonioMonocoup = 0x73;

    //Level calibration request
    public const int MreDemCalibration = 0x74;

    //Level calibration result
    public const int MreResCalibration = 0x75;

    //Forced sub-band request
    public const int MreDemForcageSousGamme = 0x76;

    //Manual channels deletion request
    public const int MreDemSuppGonioManuel = 0x77;

    //Continuous direction-finding request
    public const int MreDemGonioContinu = 0x78;

    //AGC measurement request
    public const int MreDemMesureCag = 0x79;

    //Interface version used by LINUX.
    public const int MreInfoInterfaceVersion = 0x80;

    //Change of active phase at the initiative of LINUX
    public const int MreInfoChgtPhaseactive = 0x81;

    //ITU measurement request
    public const int MreDemMesureuit = 0x82;

    //Result of ITU measurements
    public const int MreResMesureuit = 0x83;

    //Audio demodulation request
    public const int MreDemDemodaudio = 0x84;

    //Audio recording request
    public const int MreDemRecordaudio = 0x85;

    //Request for addition of a list of manual channels
    public const int MreDemListeCanauxManuels = 0x86;

    //Configuration of the percentage occupancy, TODO
    public const int MreCfgTauxOccupation = 0x87;

    //Result of the percentage occupancy, TODO
    public const int MreResTauxOccupation = 0x88;

    //List of audio demodulators available, TODO
    public const int MreResConfiglogdemodaudio = 0x89;

    //Demodulated audio result, TODO
    public const int MreResAudio = 0x90;

    //Audio file end of recording result, TODO
    public const int MreResFinfichierwave = 0x91;

    //Current recording duration, TODO
    public const int MreResDureefichierecoute = 0x92;

    //Request for squelch during audio demodulation
    public const int MreDemSilencieux = 0x93;

    //COR activation request for recording, TODO
    public const int MreDemCor = 0x94;

    //Information of scanning cycle
    public const int MreInfoCycleBalayage = 0x97;

    //Antenna Modification
    public const int MreDemAntennaModification = 0x99;

    //Acquisition TDOA request, TODO
    public const int MreDemAcqTdoa = 0x9A;

    //Acquisition TDOA result, TODO
    public const int MreResTdoa = 0x9B;

    //Information of manuals channel group, TODO
    public const int MreInfoManualChannelsGroups = 0x9C;

    // Control of memory scanning
    public const int MreVcyCommand = 0x9D;

    //Send table of memory scanning emission
    public const int MreVcyTableEmission = 0x9E;

    //New memory scanning emission beginning
    public const int IdMsgVcyEmissionBeginning = 0x9F;

    //New memory scanning emission end
    public const int IdMsgVcyEmissionEnd = 0xA0;

    //Single station location configuration, TODO:
    public const int MreDemSingleStationLocationConfiguration = 0xA1;

    //Control of single station location, TODO:
    public const int MreDemSingleStationLocationControl = 0xA2;

    //Single station location results, TODO:
    public const int MreResSingleStationLocation = 0xA3;

    // Direction Finding Quality Mark Threshold Configuration
    public const int MreDemDfQualityThreshold = 0xA4;

    // Synchronization with a Jammer request
    public const int MreInfoSynchroJammer = 0xA5;

    //Verification of the presence of the WBAT. TODO:确认是0xA6还是0xA7?
    public const int MreInfoPresence = 0xA6;
}

internal static class DataType
{
    //for a group of parameters , i.e. an ILV message inside the ILV message received.
    public const int TypeEnsParam = 0x00;

    //for data defined by the user (data incompatible with basic types)
    public const int TypeUserDefine = 0x01;

    //for CHAR data type
    public const int TypeChar = 0x02;

    //for SHORT data type
    public const int TypeShort = 0x03;

    //for LONG data type
    public const int TypeLong = 0x04;

    //for int64 data type
    public const int TypeInt64 = 0x05;

    //for Unicode string type
    public const int TypeString = 0x06;

    //for unsigned CHAR data type
    public const int TypeUchar = 0x07;

    //for unsigned SHORT data type
    public const int TypeUshort = 0x08;

    //for unsigned LONG data type
    public const int TypeUlong = 0x09;

    //for unsigned int64 data type
    public const int TypeUint64 = 0x0A;

    //multi bytes string type
    public const int TypeStringMultiBytes = 0x0B;

    //for DOUBLE data type
    public const int TypeDouble = 0x0C;

    //for FLOAT data type
    public const int TypeFloat = 0x0D;
}

internal static class AckReturnCode
{
    public const uint Ok = 0x00;

    public static string GetCodeDescription(uint code)
    {
        var info = string.Empty;
        switch (code)
        {
            case 0x00:
                info = "OK";
                break;
            case 0x01:
                info = "KO";
                break;
            case 0x02:
                info = "Obsolete message";
                break;
            case 0x03:
                info = "Message is too large";
                break;
            case 0x10:
                info = "Test running";
                break;
            case 0x11:
                info = "Cannot end acquisition";
                break;
            case 0x12:
                info = "Cannot mount shared folder";
                break;
            case 0x13:
                info = "Too late to synchronize on PPS";
                break;
            case 0x20:
                info = "Too many units bands";
                break;
            case 0x21:
                info = "Not enough FFT channels";
                break;
            case 0x70:
                info = "VCY command cannot be executed because no VCY table has been defined";
                break;
            case 0x71:
                info = "Can’t switch VCY state because a command incompatible with the current state has been issued";
                break;
            case 0x72:
                info = "Acquisition parameter invalid";
                break;
            case 0x73:
                info =
                    "CTP mode (synchronization with a jammer) can’t be activated, permanent blanking (if GPS is not activated)";
                break;
            case 0x74:
                info =
                    "CTP mode (synchronization with a jammer) can’t be activated, permanent blanking (if not enough GPS)";
                break;
            case 0x75:
                info = "Memory scanning plan not compatible with the resolution";
                break;
            case 0x76:
                info = "Mission parameters not consistent";
                break;
            case 0x99:
                info = "Message number is not supported";
                break;
        }

        return info;
    }
}

internal enum ChannelType
{
    BroadBand = 0,
    NarrowBand = 1
}

internal enum MeasurementType
{
    //broadband FFT
    Bbfft = 0x00000001,

    //broadband direction-finding results
    Bbdf = 0x00000002,

    //extraction results
    Extraction = 0x00000004,

    //tracking results (not used)
    Tracking = 0x00000008,

    //ITU measurement results
    Itu = 0x00000010,

    //percentage occupancy results
    Occupany = 0x00000020,

    //maintenance direction-finding results
    Mtdf = 0x00000040,

    //narrow band IQ flux
    Nbiq = 0x00000080,

    //mission
    Mission = 0x00000100,

    //CTP mode (synchronization with a jammer)
    Ctp = 0x00000200,

    //Burst extraction on one direction-finding cycle for satellite communications
    BurstExtraction = 0x00000400,

    //optional direction-finding results for frequency extraction
    OptionalDf = 0x00000800
}

/// <summary>
///     测向模式
/// </summary>
internal enum XedFindMode
{
    Homing = 0, //归航模式
    Normal = 1, //常规,即宽带测向
    Continue = 2 //连续测向
}

internal enum XeScanMode
{
    Fast = 0, //快速截收, 25k, 快速模式
    Normal = 1, //常规, 可设置任意带宽, 快速模式
    Sensitivity = 2 //灵敏, 可设置25k及以下带宽，灵敏模式
}

internal struct XeAntennaSubRange
{
    //对应射频接入口，J1，J2，J3
    public int Jnumer;

    //最小频率，单位Hz
    public uint Fmin;

    //最大频率，单位Hz
    public uint Fmax;
}

internal enum ClientConnectionState
{
    TcpConnectionEstablished,
    ReceivedInterfaceVersion,
    SentHardwareConfigRequest,
    ReceivedHardwareConfigResult,
    SentAvailableStatusRequest,
    ReceivedAvailableStatusResult,
    SentSoftwareConfigRequest,
    ReceivedSoftwareConfigResult,
    SentTestRequest,
    ReceivedTestResult,
    ConnectionActive
}
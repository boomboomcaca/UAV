namespace Magneto.Device.G33DDC.SDK;

internal static class Define
{
    public const uint MinIfFftSize = 1024;
    public const uint MaxIfFftSize = 65536;
    public const uint MaxIfFreq = 50000000;
    public const uint MinFftSize = 1024;
    public const uint MaxFftSize = 32768;
    public const uint AudioSampleRate = 48000;
    public const short IfUpdateInterval = 50;
}

internal enum DemodulatorMode : uint
{
    /// <summary>
    ///     Continuous wave
    /// </summary>
    G3XddcModeCw = 0,

    /// <summary>
    ///     Amplitude modulation
    /// </summary>
    G3XddcModeAm,

    /// <summary>
    ///     Frequency modulation
    /// </summary>
    G3XddcModeFm,

    /// <summary>
    ///     Lower sideband modulation
    /// </summary>
    G3XddcModeLsb,

    /// <summary>
    ///     Upper sideband modulation
    /// </summary>
    G3XddcModeUsb,

    /// <summary>
    ///     Amplitude modulation
    /// </summary>
    G3XddcModeAms,

    /// <summary>
    ///     Double sideband modulation
    /// </summary>
    G3XddcModeDsb,

    /// <summary>
    ///     Independent sideband modulation
    /// </summary>
    G3XddcModeIsb,

    /// <summary>
    ///     Digital Radio Mondiale
    /// </summary>
    G3XddcModeDrm
}

internal enum DemodulatorCode : uint
{
    /// <summary>
    ///     Side band for synchronous AM demodulation.
    ///     The Buffer parameter has to be pointer to an UINT32 variable, and the BufferSize parameter has to be
    ///     sizeof(UINT32).
    ///     Value of the variable pointed to by the Buffer parameter can be one of the following:
    ///     G3XDDC_SIDE_BAND_LOWER: AMS demodulator will use lower sideband
    ///     G3XDDC_SIDE_BAND_UPPER: AMS demodulator will use upper sideband
    ///     G3XDDC_SIDE_BAND_BOTH: AMS demodulator will use both side bands.
    /// </summary>
    G3XddcDemodulatorParamAmsSideBand = 1,

    /// <summary>
    ///     Capture range of synchronous AM demodulator.
    ///     The Buffer parameter has to be pointer to a G3XDDC_AMS_CAPTURE_RANGE structure,
    ///     and the BufferSize parameter has to be sizeof(G3XDDC_AMS_CAPTURE_RANGE).
    /// </summary>
    G3XddcDemodulatorParamAmsCaptureRange,

    /// <summary>
    ///     CW tone frequency
    ///     The Buffer parameter has to be pointer to a INT32 variable, and the BufferSize parameter has to be sizeof(INT32).
    ///     Value of the variable pointed to by the Buffer parameter is CW tone frequency in Hz.
    /// </summary>
    G3XddcDemodulatorParamCwFrequency,

    /// <summary>
    ///     Side band for DSB demodulation.
    ///     The Buffer parameter has to be pointer to an UINT32 variable, and the BufferSize parameter has to be
    ///     sizeof(UINT32).
    ///     Value of the variable pointed to by the Buffer parameter can be one of the following:
    ///     G3XDDC_SIDE_BAND_LOWER: DSB demodulator will use lower sideband
    ///     G3XDDC_SIDE_BAND_UPPER: DSB demodulator will use upper sideband
    ///     G3XDDC_SIDE_BAND_BOTH: DSB demodulator will use both side bands.
    /// </summary>
    G3XddcDemodulatorParamDsbSideBand,

    /// <summary>
    ///     Side band for ISB demodulation.
    ///     The Buffer parameter has to be pointer to an UINT32 variable, and the BufferSize parameter has to be
    ///     sizeof(UINT32).
    ///     Value of the variable pointed to by the Buffer parameter can be one of the following:
    ///     G3XDDC_SIDE_BAND_LOWER: ISB demodulator will use lower sideband
    ///     G3XDDC_SIDE_BAND_UPPER: ISB demodulator will use upper sideband
    ///     G3XDDC_SIDE_BAND_BOTH: ISB demodulator will use both side bands.
    /// </summary>
    G3XddcDemodulatorParamIsbSideBand,

    /// <summary>
    ///     Audio service of DRM demodulator/decoder to be listening to.
    ///     The Buffer parameter has to be pointer to an UINT32 variable, and the BufferSize parameter has to be
    ///     sizeof(UINT32).
    ///     Value of the variable pointed to by the Buffer parameter is index of the audio service.
    ///     Possible value are: 1, 2, 3, 4,
    ///     where 1 is the first audio service,
    ///     2 is the second one, etc.
    ///     Use the GetDemodulatorState function with G3XDDC_DEMODULATOR_STATE_DRM_STATUS to retrieve information about
    ///     available audio services for currently received DRM station.
    /// </summary>
    G3XddcDemodulatorParamDrmAudioService,

    /// <summary>
    ///     Multimedia service of DRM demodulator/decoder to be decoded.
    ///     The Buffer parameter has to be pointer to an UINT32 variable, and the BufferSize parameter has to be
    ///     sizeof(UINT32).
    ///     Value of the variable pointed to by the Buffer parameter is index of the multimedia service.
    ///     Possible value are: 1, 2, 3, 4, where 1 is the first audio service, 2 is the second one, etc.
    ///     Use the GetDemodulatorState function with G3XDDC_DEMODULATOR_STATE_DRM_STATUS to retrieve information about
    ///     available multimedia services for currently received DRM station.
    ///     It is required that DRM multimedia player has to be installed to display multimedia content.
    ///     It is included in G33DDC software installer as optional.
    /// </summary>
    G3XddcDemodulatorParamDrmMultimediaService
}

internal enum DemodulatorState : uint
{
    /// <summary>
    ///     Lock state of synchronous AM demodulation.
    ///     The Buffer parameter has to be pointer to a BOOL variable, and the BufferSize parameter has to be sizeof(BOOL).
    ///     Received value is non-zero if synchronous AM demodulator is locked to signal, and zero if it is not locked.
    /// </summary>
    G3XddcDemodulatorStateAmsLock = 1,

    /// <summary>
    ///     实时频率？
    ///     Frequency in Hz which synchronous AM demodulator is locked to. It is relative to center of the demodulator. It can
    ///     be negative.
    ///     The Buffer parameter has to be pointer to a double variable, and the BufferSize parameter has to be sizeof(double).
    /// </summary>
    G3XddcDemodulatorStateAmsFrequency = 2,

    /// <summary>
    ///     AM调制深度
    ///     Depth of AM modulation in %.
    ///     The Buffer parameter has to be pointer to a double variable, and the BufferSize parameter has to be sizeof(double).
    /// </summary>
    G3XddcDemodulatorStateAmDepth = 3,

    /// <summary>
    ///     Lock state of DSB demodulation.
    ///     The Buffer parameter has to be pointer to a BOOL variable, and the BufferSize parameter has to be sizeof(BOOL).
    ///     Received value is non-zero if DSB demodulator is locked to signal, and zero if it is not locked.
    /// </summary>
    G3XddcDemodulatorStateDsbLock = 7,

    /// <summary>
    ///     实时频率？
    ///     Frequency in Hz which DSB demodulator is locked to. It is relative to center of the demodulator. It can be
    ///     negative.
    ///     The Buffer parameter has to be pointer to a double variable, and the BufferSize parameter has to be sizeof(double).
    /// </summary>
    G3XddcDemodulatorStateDsbFrequency = 8,

    /// <summary>
    ///     Estimated tune error in Hz.
    ///     The Buffer parameter has to be pointer to an INT32 variable, and the BufferSize parameter has to be sizeof(INT32).
    ///     Received value is difference between demodulator frequency and frequency of received signal. Subtract the returned
    ///     tune error from demodulator frequency to get frequency of the received signal. Tune error is relative to center of
    ///     the demodulator and it can be negative.
    /// </summary>
    G3XddcDemodulatorStateTuneError = 4,

    /// <summary>
    ///     Status of DRM demodulator/decoder.
    ///     The Buffer parameter has to be pointer to a G3XDDC_DRM_STATUS structure, and the BufferSize parameter has to be
    ///     sizeof(G3XDDC_DRM_STATUS).
    /// </summary>
    G3XddcDemodulatorStateDrmStatus = 5,

    /// <summary>
    ///     频率偏移
    ///     Estimated frequency deviation in Hz.
    ///     The Buffer parameter has to be pointer to an UINT32 variable, and the BufferSize parameter has to be
    ///     sizeof(UINT32).
    /// </summary>
    G3XddcDemodulatorStateFmDeviation = 6
}

public enum SideBandType : uint
{
    G3XddcSideBandLower = 0,
    G3XddcSideBandUpper,
    G3XddcSideBandBoth
}

public enum AgcMode
{
    AgcOff = 0,
    AgcSlow,
    AgcMedium,
    AgcFast
}
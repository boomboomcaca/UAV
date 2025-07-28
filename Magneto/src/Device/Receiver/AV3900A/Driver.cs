using System;
using System.Runtime.InteropServices;
using Magneto.Device.AV3900A.Common;

namespace Magneto.Device.AV3900A;

internal class Driver
{
    public const string DriverLibName = "library\\AV3900A\\x86\\EISAL.dll";
    public const CallingConvention DriverCallingConvention = CallingConvention.StdCall;

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salConnectSensor2")]
    public static extern SalErrorType ConnectSensor(ref UIntPtr sensorHandle, UIntPtr smsHandle, string sensorName,
        string applicationName, int options);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salDisconnectSensor")]
    public static extern SalErrorType DisconnectSensor(UIntPtr sensorHandle);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salClose")]
    public static extern SalErrorType Close(UIntPtr sensorHandle);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salGetSensorTime")]
    public static extern SalErrorType GetSensorTime(UIntPtr sensorHandle, ref SalTimeInfo sensorTimeInfo);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salStartSweep")]
    public static extern SalErrorType StartSweep(ref UIntPtr measHandle, UIntPtr sensorHandle,
        ref SalSweepParams sweepParams,
        SalFrequencySegment[] frequencySegment, IntPtr callback);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salSendSweepCommand")]
    public static extern SalErrorType SendSweepCommand(UIntPtr measHandle, SalSweepCommand cmd);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salGetSegmentData")]
    public static extern SalErrorType GetSegmentData([In] UIntPtr measHandle, ref SalSegmentData segmentData,
        [Out] float[] amplitudes, [In] uint userDataBufferBytes);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salGetSegmentRawData")]
    public static extern SalErrorType GetSegmentRawData(UIntPtr measHandle, ref SalSegmentData dataHdr,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
        uint[] rawData, uint userDataBufferBytes);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salSetTuner")]
    public static extern SalErrorType SetTuner(UIntPtr sensorHandle, ref SalTunerParams tunerParams);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salGetTimeData")]
    public static extern SalErrorType GetTimeData(UIntPtr measHandle, ref SalTimeData dataHdr,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
        short[] userDataBuffer, uint userDataBufferBytes);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salRequestTimeData")]
    public static extern SalErrorType RequestTimeData(ref UIntPtr measHandle, UIntPtr sensorHandle,
        ref SalTimeDataParms parms);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention,
        EntryPoint = "salRequestTimeDataByTimeTrigger")]
    public static extern SalErrorType RequestTimeDataByTimeTrigger(ref UIntPtr measHandle, UIntPtr sensorHandle,
        ref SalTimeDataParms iqParams, ref SalTimeTrigParms triggerParams);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention,
        EntryPoint = "salRequestTimeDataByLevelTrigger")]
    public static extern SalErrorType RequestTimeDataByLevelTrigger(ref UIntPtr measHandle, UIntPtr sensorHandle,
        ref SalTimeDataParms iqParams, ref SalLevelTrigParms triggerParams);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salSendTimeDataCommand")]
    public static extern SalErrorType SendTimeDataCommand(UIntPtr measHandle, SalTimeDataCmd cmd);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salGetGPSStatus")]
    public static extern SalErrorType GetGPSStatus(UIntPtr sensorHandle, ref uint timeAlarms);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salSetGPSMode")]
    public static extern SalErrorType SetGPSMode(UIntPtr sensorHandle, GpsMode mode);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salRequestDemodData")]
    public static extern SalErrorType RequestDemodData(ref UIntPtr measHandle, UIntPtr sensorHandle,
        ref SalDemodParms parms);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salGetDemodData")]
    public static extern SalErrorType GetDemodData(UIntPtr measHandle, ref SalDemodData dataHdr,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4, SizeConst = 1024)]
        int[] userDataBuffer,
        uint userDataBufferBytes = 4096);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salSendDemodCommand")]
    public static extern SalErrorType SendDemodCommand(UIntPtr measHandle, SalDemodCmd mode);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salAbortAll")]
    public static extern SalErrorType AbortAll(UIntPtr sensorHandle);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention,
        EntryPoint = "salComputeFftSegmentTableSize")]
    public static extern SalErrorType ComputeFftSegmentTableSize(ref SalSweepComputationParams computeParms,
        ref SalSweepParams sweepParms, ref SalSweepComputationResults results);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salInitializeFftSegmentTable")]
    public static extern SalErrorType InitializeFftSegmentTable(ref SalSweepComputationParams computeParms,
        ref SalSweepParams sweepParms,
        ref SalFrequencySegment exampleSegment, [Out] SalFrequencySegment[] segmentTable,
        ref SalSweepComputationResults results);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salForceSmsGarbageCollection")]
    public static extern SalErrorType ForceSmsGarbageCollection(UIntPtr handle);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salContinueAcquire")]
    public static extern SalErrorType ContinueAcquire(UIntPtr measHandle);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salGetSensorLocation")]
    public static extern SalErrorType GetSensorLocation(UIntPtr sensorHandle, ref SalLocation location);

    [DllImport(DriverLibName, CallingConvention = DriverCallingConvention, EntryPoint = "salSetSensorMode2")]
    public static extern SalErrorType SetSensorMode2(UIntPtr sensorHandle, SalSensorMode mode);
}
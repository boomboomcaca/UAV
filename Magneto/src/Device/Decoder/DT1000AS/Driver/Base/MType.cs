namespace Magneto.Device.DT1000AS.Driver.Base;

public enum MType
{
    MImmediateAssignment = 1,
    MImmediateAssignmentExtended,
    MAssignmentCommand,
    MAssignmentComplete,
    MAssignmentFailure,
    MCipheringModeCommand,
    MCipheringModeComplete,
    MHandoverComplete,
    MHandoverFailure,
    MHandoverCommand,
    MPhysicalInformation,
    MPagingResponse,
    MPagingRequestType1,
    MPagingRequestType2,
    MPagingRequestType3,
    MSystemInformationType1,
    MSystemInformationType2,
    MSystemInformationType3,
    MSystemInformationType4,
    MLocationUpdatingRequest,
    MMmInformation,
    MTmsiReallocationCommand,
    MTmsiReallocationComplete,
    MAuthenticationRequest,
    MAuthenticationResponse,
    MCmServiceRequest,

    ////// CC
    MAlerting,
    MConnect,
    MConnectAcknowledge,
    MDisconnect,
    MSetup,
    MCallProce,
    MRelease,
    MReleaseComplete,
    MChannelRelease,
    MSms
}
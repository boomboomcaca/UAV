namespace GsmReceiver.Base;

public enum ExitCauseType
{
    ExitCauseSms = 1,
    ExitCauseSpeech,
    ExitCauseNoUplink,
    ExitCauseDnSdcchError,
    ExitCauseSmsEnd,
    ExitCausePhoneidNotmatch,
    ExitCauseTmsiNotmatch,
    ExitCauseTmsiNotGet,
    ExitCauseImsiNotmatch,
    ExitCauseFilterPhoneid,
    ExitCauseOriginateSmsNouplink,
    ExitCauseAssignmentCommand,
    ExitCauseAuthRequest,
    ExitCauseSmsStatusReport,
    ExitCauseSmsPhoneidLengthNotmatch,
    ExitCauseChannelRelease,
    ExitCauseVoidFrame,
    ExitCausePageIaNoPageResponse,
    ExitCauseOriginatingIaNoCmService,
    ExitCauseServiceOthers,
    ExitCauseLocationUpdate
}
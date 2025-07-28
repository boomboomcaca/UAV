/***************************************************************************
                      tmMpegCodeID.h  -  description
                      -----------------------------
    begin                : 04\07\2005
    copyright            : (C) 2004 by stone
    email                : stone_0815@sina.com
 ***************************************************************************/
/***************************************************************************
 *                                                                         *
 *   本文件头定义了解，压器所用的结构和参数值，在引用解压库时必须包含本头  *
 *   文件																   *
 *                                                                         *
 ***************************************************************************/
//Mpeg CodecID

namespace Magneto.Device.Nvt.SDK;

public enum MpegCodecId
{
    MpegCodecIdNone = 0x00000000,
    MpegCodecIdMpeg1Video,
    MpegCodecIdMpeg2Video, // prefered ID for MPEG Video 1 or 2 decoding
    MpegCodecIdMpeg2VideoXvmc,
    MpegCodecIdH261,
    MpegCodecIdH263,
    MpegCodecIdRv10,
    MpegCodecIdRv20,
    MpegCodecIdMp2,
    MpegCodecIdMp3, // prefered ID for MPEG Audio layer 1, 2 or3 decoding
    MpegCodecIdVorbis,
    MpegCodecIdAc3,
    MpegCodecIdMjpeg,
    MpegCodecIdMjpegb,
    MpegCodecIdLjpeg,
    MpegCodecIdSp5X,
    MpegCodecIdMpeg4,
    MpegCodecIdRawvideo,
    MpegCodecIdMsmpeg4V1,
    MpegCodecIdMsmpeg4V2,
    MpegCodecIdMsmpeg4V3,
    MpegCodecIdWmv1,
    MpegCodecIdWmv2,
    MpegCodecIdH263P,
    MpegCodecIdH263I,
    MpegCodecIdFlv1,
    MpegCodecIdSvq1,
    MpegCodecIdSvq3,
    MpegCodecIdDvvideo,
    MpegCodecIdDvaudio,
    MpegCodecIdWmav1,
    MpegCodecIdWmav2,
    MpegCodecIdMace3,
    MpegCodecIdMace6,
    MpegCodecIdHuffyuv,
    MpegCodecIdCyuv,
    MpegCodecIdH264,
    MpegCodecIdIndeo3,
    MpegCodecIdVp3,
    MpegCodecIdTheora,
    MpegCodecIdAac,
    MpegCodecIdMpeg4Aac,
    MpegCodecIdAsv1,
    MpegCodecIdAsv2,
    MpegCodecIdFfv1,
    MpegCodecId4Xm,
    MpegCodecIdVcr1,
    MpegCodecIdCljr,
    MpegCodecIdMdec,
    MpegCodecIdRoq,
    MpegCodecIdInterplayVideo,
    MpegCodecIdXanWc3,
    MpegCodecIdXanWc4,
    MpegCodecIdRpza,
    MpegCodecIdCinepak,
    MpegCodecIdWsVqa,
    MpegCodecIdMsrle,
    MpegCodecIdMsvideo1,
    MpegCodecIdIdcin,
    MpegCodecId8Bps,
    MpegCodecIdSmc,
    MpegCodecIdFlic,
    MpegCodecIdTruemotion1,
    MpegCodecIdVmdvideo,
    MpegCodecIdVmdaudio,
    MpegCodecIdMszh,
    MpegCodecIdZlib,
    MpegCodecIdQtrle,

    /* various pcm "codecs" */
    MpegCodecIdPcmS16Le,
    MpegCodecIdPcmS16Be,
    MpegCodecIdPcmU16Le,
    MpegCodecIdPcmU16Be,
    MpegCodecIdPcmS8,
    MpegCodecIdPcmU8,
    MpegCodecIdPcmMulaw,
    MpegCodecIdPcmAlaw,

    /* various adpcm codecs */
    MpegCodecIdAdpcmImaQt,
    MpegCodecIdAdpcmImaWav,
    MpegCodecIdAdpcmImaDk3,
    MpegCodecIdAdpcmImaDk4,
    MpegCodecIdAdpcmImaWs,
    MpegCodecIdAdpcmImaSmjpeg,
    MpegCodecIdAdpcmMs,
    MpegCodecIdAdpcm4Xm,
    MpegCodecIdAdpcmXa,
    MpegCodecIdAdpcmAdx,
    MpegCodecIdAdpcmEa,
    MpegCodecIdAdpcmG726,

    /* AMR */
    MpegCodecIdAmrNb,
    MpegCodecIdAmrWb,

    /* RealAudio codecs*/
    MpegCodecIdRa144,
    MpegCodecIdRa288,

    /* various DPCM codecs */
    MpegCodecIdRoqDpcm,
    MpegCodecIdInterplayDpcm,
    MpegCodecIdXanDpcm,
    MpegCodecIdFlac,
    MpegCodecIdMpeg2Ts, /* _FAKE_ codec to indicate a raw MPEG2 transport
                         stream (only used by libavformat) */
    MpegCodecIdDts,
    MpegCodecIdBkmpeg4,
    MpegCodecIdMp1,
    MpegCodecIdMp123,
    MpegCodecIdLibmpeg2,
    MpegCodecIdMpeg2Virtualdub,
    MpegCodecIdA52, //ac3decoder
    MpegCodecIdG721,
    MpegCodecIdG722,
    MpegCodecIdG72324,
    MpegCodecIdG72340,
    MpegCodecIdG726,
    MpegCodecIdG729,
    MpegCodecIdMask = 0x00000FFF
}
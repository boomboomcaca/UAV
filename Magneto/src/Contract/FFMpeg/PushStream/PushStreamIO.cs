using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FFmpeg.AutoGen;

namespace Magneto.Contract.FFMpeg.PushStream;

/// <summary>
///     内存读取
/// </summary>
internal unsafe class PushStreamIo
{
    /// <summary>
    ///     AVIO 内存大小
    /// </summary>
    private readonly int _avioCtxBufferSize = 1024 * 32; // 32768

    // https://blog.csdn.net/leixiaohua1020/article/details/12980423
    // https://www.cnblogs.com/leisure_chn/p/10318145.html?spm=a2c6h.12873639.0.0.334c68911DWhfX
    // https://blog.csdn.net/zottffssent123/article/details/108845117
    // https://blog.csdn.net/zottffssent123/article/details/108845117?utm_medium=distribute.pc_relevant.none-task-blog-2%7Edefault%7EBlogCommendFromMachineLearnPai2%7Edefault-10.control&dist_request_id=1329187.21469.16179516907882717&depth_1-utm_source=distribute.pc_relevant.none-task-blog-2%7Edefault%7EBlogCommendFromMachineLearnPai2%7Edefault-10.control
    /// <summary>
    ///     缓存数据填充模式
    /// </summary>
    private IoFillModel _fillModel = IoFillModel.ReadLocalFile;

    /// <summary>
    ///     内存IO模式
    ///     这个方法会阻塞
    /// </summary>
    /// <param name="fillModel"></param>
    /// <param name="stream"></param>
    /// <param name="outFilename"></param>
    public int AVIO_PushStreamToRmtp(IoFillModel fillModel, StreamItem stream, string outFilename)
    {
        _fillModel = fillModel;

        #region ffmpeg 日志

        // 设置记录ffmpeg日志级别
        ffmpeg.av_log_set_level(ffmpeg.AV_LOG_MAX_OFFSET);

        void LogCallback(void* p0, int level, string format, byte* vl)
        {
            if (level > ffmpeg.av_log_get_level()) return;
            var lineSize = 1024;
            var lineBuffer = stackalloc byte[lineSize];
            var printPrefix = 1;
            ffmpeg.av_log_format_line(p0, level, format, vl, lineBuffer, lineSize, &printPrefix);
            var line = Marshal.PtrToStringAnsi((IntPtr)lineBuffer);
            Debug.WriteLine(line);
            // LogHelper.Log("ffmpeg-操作日志", line);
        }

        ffmpeg.av_log_set_callback((av_log_set_callback_callback)LogCallback);

        #endregion

        // 输出媒体文件的基本信息
        AVFormatContext* ofmtCtx = null;

        #region 初始化

        // 初始化libavformat和注册所有的复用器和解复用器和协议。
        // 如果不调用这个函数，可以使用av_register_input_format()和av_register_out_format()来选择支持的格式。
        // 这个函数现在已经不需要了,具体说明参见:https://www.jianshu.com/p/ebb219ec1c0f
        // ffmpeg.av_register_all(); 
        // 全局地初始化网络组件，需要用到网络功能的时候需要调用。
        ffmpeg.avformat_network_init();

        #endregion

        #region 内存数据作为输入流

        int ret;
        int i;
        // 创建输入流对象
        var ifmtCtx = ffmpeg.avformat_alloc_context();
        if (ifmtCtx == null)
        {
            ret = ffmpeg.AVERROR(ffmpeg.ENOMEM);
            goto end;
        }

        // 内存IO大小，4KB
        // int avio_ctx_buffer_size = 1024 * 32;// 32768
        // 分配IO内存
        var iobuffer = (byte*)ffmpeg.av_malloc((ulong)_avioCtxBufferSize);
        if (iobuffer == null)
        {
            ret = ffmpeg.AVERROR(ffmpeg.ENOMEM);
            goto end;
        }

        // 转换为 IntPtr 
        var pA = Utils.StructToIntPtr(stream);
        // 分配AVIOContext，第三个参数write_flag为0
        var avio = ffmpeg.avio_alloc_context(iobuffer,
            _avioCtxBufferSize,
            0,
            (void*)pA,
            (avio_alloc_context_read_packet_func)Fill_iobuffer,
            null,
            null);
        if (avio == null)
        {
            ret = ffmpeg.AVERROR(ffmpeg.ENOMEM);
            goto end;
        }

        ifmtCtx->pb = avio;
        ifmtCtx->flags = ffmpeg.AVFMT_FLAG_CUSTOM_IO; // 自定义IO标识
        // 打开多媒体数据并且获得一些相关的信息
        ret = ffmpeg.avformat_open_input(&ifmtCtx, null, null, null);
        if (ret < 0)
        {
            Console.WriteLine("Could not open input file.");
            goto end;
        }

        #endregion

        #region 获取视频流信息 - 探测文件的格式

        // 读取一部分视音频数据并且获得一些相关的信息, 给每个媒体流（音频/视频）的AVStream结构体赋值。
        ifmtCtx->flags |= ffmpeg.AVFMT_FLAG_NOBUFFER;
        ret = ffmpeg.avformat_find_stream_info(ifmtCtx, null);
        if (ret < 0)
        {
            Console.WriteLine("Failed to retrieve input stream information");
            goto end;
        }

        #endregion

        #region 获取视频流序号

        var videoindex = -1; // 视频流序号
        // 遍历输入媒体文件中流 AVFormatContext.streams
        for (i = 0; i < ifmtCtx->nb_streams; i++)
            // 查找视频编码的流
            if (ifmtCtx->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
            {
                videoindex = i;
                break;
            }

        #endregion

        #region 打印输入流信息

        // 打印Metadata信息(元信息), 关于输入或输出格式的详细信息，
        // 例如持续时间，比特率，流，容器，程序，元数据，边数据，编解码器和时基。
        ffmpeg.av_dump_format(ifmtCtx, 0, null, 0);

        #endregion

        #region 输出流对象

        // 输出文件容器格式
        // 初始化一个用于输出的AVFormatContext结构体。
        /*
            ctx：函数调用成功之后创建的AVFormatContext结构体。
            oformat：指定AVFormatContext中的AVOutputFormat，用于确定输出格式。如果指定为NULL，可以设定后两个参数（format_name或者filename）由FFmpeg猜测输出格式。
            format_name：指定输出格式的名称。根据格式名称，FFmpeg会推测输出格式。输出格式可以是“flv”，“mkv”等等。
            filename：指定输出文件的名称。根据文件名称，FFmpeg会推测输出格式。文件名称可以是“xx.flv”，“yy.mkv”等等。
         */
        ret = ffmpeg.avformat_alloc_output_context2(&ofmtCtx, null, "hls", outFilename); //当推送数据到RTMP服务器时，必须设置为flv
        if (ret < 0)
        {
            Console.WriteLine("Error avformat_alloc_output_context2\n");
            goto end;
        }

        // 创建输出对象失败
        if (ofmtCtx == null)
        {
            Console.WriteLine("Could not create output context\n");
            ret = ffmpeg.AVERROR_UNKNOWN;
            goto end;
        }

        // 赋值输出文件容器对象
        var ofmt = ofmtCtx->oformat;
        // 将输入流音视频编码信息复制到输出流中
        for (i = 0; i < ifmtCtx->nb_streams; i++)
        {
            // 根据输入的媒体文件流信息，创建输出流对象
            var inStream = ifmtCtx->streams[i];
            var codec = ffmpeg.avcodec_find_decoder(inStream->codecpar->codec_id);
#pragma warning disable CS0618
            // AVPixelFormat.AV_PIX_FMT_YUV420P
            var outStream = ffmpeg.avformat_new_stream(ofmtCtx, codec);
            if (outStream == null)
            {
                Console.WriteLine("Failed allocating output stream\n");
                ret = ffmpeg.AVERROR_UNKNOWN;
                goto end;
            }

            var codecCtx = ffmpeg.avcodec_alloc_context3(codec);
            ret = ffmpeg.avcodec_parameters_to_context(codecCtx, inStream->codecpar);
            if (ret < 0)
            {
                Console.WriteLine("Failed to copy in_stream codecpar to codec context\n");
                goto end;
            }

            codecCtx->codec_tag = 0;
            /*
                打开媒体格式上下文后，如果输出媒体格式有 AVFMT_GLOBALHEADER 这个标记，
                那么音视频编码器创建的时候也需要设置 AV_CODEC_FLAG_GLOBAL_HEADER 标记。
                即：音视频编码器创建时需要
             */
            if ((ofmtCtx->oformat->flags & ffmpeg.AVFMT_GLOBALHEADER) > 0)
                codecCtx->flags |= ffmpeg.AV_CODEC_FLAG_GLOBAL_HEADER;
            ret = ffmpeg.avcodec_parameters_from_context(outStream->codecpar, codecCtx);
            if (ret < 0)
            {
                Console.WriteLine("Failed to copy codec context to out_stream codecpar context\n");
                goto end;
            }
            //// 编码参数上下文的拷贝，将输入流的编码信息拷贝到输出流中
            //ret = ffmpeg.avcodec_copy_context(out_stream->codec, in_stream->codec);
            //if (ret < 0)
            //{
            //    Console.WriteLine("Failed to copy context from input to output stream codec context\n");
            //    goto end;
            //}
        }

        #endregion

        #region 打印输出流信息

        // 输出输出流元信息
        ffmpeg.av_dump_format(ofmtCtx, 0, outFilename, 1);

        #endregion

        #region 打开输出流

        /*
            AVFMT_NOFILE主要针对的是URL资源，不具备本地文件性质的打开或者关闭。
            例如使用FFmpeg推流RTMP,需要打开URL，调用函数 avio_open(pAVIOContext, szURL, AVIO_FLAG_WRITE)
            如果输出格式不是 AVFMT_NOFILE 类型，需要使用 avio_open 来 创建并初始化一个AVIOContext来访问filename。
            如果 flags 设置了 AVFMT_NOFILE 就不要给 pb 赋值。
            在这种情况下，封装、解封装将使用其它方式操作IO，这个字段应该设置为空
        */
        if ((ofmt->flags & ffmpeg.AVFMT_NOFILE) == 0)
        {
            // 打开FFmpeg的输入输出文件
            ret = ffmpeg.avio_open(&ofmtCtx->pb, outFilename, ffmpeg.AVIO_FLAG_WRITE);
            if (ret < 0)
            {
                Console.WriteLine($"Could not open output URL '{outFilename}'");
                goto end;
            }
        }

        #endregion

        #region 写入头部信息

        // 写入头部信息
        ret = ffmpeg.avformat_write_header(ofmtCtx, null);
        if (ret < 0)
        {
            Console.WriteLine("Error occurred when opening output URL\n");
            goto end;
        }

        #endregion

        #region 逐一推送每帧数据

        // 获取当前系统时间，单位：毫秒
        var startTime = ffmpeg.av_gettime();
        // 视频帧序号
        long frameIndex = 0;
        // 存储压缩编码数据的数据结构
        long pts = 0;
        long dts = 0;
        while (true)
        {
            // 输入输出流
            // 从输入媒体信息中获取一个AVPacket
            AVPacket pkt;
            ret = ffmpeg.av_read_frame(ifmtCtx, &pkt);
            if (ret < 0) break;
            /*
                PTS（Presentation Time Stamp）显示播放时间
                DTS（Decoding Time Stamp）解码时间
            */
            // 没有显示时间（比如未解码的 H.264 ）
            if (pkt.pts == ffmpeg.AV_NOPTS_VALUE)
            {
                Console.WriteLine($"异常pkt   当前pts: {pkt.pts}  dts: {pkt.dts}  上一包的pts: {pts} dts: {dts}");
                ffmpeg.av_packet_unref(&pkt);
                continue;
                // #region 没有封装格式的裸流
                // // （例如H.264裸流）是不包含PTS、DTS这些参数的。在发送这种数据的时候，需要自己计算并写入AVPacket的PTS，DTS，duration等参数。
                // // 取出时间基
                // AVRational time_base1 = ifmt_ctx->streams[videoindex]->time_base;
                // // 计算两帧之间的时间
                // /*
                //  r_frame_rate 基流帧速率  （不是太懂）
                //  av_q2d 转化为double类型
                // */
                // var calc_duration = (long)(ffmpeg.AV_TIME_BASE / ffmpeg.av_q2d(ifmt_ctx->streams[videoindex]->r_frame_rate));
                // // 配置参数
                // pkt.pts = (long)((frame_index * calc_duration) / (ffmpeg.av_q2d(time_base1) * ffmpeg.AV_TIME_BASE));
                // pkt.dts = pkt.pts;
                // pkt.duration = (long)(calc_duration / (ffmpeg.av_q2d(time_base1) * ffmpeg.AV_TIME_BASE));
                // #endregion
            }

            // 对视频帧进行延时处理
            if (pkt.stream_index == videoindex)
            {
                // 视频时间基
                var timeBase = ifmtCtx->streams[videoindex]->time_base;
                // 内部时间基的分数表示，实际上它是AV_TIME_BASE的倒数
                var timeBaseQ = new AVRational { num = 1, den = ffmpeg.AV_TIME_BASE };
                // 计算视频播放时间
                var ptsTime = ffmpeg.av_rescale_q(pkt.dts, timeBase, timeBaseQ);
                // 计算实际视频的播放时间
                var nowTime = ffmpeg.av_gettime() - startTime;
                if (ptsTime > nowTime)
                {
                    // var sleep = (uint)(pts_time - now_time);
                    // Console.WriteLine($"视频播放时间:{pts_time},实际视频的播放时间:{now_time},睡眠时间:{sleep}毫秒.");
                    // Trace.WriteLine($"视频播放时间:{pts_time},实际视频的播放时间:{now_time},睡眠时间:{sleep}毫秒.", "视频推送延时处理日志");
                    // // 睡眠一段时间（目的是让当前视频记录的播放时间与实际时间同步）
                    // ffmpeg.av_usleep(sleep);
                }
            }

            var inStream = ifmtCtx->streams[pkt.stream_index];
            var outStream = ofmtCtx->streams[pkt.stream_index];
            pts = pkt.pts;
            dts = pkt.dts;
            // 计算延时后，重新指定时间戳
            // TODO : 测试这个枚举"AVRounding"是否有问题
            pkt.pts = ffmpeg.av_rescale_q_rnd(pkt.pts,
                inStream->time_base,
                outStream->time_base,
                AVRounding.AV_ROUND_NEAR_INF );
            pkt.dts = ffmpeg.av_rescale_q_rnd(pkt.dts,
                inStream->time_base,
                outStream->time_base,
                AVRounding.AV_ROUND_NEAR_INF );
            // 数据的时长
            pkt.duration = ffmpeg.av_rescale_q(pkt.duration, inStream->time_base, outStream->time_base);
            // 该数据在媒体流中的字节偏移量，-1 表示不知道字节流位置
            pkt.pos = -1;
            // 打印视频数据发送信息
            if (pkt.stream_index == videoindex)
            {
                Console.WriteLine($"Send {frameIndex} video frames to output URL\r\n");
                Trace.WriteLine($"推送 {frameIndex} 帧数据.....", "视频数据推送日志");
                frameIndex++;
            }

            // 向输出上下文发送（向地址推送）
            ret = ffmpeg.av_interleaved_write_frame(ofmtCtx, &pkt);
            if (ret < 0)
            {
                Console.WriteLine("Error muxing packet\n");
                ffmpeg.av_packet_unref(&pkt);
                break;
            }

            // 释放 packet，否则会内存泄露
            // ffmpeg.av_free_packet(&pkt); //ffmpeg.av_free_packet(AVPacket*)”已过时:“Use av_packet_unref
            ffmpeg.av_packet_unref(&pkt);
        }

        #endregion

        #region 输出文件尾

        // 用于输出文件尾
        ffmpeg.av_write_trailer(ofmtCtx);

        #endregion

        end:

        #region 释放资源

        // 释放资源
        if (ifmtCtx->pb != null) ffmpeg.av_freep(ifmtCtx->pb);
        // 该函数会释放用户自定义的IO buffer
        // 上面不再释放，否则会corrupted double-linked list
        ffmpeg.avformat_close_input(&ifmtCtx);
        ffmpeg.avformat_free_context(ifmtCtx);
        if (ofmtCtx != null &&
            ofmtCtx->pb != null &&
            (ofmtCtx->flags & ffmpeg.AVFMT_NOFILE) == 0)
            ffmpeg.avio_close(ofmtCtx->pb);
        if (ofmtCtx != null) ffmpeg.avformat_free_context(ofmtCtx);
        if (ret < 0 && ret != ffmpeg.AVERROR_EOF)
        {
            Console.WriteLine("Error occurred.\n");
            return -1;
        }

        #endregion

        return 0;
    }

    /// <summary>
    ///     文件内容
    /// </summary>
    private byte[] _fileContent;

    private readonly ConcurrentQueue<byte> _gData = new();

    /// <summary>
    ///     读取数据的回调函数
    ///     AVIOContext使用的回调函数！
    ///     注意：返回值是读取的字节数
    ///     手动初始化AVIOContext只需要两个东西：内容来源的buffer，和读取这个Buffer到FFmpeg中的函数
    ///     回调函数，功能就是：把buf_size字节数据送入buf即可
    ///     第一个参数(void *opaque)一般情况下可以不用
    /// </summary>
    /// <param name="opaque">可以为任一类型</param>
    /// <param name="buf"></param>
    /// <param name="bufsize"></param>
    private int Fill_iobuffer(void* opaque, byte* buf, int bufsize)
    {
        // 转换为 IntPtr
        var ptr = (IntPtr)opaque;
        // opaque 传入的是 StreamItem 类型对象
        // IntPtr 转换为 StreamItem 类型
        var stream = Utils.IntPtrToStruct<StreamItem>(ptr);
        byte[] buffer = null;
        var afterOffset = 0;
        var beforeOffset = stream.Offset;
        // 根据内容填充模式获取数据块
        switch (_fillModel)
        {
            case IoFillModel.ReadLocalFile:
            {
                #region 读取本地文件

                if (!string.IsNullOrWhiteSpace(stream.FileName)
                    && File.Exists(stream.FileName))
                {
                    // 读取文件内容，控制只需要读取一次
                    if (_fileContent == null)
                    {
                        _fileContent = File.ReadAllBytes(stream.FileName);
                        AddData(_fileContent);
                    }

                    if (_fileContent?.Length > 0)
                    {
                        #region 方式一

                        // 按照指定偏移量及大小取内容块
                        // buffer = Utils.GetPart(_file_content, ref stream.Offset, bufsize);
                        // 写入偏移量
                        // after_offset = stream.Offset;
                        // Marshal.WriteInt32(ptr, after_offset);
                        // 直接对 stream.Offset 进行 stream.Offset +=  buffer.Length; 的方式进行赋值，
                        // 再次进入函数 fill_iobuffer 时，stream.Offset 还是保持原来的值，没有改变，
                        // 上面调用 Marshal.WriteInt32 的方式，直接向指定内存地址写入数据，
                        // Offset 字段是结构 StreamItem 的第一个字节，所以上面直接向结构的首地址写入 

                        #endregion

                        #region 方式二

                        /*
                         也可以不使用 Offset 来控制获取数据块，当获取完一块数据后，直接将这部分数据移除，
                         这样一来获取数据时，偏移量永远为 0，大小为 bufsize
                        */
                        // 按照指定偏移量及大小取内容块
                        buffer = GetData(0, bufsize);

                        #endregion
                    }
                }

                #endregion
            }
                break;
            case IoFillModel.ContinueWrite:
            {
                #region 持续写入缓存

                if (MDataCacheManager.Instance != null)
                {
                    var mdata = MDataCacheManager.Instance.GetMDataItem(stream.TerminalNum, stream.Channel);
                    if (mdata != null)
                    {
                        // 判断缓存数据大小
                        var msize = mdata.Size < bufsize;
                        if (msize)
                        {
                            Trace.WriteLine($"[{mdata.Size}<{bufsize}]缓存数据不足，等待中...", "缓存数据大小检查日志");
                            // 缓存数据不足，则循环等待
                            while (msize)
                            {
                                Task.Delay(200).ConfigureAwait(false).GetAwaiter().GetResult();
                                msize = mdata.Size < bufsize;
                            }

                            Trace.WriteLine("缓存数据已填充，可以读取数据.", "缓存数据大小检查日志");
                        }

                        // 获取数据块
                        buffer = mdata.GetData(0, bufsize);
                    }
                }

                #endregion
            }
                break;
        }

        if (buffer != null)
            // 重新分配内存，从实际读取的日志信息来看，
            // bufsize 有大于 4KB 的情况，初始化时缓存只分配 4KB 大小，
            // 避免内存溢出，下面对内存大小进行重新分配
            //if (buffer.Length > avio_ctx_buffer_size)
            //{
            //    ffmpeg.av_realloc(buf, (ulong)buffer.Length);// 无法读写受保护的内存
            //}
            // 拷贝数据到 buf 指向的地址，从首地址开始，大小为 buffer.Length
            Marshal.Copy(buffer, 0, (IntPtr)buf, buffer.Length);
        // 下面这行代码为前期开发时编写，实质上对原指针进行了重新赋值，指向了新的内存
        // 程序运行后无法正常读取到数据
        // buf = (byte*)Utils.ArrayToIntptr(buffer);

        #region 读取大小记录

        if (!_offsetDic.TryAdd(bufsize, 1))
            _offsetDic[bufsize]++;
        if (_min == 0)
        {
            _min = bufsize;
            _max = bufsize;
        }

        if (bufsize < _min) _min = bufsize;
        if (bufsize > _max) _max = bufsize;

        #endregion

        #region 日志记录

        if (buffer != null)
        {
            var info =
                $"########## \nbefore_offset={beforeOffset}\nafter_offset={afterOffset}\nbuffer.Length={buffer.Length}\nbufsize={bufsize}\nbufsizeDic(min={_min},max={_max}):\n{GetODic()}\n**********\r\n";
            Debug.WriteLine(info);
            Trace.WriteLine(info, "容读取日志");
        }

        #endregion

        if (buffer?.Length > 0) return buffer.Length;
        return -1;
    }

    #region 缓存读取

    private void AddData(byte[] buf)
    {
        foreach (var item in buf) _gData.Enqueue(item);
    }

    private byte[] GetData(int offset, int size)
    {
        var result = Array.Empty<byte>();
        if (_gData?.Count > 0 && offset <= _gData.Count - 1)
        {
            int len;
            // 计算实际数组长度
            if (offset + size <= _gData.Count - 1)
                len = size;
            else
                len = _gData.Count - offset;
            result = new byte[len];
            for (var index = 0; index < len; index++)
            {
                var suc = _gData.TryDequeue(out var value);
                if (suc) result[index] = value;
            }
        }

        return result;
    }

    #endregion

    #region 记录每次读取的大小

    private int _min;
    private int _max;
    private readonly Dictionary<int, int> _offsetDic = new();

    private string GetODic()
    {
        var info = string.Join("\r\n", _offsetDic.Select(t => $"{t.Key}:{t.Value}"));
        return info;
    }

    #endregion
}
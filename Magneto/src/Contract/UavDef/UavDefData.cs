using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Magneto.Contract.UavDef;

#region Record Data

[Table("uav_def_records")]
public class Record
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public DateTime Time { get; set; }
    public TimeSpan Duration { get; set; }
    public InvasionAreaType InvasionArea { get; set; }
    public string[] DetectionEquipments { get; set; }
    public int NumOfFlyingObjects { get; set; }
    public ICollection<Evidence> Evidence { get; set; }
    public ICollection<UavPath> UavPaths { get; set; }
    public ICollection<Disposal> Disposals { get; set; }
}

public enum InvasionAreaType
{
    ProtectedArea,
    IdentificationAndDisposalArea,
    AlertArea,
    WarningArea
}

#region Evidence Data

[Table("uav_def_evidence")]
public class Evidence
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [StringLength(50)]
    public string Model { get; set; }
    [StringLength(50)]
    public string Type { get; set; }
    public double RadioFrequency { get; set; }
    [StringLength(50)]
    public string ElectronicFingerprint { get; set; }
    public double LastFlightLatitude { get; set; }
    public double LastFlightLongitude { get; set; }
    public double PilotLatitude { get; set; }
    public double PilotLongitude { get; set; }
    public double ReturnLatitude { get; set; }
    public double ReturnLongitude { get; set; }
    public double LastFlightVerticalSpeed { get; set; }
    public double LastFlightHorizontalSpeed { get; set; }
    public double LastFlightAltitude { get; set; }
    public double LastFlightBearing { get; set; }
    [InverseProperty("Evidence")] public ICollection<EvdAndFile> EvdAndFiles { get; set; }
    public int RecordId { get; set; }
    public Record Record { get; set; }
}

public enum FileType
{
    Image,
    Audio,
    Video,
    ScanData
}

[Table("uav_def_playback_files")]
public class PlaybackFile
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [StringLength(50)]
    public string FileName { get; set; }
    public FileType FileType { get; set; }
    [StringLength(100)]
    public string FilePath { get; set; }
    public DateTime CreatedAt { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime UpDatedAt { get; set; }

    /// <summary>
    ///     使用json表示多段:其实频率,终止频率,步进。
    /// </summary>
    [StringLength(100)]
    public string Segments { get; set; }

    /// <summary>
    ///     总的帧数
    /// </summary>
    public int TotalFrames { get; set; }

    [InverseProperty("PlaybackFile")] public ICollection<EvdAndFile> EvdAndFiles { get; set; }
}

[Table("uav_def_evd_and_files")]
public class EvdAndFile
{
    [Key][Column(Order = 0)] public int EvdId { get; set; }
    [ForeignKey("EvidenceId")] public Evidence Evidence { get; set; }
    [Key][Column(Order = 1)] public int FileId { get; set; }
    [ForeignKey("PlaybackFileId")] public PlaybackFile PlaybackFile { get; set; }
}

#endregion

[Table("uav_def_uav_paths")]
public class UavPath
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [StringLength(50)]
    public string UavSerialNum { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int RecordId { get; set; }
    public Record Record { get; set; }
}

[Table("uav_def_disposals")]
public class Disposal
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public DateTime DateTime { get; set; }
    [StringLength(50)]
    public string Content { get; set; }
    public int RecordId { get; set; }
    public Record Record { get; set; }
}

[Table("uav_def_white_lists")]
public class WhiteList
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [StringLength(50)]
    public string DroneSerialNum { get; set; }
}

#endregion

public class UavDefDbContext : DbContext
{
    public DbSet<Record> Records { get; set; }
    public DbSet<Evidence> Evidence { get; set; }
    public DbSet<Disposal> Disposals { get; set; }
    public DbSet<PlaybackFile> Videos { get; set; }
    public DbSet<UavPath> UavPaths { get; set; }
    public DbSet<WhiteList> WhiteLists { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(UavDefDataBase.ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<EvdAndFile>()
            .HasKey(e => new { e.EvdId, EvdFileId = e.FileId });
    }
}
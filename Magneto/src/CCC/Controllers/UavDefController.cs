using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dapper;
using Magneto.Contract.Storage;
using Magneto.Contract.UavDef;
using MessagePack;
using Microsoft.AspNetCore.Mvc;

namespace CCC.Controllers;

[ApiController]
[Route("[controller]")]
public class UavDefController : ControllerBase
{
    [HttpGet("Records")]
    public IEnumerable<Record> GetRecords([FromQuery] IEnumerable<int> ids,
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime stopTime)
    {
        var conditions = string.Empty;
        var parameters = new DynamicParameters();
        var enumerable = ids.ToList();
        if (enumerable.Count > 0) conditions += $"AND \"Id\" IN ({string.Join(",", enumerable)})";
        conditions += "AND \"Time\" BETWEEN @startTime AND @stopTime";
        parameters.Add("@startTime", startTime);
        parameters.Add("@stopTime", stopTime);
        var re = UavDefDataBase.Select<Record>(typeof(Record), conditions,
            parameters).ToArray();
        conditions = string.Empty;
        foreach (var record in re)
        {
            conditions += $"AND \"RecordId\" = {record.Id}";
            var reEvd = UavDefDataBase.Select<Evidence>(typeof(Evidence), conditions,
                parameters).ToArray();
            if (!reEvd.Any()) continue;
            record.Evidence = new List<Evidence>
            {
                Capacity = 0
            };
            record.Evidence.Add(reEvd.Last());
        }

        return re;
    }

    [HttpPatch("Records")]
    public bool UpdateRecords([FromQuery] IEnumerable<Record> records)
    {
        return records.Any();
    }

    [HttpDelete("Records")]
    public bool DeleteRecords([FromQuery] IEnumerable<int> ids)
    {
        var re = UavDefDataBase.Delete(typeof(Record), ids.ToArray());
        return re;
    }

    [HttpGet("Disposals")]
    public IEnumerable<Disposal> GetDisposals([FromQuery] IEnumerable<int> recordIds)
    {
        var conditions = string.Empty;
        var parameters = new DynamicParameters();
        var enumerable = recordIds.ToList();
        if (enumerable.Count < 1)
            return UavDefDataBase.Select<Disposal>(typeof(Disposal), conditions, parameters);
        conditions += $"AND \"RecordId\" IN ({string.Join(",", enumerable)})";
        return UavDefDataBase.Select<Disposal>(typeof(Disposal), conditions, parameters);
    }

    [HttpDelete("Disposals")]
    public bool DeleteDisposals([FromQuery] IEnumerable<int> ids)
    {
        var re = UavDefDataBase.Delete(typeof(Disposal), ids.ToArray());
        return re;
    }

    [HttpGet("Evidence")]
    public IEnumerable<Evidence> GetEvidence([FromQuery] IEnumerable<int> recordIds)
    {
        var conditions = string.Empty;
        var parameters = new DynamicParameters();
        var enumerable = recordIds.ToList();
        if (enumerable.Count < 1)
            return UavDefDataBase.Select<Evidence>(typeof(Evidence),
                conditions, parameters);
        conditions += $"AND \"RecordId\" IN ({string.Join(",", enumerable)})";
        return UavDefDataBase.Select<Evidence>(typeof(Evidence), conditions,
            parameters);
    }

    [HttpDelete("Evidence")]
    public bool DeleteEvidence([FromQuery] IEnumerable<int> ids)
    {
        var re = UavDefDataBase.Delete(typeof(Evidence), ids.ToArray());
        return re;
    }

    [HttpGet("PlaybackFiles")]
    public IEnumerable<PlaybackFile> GetPlaybackFiles([FromQuery] IEnumerable<int> evidenceIds)
    {
        var conditions = string.Empty;
        var parameters = new DynamicParameters();
        var enumerable = evidenceIds.ToList();
        if (enumerable.Count < 1)
            return UavDefDataBase.Select<PlaybackFile>(typeof(PlaybackFile), conditions,
                parameters);
        conditions += $"AND \"EvdId\" IN ({string.Join(",", enumerable)})";
        var filedIds = UavDefDataBase.Select<EvdAndFile>(typeof(EvdAndFile), conditions, parameters)
            .Select(s => s.FileId).ToArray();
        if (filedIds.Length < 1) return null;
        conditions = $"AND \"Id\" IN ({string.Join(",", filedIds)})";
        return UavDefDataBase.Select<PlaybackFile>(typeof(PlaybackFile), conditions, parameters);
    }

    [HttpDelete("PlaybackFiles")]
    public bool DeletePlaybackFiles([FromQuery] IEnumerable<int> ids)
    {
        var re = UavDefDataBase.Delete(typeof(PlaybackFile), ids.ToArray());
        return re;
    }

    [HttpGet("UavPaths")]
    public IEnumerable<UavPath> GetUavPaths([FromQuery] IEnumerable<int> recordIds)
    {
        var conditions = string.Empty;
        var parameters = new DynamicParameters();
        var enumerable = recordIds.ToList();
        if (enumerable.Count < 1)
            return UavDefDataBase.Select<UavPath>(typeof(UavPath), conditions, parameters);
        conditions += $"AND \"RecordId\" IN ({string.Join(",", enumerable)})";
        var re = UavDefDataBase.Select<UavPath>(typeof(UavPath), conditions, parameters).ToArray();
        return re;
    }

    [HttpGet("GetFrame")]
    public IActionResult GetFrame([FromQuery] string filePath, [FromQuery] string fileName, [FromQuery] int frameId)
    {
        fileName = Path.GetFileNameWithoutExtension(fileName);
        var data = RawDataStorage.Instance.GetFrame(filePath, fileName, frameId);
        if (data is null) return NotFound();
        //var sub1 = new JArray(data[0]);
        //var sub2 = new JArray(data[1]);
        //var dataJArray = new JArray(sub1, sub2);
        //var re = new JObject
        //{
        //    new JProperty("result","ok"),
        //    new JProperty("data",dataJArray),
        //};
        var re = new
        {
            result = "ok",
            data
        };
        return File(MessagePackSerializer.Serialize(re), "application/octet-stream");
    }

    [HttpGet("GetWhiteLists")]
    public IEnumerable<WhiteList> GetWhiteLists()
    {
        return UavDefDataBase.Select<WhiteList>(typeof(WhiteList), string.Empty, new DynamicParameters());
    }

    [HttpPut("InsertWhiteLists")]
    public bool InsertWhiteLists([FromQuery] IEnumerable<string> droneSerialNum)
    {
        var whiteList = new WhiteList();
        foreach (var s in droneSerialNum)
        {
            whiteList.DroneSerialNum = s;
            if (UavDefDataBase.Insert(whiteList) < 0) return false;
        }
        return true;
    }

    [HttpDelete("DeleteWhiteLists")]
    public bool DeleteWhiteLists([FromQuery] IEnumerable<int> ids)
    {
        return UavDefDataBase.Delete(typeof(WhiteList), ids.ToArray());
    }
}
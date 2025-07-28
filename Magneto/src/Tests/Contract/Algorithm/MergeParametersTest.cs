using System;
using System.Collections.Generic;
using System.IO;
using Contract.Algorithm;
using Magneto.Protocol.Define;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Tests.Contract.Algorithm;

public class MergeParametersTest
{
    [Test]
    [Order(1)]
    public void GetMergeParametersTest()
    {
        try
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\doc\\Template\\Device");
            List<List<Parameter>> list = new();
            var diri = new DirectoryInfo(path);
            var count = 0;
            foreach (var file in diri.GetFiles())
            {
                if (file.Extension != ".json") continue;
                var json = File.ReadAllText(file.FullName);
                var module = JsonConvert.DeserializeObject<ModuleInfo>(json);
                if (module.ModuleType != ModuleType.Device || module.Category != ModuleCategory.Monitoring) continue;
                list.Add(module.Parameters);
                count++;
                if (count == 3) break;
            }

            MergeParameters.GetMergeParameters(list.ToArray());
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }
}
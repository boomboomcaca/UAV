using System;
using System.Collections.Generic;
using System.Linq;
using Magneto.Protocol.Define;

namespace Magneto.Contract.Algorithm;

public static class MergeParameters
{
    public static List<Parameter> GetMergeParameters(params List<Parameter>[] multiEdgeParameters)
    {
        if (multiEdgeParameters == null || multiEdgeParameters.Length == 0) return null;
        Dictionary<string, Parameter> dic = new();
        foreach (var parameters in multiEdgeParameters)
        {
            if (parameters == null || parameters.Count == 0) continue;
            foreach (var parameter in parameters)
            {
                // 如果参数不存在则直接添加，取并集
                if (!dic.ContainsKey(parameter.Name))
                {
                    dic.Add(parameter.Name, parameter);
                    continue;
                }

                // 如果参数已经存在，则对参数的每个字段进行合并
                // values取交集，max与min取交集
                var mergePara = dic[parameter.Name];
                // 1. 处理maximum,minimum,step
                // 最大值取较小值
                MergeMaxMinStep(mergePara, parameter);
                // 2. 处理values与displayValues，取交集
                MergeValues(mergePara, parameter);
                // 3. 处理子参数template
                if (parameter.Template != null)
                {
                    if (mergePara.Template == null)
                        mergePara.Template = parameter.Template;
                    else
                        mergePara.Template = GetMergeParameters(mergePara.Template, parameter.Template);
                }
            }
        }

        return dic.Select(p => p.Value).ToList();
    }

    private static void MergeMaxMinStep(Parameter mergePara, Parameter newPara)
    {
        if (newPara.Maximum != null
            && double.TryParse(newPara.Maximum.ToString(), out var max))
        {
            if (mergePara.Maximum == null)
                mergePara.Maximum = max;
            else if (double.TryParse(mergePara.Maximum.ToString(), out var mm))
                if (max < mm)
                    mergePara.Maximum = max;
        }

        // 最小值取较大值
        if (newPara.Minimum != null
            && double.TryParse(newPara.Minimum.ToString(), out var min))
        {
            if (mergePara.Minimum == null)
                mergePara.Minimum = min;
            else if (double.TryParse(mergePara.Maximum.ToString(), out var mm))
                if (min > mm)
                    mergePara.Minimum = min;
        }

        // 步进值取较小值
        if (newPara.Step != null && double.TryParse(newPara.Step.ToString(), out var step))
        {
            if (mergePara.Step == null)
                mergePara.Step = step;
            else if (double.TryParse(mergePara.Step.ToString(), out var ms))
                if (step < ms)
                    mergePara.Step = step;
        }
    }

    private static void MergeValues(Parameter mergePara, Parameter newPara)
    {
        if (newPara.Values == null || newPara.DisplayValues == null || newPara.Values.Count == 0 ||
            newPara.DisplayValues.Count == 0) return;
        if (mergePara.Values == null || mergePara.DisplayValues == null || newPara.Values.Count == 0 ||
            newPara.DisplayValues.Count == 0)
        {
            mergePara.Values = newPara.Values;
            mergePara.DisplayValues = newPara.DisplayValues;
            return;
        }

        switch (mergePara.Type)
        {
            case ParameterDataType.None:
            case ParameterDataType.String:
            {
                var mvs = mergePara.Values.ConvertAll(item => item.ToString());
                var vs = newPara.Values.ConvertAll(item => item.ToString());
                List<object> mlist = new(); // values交集
                List<string> dlist = new(); // displays交集
                for (var i = 0; i < mvs.Count; i++)
                    if (vs.Contains(mvs[i]))
                    {
                        mlist.Add(mvs[i]);
                        dlist.Add(mergePara.DisplayValues[i]);
                    }

                mergePara.Values = mlist;
                mergePara.DisplayValues = dlist;
                if (!mlist.Contains(mergePara.Value) && mergePara.Values.Count > 0)
                    mergePara.Value = mergePara.Values[0];
            }
                break;
            case ParameterDataType.Bool:
            {
                var mvs = mergePara.Values.ConvertAll(item => Convert.ToBoolean(item));
                var vs = newPara.Values.ConvertAll(item => Convert.ToBoolean(item));
                List<object> mlist = new(); // values交集
                List<string> dlist = new(); // displays交集
                for (var i = 0; i < mvs.Count; i++)
                    if (vs.Contains(mvs[i]))
                    {
                        mlist.Add(mvs[i]);
                        dlist.Add(mergePara.DisplayValues[i]);
                    }

                mergePara.Values = mlist;
                mergePara.DisplayValues = dlist;
                if (!mlist.Contains(Convert.ToBoolean(mergePara.Value)) && mergePara.Values.Count > 0)
                    mergePara.Value = mergePara.Values[0];
            }
                break;
            case ParameterDataType.Number:
            {
                var mvs = mergePara.Values.ConvertAll(item => Convert.ToDouble(item));
                var vs = newPara.Values.ConvertAll(item => Convert.ToDouble(item));
                List<object> mlist = new(); // values交集
                List<string> dlist = new(); // displays交集
                for (var i = 0; i < mvs.Count; i++)
                    // 数值相等判断
                    if (vs.Any(item => Math.Abs(item - mvs[i]) < 1e-7))
                    {
                        mlist.Add(mvs[i]);
                        dlist.Add(mergePara.DisplayValues[i]);
                    }

                mergePara.Values = mlist;
                mergePara.DisplayValues = dlist;
                if (!mlist.Contains(Convert.ToDouble(mergePara.Value)) && mergePara.Values.Count > 0)
                    mergePara.Value = mergePara.Values[0];
            }
                break;
            case ParameterDataType.List:
                // 这种情况不存在
                break;
        }
    }
}
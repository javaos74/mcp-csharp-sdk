using Microsoft.VisualBasic;
using ModelContextProtocol.Server;
using System.ComponentModel;
using UiPath.Robot.Api;
using PTST.UiPath.Orchestrator.API;
using PTST.UiPath.Orchestrator.Models;
using System.Diagnostics;
using Newtonsoft.Json;

namespace UiPath.Robot.MCP.Tools;

[McpServerToolType]
public class UiPathRobotTool
{
 
    [McpServerTool, Description("Get installed process list")]
    public static async Task<string> GetProcessList() 
    {
        var helper = RobotHelper.getRobotHelper();
        //var client = new RobotClient();
        var processes = await helper.getRobotClient().GetProcesses();
        if( processes == null || processes.Count == 0)
        {
            return "No processes found in robot.";
        }
        else
        {
            return string.Join("\n---\n", processes.Select(p =>
            {
                return $"""
                    Process Name: {p.Name}
                    Process Description: {p.Description}
                    Process Key: {p.Key}
                    """;
            }));
        }

    }

    [McpServerTool, Description("Get specific process input argument for invocation")] 
    public static string GetProcessInputParameter(
        [Description("Process Key to get process input argument")] string processKey)   
    {
#if DEBUG
        //Debugger.Launch();
#endif
        var helper = RobotHelper.getRobotHelper();
        var release = helper.findProcessWithKey(processKey);
        if( release == null)
        {
            return "No processes found in specified folders.";
        }
        else        
        {
            var inputArguments = release.Arguments.Input;
            return helper.ConvertToParameter(inputArguments);
        }   
    }


    [McpServerTool, Description("Invoke process with given arguments")]
    public static async Task<string> InvokeProcess(
        [Description("Process Key to invoke")] string processKey,
        [Description("Input Arguments")] Dictionary<string, object> inputArguments)
    {
#if DEBUG
        //Debugger.Launch();
#endif
        var helper = RobotHelper.getRobotHelper();
        var process = helper.getRobotClient().GetProcesses().Result.Where( p => p.Key.ToString() == processKey).FirstOrDefault();
        if( process == null)
        {
            return "No processes found in specified folders.";
        }
        else
        {
            var job = process.ToJob();
            foreach(var k in inputArguments.Keys)
            { 
                var v = (System.Text.Json.JsonElement)inputArguments[k];
                switch( v.ValueKind)
                {
                    case System.Text.Json.JsonValueKind.String:
                        job.InputArguments[k] = v.GetString();
                        break;
                    case System.Text.Json.JsonValueKind.Number:
                        job.InputArguments[k] = v.GetInt64();
                        break;
                    case System.Text.Json.JsonValueKind.True:
                    case System.Text.Json.JsonValueKind.False:
                        job.InputArguments[k] = v.GetBoolean() ;
                        break;
                }
            }
            var result = await helper.getRobotClient().RunJob(job);
            return JsonConvert.SerializeObject(result.Arguments);
        }
    }
   

}

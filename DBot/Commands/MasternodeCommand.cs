using DBot.Utils;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DBot.Commands
{    
    public class MasternodeCommand
    {
        static string MASTERNODE_URL = "http://127.0.0.1:9918/";
        static NetworkCredential _credentials = new NetworkCredential("someuser", "somepass");
        const string ERROR = "Unable to reach absolute daemon";

        public MasternodeCommand()
        {
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["jsonrpc_url"]))
                MASTERNODE_URL = ConfigurationManager.AppSettings["jsonrpc_url"];

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["jsonrpc_user"]) && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["jsonrpc_pass"]))
                _credentials = new NetworkCredential(ConfigurationManager.AppSettings["jsonrpc_user"], ConfigurationManager.AppSettings["jsonrpc_pass"]);
        }
                       

        [Command("node-count")] // let's define this method as a command
        [Description("Show current masternode count")] // this will be displayed to tell users what this command does when they invoke help
        [Aliases("masternode-count")] // alternative names for the command
        public async Task Count(CommandContext ctx) // this command takes no arguments
        {
            // let's trigger a typing indicator to let
            // users know we're working
            await ctx.TriggerTypingAsync();

            string result = string.Empty;
            using (System.Net.Http.HttpClient _client = new System.Net.Http.HttpClient(new HttpClientHandler { Credentials = _credentials }))
            {
                RPCRequest command = new RPCRequest()
                {
                    Id = "node-count",
                    Method = "masternode",
                    Parameters = new List<string>() { "count" }

                };
                try
                {


                    var response = await _client.PostAsync(MASTERNODE_URL, new System.Net.Http.StringContent(command.ToString()));
                    response.EnsureSuccessStatusCode();

                    var json = JsonConvert.DeserializeObject<RPCResponse>(await response.Content.ReadAsStringAsync());
                    var emoji = DiscordEmoji.FromName(ctx.Client, ":thumbsup:");
                    result = $"Actually we have {json.Result} Masternodes online! Not so bad huh! {emoji}";
                }
                catch (Exception e)
                {
                    ctx.Client.DebugLogger.LogMessage(LogLevel.Error, "DBot", $"{ctx.User.Username} tried executing '{ctx.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.GetType()}: {e.Message ?? "<no message>"}", DateTime.Now);

                    result = ERROR;
                }
            }

            await ctx.RespondAsync(result);
        }

        [Command("node-status")] // let's define this method as a command
        [Description("Show your current masternode status - DM")] // this will be displayed to tell users what this command does when they invoke help
        [Aliases("masternode-status")] // alternative names for the command
        public async Task Status(CommandContext ctx, string ip) 
        {
            // let's trigger a typing indicator to let
            // users know we're working
            await ctx.TriggerTypingAsync();

            string result = string.Empty;
            using (System.Net.Http.HttpClient _client = new System.Net.Http.HttpClient(new HttpClientHandler { Credentials = _credentials }))
            {
                RPCRequest command = new RPCRequest()
                {
                    Id = "node-status",
                    Method = "masternodelist",
                    Parameters = new List<string>() { "info", ip }

                };
                try
                {
                    var response = await _client.PostAsync(MASTERNODE_URL, new System.Net.Http.StringContent(command.ToString()));
                    response.EnsureSuccessStatusCode();

                    var json = JsonConvert.DeserializeObject<RPCResponse>(await response.Content.ReadAsStringAsync());
                    string line = ((JObject)json.Result).First.First.ToString();
                    string[] values = line.Split(' ').Where(lc=>lc.Trim().Length > 0).ToArray();
                    StringBuilder c = new StringBuilder("Status : ");
                    c.AppendLine(values[0]);
                    c.AppendFormat("Last seen : {0:MM/dd/yyy - HH\\:mm\\:ss}\r\n", values[3].FromUnixTime());

                    c.Append("Up time : ");
                    if (!string.IsNullOrEmpty(values[4]))
                        c.AppendLine(TimeSpan.FromSeconds(int.Parse(values[4])).ToString().Replace(".", "d "));
                    else
                        c.AppendLine("N/A");

                    c.Append("Sentinel version : ");
                    if(!string.IsNullOrEmpty(values[5]))
                        c.AppendFormat("{0}", values[5]);
                    else
                        c.AppendLine("N/A");

                    result = c.ToString();

                }
                catch (Exception e)
                {
                    ctx.Client.DebugLogger.LogMessage(LogLevel.Error, "DBot", $"{ctx.User.Username} tried executing '{ctx.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.GetType()}: {e.Message ?? "<no message>"}", DateTime.Now);

                    result = ERROR;
                }
            }
            
            await ctx.RespondAsync(result);
        }
    }

    class RPCRequest
    {
        [JsonProperty("jsonrpc")]
        public string JsonRPC { get { return "1.0"; } }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("method")]
        public string Method { get; set; }
        [JsonProperty("params")]
        public List<string> Parameters { get; set; } = new List<string>();

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    class RPCResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("error")]
        public object Error { get; set; }
        [JsonProperty("result")]
        public object Result { get; set; }

        [JsonIgnore]
        public bool IsError { get { return Error != null; } }
    }
}

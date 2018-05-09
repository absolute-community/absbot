using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBot.Commands
{
    public class PoolCommand
    {
        static List<PoolOject> _pools;
        public PoolCommand()
        {
            string json = System.IO.File.ReadAllText(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Files", "pools.json"));
            _pools = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PoolOject>>(json);
        }

        [Command("pool-list")] // let's define this method as a command
        [Description("List all pools")] // this will be displayed to tell users what this command does when they invoke help
        [Aliases("pools")] // alternative names for the command
        public async Task Pools(CommandContext ctx) // this command takes no arguments
        {
            // let's trigger a typing indicator to let
            // users know we're working
            await ctx.TriggerTypingAsync();

            int i = 0;
            await ctx.RespondAsync(string.Format("Pool list : \r\n{0}\r\n\r\nType pool <index> to get detail.",
                    string.Join("\r\n", _pools.Select(c => string.Format("{0} - {1}", ++i, c.Name))
                )));
        }

        [Command("pool-detail")]
        [Description("Get pool info")]
        [Aliases("pool")]
        public async Task Pool(CommandContext ctx, [Description("pool index.")] int index)
        {
            await ctx.TriggerTypingAsync();

            index = Math.Max(1, index);
            index = Math.Min(index, _pools.Count);
            await ctx.RespondAsync(_pools[index - 1].ToString());
        }
    }

    public class PoolOject
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Command { get; set; }

        public override string ToString()
        {
            return $"Name : {Name} \r\nUrl : {Url}\r\nCommand : {Command}";
        }
    }
}

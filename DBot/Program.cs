using DBot.Commands;
using DBot.Formatter;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBot
{
    public class Program
    {
        public DiscordClient Client { get; set; }
        public CommandsNextModule Commands { get; set; }        

        const string TOKEN = "NDMxMDE4MTE3ODAwNTI1ODI0.DaYrIg.CsQLf8vAB02i50vGAJObbebFD_c";
        static void Main(string[] args)
        {
            var prog = new Program();
            prog.RunBotAsync().GetAwaiter().GetResult();
        }

        public async Task RunBotAsync()
        {
            var cfg = new DiscordConfiguration
            {
                Token = TOKEN,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                LogLevel = LogLevel.Info,
                UseInternalLogHandler = true
            };


            
            this.Client = new DiscordClient(cfg);
            this.Client.Ready += Client_Ready;
            this.Client.GuildAvailable += Client_GuildAvailable;
            this.Client.ClientErrored += Client_ClientErrored;
            

            var ccfg = new CommandsNextConfiguration
            {
                // let's use the string prefix defined in config.json
                StringPrefix = ConfigurationManager.AppSettings["stringprefix"],

                // enable responding in direct messages
                EnableDms = (ConfigurationManager.AppSettings["enable_DM"] == "true"),

                // enable mentioning the bot as a command prefix
                EnableMentionPrefix = true
            };

            // and hook them up
            this.Commands = this.Client.UseCommandsNext(ccfg);
            
            this.Commands.CommandExecuted += this.Commands_CommandExecuted;
            this.Commands.CommandErrored += this.Commands_CommandErrored;
            
            
            
            // up next, let's register our commands
            this.Commands.RegisterCommands<InfoCommand>();
            this.Commands.RegisterCommands<PriceCommand>();
            this.Commands.RegisterCommands<PoolCommand>();
            this.Commands.RegisterCommands<MasternodeCommand>();
            // set up our custom help formatter
            this.Commands.SetHelpFormatter<HelpFormatter>();

            // finally, let's connect and log in
            await this.Client.ConnectAsync();

            // and this is to prevent premature quitting
            await Task.Delay(-1);
        }

      

        private Task Client_ClientErrored(DSharpPlus.EventArgs.ClientErrorEventArgs e)
        {
            // let's log the details of the error that just 
            // occured in our client
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "DBot", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(DSharpPlus.EventArgs.GuildCreateEventArgs e)
        {
            // let's log the name of the guild that was just
            // sent to our client
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "DBot", $"Guild available: {e.Guild.Name}", DateTime.Now);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Client_Ready(DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            // let's log the fact that this event occured
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "DBot", "Client is ready to process events.", DateTime.Now);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            // let's log the name of the command and user
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "DBot", $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            // let's log the error details
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "DBot", $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

            // let's check if the error is a result of lack
            // of required permissions
            if (e.Exception is ChecksFailedException ex)
            {
                // yes, the user lacks required permissions, 
                // let them know

                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                // let's wrap the response into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = new DiscordColor(0xFF0000) // red
                    // there are also some pre-defined colors available
                    // as static members of the DiscordColor struct
                };
                await e.Context.RespondAsync("", embed: embed);
            }
        }
    }
}


using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBot.Formatter
{
    class HelpFormatter : IHelpFormatter
    {
        private StringBuilder MessageBuilder { get; }

        public HelpFormatter()
        {
            this.MessageBuilder = new StringBuilder();
        }

        // this method is called first, it sets the current command's name
        // if no command is currently being processed, it won't be called
        public IHelpFormatter WithCommandName(string name)
        {
            this.MessageBuilder.Append("Command: ")
                .AppendLine(DSharpPlus.Formatter.Bold(name))
                .AppendLine();

            return this;
        }

        // this method is called second, it sets the current command's 
        // description. if no command is currently being processed, it 
        // won't be called
        public IHelpFormatter WithDescription(string description)
        {
            this.MessageBuilder.Append("Description: ")
                .AppendLine(description)
                .AppendLine();

            return this;
        }

        // this method is called third, it is used when currently 
        // processed group can be executed as a standalone command, 
        // otherwise not called
        public IHelpFormatter WithGroupExecutable()
        {
            this.MessageBuilder.AppendLine("This group is a standalone command.")
                .AppendLine();

            return this;
        }

        // this method is called fourth, it sets the current command's 
        // aliases. if no command is currently being processed, it won't
        // be called
        public IHelpFormatter WithAliases(IEnumerable<string> aliases)
        {
            this.MessageBuilder.Append("Aliases: ")
                .AppendLine(string.Join(", ", aliases))
                .AppendLine();

            return this;
        }

        // this method is called fifth, it sets the current command's 
        // arguments. if no command is currently being processed, it won't 
        // be called
        public IHelpFormatter WithArguments(IEnumerable<CommandArgument> arguments)
        {
            this.MessageBuilder.Append("Arguments: ")
                .AppendLine(string.Join(", ", arguments.Select(xarg => $"{xarg.Name} ({xarg.Type.ToUserFriendlyName()})")))
                .AppendLine();

            return this;
        }

        // this method is called sixth, it sets the current group's subcommands
        // if no group is being processed or current command is not a group, it 
        // won't be called
        public IHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            this.MessageBuilder.Append("Subcommands: ")
                .AppendLine(string.Join(", ", subcommands.Select(xc => xc.Name)))
                .AppendLine();

            return this;
        }

        // this is called as the last method, this should produce the final 
        // message, and return it
        public CommandHelpMessage Build()
        {
            return new CommandHelpMessage(this.MessageBuilder.ToString().Replace("\r\n", "\n"));
        }
    }
}

using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Farmingway
{
	// Create a module with no prefix
	public class InfoModule : ModuleBase<SocketCommandContext>
	{
		// ~say hello world -> hello world
		[Command("say")]
		[Summary("Echoes a message.")]
		public Task SayAsync([Remainder][Summary("The text to echo")] string echo)
			=> ReplyAsync(echo);

		// ReplyAsync is a method on ModuleBase 

		// ~char 931843984 -> Xiaofei Li ...
		[Command("char")]
		[Summary("Prints information about a character")]
		public Task CharAsync([Remainder][Summary("The id of your character")] string charId)
        {
			return ReplyAsync("Looking up info for id: " + charId);
        }

		// ReplyAsync is a method on ModuleBase 
	}
}

using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Farmingway
{
	public class PingModule : ModuleBase<SocketCommandContext>
	{
		// ~ping -> [pong]
		[Command("ping")]
		[Summary("It replies pong.")]
		public Task SayAsync()
			=> ReplyAsync("Pong!");

		// ReplyAsync is a method on ModuleBase 
	}
}

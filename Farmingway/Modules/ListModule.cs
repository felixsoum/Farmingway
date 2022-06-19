using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Farmingway.Modules
{
    [Group("list")]

    public class ListModule : ModuleBase<SocketCommandContext>
    {
		// !list ponies
		[Command("ponies")]
		[Summary("List all ponies.")]
		public async Task PoniesAsync()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Here's a list of all ponies:");
            foreach (var pony in MountDatabase.GetPonies())
            {
				sb.AppendLine($"{pony.Id} - {pony.Name}");
			}
			await ReplyAsync(sb.ToString());
		}
	}
}

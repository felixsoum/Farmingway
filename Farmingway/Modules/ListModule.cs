using Discord.Commands;
using System.Text;
using System.Threading.Tasks;

namespace Farmingway.Modules
{
    [Group("list")]

    public class ListModule : ModuleBase<SocketCommandContext>
    {
		[Command("")]
		[Summary("Print help for !list.")]
		public Task ListAsync() => ReplyAsync("Please specify which type of mount to list. Example: `!list ponies`");

		// !list ponies
		[Command("ponies")]
		[Summary("List all ponies.")]
		public async Task PoniesAsync()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("**Here's a list of all ponies:**");
            foreach (var pony in MountDatabase.GetPonies())
            {
				sb.AppendLine($"{pony.Id}: {pony.Name}, {pony.Sources[0].Type} - {pony.Sources[0].Text}");
			}
			await ReplyAsync(sb.ToString());
		}
	}
}

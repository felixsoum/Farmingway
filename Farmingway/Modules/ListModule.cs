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
		public Task ListAsync() => ReplyAsync("Please specify which type of mount to list.\n(ponies, birdies, doggos, gwibs, gatos)\nExample: `!list ponies`");

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

		// !list birdies
		[Command("birdies")]
		[Summary("List all birdies.")]
		public async Task BirdiesAsync()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("**Here's a list of all birdies:**");
			foreach (var birdie in MountDatabase.GetBirdies())
			{
				sb.AppendLine($"{birdie.Id}: {birdie.Name}, {birdie.Sources[0].Type} - {birdie.Sources[0].Text}");
			}
			await ReplyAsync(sb.ToString());
		}

		// !list doggos
		[Command("doggos")]
		[Summary("List all doggos.")]
		public async Task DoggosAsync()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("**Here's a list of all doggos:**");
			foreach (var doggo in MountDatabase.GetDoggos())
			{
				sb.AppendLine($"{doggo.Id}: {doggo.Name}, {doggo.Sources[0].Type} - {doggo.Sources[0].Text}");
			}
			await ReplyAsync(sb.ToString());
		}

		// !list doggos
		[Command("gwibs")]
		[Summary("List all gwibs.")]
		public async Task GwibsAsync()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("**Here's a list of all gwibs:**");
			foreach (var gwib in MountDatabase.GetGwibs())
			{
				sb.AppendLine($"{gwib.Id}: {gwib.Name}, {gwib.Sources[0].Type} - {gwib.Sources[0].Text}");
			}
			await ReplyAsync(sb.ToString());
		}

		// !list gatos
		[Command("gatos")]
		[Summary("List all gatos.")]
		public async Task GatosAsync()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("**Here's a list of all gatos:**");
			foreach (var gwib in MountDatabase.GetGatos())
			{
				sb.AppendLine($"{gwib.Id}: {gwib.Name}, {gwib.Sources[0].Type} - {gwib.Sources[0].Text}");
			}
			await ReplyAsync(sb.ToString());
		}
	}
}

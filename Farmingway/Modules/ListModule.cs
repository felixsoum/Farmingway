using Discord.Commands;
using Farmingway.RestResponses;
using System.Collections.Generic;
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

		static StringBuilder BuildList(string name, List<MountResponse> mounts)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"**Here's a list of all {name}**");
			foreach (var mount in mounts)
			{
				sb.AppendLine($"{mount.Name} [{mount.Id}] *{mount.Sources[0].Text}*");
			}

			return sb;
		}

		// !list ponies
		[Command("ponies")]
		[Summary("List all ponies.")]
		public async Task PoniesAsync()
        {
            StringBuilder sb = BuildList("ponies :racehorse:", MountDatabase.GetPonies());
            await ReplyAsync(sb.ToString());
        }

        // !list birdies
        [Command("birdies")]
		[Summary("List all birdies.")]
		public async Task BirdiesAsync()
		{
            StringBuilder sb = BuildList("birdies :bird:", MountDatabase.GetBirdies());
			await ReplyAsync(sb.ToString());
		}

		// !list doggos
		[Command("doggos")]
		[Summary("List all doggos.")]
		public async Task DoggosAsync()
		{
            StringBuilder sb = BuildList("doggos :dog2:", MountDatabase.GetDoggos());
			await ReplyAsync(sb.ToString());
		}

		// !list gwibs
		[Command("gwibs")]
		[Summary("List all gwibs.")]
		public async Task GwibsAsync()
		{
            StringBuilder sb = BuildList("gwibs :dragon:", MountDatabase.GetGwibs());
			await ReplyAsync(sb.ToString());
		}

		// !list gatos
		[Command("gatos")]
		[Summary("List all gatos.")]
		public async Task GatosAsync()
		{
            StringBuilder sb = BuildList("gatos :cat2:", MountDatabase.GetGatos());
			await ReplyAsync(sb.ToString());
		}
	}
}

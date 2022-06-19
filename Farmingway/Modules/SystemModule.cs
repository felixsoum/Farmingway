using Discord.Commands;
using System.Threading.Tasks;

namespace Farmingway.Modules
{
    public class SystemModule : ModuleBase<SocketCommandContext>
    {
        const ulong XiaofeiID = 139024035894788096;
        const ulong MioID = 158402154632052736;

        // !exit -> Exit()
        [Command("exit")]
        [Summary("Nicely shut down..")]
        public async Task ExitAsync()
        {
            if (IsAuthorised(Context.User.Id))
            {
                await ReplyAsync("Signing off for a little nap... zzz...");
                Program.Exit();
            }
            else
            {
                await ReplyAsync($"{Context.User.Username}, you are not authorised for this command!");
            }
        }

        private bool IsAuthorised(ulong id) => id == XiaofeiID || id == MioID;

        // !ping -> Pong.
        [Command("ping")]
        [Summary("It replies pong.")]
        public Task PingAsync() => ReplyAsync("Pong!");
    }
}

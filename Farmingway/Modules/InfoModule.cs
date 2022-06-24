using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Farmingway.Modules
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
        public Task CharAsync([Remainder][Summary("The id of your character")] int charId)
        {
            var embed = new EmbedBuilder();
            
            try
            {
                var character = CollectService.GetCharacter(charId);
                var highestMountID = character.Mounts.IDs[character.Mounts.IDs.Length - 1];

                var mount = CollectService.GetMount(highestMountID);

                embed.WithColor(new Color(0, 255, 0))
                    .WithTitle($"Character data for ID {charId}")
                    .AddField("Name", character.Name)
                    .AddField("Server", character.Server)
                    .AddField("Mounts", character.Mounts.Count)
                    .AddField("Highest Mount", $"{mount.Name} (ID {mount.Id})")
                    .WithImageUrl(character.Portrait);
            }
            catch(Exception e)
            {
                Console.Write("ERROR: " + e.Message);
                embed.WithColor(new Color(255, 0, 0))
                    .WithTitle("ERROR")
                    .WithDescription(e.Message);
            }

            return ReplyAsync(embed: embed.Build());
        }

        [Command("find")]
        [Summary("Prints information about a character by Discord username")]
        public async Task FindByUserAsync([Summary("The user requested")] params IGuildUser[] users)
        {
            foreach (var user in users)
            {
                try
                {
                    await ReplyAsync(embed: DiscordUtils.CreateCharacterEmbed(user));
                }
                catch(Exception e)
                {
                    Console.Write("ERROR: " + e.Message);
                    await ReplyAsync(embed: DiscordUtils.CreateErrorEmbed(e.Message));
                }
            }
            
        }
        
        [Command("find")]
        [Summary("Prints information about a character by Discord username")]
        public async Task FindByUsernameAsync([Summary("The user requested")] params string[] usernames)
        {
            List<IGuildUser> matchedUsers;
            try
            {
                matchedUsers = await DiscordUtils.GetUsersFromUsernames(Context, usernames);
            }
            catch (Exception e)
            {
                await ReplyAsync(embed: DiscordUtils.CreateErrorEmbed(e.Message));
                return;
            }

            await FindByUserAsync(matchedUsers.ToArray());
        }

        // !whoshere
        [Command("whoshere")]
        [Summary("Print list of users in thread.")]
        public async Task WhosHereASync()
        {
            var users = Context.Channel.GetUsersAsync();
            var flatUsers = await AsyncEnumerableExtensions.FlattenAsync(users);
            var filteredUsers = flatUsers.Where(x => !x.IsBot).ToList();
            var sb = new StringBuilder();
            sb.AppendLine($"I can find {filteredUsers.Count} users here.");
            foreach (var user in filteredUsers)
            {
                sb.AppendLine(user.Username);
            }
            await ReplyAsync(sb.ToString());
        }

    }
}

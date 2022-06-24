using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Farmingway.Modules
{
    public class MountsModule : ModuleBase<SocketCommandContext>
    {
        [Command("mounts")]
        [Summary("Prints mounts that users are missing")]
        public Task MountByUserAsync([Summary("The users to search")] params IGuildUser[] users)
        {
            var userList = users.ToList();
            userList.Insert(0, (IGuildUser)Context.User);
            
            var userMounts = new HashSet<int>();

            foreach (var user in userList)
            {
                try
                {
                    var character = CollectService.GetCharacterFromDiscord(user);
                    userMounts.UnionWith(character.Mounts.IDs);
                }
                catch (Exception e)
                {
                    return ReplyAsync(embed: DiscordUtils.CreateErrorEmbed(e.Message));
                }
            }

            var missing = MountDatabase.mounts.Values
                .Where(m => Array.Exists(m.Sources, s => s.Type.Equals("Trial")))
                .Select(m => m.Id)
                .Except(userMounts)
                .ToList();

            var userStrings = userList.Select(u => $"{u.Username}#{u.Discriminator}");
            var embed = new EmbedBuilder();

            embed.WithTitle($"Farm search for users {string.Join(", ", userStrings)}")
                .WithColor(new Color(0, 255, 0));

            if (missing.Count > 0)
            {
                embed.WithDescription($"Found {missing.Count} mounts to farm");
                foreach (var i in missing)
                {
                    embed.AddField($"{i} - {MountDatabase.mounts[i].Name}", MountDatabase.mounts[i].Sources[0].Text);
                }

            }
            else
            {
                embed.WithDescription("The users do not have common missing trial mounts");
            }

            return ReplyAsync(embed: embed.Build());
        }
        
        [Command("mounts")]
        [Summary("Prints mounts that users are missing")]
        public async Task MountByUsernameAsync([Summary("The users to search")] params string[] usernames)
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

            await MountByUserAsync(matchedUsers.ToArray());
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Farmingway.RestResponses;

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

            var characters = userList.Select(CollectService.GetCharacterFromDiscord).ToList();
            return ReplyAsync(embed: Suggest(characters));
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
        
        [Command("mounts")]
        [Summary("Prints mounts that users are missing")]
        public Task MountByLodestoneIdAsync([Summary("The characters to search")] params int[] charIds)
        {
            var characters = charIds.Select(CollectService.GetCharacter).ToList();
            return ReplyAsync(embed: Suggest(characters));
        }
        
        private Embed Suggest(List<CharacterResponse> characters)
        {
            var mountLists = characters.Select(c => new HashSet<int>(c.Mounts.IDs)).ToList();
            var mountCount = MountDatabase.GetTrialMounts().ToDictionary(
                mount => mount.Id, 
                mount => mountLists.Count(s => s.Contains(mount.Id))
            );

            // Take the 5 least-collected mounts from the group
            var suggestion = mountCount.Where(e => e.Value < characters.Count)
                .OrderBy(e => e.Value)
                .ThenByDescending(e =>
                {
                    var ownedString = MountDatabase.mounts[e.Key].Owned;
                    return float.Parse(ownedString.Substring(0, ownedString.Length - 1));
                })
                .Take(5)
                .ToImmutableSortedDictionary();
            
            var charNames = characters.Select(c => c.Name);
            
            var embed = new EmbedBuilder();
            embed.WithTitle($"Farmingway's suggestion for {string.Join(", ", charNames)}")
                .WithColor(new Color(0, 255, 0));

            if (suggestion.Count > 0)
            {
                foreach (var i in suggestion)
                {
                    var mount = MountDatabase.mounts[i.Key];
                    var info = $"{mount.Sources[0].Text} -- obtained by " +
                               (i.Value > 0 ? $"{i.Value} in group, " : "") + $"{mount.Owned} overall";
                    embed.AddField(mount.Name, info);
                }

            }
            else
            {
                embed.WithDescription("Could not find a suitable trial farm suggestion");
            }

            return embed.Build();
        }
    }
}
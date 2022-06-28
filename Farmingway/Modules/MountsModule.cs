using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Farmingway.RestResponses;

namespace Farmingway.Modules
{
    public class MountsModule : ModuleBase<SocketCommandContext>
    {
        private static NetstoneService _service = new();

        [Command("mounts")]
        [Summary("Prints mounts that users are missing")]
        public Task MountByUserAsync([Summary("The users to search")] params IGuildUser[] users)
        {
            var userList = users.ToList();
            userList.Insert(0, (IGuildUser)Context.User);

            var characters = userList.Select(CollectService.GetCharacterFromDiscord).ToList();

            return MountByCharacterResponse(characters);
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
            return MountByCharacterResponse(characters);
        }

        // TODO Enable lodestone search when Netstone is updated
        // [Command("mountsbyid")]
        public async Task MountByLodestoneIdXIVAPIAsync([Summary("The characters to search")] params int[] charIds)
        {
            if (!_service.isInit)
            {
                await _service.Init();
            }
            
            var mountLists = await Task.WhenAll(charIds.Select(id => _service.GetMountIDs(id)));
            var names = await Task.WhenAll(charIds.Select(id => _service.GetName(id)));
            await ReplyAsync(embed: Suggest(names.ToList(), mountLists.ToList()));
        }

        private Task MountByCharacterResponse(List<CharacterResponse> characters)
        {
            var mountLists = characters.Select(c => new HashSet<int>(c.Mounts.IDs)).ToList();
            var names = characters.Select(c => c.Name).ToList();
            
            return ReplyAsync(embed: Suggest(names, mountLists));
        }
        
        private Embed Suggest(List<string> names, List<HashSet<int>> mountLists)
        {
            var mountCount = MountDatabase.GetTrialMounts().Select(m => new MountCount
            {
                count = mountLists.Count(s => s.Contains(m.Id)),
                mount = m
            });

            // Take the 5 least-collected mounts from the group
            var suggestion = mountCount.Where(m => m.count < mountLists.Count)
                .OrderBy(m => m.count)
                .ThenByDescending(m =>
                {
                    var ownedString = m.mount.Owned;
                    return float.Parse(ownedString.Substring(0, ownedString.Length - 1));
                })
                .Take(5)
                .ToList();
            
            var embed = new EmbedBuilder();
            embed.WithTitle($"Farmingway's suggestion for {string.Join(", ", names)}")
                .WithColor(new Color(0, 255, 0));

            if (suggestion.Count > 0)
            {
                foreach (var i in suggestion)
                {
                    var mount = i.mount;
                    var info = $"{mount.Sources[0].Text} -- obtained by " +
                               (i.count > 0 ? $"{i.count} in group, " : "") + $"{mount.Owned} overall";
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
    
    internal class MountCount
    {
        public int count { get; set; }
        public MountResponse mount { get; set; }
    }
}
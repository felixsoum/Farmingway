using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Farmingway.Exceptions;
using Farmingway.RestResponses;
using Farmingway.Services;
using Farmingway.TypeReaders;

namespace Farmingway.Modules
{
    public class MountsModule : ModuleBase<SocketCommandContext>
    {
        class StoredSuggestion
        {
            public List<string> names;
            public IEnumerable<MountCount> storedMountCounts;

            public StoredSuggestion(List<string> names, IEnumerable<MountCount> storedMountCounts)
            {
                this.names = names;
                this.storedMountCounts = storedMountCounts;
            }

            public int Count => storedMountCounts.ToList().Count;

            public StoredSuggestion TakeFirstFive()
            {
                return new StoredSuggestion(names, storedMountCounts.Take(5));
            }

            public StoredSuggestion TakePastFive()
            {
                return new StoredSuggestion(names, storedMountCounts.TakeLast(Count - 5));
            }
        }

        private static NetstoneService _service = new();
        private static Dictionary<ulong, StoredSuggestion> storedMountCounts = new();

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

        [Command("mountsbyid")]
        public async Task MountByLodestoneIdXIVAPIAsync([Remainder] MountTypeParams searchParams) {
            if (!_service.isInit)
            {
                await _service.Init();
            }

            var charIds = searchParams.charIds;
            var fullMountList = searchParams.mountType == null 
                ? MountDatabase.GetTrialAndRaid() 
                : MountDatabase.GetMountsByOrigin(searchParams.mountType);

            if (fullMountList == null)
            {
                await ReplyAsync(embed: DiscordUtils.CreateErrorEmbed(
                    "Invalid farm option. Please specify `trial`, `raid`, or omit the parameter to search for both."));
                return;
            }

            HashSet<int>[] mountLists;
            string[] names;
            try
            {
                mountLists = await Task.WhenAll(charIds.Select(id => _service.GetMountIDs(id)));
                names = await Task.WhenAll(charIds.Select(id => _service.GetName(id)));
            }
            catch (NotFoundException e)
            {
                await ReplyAsync(embed: DiscordUtils.CreateErrorEmbed(e.Message));
                return;
            }
            

            var mountCount = fullMountList.Select(m => new MountCount
            {
                count = mountLists.Count(s => s.Contains(m.Id)),
                mount = m
            });

            var suggestion = mountCount.Where(m => m.count < mountLists.ToList().Count)
                .OrderBy(m => m.count)
                .ThenByDescending(m =>
                {
                    var ownedString = m.mount.Owned;
                    return float.Parse(ownedString.Substring(0, ownedString.Length - 1));
                })
                .ToList();

            var storedSuggestion = new StoredSuggestion(names.ToList(), suggestion);
            ulong key = Context.Message.Id;
            storedMountCounts.Add(key, storedSuggestion.TakePastFive());

            var builder = new ComponentBuilder().WithButton("More?", key.ToString());
            await ReplyAsync(embed: Suggest(storedSuggestion.TakeFirstFive()), components: builder.Build());
        }

        private Task MountByCharacterResponse(List<CharacterResponse> characters)
        {
            var mountLists = characters.Select(c => new HashSet<int>(c.Mounts.IDs)).ToList();
            var names = characters.Select(c => c.Name).ToList();
            
            return ReplyAsync(embed: Suggest(names, mountLists));
        }

        public static Tuple<Embed, bool> SuggestMore(ulong key)
        {
            if (storedMountCounts.ContainsKey(key))
            {
                var storedSuggestion = storedMountCounts[key];
                var embedResult = Suggest(storedSuggestion.TakeFirstFive());

                if (storedSuggestion.Count > 5)
                {
                    storedMountCounts[key] = storedSuggestion.TakePastFive();
                    return new Tuple<Embed, bool>(embedResult, true);
                }
                else
                {
                    storedMountCounts.Remove(key);
                    return new Tuple<Embed, bool>(embedResult, false);
                }
            }
            else
            {
                return null;
            }
        }

        private static Embed Suggest(StoredSuggestion storedSuggestion)
        {
            var suggestion = storedSuggestion.storedMountCounts.ToList();

            var embed = new EmbedBuilder();
            embed.WithTitle($"Farmingway's suggestion for {string.Join(", ", storedSuggestion.names)}")
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

        private Embed Suggest(List<string> names, List<HashSet<int>> mountLists)
        {
            var mountCount = MountDatabase.GetTrialAndRaid().Select(m => new MountCount
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
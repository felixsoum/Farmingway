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
        const char BackButtonKeycode = 'b';
        const char NextButtonKeycode = 'n';

        public static bool IsNextButtonKeycode(char code) => code == NextButtonKeycode;
        public static string MakeBackButtonKeycode(ulong key) => BackButtonKeycode.ToString() + key;
        public static string MakeNextButtonKeycode(ulong key) => NextButtonKeycode.ToString() + key;

        class StoredSuggestion
        {
            public List<string> names;
            public List<MountCount> storedMountCounts;
            public int pageIndex;
            const int ResultsPerPage = 5;
            public StoredSuggestion(List<string> names, IEnumerable<MountCount> storedMountCounts)
            {
                this.names = names;
                this.storedMountCounts = storedMountCounts.ToList();
            }

            public int PageCount => (Count - 1) / ResultsPerPage + 1;
            public int Count => storedMountCounts.ToList().Count;

            public List<MountCount> TakePage()
            {
                var result = new List<MountCount>();
                for (int i = 0; i < ResultsPerPage; i++)
                {
                    int index = i + pageIndex * ResultsPerPage;
                    if (index >= storedMountCounts.Count)
                    {
                        break;
                    }
                    result.Add(storedMountCounts[index]);
                }
                return result;
            }

            public Tuple<bool, bool> GetNavigationState() => new(HasBack(), HasNext());

            public bool HasBack() =>  pageIndex > 0;

            internal bool HasNext() => (pageIndex + 1) * ResultsPerPage < storedMountCounts.Count;

            internal void Next()
            {
                pageIndex++;
            }

            internal void Back()
            {
                pageIndex--;
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
        public async Task MountByLodestoneIdXIVAPIAsync([Remainder] MountTypeParams searchParams)
        {
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
            storedMountCounts.Add(key, storedSuggestion);

            var builder = new ComponentBuilder()
                .WithButton("Back", 'b' + key.ToString(), disabled: true, emote: new Emoji("\u2B05"))
                .WithButton("Next", 'n' + key.ToString(), emote: new Emoji("\u27A1"));
            await ReplyAsync(embed: Suggest(storedSuggestion), components: builder.Build());
        }

        [Command("mountsbymentions")]
        public async Task MountsByMentionsAsync(params string[] mentions)
        {
            RecordModule.InitDB();

            if (!_service.isInit)
            {
                await _service.Init();
            }

            var charIds = mentions.Select(x => int.Parse(RecordModule.MentionToLodestoneID(x))).ToList();
            var fullMountList = MountDatabase.GetTrialAndRaid();

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
                names = charIds.Select(x => x.ToString()).ToArray();
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
            storedMountCounts.Add(key, storedSuggestion);

            var builder = new ComponentBuilder()
                .WithButton("Back", 'b' + key.ToString(), disabled: true, emote: new Emoji("\u2B05"))
                .WithButton("Next", 'n' + key.ToString(), emote: new Emoji("\u27A1"));
            await ReplyAsync(embed: Suggest(storedSuggestion), components: builder.Build());
        }

        [Command("sm")]
        public async Task StandardMountsByIdAsync(string mountType = null)
        {
            var userIds = new HashSet<int> { 18997658, 36828752, 15921164, 30744572, 18356514, 37378384 };
            await MountByLodestoneIdXIVAPIAsync(new MountTypeParams(userIds, mountType));
        }

        private Task MountByCharacterResponse(List<CharacterResponse> characters)
        {
            var mountLists = characters.Select(c => new HashSet<int>(c.Mounts.IDs)).ToList();
            var names = characters.Select(c => c.Name).ToList();

            return ReplyAsync(embed: Suggest(names, mountLists));
        }

        public static Tuple<Embed, Tuple<bool, bool>> SuggestMore(ulong key, bool isNext)
        {
            if (storedMountCounts.ContainsKey(key))
            {
                var storedSuggestion = storedMountCounts[key];

                if (isNext)
                {
                    storedSuggestion.Next();
                }
                else
                {
                    storedSuggestion.Back();
                }

                return new(Suggest(storedSuggestion), storedSuggestion.GetNavigationState());
            }
            else
            {
                return null;
            }
        }

        private static Embed Suggest(StoredSuggestion storedSuggestion)
        {
            var suggestion = storedSuggestion.TakePage();
            string pageInfo = $"(page {storedSuggestion.pageIndex + 1}/{storedSuggestion.PageCount})";

            var embed = new EmbedBuilder();
            embed.WithTitle($"Farmingway's suggestion for {string.Join(", ", storedSuggestion.names)}. {pageInfo}")
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
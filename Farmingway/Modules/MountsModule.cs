using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Farmingway.Exceptions;
using Farmingway.Modules.Mounts;
using Farmingway.Services;
using Farmingway.TypeReaders;

namespace Farmingway.Modules
{
    public class MountsModule : ModuleBase<SocketCommandContext>
    {
        private static NetstoneService _service = new();
        private static Dictionary<ulong, StoredSuggestion> storedMountCounts = new();

        private static Func<MountCount, float> PartyCollected = m => m.count;

        private static Func<MountCount, float> TotalCollected = m =>
        {
            var ownedString = m.mount.Owned;
            return float.Parse(ownedString[..^1]);
        };

        [Command("mountsbyusername")]
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

            await MountsByMentionsAsync(matchedUsers.Select(u => u.Id.ToString()).ToArray());
        }

        [Command("mounts")]
        [Summary("Prints mounts that users are missing")]
        public async Task MountByLodestoneIdXIVAPIAsync([Remainder] MountTypeParams searchParams)
        {
            if (!_service.isInit)
            {
                await _service.Init();
            }

            var charIds = searchParams.charIds;

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

            var key = Context.Message.Id;
            StoredSuggestion storedSuggestion;
            try
            {
                storedSuggestion = Suggest(names, mountLists, searchParams.mountType, key, searchParams.suggestionPreference);
            }
            catch (Exception e)
            {
                await ReplyAsync( embed: DiscordUtils.CreateErrorEmbed(e.Message));
                return;
            }
            
            await ReplyWithSuggestion(storedSuggestion.BuildFirstPage(key));
        }

        [Command("mounts")]
        public async Task MountsByMentionsAsync(params string[] mentions)
        {
            RecordModule.InitDB();

            var charIds = mentions.Select(x => int.Parse(RecordModule.MentionToLodestoneID(x))).ToHashSet();
            await MountByLodestoneIdXIVAPIAsync(new MountTypeParams(charIds, null, 1));
        }

        [Command("sm")]
        public async Task StandardMountsByIdAsync(int suggestionPreference = 1, string mountType = null)
        {
            var charIds = new HashSet<int> { 18997658, 36828752, 15921164, 30744572, 18356514, 37378384 };
            await MountByLodestoneIdXIVAPIAsync(new MountTypeParams(charIds, mountType, suggestionPreference));
        }

        public static StoredSuggestion GetSuggestion(ulong key)
        {
            return storedMountCounts.ContainsKey(key) ? storedMountCounts[key] : null;
        }

        private Task ReplyWithSuggestion(Tuple<Embed, MessageComponent> messageDetails)
        {
            return ReplyAsync(embed: messageDetails.Item1, components: messageDetails.Item2);
        }
        
        /// <summary>
        /// Generate a StoredSuggestion for given user names and mount collections
        /// </summary>
        /// <param name="names">Character names in suggestion</param>
        /// <param name="mountLists">List of mounts obtained by characters</param>
        /// <param name="mountType">"trial", "raid", or null</param>
        /// <param name="messageId">Message ID associated with this request</param>
        /// <param name="suggestionPreference">0-indexed suggestionPreference from command</param>
        /// <returns></returns>
        /// <exception cref="Exception">If the mountType or suggestionPreference is invalid</exception>
        private static StoredSuggestion Suggest(
            IEnumerable<string> names,
            IEnumerable<HashSet<int>> mountLists,
            string mountType,
            ulong messageId,
            int suggestionPreference
        )
        {
            if (suggestionPreference is < 0 or > 7)
            {
                throw new Exception("Invalid suggestion preference. Please select a preset from 1-8.");
            }
            
            var suggestionMounts = mountType == null
                ? MountDatabase.GetTrialAndRaid()
                : MountDatabase.GetMountsByOrigin(mountType);

            if (suggestionMounts == null)
            {
                throw new Exception("Invalid farm option. Please specify `trial`, `raid`, or omit the parameter to search for both.");
            }
            
            var mountCount = suggestionMounts.Select(m => new MountCount
            {
                count = mountLists.Count(s => s.Contains(m.Id)),
                mount = m
            });

            var playerCount = mountLists.Count();
            var unsorted = mountCount.Where(m => m.count < playerCount);

            var firstSort = suggestionPreference % 2 == 0 ? PartyCollected : TotalCollected;
            var secondSort = suggestionPreference % 2 == 1 ? PartyCollected : TotalCollected;
            
            var sortedOnce = suggestionPreference < 4 
                    ? unsorted.OrderBy(firstSort) 
                    : unsorted.OrderByDescending(firstSort);
                
            var suggestionList = suggestionPreference % 4 < 2
                ? sortedOnce.ThenByDescending(secondSort)
                : sortedOnce.ThenBy(secondSort);

            var suggestion = new StoredSuggestion(names, suggestionList);
            storedMountCounts.Add(messageId, suggestion);
            return suggestion;
        }
    }

    
}
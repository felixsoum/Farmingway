using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Farmingway.Exceptions;
using NetStone;
using NetStone.Search.Character;

namespace Farmingway.Services
{
    public class NetstoneService
    {
        private LodestoneClient _client;
        public bool isInit { get; set; }

        public async Task Init()
        {
            _client = await LodestoneClient.GetClientAsync();
            isInit = true;
        }

        public async Task<string> GetName(int id)
        {
            var character = await _client.GetCharacter(id.ToString());

            if (character == null)
            {
                throw new NotFoundException($"Could not find character {id}");
            }

            return character.Name;
        }

        public async Task<HashSet<int>> GetMountIDs(int id)
        {
            var mounts = await _client.GetCharacterMount(id.ToString());

            if (mounts == null)
            {
                throw new NotFoundException($"Could not find character {id}");
            }

            return mounts
                .Collectables
                .Where(c => MountDatabase.mounts.ContainsKey(c.Name))
                .Select(c => MountDatabase.mounts[c.Name].Id)
                .ToHashSet();
        }

        public async Task<string> GetID(string characterName, string dataCenter)
        {
            var query = new CharacterSearchQuery();
            query.CharacterName = characterName;
            query.World = dataCenter[0].ToString().ToUpperInvariant() + dataCenter.Substring(1, dataCenter.Length - 1).ToLowerInvariant();

            var character = await _client.SearchCharacter(query);

            if (character == null || !character.HasResults)
            {
                throw new NotFoundException($"Could not find character {characterName} in {dataCenter}");
            }

            foreach (var result in character.Results)
            {
                return result.Id;
            }

            throw new NotFoundException($"Could not find character {characterName} in {dataCenter}");
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetStone;

namespace Farmingway
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
            return character.Name;
        }
        
        public async Task<HashSet<int>> GetMountIDs(int id)
        {
            var mounts = await _client.GetCharacterMount(id.ToString());
            return mounts
                .Collectables
                .Select(c => MountDatabase.mounts[c.Name].Id)
                .ToHashSet();
        }
    }
}
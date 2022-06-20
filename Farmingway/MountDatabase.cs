using Farmingway.RestResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Farmingway
{
    internal static class MountDatabase
    {
        internal static Dictionary<int, MountResponse> mounts = new Dictionary<int, MountResponse>();

        internal static async Task Init()
        {
            await Task.Run(() =>
            {
                var data = CollectService.GetMounts();
                foreach (var mount in data)
                {
                    mounts.Add(mount.Id, mount);
                }

                Console.WriteLine($"Successfully recorded {mounts.Count} mounts.");
            });
        }

        internal static List<MountResponse> GetPonies()
        {
            var ponies = new SortedList<int, MountResponse>();
            foreach (var mount in mounts)
            {
                if (IsPony(mount.Key))
                {
                    ponies.Add(mount.Key, mount.Value);
                }
            }
            return ponies.Values.ToList();
        }

        internal static bool IsPony(int id)
        {
            switch (id)
            {
                case 28: // Aithon (Ifrit)
                case 29: // Xanthos (Garuda)
                case 30: // Gullfaxi (Titan)
                case 31: // Enbarr (Leviathan)
                case 40: // Markab (Ramuh)
                case 43: // Boreas (Shiva)
                    return true;
                default:
                    return false;
            }
        }

        internal static List<MountResponse> GetBirdies()
        {
            var birdies = new SortedList<int, MountResponse>();
            foreach (var mount in mounts)
            {
                if (mount.Value.Name.Contains("Lanner"))
                {
                    birdies.Add(mount.Key, mount.Value);
                }
            }
            return birdies.Values.ToList();
        }

        internal static List<MountResponse> GetDoggos()
        {
            var doggos = new SortedList<int, MountResponse>();
            foreach (var mount in mounts)
            {
                if (mount.Value.Name.Contains("Kamuy") && mount.Key != 181)
                {
                    doggos.Add(mount.Key, mount.Value);
                }
            }
            return doggos.Values.ToList();
        }

        internal static List<MountResponse> GetGwibs()
        {
            var gwibbers = new SortedList<int, MountResponse>();
            foreach (var mount in mounts)
            {
                if (mount.Value.Name.Contains("Gwiber"))
                {
                    gwibbers.Add(mount.Key, mount.Value);
                }
            }
            return gwibbers.Values.ToList();
        }

        internal static List<MountResponse> GetGatos()
        {
            var gatos = new SortedList<int, MountResponse>();
            foreach (var mount in mounts)
            {
                if (mount.Value.Name.Contains("Lynx"))
                {
                    gatos.Add(mount.Key, mount.Value);
                }
            }
            return gatos.Values.ToList();
        }
    }
}

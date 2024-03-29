﻿using Farmingway.RestResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Farmingway.Services;

namespace Farmingway
{
    internal static class MountDatabase
    {
        internal static Dictionary<string, MountResponse> mounts = new();

        internal static async Task Init()
        {
            await Task.Run(() =>
            {
                var data = CollectService.GetMounts();
                foreach (var mount in data)
                {
                    mounts.Add(mount.Name, mount);
                }

                Console.WriteLine($"Successfully recorded {mounts.Count} mounts.");
            });
        }

        internal static List<MountResponse> GetPonies()
        {
            var ponies = new SortedList<int, MountResponse>();
            foreach (var mount in mounts.Values)
            {
                if (IsPony(mount.Id))
                {
                    ponies.Add(mount.Id, mount);
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
            foreach (var mount in mounts.Values)
            {
                if (mount.Name.Contains("Lanner"))
                {
                    birdies.Add(mount.Id, mount);
                }
            }
            return birdies.Values.ToList();
        }

        internal static List<MountResponse> GetDoggos()
        {
            var doggos = new SortedList<int, MountResponse>();
            foreach (var mount in mounts.Values)
            {
                if (mount.Name.Contains("Kamuy") && mount.Id != 181)
                {
                    doggos.Add(mount.Id, mount);
                }
            }
            return doggos.Values.ToList();
        }

        internal static List<MountResponse> GetGwibs()
        {
            var gwibbers = new SortedList<int, MountResponse>();
            foreach (var mount in mounts.Values)
            {
                if (mount.Name.Contains("Gwiber"))
                {
                    gwibbers.Add(mount.Id, mount);
                }
            }
            return gwibbers.Values.ToList();
        }

        internal static List<MountResponse> GetGatos()
        {
            var gatos = new SortedList<int, MountResponse>();
            foreach (var mount in mounts.Values)
            {
                if (mount.Name.Contains("Lynx"))
                {
                    gatos.Add(mount.Id, mount);
                }
            }
            return gatos.Values.ToList();
        }

        internal static List<MountResponse> GetMountsByOrigin(string mountType)
        {
            if (mountType.Equals("trial", StringComparison.InvariantCultureIgnoreCase) 
                || mountType.Equals("raid", StringComparison.InvariantCultureIgnoreCase))
            {
                return mounts
                    .Values
                    .Where(m => 
                        !m.Name.Equals("Rathalos") 
                        && m.Sources[0].Type.Equals(mountType, StringComparison.InvariantCultureIgnoreCase)
                    )
                    .ToList();
            }

            return null;
        }

        internal static List<MountResponse> GetTrialAndRaid()
        {
            return mounts
                .Values
                .Where(m =>
                {
                    var mountType = m.Sources[0].Type;
                    return !m.Name.Equals("Rathalos") 
                           && (mountType.Equals("trial", StringComparison.InvariantCultureIgnoreCase) 
                               || mountType.Equals("raid", StringComparison.InvariantCultureIgnoreCase));
                })
                .ToList();
        }
    }
}

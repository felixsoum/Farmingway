using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;

namespace Farmingway.TypeReaders
{
    public class MountTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var args = input.Split(" ");

            string mountType = null;
            int suggestionPreference = 1;
            var charIds = new HashSet<int>();
            foreach (var arg in args)
            {
                if (arg.Equals(""))
                {
                    return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "No args provided"));
                }
                
                if (int.TryParse(arg, out var i))
                {
                    if (i is > 1 and < 8)
                    {
                        suggestionPreference = i;
                    }
                    else
                    {
                        charIds.Add(i);
                    }
                }
                else
                {
                    if (mountType != null)
                    {
                        return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Specified multiple mount types"));
                    }
                    
                    mountType = arg;
                }
            }

            if (charIds.Count == 0)
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "No character IDs specified"));
            }

            return Task.FromResult(TypeReaderResult.FromSuccess(new MountTypeParams(charIds, mountType, suggestionPreference)));
        }
    }

    public class MountTypeParams
    {
        public HashSet<int> charIds { get; set; }
        public string mountType { get; set; }
        public int suggestionPreference { get; set; }

        /// Select a preset from 1-8:
        /// 1- Order by ascending number of party players who possess a mount, then descending total players
        /// 2- Order by ascending number of total players who possess a mount, then descending party players
        /// 3- Order by ascending number of party players who possess a mount, then ascending total players
        /// 4- Order by ascending number of total players who possess a mount, then ascending party players
        /// 5- Order by descending number of party players who possess a mount, then descending total players
        /// 6- Order by descending number of total players who possess a mount, then descending party players
        /// 7- Order by descending number of party players who possess a mount, then ascending total players
        /// 8- Order by descending number of total players who possess a mount, then ascending party players
        public MountTypeParams(HashSet<int> charIds, string mountType, int suggestionPreference)
        {
            this.charIds = charIds;
            this.mountType = mountType;
            this.suggestionPreference = suggestionPreference - 1; // 0-index this param
        }
    }
}
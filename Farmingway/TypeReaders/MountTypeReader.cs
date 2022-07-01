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
            var charIds = new HashSet<int>();
            foreach (var arg in args)
            {
                if (arg.Equals(""))
                {
                    return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "No args provided"));
                }
                
                if (int.TryParse(arg, out var i))
                {
                    charIds.Add(i);
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

            return Task.FromResult(TypeReaderResult.FromSuccess(new MountTypeParams(charIds, mountType)));
        }
    }

    public class MountTypeParams
    {
        public HashSet<int> charIds { get; set; }
        public string mountType { get; set; }

        public MountTypeParams(HashSet<int> charIds, string mountType)
        {
            this.charIds = charIds;
            this.mountType = mountType;
        }
    }
}
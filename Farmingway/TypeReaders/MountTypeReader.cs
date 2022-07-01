using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace Farmingway.TypeReaders
{
    public class MountTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var args = input.Split(" ");

            var possibleMountType = args.First();
            if (possibleMountType.Equals(""))
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "No args provided"));
            }
            
            string mountType = null;
            List<string> charIdStrings;
            if (int.TryParse(possibleMountType, out _))
            {
                // Mount type not specified
                charIdStrings = args.ToList();
            }
            else
            {
                mountType = possibleMountType;
                charIdStrings = args.Skip(1).ToList();
            }

            List<int> charIds = new List<int>();
            foreach (var idString in charIdStrings)
            {
                if (!int.TryParse(idString, out var id))
                {
                    return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Non-numeric Lodestone ID detected"));
                }
                
                charIds.Add(id);
            }

            return Task.FromResult(TypeReaderResult.FromSuccess(new MountTypeParams(charIds, mountType)));
        }
    }

    public class MountTypeParams
    {
        public List<int> charIds { get; set; }
        public string mountType { get; set; }

        public MountTypeParams(List<int> charIds, string mountType)
        {
            this.charIds = charIds;
            this.mountType = mountType;
        }
    }
}
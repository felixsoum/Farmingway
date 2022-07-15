using System;
using System.Collections.Generic;
using System.Linq;
using Discord;

namespace Farmingway.Modules.Mounts;

public class StoredSuggestion
{
    const char BackButtonKeycode = 'b';
    const char NextButtonKeycode = 'n';
    const int ResultsPerPage = 5;
    private static readonly Emoji BackEmoji = new("\u2B05");
    private static readonly Emoji NextEmoji = new("\u27A1");

    public static bool IsNextButtonKeycode(char code) => code == NextButtonKeycode;
    public static string MakeBackButtonKeycode(ulong key) => BackButtonKeycode.ToString() + key;
    public static string MakeNextButtonKeycode(ulong key) => NextButtonKeycode.ToString() + key;
    
    public IEnumerable<string> names;
    public List<MountCount> storedMountCounts;
    public int pageIndex;
    public readonly int pageCount;
    public StoredSuggestion(IEnumerable<string> names, IEnumerable<MountCount> storedMountCounts)
    {
        this.names = names;
        this.storedMountCounts = storedMountCounts.ToList();
        this.pageIndex = 0;
        this.pageCount = (this.storedMountCounts.Count - 1) / ResultsPerPage + 1;
    }

    public Tuple<Embed, MessageComponent> BuildFirstPage(ulong key)
    {
        pageIndex = 0;
        return BuildPageEmbed(storedMountCounts.Take(5).ToList(), key);
    }

    public Tuple<Embed, MessageComponent> BuildPage(bool isNext, ulong key)
    {
        if (isNext)
        {
            pageIndex++;
            
        }
        else
        {
            pageIndex--;
        }
        var mounts = storedMountCounts.Skip(pageIndex * ResultsPerPage).Take(5).ToList();
        return BuildPageEmbed(mounts, key);
    }

    private Tuple<Embed, MessageComponent> BuildPageEmbed(List<MountCount> mounts, ulong key)
    {
        string pageInfo = $"(page {pageIndex + 1}/{pageCount})";

        var eb = new EmbedBuilder();
        eb.WithTitle($"Farmingway's suggestion for {string.Join(", ", names)}. {pageInfo}")
            .WithColor(new Color(0, 255, 0));

        if (mounts.Count > 0)
        {
            foreach (var i in mounts)
            {
                var mount = i.mount;
                var info = $"{mount.Sources[0].Text} -- obtained by " +
                           (i.count > 0 ? $"{i.count} in group, " : "") + $"{mount.Owned} overall";
                eb.AddField(mount.Name, info);
            }
        }
        else
        {
            eb.WithDescription("Could not find a suitable farm suggestion");
        }

        var embed = eb.Build();
        var components = new ComponentBuilder()
            .WithButton("Back", MakeBackButtonKeycode(key), disabled: pageIndex <= 0, emote: BackEmoji)
            .WithButton("Next", MakeNextButtonKeycode(key), disabled: (pageIndex + 1) >= pageCount, emote: NextEmoji)
            .Build();

        return new Tuple<Embed, MessageComponent>(embed, components);
    }
}

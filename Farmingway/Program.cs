using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Farmingway
{
    internal class Program
    {
        public static Task Main(string[] args) => new Program().MainAsync();
        public static bool isExiting;

        DiscordSocketClient client;
        CommandService commandService;
        CommandHandler commandHandler;

        public async Task MainAsync()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers
            });
            commandService = new CommandService();
            commandHandler = new CommandHandler(client, commandService);

            client.Log += Log;
            client.ButtonExecuted += HandleButtonExecuted;
            commandService.Log += Log;

            //  You can assign your bot token to a string, and pass that in to connect.
            //  This is, however, insecure, particularly if you plan to have your code hosted in a public repository.
            var token = Secret.DiscordToken;

            // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
            // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
            // var token = File.ReadAllText("token.txt");
            // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

            await MountDatabase.Init();

            await commandHandler.InstallCommandsAsync();
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            // Block this task until the exit command is used.
            await Task.Run(() =>
            {
                while (!isExiting) { }
            });
        }

        private async Task HandleButtonExecuted(SocketMessageComponent component)
        {
            char code = component.Data.CustomId[0];
            ulong key = ulong.Parse(component.Data.CustomId.Substring(1));

            bool isNext = Modules.MountsModule.IsNextButtonKeycode(code);
            var result = Modules.MountsModule.SuggestMore(key, isNext);

            if (result != null && result.Item1 != null)
            {
                await component.UpdateAsync(x =>
                {
                    x.Embed = result.Item1;
                    bool hasBack = result.Item2.Item1;
                    bool hasNext = result.Item2.Item2;

                    x.Components = new ComponentBuilder()
                    .WithButton("Back", Modules.MountsModule.MakeBackButtonKeycode(key), disabled: !hasBack, emote: new Emoji("\u2B05"))
                    .WithButton("Next", Modules.MountsModule.MakeNextButtonKeycode(key), disabled: !hasNext, emote: new Emoji("\u27A1"))
                    .Build();
                });
            }
            else
            {
                await component.RespondAsync($"{component.User.Mention}! There are no more suggestions...");
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine("Log: " + msg);
            return Task.CompletedTask;
        }

        internal static void Exit() => isExiting = true;
    }
}

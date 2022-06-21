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

        private Task Log(LogMessage msg)
        {
            Console.WriteLine("Log: " + msg);
            return Task.CompletedTask;
        }

        internal static void Exit() => isExiting = true;
    }
}

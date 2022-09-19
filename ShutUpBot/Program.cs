using Discord;
using Discord.WebSocket;

namespace ShutUpBot
{
    public class Program
    {
        private DiscordSocketClient _client;
        private Dictionary<String, Dictionary<ulong, int>> _guilds = new Dictionary<String, Dictionary<ulong, int>>();
        private int _lastHour = DateTime.Now.Hour;

        public static Task Main(string[] args) => new Program().MainAsync();
        public async Task MainAsync()
        {
            // Initialize socket client
            _client = new DiscordSocketClient();
            
            // Register events
            _client.MessageReceived += MessageHandler;
            _client.Log += Log;

            // Get bot configuration
            var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
            
            // Login and start bot
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
        
        // Log messages to console
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
        
        // Handle received message
        private Task MessageHandler(SocketMessage message)
        {
            // Ignore messages from bots
            if(message.Author.IsBot)
                return Task.CompletedTask;
            
            // Reset bot data every hour
            if(_lastHour != DateTime.Now.Hour)
            {
                _guilds.Clear();
                _lastHour = DateTime.Now.Hour;
            }
            
            // Get guild data
            Dictionary<ulong, int> users;
            if(_guilds.ContainsKey((message.Channel as SocketGuildChannel).Name)){
                users = _guilds[(message.Channel as SocketGuildChannel).Name];
            }
            // Create guild data if it doesn't exist
            else
            {
                users = new Dictionary<ulong, int>();
                _guilds.Add((message.Channel as SocketGuildChannel).Name, users);
            }

            // Count messages per user
            if(users.ContainsKey(message.Author.Id))
            {
                users[message.Author.Id]++;
                
                // Send message if user has sent 50 messages
                if(users[message.Author.Id] == 50)
                {
                    message.Channel.SendMessageAsync("You talk too much, " + message.Author.Mention + ". Shut up.");
                }
                
                // React to message if user has send more than 50 messages
                if(users[message.Author.Id] > 50)
                {
                    SocketGuild guild = ((SocketGuildChannel)message.Channel).Guild;
                    IEmote emote = guild.Emotes.First(e => e.Name == Environment.GetEnvironmentVariable("EMOTE_NAME"));
                    message.AddReactionAsync(emote);
                }
            }
            // Add user if they don't exist in dictionary
            else
            {
                users.Add(message.Author.Id, 1);
            }


            return Task.CompletedTask;
        }
    }
}
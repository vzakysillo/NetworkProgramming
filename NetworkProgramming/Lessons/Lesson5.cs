using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetworkProgramming.Lessons
{
    public class Message
    {
        public string MessageHeader { get; set; } = "";

        public string m { get; set; } = "";

        public DateTime DateStamp { get; set; }
    }

    public class ChatService
    {
        private readonly HttpClient _client;
        private readonly Dictionary<string, string> _users;
        private readonly string _clientId;
        private readonly string _password;

        public ChatService(string clientId, string password, Dictionary<string, string> users)
        {
            _client = new HttpClient { BaseAddress = new Uri("https://sabatex.francecentral.cloudapp.azure.com") };
            _clientId = clientId;
            _password = password;
            _users = users;
        }

        public async Task<bool> AuthenticateAsync()
        {
            var loginData = new { clientId = new Guid(_clientId), password = _password };
            var response = await _client.PostAsJsonAsync("api/v1/login", loginData);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Authentication failed: {response.StatusCode}");
                return false;
            }

            var content = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<Token>(content);

            if (token == null)
            {
                Console.WriteLine("Failed to parse authentication token.");
                return false;
            }

            _client.DefaultRequestHeaders.Add("clientId", _clientId);
            _client.DefaultRequestHeaders.Add("apiToken", token.AccessToken);

            return true;
        }

        public async Task SendMessageAsync(string username, string message)
        {
            if (!_users.TryGetValue(username, out var destinationId))
            {
                Console.WriteLine($"User {username} not found.");
                return;
            }

            _client.DefaultRequestHeaders.Remove("destinationId");
            _client.DefaultRequestHeaders.Add("destinationId", destinationId);

            Message messageObject = new Message
            {
                MessageHeader = "Personal Message",
                m = message,
                DateStamp = DateTime.Now
            };

            var response = await _client.PostAsJsonAsync("api/v1/objects", messageObject);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Message to {username} sent successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to send message to {username}: {response.StatusCode}");
            }
        }

        public async Task BroadcastMessageAsync(string message)
        {
            foreach (var user in _users)
            {
                await SendMessageAsync(user.Key, message);
            }
        }

        public async Task ReadMessagesAsync()
        {
            foreach (var user in _users)
            {
                _client.DefaultRequestHeaders.Remove("destinationId");
                _client.DefaultRequestHeaders.Add("destinationId", user.Value);

                var response = await _client.GetAsync("api/v1/objects?take=100");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to fetch messages for {user.Key}: {response.StatusCode}");
                    continue;
                }

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var messages = JsonSerializer.Deserialize<List<Message>>(content, options);

                if (messages == null || messages.Count == 0)
                {
                    Console.WriteLine($"No messages received from {user.Key}.");
                    continue;
                }

                Console.WriteLine($"Received messages from {user.Key}:");
                foreach (Message message in messages)
                {
                    Console.WriteLine($"[{message.DateStamp}] {message.MessageHeader}: {message.m}");
                }
            }
        }
    }

    static public class Lesson5
    {
        public static Dictionary<string, string> users = new Dictionary<string, string>
         {
             { "serhiy.lakas", "2a2c2a72-0ecb-4369-8e1e-c84be1f201ee" },
             { "Kostyikevych.Vitaliy", "76efc553-f68d-4aa9-b1af-c25a6dfb0390" },
             { "Shalovilo.Bogdan", "1134a432-9005-4232-b0e9-8ead9cf63c0b" },
             { "Nikolaichyk.Vlad", "fdb887f6-61ee-48b9-992f-6fff53207a3c" },
             { "Andryii.Rabosh", "7e23a154-09d2-4ab8-b23a-4ca24181e1ef" },
             { "Iskachov.Bohdan", "b3f1b17d-8caa-4c74-87ab-b53c67788e98" },
             { "Mikhnevych.Danylo", "c8a41470-25d3-4f2e-9dc6-1cb9955587d1" },
             { "Zakusilo.Vitaliy", "b862de90-f671-4f96-a8aa-154841d18b95" }, //XCuiemn762
             { "Bobrovnitskiy.Matviy", "b934d65c-83e7-4f74-834e-94fc12004ad7" }
         };

        public static async Task Run()
        {
            Console.Write("Enter login: ");
            string login = users[Console.ReadLine() ?? ""];

            Console.Write("Enter password: ");
            string password = Console.ReadLine() ?? "";

            var chatService = new ChatService(login, password, users);

            if (!await chatService.AuthenticateAsync())
            {
                Console.WriteLine("Failed to authenticate.");
                return;
            }

            Console.WriteLine("Welcome to the chat! Type 'help' to see available commands.");

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                var command = parts[0].ToLower();

                switch (command)
                {
                    case "help":
                        Console.WriteLine("Available commands:");
                        Console.WriteLine("\tsend [username] [message] - Send a personal message.");
                        Console.WriteLine("\tbroadcast [message] - Send a message to all users.");
                        Console.WriteLine("\tread - Read all received messages.");
                        Console.WriteLine("\tclear - clear the sreen.");
                        Console.WriteLine("\texit - Exit the chat.");
                        break;

                    case "send":
                        if (parts.Length < 2)
                        {
                            Console.WriteLine("Usage: send [username] [message]");
                            break;
                        }

                        var messageParts = parts[1].Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                        if (messageParts.Length < 2)
                        {
                            Console.WriteLine("Usage: send [username] [message]");
                            break;
                        }

                        var username = messageParts[0];
                        var message = messageParts[1];

                        await chatService.SendMessageAsync(username, message);
                        break;

                    case "broadcast":
                        if (parts.Length < 2)
                        {
                            Console.WriteLine("Usage: broadcast [message]");
                            break;
                        }

                        await chatService.BroadcastMessageAsync(parts[1]);
                        break;

                    case "read":
                        await chatService.ReadMessagesAsync();
                        break;

                    case "clear":
                        Console.Clear();
                        break;

                    case "exit":
                        Console.WriteLine("Goodbye!");
                        return;

                    default:
                        Console.WriteLine("Unknown command. Type 'help' for a list of commands.");
                        break;
                }
            }
        }
    }
}

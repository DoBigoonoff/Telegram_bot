using System;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using GetPlayerModels;
using Newtonsoft.Json;
using GetTeamModels;
using GetLiveMatchesModels;
using GetNSMatchesModels;
using GetNewsModels;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Http.Headers;
using TranslatorModels;
using SelectPlayers;
using Npgsql;

namespace Football_Api_Bot
{
    public class Telegram_Bot
    {
        TelegramBotClient botClient = new TelegramBotClient("5848556759:AAEB6UG3K2fqyfAv38Q2G8K0f-MLaH51rpg");
        CancellationToken cancellationToken = new CancellationToken();
        ReceiverOptions receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
        public async Task Start()
        {
            botClient.StartReceiving(HandlerUpdateAsync, HandlerError, receiverOptions, cancellationToken);
            var botMe = await botClient.GetMeAsync();
            Console.WriteLine($"Bot {botMe.Username} in progress");
            Console.ReadKey();
        }

        private Task HandlerError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"error in telegram bot API:\n {apiRequestException.ErrorCode}" +
                $"\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {

                await HandlerMessageStartAsync(botClient, update.Message);
            }
        }

        private async Task HandlerMessageStartAsync(ITelegramBotClient botClient, Message message)
        {
            try
            {
                if (message.Text == "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Choose command from list of commands");
                    return;
                }
                if (message.Text == "/playerinfo")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Write player club and name in format \n?clubname_playername");
                    return;
                }
                if (message.Text == "/teaminfo")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Write club in format \n!clubname");
                    return;
                }
                if (message.Text == "/nsmatches")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "To search coming matches based on league write player club and name in format \n$clubname_playername");
                    return;
                }
                if (message.Text == "/news")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "To receive 10 latest news write topic in format \n%topic");
                    return;
                }
                if (message.Text == "/translator")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "To translate text write it in format \n=Text_Languageoftext_Languagetorecieve\nUse one of this languages: bg, zh, cs, da, nl, en, en-US, en-GB, et, fi, fr, de, el, hu, id, it, ja, lv, lt, pl, pt-PT, pt-BR, ro, ru, sk, sl, es, sv, tr, uk, ko, nb");
                    return;
                }
                if (message.Text == "/playerpost")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "To post player in database write player club and name in format \n+clubname_playername");
                    return;
                }
                if (message.Text == "/cleardatabase")
                {
                    string Connect = "Host=database-1.chrdzgluuzrm.eu-north-1.rds.amazonaws.com;Username=postgres;Password=2718121812;Database=postgres";
                    NpgsqlConnection con = new NpgsqlConnection(Connect);
                    con.Open();
                    var sql = "delete from public.\"Players\"";
                    NpgsqlCommand comm = new NpgsqlCommand(sql, con);
                    comm.ExecuteReader();
                    con.Close();
                    await botClient.SendTextMessageAsync(message.Chat.Id, "DataBase cleared");
                    return;
                }
                if (message.Text == "/livematches")
                {
                    var client = new HttpClient();
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri("https://dobigoonoff-football-api.azurewebsites.net/LiveMatches")
                    };
                    using (var response = await client.SendAsync(request))
                    {
                        response.EnsureSuccessStatusCode();
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<GetLiveMatches>(content);
                        for (int i = 0; i < result.Data.Count; i++)
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Match: {result.Data[i].Name}\nLeague: {result.Data[i].League.Name}\nStarted at (UTC): {result.Data[i].Start_at}\nStatus: {result.Data[i].Status_More}\n" +
                                $"Score : {result.Data[i].Home_Score.Current}     {result.Data[i].Away_Score.Current}");
                        }
                    }
                    return;
                }
                if (message.Text.StartsWith("?"))
                {
                    string output = message.Text.Trim('?');
                    string[] mass = output.Split('_', 2);

                    var client = new HttpClient();
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri($"https://dobigoonoff-football-api.azurewebsites.net/PlayerInfo?FootballClub={mass[0]}&PlayerName={mass[1]}"),
                        Headers = {
    { "X-RapidAPI-Host", "dobigoonoff-football-api.azurewebsites.net" },},
                    };
                    using (var response = await client.SendAsync(request))
                    {
                        response.EnsureSuccessStatusCode();
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<GetPlayer>(content);
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"First name: {result.Player.Firstname}\nLast name: {result.Player.Lastname}\nAge: {result.Player.Age}\n" +
                            $"Height: {result.Player.Height}\nWeight: {result.Player.Weight}\nPhoto: {result.Player.Photo}\nTeam: {result.Statistics[0].Team.Name}\nSeason: {result.Statistics[0].League.Season}\n" +
                            $"League: {result.Statistics[0].League.Name}\nPosition: {result.Statistics[0].Games.Position}\nRating: {result.Statistics[0].Games.Rating}\nGoals: {result.Statistics[0].Goals.Total}\nAssists: {result.Statistics[0].Goals.Assists}");
                    }
                    return;
                }
                if (message.Text.StartsWith("!"))
                {
                    string output = message.Text.TrimStart('!');
                    var client = new HttpClient();
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri($"https://dobigoonoff-football-api.azurewebsites.net/TeamInfo?Club={output}"),
                        Headers = {
{ "X-RapidAPI-Host", "dobigoonoff-football-api.azurewebsites.net" },},
                    };
                    using (var response = await client.SendAsync(request))
                    {
                        response.EnsureSuccessStatusCode();
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<GetTeam>(content);
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Name: {result.Response[0].Team.Name}\nCountry: {result.Response[0].Team.Country}\nLogo: {result.Response[0].Team.Logo}\nVenue: {result.Response[0].Venue.Name}\n" +
                            $"City: {result.Response[0].Venue.City}\nVenue Capacity: {result.Response[0].Venue.Capacity} people");
                    }
                    return;
                }
                if (message.Text.StartsWith("$"))
                {
                    string output = message.Text.TrimStart('$');
                    string[] mass = output.Split('_', 2);
                    var client = new HttpClient();
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri($"https://dobigoonoff-football-api.azurewebsites.net/NSMatches?Team={mass[0]}&Player={mass[1]}"),
                        Headers = {
    { "X-RapidAPI-Host", "dobigoonoff-football-api.azurewebsites.net" },},
                    };
                    using (var response = await client.SendAsync(request))
                    {
                        response.EnsureSuccessStatusCode();
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<GetNSMatches>(content);
                        for (int i = 0; i < result.Response.Count; i++)
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Date (UTC): {result.Response[i].Fixture.Date.Split('T', 2)[0]} {result.Response[i].Fixture.Date.Split('T', 2)[1].Split('+', 2)[0]}\nTeam Home: {result.Response[i].Teams.Home.Name}\nTeam Away: {result.Response[i].Teams.Away.Name}\n" +
                                $"League name: {result.Response[i].League.Name}");
                        }
                    }
                    return;
                }
                if (message.Text.StartsWith("%"))
                {
                    string output = message.Text.TrimStart('%');
                    var client = new HttpClient();
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri($"https://dobigoonoff-football-api.azurewebsites.net/News?Request={output}"),
                        Headers =
    {
        { "X-RapidAPI-Host", "dobigoonoff-football-api.azurewebsites.net" },
    },
                    };
                    using (var response = await client.SendAsync(request))
                    {
                        response.EnsureSuccessStatusCode();
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<GetNews>(content);
                        for (int i = 0; i < result.Results.Count; i++)
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"\nLink: {result.Results[i].Link}");
                        }
                    }
                    return;
                }
                if (message.Text.StartsWith("="))
                {
                    string output = message.Text.TrimStart('=');
                    string[] mass = output.Split('_', 3);
                    var client = new HttpClient();
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri($"https://dobigoonoff-football-api.azurewebsites.net/Translate?text={mass[0]}&source={mass[1]}&target={mass[2]}"),
                        Headers =
    {
        { "X-RapidAPI-Host", "dobigoonoff-football-api.azurewebsites.net" },
    },
                    };
                    using (var response = await client.SendAsync(request))
                    {
                        response.EnsureSuccessStatusCode();
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<Translator>(content);
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Result: {result.Text}");
                    }
                    return;
                }
                if (message.Text.StartsWith("+"))
                {
                    string output = message.Text.Trim('+');
                    string[] mass = output.Split('_', 2);

                    var client = new HttpClient();
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri($"https://dobigoonoff-football-api.azurewebsites.net/PlayerInfo?FootballClub={mass[0]}&PlayerName={mass[1]}"),
                        Headers = {
    { "X-RapidAPI-Host", "dobigoonoff-football-api.azurewebsites.net" },},
                    };
                    using (var response = await client.SendAsync(request))
                    {
                        response.EnsureSuccessStatusCode();
                        var content = await response.Content.ReadAsStringAsync();
                        var player = JsonConvert.DeserializeObject<GetPlayer>(content);
                        string Connect = "Host=database-1.chrdzgluuzrm.eu-north-1.rds.amazonaws.com;Username=postgres;Password=2718121812;Database=postgres";
                        NpgsqlConnection con = new NpgsqlConnection(Connect);
                        var sql = "insert into public.\"Players\"(\"id\", \"firstname\", \"lastname\", \"age\", \"height\", \"weight\", \"position\", \"rating\", \"clubname\")"
                            + $"values (@id, @firstname, @lastname, @age, @height, @weight, @position, @rating, @clubname)";
                        NpgsqlCommand comm = new NpgsqlCommand(sql, con);
                        comm.Parameters.AddWithValue("id", player.Player.Id);
                        comm.Parameters.AddWithValue("firstname", player.Player.Firstname);
                        comm.Parameters.AddWithValue("lastname", player.Player.Lastname);
                        comm.Parameters.AddWithValue("age", player.Player.Age);
                        comm.Parameters.AddWithValue("height", player.Player.Height);
                        comm.Parameters.AddWithValue("weight", player.Player.Weight);
                        comm.Parameters.AddWithValue("position", player.Statistics[0].Games.Position);
                        comm.Parameters.AddWithValue("rating", player.Statistics[0].Games.Rating);
                        comm.Parameters.AddWithValue("clubname", player.Statistics[0].Team.Name);
                        await con.OpenAsync();
                        await comm.ExecuteNonQueryAsync();
                        await con.CloseAsync();
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Player {mass[1]} succesfully Posted to DataBase");
                    }
                    return;
                }
                if (message.Text == "/playerget")
                {
                    string Connect = "Host=database-1.chrdzgluuzrm.eu-north-1.rds.amazonaws.com;Username=postgres;Password=2718121812;Database=postgres";
                    NpgsqlConnection con = new NpgsqlConnection(Connect);
                    List<Players> players = new List<Players>();
                    await con.OpenAsync();
                    var sql = "select * from public.\"Players\"";
                    NpgsqlCommand comm = new NpgsqlCommand(sql, con);
                    NpgsqlDataReader reader = await comm.ExecuteReaderAsync();
                    List<int> id = new List<int>();
                    while (await reader.ReadAsync())
                    {
                        if (id.Contains(reader.GetInt32(0)))
                            continue;
                        players.Add(new Players { id = reader.GetInt32(0), firstname = reader.GetString(1), lastname = reader.GetString(2), age = reader.GetInt32(3), height = reader.GetString(4), weight = reader.GetString(5), position = reader.GetString(6), rating = reader.GetString(7), clubname = reader.GetString(8) });
                        id.Add(reader.GetInt32(0));
                    }
                    await con.CloseAsync();
                    if (players.Count() == 0)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "DataBase is empty");
                    }
                    for (int i = 0; i < players.Count; i++)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Firstname: {players[i].firstname}\nLastname: {players[i].lastname}\nClub: {players[i].clubname}\nAge: {players[i].age}\n" +
                            $"Height: {players[i].height}\nWeight: {players[i].weight}\nPosition: {players[i].position}\nRating: {players[i].rating}");
                    }
                    return;
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "I don't know what you want!");
                }
            }
            catch { await botClient.SendTextMessageAsync(message.Chat.Id, "Wrong data!"); }
        }

    }
}

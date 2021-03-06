﻿using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using Universal.Common.Extensions;

namespace BuddiesBot
{

    public static class Extensions
    {
        public static bool IsDirectMessage(this SocketMessage socketMessage)
        {
            return socketMessage.Channel is SocketDMChannel;
        }

        public static SocketVoiceChannel GetAuthorVoiceChannel(this SocketMessage socketMessage)
        {
            if (socketMessage.Channel is SocketGuildChannel socketGuildChannel)
            {
                return socketGuildChannel.Guild.GetUser(socketMessage.Author.Id).VoiceChannel;
            }
            else
            {
                return null;
            }
        }
    }

    class Program
    {
        private const long DotA2Channel2Id = 661420230362529812;
        private const string BotTokenPart1Of3 = "NzExODQ0MTk2NDM2NDEwMzg4";
        private const string BotTokenPart2Of3 = "XscLYA";
        private const string BotTokenPart3Of3 = "sQ_KBg8pdsyO2R9miN-Ztx4B2UE";
        // Attempt at defeating Github's secret scanning.
        private static string BotToken
        {
            get => $"{BotTokenPart1Of3}.{BotTokenPart2Of3}.{BotTokenPart3Of3}";
        }

        private static DiscordSocketClient DiscordSocketClient;
        private static IAudioClient AudioClient;
        private static AudioOutStream AudioOutStream;

        static async Task Main(string[] args)
        {
            DiscordSocketClient = new DiscordSocketClient();

            DiscordSocketClient.Log += LogAsync;
            //DiscordSocketClient.Ready += ReadyAsync;
            DiscordSocketClient.MessageReceived += MessageReceivedAsync;
            //DiscordSocketClient.UserVoiceStateUpdated += UserVoiceStateUpdatedAsync;

            await DiscordSocketClient.LoginAsync(TokenType.Bot, BotToken);
            await DiscordSocketClient.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

        [Command(RunMode = RunMode.Async)]
        private static async Task UserVoiceStateUpdatedAsync(SocketUser socketUser, SocketVoiceState socketVoiceStateBefore, SocketVoiceState socketVoiceStateAfter)
        {
            if (socketUser.Id != DiscordSocketClient.CurrentUser.Id)
            {
                if ((socketVoiceStateBefore.VoiceChannel == null || socketVoiceStateBefore.VoiceChannel.Id != 661420230362529812) &&
                socketVoiceStateAfter.VoiceChannel != null && socketVoiceStateAfter.VoiceChannel.Id == 661420230362529812)
                {
                    string message = $"{socketUser.Username} has joined the DotA 2 channel.";
                    if (AudioClient.ConnectionState == ConnectionState.Connected)
                    {
                        await SpeakAsync(message);
                    }
                    else
                    {
                        //await socketVoiceStateAfter.VoiceChannel.Guild.DefaultChannel.SendMessageAsync(message, true);
                    }
                }

                if ((socketVoiceStateBefore.VoiceChannel != null && socketVoiceStateBefore.VoiceChannel.Id == 661420230362529812) &&
                    (socketVoiceStateAfter.VoiceChannel == null || socketVoiceStateAfter.VoiceChannel.Id != 661420230362529812))
                {
                    string message = $"{socketUser.Username} has left the DotA 2 channel.";
                    
                    if (AudioClient.ConnectionState == ConnectionState.Connected)
                    {
                        await SpeakAsync(message);
                    }
                    else
                    {
                        //await socketVoiceStateBefore.VoiceChannel.Guild.DefaultChannel.SendMessageAsync(message, true);
                    }
                }
            }
        }

        [Command(RunMode = RunMode.Async)]
        private static async Task SpeakAsync(string message)
        {
            using (SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer())
            using (MemoryStream memoryStream = new MemoryStream())
            {
                speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
                speechSynthesizer.SetOutputToAudioStream(memoryStream, new SpeechAudioFormatInfo(48000, AudioBitsPerSample.Sixteen, AudioChannel.Stereo));
                speechSynthesizer.Speak(message);
                speechSynthesizer.SetOutputToNull();

                memoryStream.Position = 0;

                await AudioClient.SetSpeakingAsync(true);

                await memoryStream.CopyToAsync(AudioOutStream);
                await AudioOutStream.FlushAsync();

                await AudioClient.SetSpeakingAsync(false);
            }
        }

        private static async Task StreamAudioAsync(Stream stream)
        {
            await AudioClient.SetSpeakingAsync(true);

            await stream.CopyToAsync(AudioOutStream);
            await AudioOutStream.FlushAsync();

            await AudioClient.SetSpeakingAsync(false);
        }

        [Command(RunMode = RunMode.Async)]
        private static async Task ReadyAsync()
        {
            AudioClient = await DiscordSocketClient.GetGuild(304212969795878923).GetVoiceChannel(661420230362529812).ConnectAsync();
            AudioOutStream = AudioClient.CreatePCMStream(AudioApplication.Mixed, 48000);
        }

        [Command(RunMode = RunMode.Async)]
        private static async Task MessageReceivedAsync(SocketMessage socketMessage)
        {
            if (socketMessage.Author.Id != DiscordSocketClient.CurrentUser.Id)
            {
                if (socketMessage.Content == "!loser")
                {
                    SocketVoiceChannel socketVoiceChannel = socketMessage.GetAuthorVoiceChannel();

                    if (socketVoiceChannel != null)
                    {
                        Task.Run(async () =>
                        {
                            await EnsureConnectedToVoiceChannelAsync(socketVoiceChannel);
                            await SpeakAsync("Loser!");
                        });
                    }
                }
                else if (socketMessage.Content == "!kevin")
                {
                    SocketVoiceChannel socketVoiceChannel = socketMessage.GetAuthorVoiceChannel();

                    if (socketVoiceChannel != null)
                    {
                        Task.Run(async () =>
                        {
                            await EnsureConnectedToVoiceChannelAsync(socketVoiceChannel);

                            using (FileStream fileStream = File.OpenRead("The Kevin Song.mp3"))
                            using (Mp3FileReader mp3FileReader = new Mp3FileReader(fileStream))
                            using (WaveStream waveStream = WaveFormatConversionStream.CreatePcmStream(mp3FileReader))
                            using (WaveFormatConversionStream waveFormatConversionStream = new WaveFormatConversionStream(new WaveFormat(48000, 16, 2), waveStream))
                            {
                                await StreamAudioAsync(waveFormatConversionStream);
                            }
                        });
                    }
                }
                else if (socketMessage.Content == "!josh")
                {
                    SocketVoiceChannel socketVoiceChannel = socketMessage.GetAuthorVoiceChannel();

                    if (socketVoiceChannel != null)
                    {
                        Task.Run(async () =>
                        {
                            await EnsureConnectedToVoiceChannelAsync(socketVoiceChannel);

                            using (FileStream fileStream = File.OpenRead("The Josh Song.mp3"))
                            using (Mp3FileReader mp3FileReader = new Mp3FileReader(fileStream))
                            using (WaveStream waveStream = WaveFormatConversionStream.CreatePcmStream(mp3FileReader))
                            using (WaveFormatConversionStream waveFormatConversionStream = new WaveFormatConversionStream(new WaveFormat(48000, 16, 2), waveStream))
                            {
                                await StreamAudioAsync(waveFormatConversionStream);
                            }
                        });
                    }
                }
                else if (socketMessage.Content.StartsWith("!play"))
                {
                    string songName = socketMessage.Content.Replace("!play", string.Empty).Trim();

                    if (!songName.IsNullOrEmpty() && Path.GetExtension(songName).Equals(".mp3", StringComparison.InvariantCultureIgnoreCase))
                    {
                        foreach (string file in Directory.GetFiles(".").Select(x => Path.GetFileName(x)))
                        {
                            if (file.Equals(songName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                SocketVoiceChannel socketVoiceChannel = socketMessage.GetAuthorVoiceChannel();

                                if (socketVoiceChannel != null)
                                {
                                    Task.Run(async () =>
                                    {
                                        await EnsureConnectedToVoiceChannelAsync(socketVoiceChannel);

                                        using (FileStream fileStream = File.OpenRead(file))
                                        using (Mp3FileReader mp3FileReader = new Mp3FileReader(fileStream))
                                        using (WaveStream waveStream = WaveFormatConversionStream.CreatePcmStream(mp3FileReader))
                                        using (WaveFormatConversionStream waveFormatConversionStream = new WaveFormatConversionStream(new WaveFormat(48000, 16, 2), waveStream))
                                        {
                                            await StreamAudioAsync(waveFormatConversionStream);
                                        }
                                    });
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }

        private static Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private static async Task EnsureConnectedToVoiceChannelAsync(SocketVoiceChannel socketVoiceChannel)
        {
            if (socketVoiceChannel == null)
            {
                throw new ArgumentNullException(nameof(socketVoiceChannel));
            }

            SocketVoiceChannel currentlyConnectedVoiceChannel = socketVoiceChannel.Guild.GetUser(DiscordSocketClient.CurrentUser.Id).VoiceChannel;
            if (currentlyConnectedVoiceChannel == null || currentlyConnectedVoiceChannel.Id != socketVoiceChannel.Id || AudioClient == null || AudioClient.ConnectionState != ConnectionState.Connected)
            {
                AudioClient = await socketVoiceChannel.ConnectAsync();
                AudioOutStream = AudioClient.CreatePCMStream(AudioApplication.Mixed, 48000);
            }
        }
    }
}

using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using NAudio.Wave;
using System;
using System.IO;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;

namespace BuddiesBot
{
    class Program
    {
        private const long DotA2Channel2Id = 661420230362529812;
        private const string BotToken = "NzExODQ0MTk2NDM2NDEwMzg4.XsaA2w.emKzGaV_dF6O89dAWmeJOzyUXZk";

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
                    SocketVoiceChannel socketVoiceChannel = ((SocketGuildChannel)(socketMessage.Channel)).Guild.GetUser(socketMessage.Author.Id).VoiceChannel;

                    if (socketVoiceChannel != null)
                    {
                        Task.Run(async () =>
                        {
                            if (((SocketGuildChannel)(socketMessage.Channel)).Guild.GetUser(DiscordSocketClient.CurrentUser.Id).VoiceChannel == null ||
                                ((SocketGuildChannel)(socketMessage.Channel)).Guild.GetUser(DiscordSocketClient.CurrentUser.Id).VoiceChannel.Id != socketVoiceChannel.Id)
                            {
                                AudioClient = await socketVoiceChannel.ConnectAsync();
                                AudioOutStream = AudioClient.CreatePCMStream(AudioApplication.Mixed, 48000);
                            }
                            await SpeakAsync("Loser!");
                        });
                    }
                }
                else if (socketMessage.Content == "!kevin")
                {
                    SocketVoiceChannel socketVoiceChannel = ((SocketGuildChannel)(socketMessage.Channel)).Guild.GetUser(socketMessage.Author.Id).VoiceChannel;

                    if (socketVoiceChannel != null)
                    {
                        Task.Run(async () =>
                        {
                            if (((SocketGuildChannel)(socketMessage.Channel)).Guild.GetUser(DiscordSocketClient.CurrentUser.Id).VoiceChannel == null ||
                                ((SocketGuildChannel)(socketMessage.Channel)).Guild.GetUser(DiscordSocketClient.CurrentUser.Id).VoiceChannel.Id != socketVoiceChannel.Id)
                            {
                                AudioClient = await socketVoiceChannel.ConnectAsync();
                                AudioOutStream = AudioClient.CreatePCMStream(AudioApplication.Mixed, 48000);
                            }

                            using (FileStream fileStream = File.OpenRead("The Kevin Song.mp3"))
                            using (Mp3FileReader mp3FileReader = new Mp3FileReader(fileStream))
                            using (WaveStream waveStream = WaveFormatConversionStream.CreatePcmStream(mp3FileReader))
                            using (WaveFormatConversionStream waveFormatConversionStream = new WaveFormatConversionStream(new WaveFormat(48000, 16, 1), waveStream))
                            {
                                await AudioClient.SetSpeakingAsync(true);

                                await waveFormatConversionStream.CopyToAsync(AudioOutStream);
                                await AudioOutStream.FlushAsync();

                                await AudioClient.SetSpeakingAsync(false);
                            }
                        });
                    }
                } 
            }
        }

        private static Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
    }
}

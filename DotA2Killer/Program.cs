using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DotA2Killer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            int failures = 0;

            while (true)
            {
                if (!AllowedToPlayDotA2())
                {
                    // Perhaps make this configurable in a config file so you can use this for more than DotA 2.
                    try
                    {
                        Process[] processes = Process.GetProcessesByName("dota2");
                        if (processes.Any())
                        {
                            foreach (Process process in processes)
                            {
                                process.Kill();
                            }
                        }

                        failures = 0;
                    }
                    catch (Exception e)
                    {
                        // Let this process fail 3 times consecutively at most.
                        failures++;

                        if (failures >= 3)
                        {
                            throw e;
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

        private static TimeSpan EightAM { get => new TimeSpan(8, 0, 0); }
        private static TimeSpan TenPM { get => new TimeSpan(22, 0, 0); }

        private static bool AllowedToPlayDotA2()
        {
            // You can potentially move this to a JSON config file.
            // Perhaps CRON format with a CRON NuGet package.
            DateTime now = DateTime.Now;
            DayOfWeek dayOfWeek = now.DayOfWeek;
            TimeSpan timeOfDay = now.TimeOfDay;

            if (dayOfWeek == DayOfWeek.Saturday)
            {
                return true;
            }
            else if (dayOfWeek == DayOfWeek.Friday)
            {
                return timeOfDay >= EightAM;
            }
            else if (dayOfWeek == DayOfWeek.Sunday)
            {
                return timeOfDay < TenPM;
            }
            else
            {
                return timeOfDay >= EightAM && timeOfDay <= TenPM;
            }
        }
    }
}

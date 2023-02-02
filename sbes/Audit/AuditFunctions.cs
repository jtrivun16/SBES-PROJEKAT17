using Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Audit
{
    public class AuditFunctions : IAuditFunctions
    {
        // Dos Attack
        // time interval and number of faild requests allowed
        public int allowedNumberOfDosAttacks = Int32.Parse(ConfigurationManager.AppSettings.Get("allowedNumberOfDosAttacks"));
        public int dosInterval = Int32.Parse(ConfigurationManager.AppSettings.Get("dosInterval"));

        public static Dictionary<string, int> dosTracker = new Dictionary<string, int>();


        public void DoSTrackerDetection()
        {
            var thread = new Thread(() =>
            {
                string user;
                string message;
                while (true)
                {
                    for (int interval = 1; interval <= dosInterval; interval++)
                    {
                        lock (dosTracker)
                        {
                            for (int i = 0; i < dosTracker.Count; i++)
                            {
                                user = (dosTracker.ElementAt(i)).Key;

                                if ((dosTracker.ElementAt(i)).Value > allowedNumberOfDosAttacks)
                                {
                                    LogEventTypes.DoSAttackDetected(user);
                                    message = $"[ EVENT LOG ] [ INFO ] DoS attack detected by user \'{user}\'.";
                                    dosTracker[user] = 0;
                                    Console.WriteLine(message);
                                }

                                // only on last piace of interval reset value to zero
                                if (interval == dosInterval)
                                    dosTracker[user] = 0;
                            }
                        }
                        Thread.Sleep(1000);
                    }
                }
            });

            thread.Start();
        
        }

        public void LogEvent(int code, string username)
        {
            string message = null;
            switch (code)
            {
                case 0:
                    try
                    {
                        LogEventTypes.ConnectSuccess(username);
                        message = $"[ EVENT LOG ] [ INFO ] User \'{username}\' connected.";
                        // added user in dictionary to track dos attacks
                        lock (dosTracker)
                        {
                            dosTracker.Add(username, 0);
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    break;
                case 1:
                    try
                    {
                        LogEventTypes.RunServiceSuccess(username);
                        message = $"[ EVENT LOG ] [ SUCCESS ] User \'{username}\' successfully run service.";
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    break;
                case 2:
                    try
                    {
                        LogEventTypes.RunServiceFailure(username);
                        message = $"[ EVENT LOG ] [ FAILURE ] User \'{username}\' failed to run service.";
                        lock (dosTracker)
                        {
                            dosTracker[username]++;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    break;
                case 3:
                    try
                    {
                        LogEventTypes.DoSAttackDetected(username);
                        message = $"[ EVENT LOG ] [ INFO ] DoS attack detected by user \'{username}\'.";
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    break;
                case 4:
                    try
                    {
                        LogEventTypes.BlacklistFileChanged();
                        message = $"[ EVENT LOG ] [ INFO ] \'blacklist.txt\' file corrupted.";
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    break;
            }
            Console.WriteLine(message);
        }
    }
}

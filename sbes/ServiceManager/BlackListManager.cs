﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceManager
{
    class BlackListManager
    {
        public static List<string> blackListPort = null;
        public static List<string> blackListProtocol = null;
        public static byte[] fileChecksum = null;

        public BlackListManager()
        {
            blackListPort = new List<string>();
            blackListProtocol = new List<string>();
            fileChecksum = BlackListChecksum();
        }

        public void ReadBlackListFile()
        {
            try
            {
                using (StreamReader sr = new StreamReader("blacklist.txt"))
                {
                    string line = null;
                    while (!String.IsNullOrEmpty(line = sr.ReadLine()))
                    {
                        string[] parts = line.Split('=');
                        string key = parts[0];
                        string value = parts[1];
                        switch (key)
                        {
                            case "port":
                                blackListPort.Add(value);
                                break;
                            case "protocol":
                                blackListProtocol.Add(value);
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static byte[] BlackListChecksum()
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead("blacklist.txt"))
                    {
                        return md5.ComputeHash(stream);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public void IsBlackListCorrupted()
        {
            var thread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(5000);
                    lock (fileChecksum)
                    {
                        byte[] help = BlackListChecksum();
                        for (int i = 0; i < fileChecksum.Length; i++)
                        {
                            if (fileChecksum[i] != help[i])
                            {
                                //Program.auditProxy.LogEvent((int)AuditEventTypes.BlacklistFileChanged, " ");
                                Console.WriteLine("Unauthorised blacklist file corrupted!!!");
                                //Program.flagShutdown = true;  //U slučaju da je integritet narušen, SM prijavljuje događaj Audit komponenti i nakon toga se zaustavlja.

                                break;
                            }
                        }
                    }
                }
            });

            thread.Start();
        }


        public static bool UpdateBlackList(string input)
        {
            lock (fileChecksum)                    // get hash stored in Data class, last valid hash
            {
                byte[] tempHash = BlackListChecksum();      // get hash of current state of .txt file
                bool write = true;                      // flag for validity of .txt
                for (int i = 0; i < fileChecksum.Length; i++)
                {
                    if (fileChecksum[i] != tempHash[i])
                    {
                        write = false;
                        return false;
                    }
                }

                if (write)
                {
                    using (StreamWriter sw = new StreamWriter("blacklist.txt", true))
                    {
                        sw.WriteLine(input);
                    }

                    fileChecksum = BlackListChecksum();        // set hash to new value
                    return true;
                }
            }
            return false;
        }


        public static bool ItemBlacklisted(string port, string protocol)
        {
            if (blackListPort.Contains(port) || blackListProtocol.Contains(protocol))
                return true;

            return false;
        }
    }
}
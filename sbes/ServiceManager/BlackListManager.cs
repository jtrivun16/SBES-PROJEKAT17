using Common;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceManager
{
    class BlackListManager
    {
        //public static List<string> blackListPort = null;
        //public static List<string> blackListProtocol = null;
        
        
        public static List<BlackListItem> blackListPort;
        public static List<BlackListItem> blackListProtocol;
        public static byte[] fileChecksum = null;

        public BlackListManager()
        {
            if (!File.Exists("blacklist.txt"))
                File.Create("blacklist.txt");

            blackListPort = new List<BlackListItem>();
            blackListProtocol = new List<BlackListItem>();
            fileChecksum = BlackListChecksum();

            ReadBlackListFile(); //load all data on start

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
                        string[] parts = line.Split(',');
                        string group = parts[0];
                        string key = parts[1];
                        string value = parts[2];

                        
                        switch (key)
                        {
                            case "port":
                                blackListPort.Add(new BlackListItem(group,value));
                                break;
                            case "protocol":
                                blackListProtocol.Add(new BlackListItem(group, value));
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
                                Program.auditProxy.LogEvent((int)Audit.AuditEventTypes.BlacklistFileChanged, " ");
                                Console.WriteLine("Unauthorised blacklist file corrupted!!!");
                                Program.exitService = true;  //U slučaju da je integritet narušen, SM prijavljuje događaj Audit komponenti i nakon toga se zaustavlja.

                                break;
                            }
                        }
                    }
                }
            });

            thread.Start();
        }


        public static bool UpdateBlackList()
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
                    System.IO.File.WriteAllText("blacklist.txt", string.Empty);
                    using (StreamWriter sw = new StreamWriter("blacklist.txt", true))
                    {    
                        foreach (var item in blackListPort)
                            sw.Write($"{item.Group},port,{item.Value}" + Environment.NewLine);
                        foreach (var item in blackListProtocol)
                            sw.Write($"{item.Group},protocol,{item.Value}" + Environment.NewLine);
                    }

                    fileChecksum = BlackListChecksum();        // set hash to new value
                    return true;
                }
            }
            return false;
        }


        public static bool ItemIsOnBlacklist(string port, string protocol)
        {
            string[] groups = GetUserGroups();

            foreach(string group in groups)
            {
                for(int i=0; i<blackListPort.Count; i++)
                {           
                    if (blackListPort[i].Group == group && blackListPort[i].Value == port)
                        return true; 
                }


                for(int i = 0; i < blackListProtocol.Count; i++)
                {
                    if (blackListProtocol[i].Group == group && blackListProtocol[i].Value == protocol)
                        return true;
                }
            }

            return false;
        }


        public static string[] GetUserGroups()
        {
            string[] groups = { string.Empty };

            WindowsIdentity windowsIdentity = (Thread.CurrentPrincipal.Identity as IIdentity) as WindowsIdentity;
            foreach (IdentityReference item in windowsIdentity.Groups)
            {
                //Trazimo SID koji je u jednom zapisu, konvertujemo ga u citljivi sid
                // group Name i trazimo permisije za njega 
                SecurityIdentifier sid = (SecurityIdentifier)item.Translate(typeof(SecurityIdentifier));
                var name = sid.Translate(typeof(NTAccount));
              

                string groupName = Formatter.ParseName(name.ToString());
                if (RolesConfig.GetPermissions(groupName, out string[] permissions))
                {
                    groups[groups.Count() - 1] = groupName;
                }

            }

            return groups;
        }
    }
}

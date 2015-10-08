using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace multyssh
{
    class Program
    {
        class ClientData
        {
            public string ip;
            public string username;
            public string password;
        }
        static List<ClientData> clients = new List<ClientData>();
        static List<string> command = new List<string>();

        static void Main(string[] args)
        {
            ReadConfig();
            CheckCommand();
            foreach (var client in clients)
            {
                RunCommand(client);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            Console.WriteLine("Finished...");
        }
        static void CheckCommand()
        {
            if (command.Count > 0)
                return;

            Console.WriteLine(">>> No command detected <<<");
            Console.WriteLine(">>> !run -- run all the command");
            Console.WriteLine(">>> !del -- del lastest command");
            Console.WriteLine(">>> !list -- list all commands");

            while (true)
            {
                Console.Write("# ");
                var cmd = Console.ReadLine();
                if(cmd.StartsWith("!run"))
                {
                    return;
                }
                else if (cmd.StartsWith("!del"))
                {
                    command.RemoveAt(command.Count - 1);
                }
                else if (cmd.StartsWith("!list"))
                {
                    foreach (var item in command)
                    { 
                        Console.WriteLine(">>> "+item);
                    }
                }
                else
                {
                    command.Add(cmd);
                }
            }

        }

        static void RunCommand(ClientData client)
        {

            IClient cli = new Client2(client.ip, client.username, client.password);
            cli.Connect();

            Console.WriteLine();
            Console.WriteLine(client.ip + " Running...");

            foreach (var cmd in command)
            {
                cli.WaitForIdle();
                cli.SendCommand(cmd);
            }

            cli.WaitForIdle();

            cli.Disconnect();
            cli = null;

            Console.WriteLine();
            Console.WriteLine(client.ip + " Finished...");
        }

        static void ReadConfig()
        {
            var fileStream = System.IO.File.Open(System.IO.Directory.GetCurrentDirectory() + "/server.txt", System.IO.FileMode.Open);
            StreamReader reader = new StreamReader(fileStream);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line.StartsWith("#") || string.IsNullOrEmpty(line))
                    continue;

                line = line.TrimStart(' ').TrimEnd(' ');

                if (line.StartsWith("sh"))
                {
                    string cmd = line.Substring(3);
                    command.Add(cmd);
                }
                else
                {
                    var datas = line.Split(' ');
                    if (datas.Length < 3) continue;

                    ClientData cdata = new ClientData();

                    cdata.ip = datas[0];
                    cdata.username = datas[1];
                    cdata.password = datas[2];

                    clients.Add(cdata);

                }
            }
        }
    }
}

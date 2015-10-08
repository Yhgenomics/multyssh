using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Tamir.SharpSsh;

namespace multyssh
{
    class Client : IClient
    {
        SshShell shell;
        public string Host;
        public string UserName;
        public string Password;

        Thread thr;
        Stream sshStream;

        Renci.SshNet.SshClient sshClient;

        int lastRead = Environment.TickCount;

        public bool ShowResponse { get; set; }

        ManualResetEvent evt = new ManualResetEvent(true);

        public bool EndOfStream
        {
            get
            {
                var delta = Environment.TickCount - lastRead;
                if (delta >= 5000) return true;
                else
                    return false;
            }
        }
        public bool CanSendCommand
        {
            get;
            private set;
        }
        public Client(string host,string username, string password)
        {
            this.Host = host;
            this.UserName = username;
            this.Password = password;

            //sshClient.ConnectionInfo.Username = "root";
            sshClient.Connect();

            ShowResponse = true;

            evt.Reset();

            shell = new SshShell(this.Host, this.UserName, this.Password);

            thr = new Thread(Read);

        }
        public void Connect()
        {
            try
            {
                shell.Connect();
                while (!shell.Connected) Thread.Sleep(1);
                while (!shell.ShellOpened) Thread.Sleep(1);
                while (!shell.ShellConnected) Thread.Sleep(1);
                
                sshStream = shell.GetStream();
                thr.Start();
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message);
            }
        }
        public void Disconnect()
        {
            if(thr!=null)
            {
                thr.Abort();
                thr = null; 
            } 

            if(shell!=null)
            {
                sshStream.Close();

                shell.Close();
            }
        }
        byte[] buffer = new byte[10240];
        void Read()
        {
            while (true)
            {
                int count = 0;

                count = sshStream.Read(buffer, 0, buffer.Length);

                lastRead = Environment.TickCount;

                var text = Encoding.ASCII.GetString(buffer, 0, count);
                Console.WriteLine(text);
                var lines = text.Split(new string[] {"\r\n" },StringSplitOptions.RemoveEmptyEntries);
                 
                if (ShowResponse)
                {
                    Console.Write(text);
                }

                foreach (var l in lines)
                {
                    if (l.StartsWith(this.shell.Username) && (l.EndsWith("# ") || l.EndsWith("#")))
                    {
                        evt.Set();
                        break;
                    }
                }
            }
        }

        public void SendCommand(string cmd)
        {
            evt.Reset();
            byte[] buf = Encoding.ASCII.GetBytes(cmd + "\n");
           // while (sshStream == null) Thread.Sleep(0);
            sshStream.Write(buf, 0, buf.Length);
           // sshStream.Flush();
            Console.WriteLine("Run: " + cmd);
        }

        public void WaitForIdle()
        {
            evt.WaitOne();
        }
    }
}

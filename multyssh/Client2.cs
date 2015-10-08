using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace multyssh
{
    public class Client2 : IClient
    {
        public string Host;
        public string UserName;
        public string Password;

        Renci.SshNet.SshClient sshClient;
        ShellStream ss;

        Thread thr;

        ManualResetEvent evt = new ManualResetEvent(true);


        public Client2(string host, string username, string password)
        { 
            this.Host = host;
            this.UserName = username;
            this.Password = password;

            sshClient = new SshClient(host, username, password);
        }

        public void Connect()
        {
            sshClient.Connect();
            ss = sshClient.CreateShellStream(Host, 10, 10, 10, 10, 1024);

            thr = new Thread(thrRead);
            thr.Start();
        }

        void thrRead()
        {
            byte[] buffer = new byte[1024]; 
            while(true)
            {
                int count = ss.Read(buffer, 0, buffer.Length);
                if(count!=0)
                {
                    string txt = Encoding.ASCII.GetString(buffer, 0, count);

                    var lines = txt.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var l in lines)
                    {
                        if(l.EndsWith("# ") || l.EndsWith("#"))
                        {
                            evt.Set();
                        }
                    }

                    Console.Write(txt);
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }
        public void WaitForIdle()
        {
            evt.WaitOne();
        }
        public void SendCommand(string cmd)
        {
            ss.WriteLine(cmd);
            evt.Reset();
        }

        public void Disconnect()
        {

            if(thr!= null)
            {
                thr.Abort();
                thr = null;
            }

            if(sshClient!= null)
            {
               if (sshClient.IsConnected)
                {
                    sshClient.Disconnect();
                }
                sshClient.Dispose();
            }
        }
    }
}

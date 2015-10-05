using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Tamir.SharpSsh;

namespace multyssh
{
    class Client
    {
        SshShell shell;
        public string Host;
        public string UserName;
        public string Password;

        Thread thr;
        Stream sshStream;

        int lastRead = Environment.TickCount;

        public bool ShowResponse { get; set; }

        public bool EndOfStream
        {
            get
            {
                var delta = Environment.TickCount - lastRead;
                if (delta >= 1000) return true;
                else
                    return false;
            }
        }
        public Client(string host,string username, string password)
        {
            this.Host = host;
            this.UserName = username;
            this.Password = password;

            ShowResponse = true;

            shell = new SshShell(this.Host, this.UserName, this.Password);

            thr = new Thread(Read);

        }
        public void Connect()
        {
            try
            {
                shell.Connect(); 
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
                shell.Close();
            }
        }
        byte[] buffer = new byte[1024];
        void Read()
        {
            while (true)
            {
                if (!shell.ShellOpened)
                {
                    Thread.Sleep(1); continue;
                }
                int count = 0;
                lock (sshStream)
                {
                    count = sshStream.Read(buffer, 0, buffer.Length);
                }
                 
                if (count > 0)
                {
                    lastRead = Environment.TickCount;
                   
                    if(ShowResponse)
                    {
                        var text = Encoding.ASCII.GetString(buffer, 0, count);
                        Console.Write(text);

                    }
                }  
            }
        }

        public void SendCommand(string cmd)
        { 
            byte[] buf = Encoding.ASCII.GetBytes(cmd + "\n");
            lock(sshStream)
            {
                sshStream.Write(buf, 0, buf.Length); 
            }
        }
    }
}

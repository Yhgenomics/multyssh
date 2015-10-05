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

        StreamReader reader;
        Stream sshStream;
        bool readResponse = false;
        public bool ShowResponse { get; set; }
        public Client(string host,string username, string password)
        {
            this.Host = host;
            this.UserName = username;
            this.Password = password;

            shell = new SshShell(this.Host, this.UserName, this.Password);

            readResponse = true;

            ShowResponse = false;

            //writer = new StreamWriter(ms);
            new Thread(Read).Start();

        }
        public void Connect()
        {
            try
            {
                shell.Connect();
                sshStream = shell.GetStream();
                reader = new StreamReader(sshStream);
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message);
            }
        }
        byte[] buffer = new byte[1024];
        void Read()
        {
            while (true)
            {
                if (reader == null)
                {
                    Thread.Sleep(1);
                    continue;
                }

                var count = sshStream.Read(buffer, 0, buffer.Length);

                if (count >0)
                {
                    Console.Write(Encoding.ASCII.GetString(buffer,0,count));
                } 

                //readResponse = false;
            }
        }

        public void SendCommand(string cmd)
        {
            //writer.WriteLine(cmd);
            //shell.WriteLine(cmd);

            byte[] buf = Encoding.ASCII.GetBytes(cmd);
            sshStream.Write(buf, 0, buf.Length);

            //readResponse = true;
        }
    }
}

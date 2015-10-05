using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace multyssh
{
    class Program
    {
        static void Main(string[] args)
        {
            Client c = new Client("121.41.88.111", "root", "oyt@w0rd1");
            c.ShowResponse = true;
            c.Connect();


            while(true)
            {
                c.SendCommand(Console.ReadLine()+"\n");
                //var t = Console.ReadLine();
                //c.SendCommand(t);
            }

        }
    }
}

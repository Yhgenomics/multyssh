using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace multyssh
{
    interface IClient
    {
        void Connect();
        void WaitForIdle();
        void SendCommand(string cmd);
        void Disconnect();
    }
}

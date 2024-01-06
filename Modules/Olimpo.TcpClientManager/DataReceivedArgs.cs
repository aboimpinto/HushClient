using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olimpo.TcpClientManager;

public class DataReceivedArgs
{
    public string Message { get; } = string.Empty;

    public DataReceivedArgs(string message)
    {
        this.Message = message;
    }
}

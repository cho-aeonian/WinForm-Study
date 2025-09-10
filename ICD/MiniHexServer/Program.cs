using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Buffers.Binary;
using MiniHex.Shared;

Console.OutputEncoding = Encoding.UTF8;
int port = 9001;
var listener = new TcpListener(IPAddress.Any, port);
listener.Start();
Console.WriteLine($"[Server] listening {port} (request/response only)");

byte state = 1;           // server-held status (1=IDLE, 2=ACTIVE)
uint numberCounter = 0;   // increases on each GET_NUMBER

_ = Task.Run(async () =>
{
    while (true)
    {
        var tcp = await listener.AcceptTcpClientAsync();
        tcp.NoDelay = true;
        Console.WriteLine($"[Server] client {tcp.Client.RemoteEndPoint}");
        _ = Handle(tcp);
    }
});

Console.WriteLine("Press Enter to quit...");
Console.ReadLine();

async Task Handle(TcpClient c)
{
    using var ns = c.GetStream();
    var buf = new byte[2048];
    var acc = new List<byte>();
    try
    {
        while (true)
        {
            int n = await ns.ReadAsync(buf, 0, buf.Length);
            if (n == 0) break;
            acc.AddRange(buf.AsSpan(0, n).ToArray());
            foreach (var raw in MiniHex.Shared.MiniFrame.ExtractFrames(acc))
            {
                if (!MiniFrame.TryParse(raw, out var cmd, out var payload))
                {
                    Console.WriteLine($"[Rx] invalid: {BitConverter.ToString(raw)}");
                    continue;
                }

                switch (cmd)
                {
                    case Cmd.HELLO:
                    {
                        string text = payload.Length > 0 ? Encoding.ASCII.GetString(payload) : "hello~";
                        Console.WriteLine($"[Rx] HELLO: \"{text}\"");
                        var ack = Encoding.ASCII.GetBytes("hello~");
                        Reply(ns, Cmd.HELLO_ACK, ack);
                        break;
                    }
                    case Cmd.GET_STATUS:
                    {
                        Console.WriteLine("[Rx] GET_STATUS");
                        Reply(ns, Cmd.STATUS, new byte[]{ state });
                        break;
                    }
                    case Cmd.GET_NUMBER:
                    {
                        Console.WriteLine("[Rx] GET_NUMBER");
                        uint value = ++numberCounter; // 1,2,3,...
                        var b = new byte[4];
                        BinaryPrimitives.WriteUInt32LittleEndian(b, value);
                        Reply(ns, Cmd.NUMBER, b);
                        break;
                    }
                    case Cmd.SET_STATUS:
                    {
                        if (payload.Length < 1) { Reply(ns, Cmd.NACK, new byte[]{ 0x01 }); break; }
                        state = payload[0];
                        Console.WriteLine($"[Rx] SET_STATUS -> {state}");
                        Reply(ns, Cmd.ACK, new byte[]{ state });
                        break;
                    }
                    default:
                        Console.WriteLine($"[Rx] unknown cmd=0x{cmd:X2}");
                        Reply(ns, Cmd.NACK, new byte[]{ 0xFF });
                        break;
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Server] client error: {ex.Message}");
    }
}

static void Reply(NetworkStream ns, byte cmd, ReadOnlySpan<byte> payload)
{
    var frame = MiniFrame.Build(cmd, payload);
    ns.Write(frame, 0, frame.Length);
    Console.WriteLine($"[Tx] {BitConverter.ToString(frame)}");
}
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static UdpClient? sender;      // For sending messages
    static UdpClient? receiver;    // For receiving messages
    static readonly int PORT = 55555;
    static string username = string.Empty;

    static void Main()
    {
        Console.Title = "Bitchat Windows";
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("Enter your nickname: ");
        Console.ResetColor();
        username = Console.ReadLine() ?? "Anonymous";

        // Setup sender (dynamic port)
        sender = new UdpClient();
        sender.EnableBroadcast = true;

        // Setup receiver with port sharing enabled
        receiver = new UdpClient();
        receiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        receiver.Client.Bind(new IPEndPoint(IPAddress.Any, PORT));

        // Listener thread
        Thread listener = new Thread(Listen) { IsBackground = true };
        listener.Start();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Connected. Broadcasting on LAN port {PORT}.");
        Console.WriteLine("Type messages and press Enter. Use /quit to exit.");
        Console.ResetColor();

        while (true)
        {
            string? msg = Console.ReadLine();
            if (msg == null || msg.Trim().ToLower() == "/quit") break;
            if (!string.IsNullOrWhiteSpace(msg))
                SendMessage($"{username}: {msg}");
        }

        sender?.Close();
        receiver?.Close();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Disconnected.");
        Console.ResetColor();
    }

    static void Listen()
    {
        while (receiver != null)
        {
            try
            {
                var remoteEP = new IPEndPoint(IPAddress.Any, 0);
                var data = receiver.Receive(ref remoteEP);
                string text = Encoding.UTF8.GetString(data);

                // Ignore messages from this user
                if (!text.StartsWith(username + ":"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[{DateTime.Now:HH:mm}] {text}");
                    Console.ResetColor();
                }
            }
            catch (SocketException)
            {
                break; // Graceful shutdown
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Error] {ex.Message}");
                Console.ResetColor();
            }
        }
    }

    static void SendMessage(string msg)
    {
        if (sender == null) return;
        byte[] data = Encoding.UTF8.GetBytes(msg);
        sender.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, PORT));
    }
}

using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static UdpClient udp;
    static IPEndPoint remoteEP = new IPEndPoint(IPAddress.Broadcast, 55555);
    static string username;

    static void Main()
    {
        Console.Write("Enter your nickname: ");
        username = Console.ReadLine();

        udp = new UdpClient();
        udp.EnableBroadcast = true;
        udp.Client.Bind(new IPEndPoint(IPAddress.Any, 55555));

        // Listen for incoming messages
        new Thread(Listen).Start();

        Console.WriteLine("Type messages and press Enter. Use /quit to exit.");
        while (true)
        {
            string msg = Console.ReadLine();
            if (msg == "/quit") break;
            SendMessage($"{username}: {msg}");
        }
    }

    static void Listen()
    {
        while (true)
        {
            var data = udp.Receive(ref remoteEP);
            string text = Encoding.UTF8.GetString(data);
            if (!text.StartsWith(username + ":"))
                Console.WriteLine(text);
        }
    }

    static void SendMessage(string msg)
    {
        byte[] data = Encoding.UTF8.GetBytes(msg);
        udp.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, 55555));
    }
}

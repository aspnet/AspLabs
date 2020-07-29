using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Client.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serverAddress = new Uri("https://localhost:5001");

            Console.WriteLine("Ready");
            Console.ReadKey();

            await ConnectAsync(serverAddress);
        }

        private static async Task ConnectAsync(Uri serverAddress)
        {
            using var client = new TcpClient();
            Console.WriteLine($"Connecting to {serverAddress}");
            await client.ConnectAsync(serverAddress.Host, serverAddress.Port);

            Stream stream = client.GetStream();

            if (serverAddress.Scheme == Uri.UriSchemeHttps)
            {
                Console.WriteLine("Negotiating TLS");
                var sslStream = new SslStream(stream);
                await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions()
                {
                    TargetHost = serverAddress.Host,
                    RemoteCertificateValidationCallback = (_, __, ___, ____) => true,
                });

                stream = sslStream;
            }

            Console.WriteLine("Sending message");
            var data = CreateMessage(serverAddress);
            for (var i = 0; i < data.Length; i++)
            {
                Console.Write(".");
                await stream.WriteAsync(data, i, 1);
                await Task.Delay(TimeSpan.FromSeconds(0.1));
            }
            Console.WriteLine();

            Console.WriteLine("Reading response");
            using var reader = new StreamReader(stream);
            var response = await reader.ReadToEndAsync();

            Console.WriteLine(response);

            stream.Dispose();
        }

        /// Proxy Protocol v2: https://www.haproxy.org/download/1.8/doc/proxy-protocol.txt Section 2.2
        private static byte[] CreateMessage(Uri serverAddress)
        {
            var memoryStream = new MemoryStream();
            var prefix = new byte[] {
                // Preamble(12 bytes) : 0D-0A-0D-0A-00-0D-0A-51-55-49-54-0A
                0x0D, 0x0A, 0x0D, 0x0A, 0x00, 0x0D, 0x0A, 0x51, 0x55, 0x49, 0x54, 0x0A,
                //  -21 Version + stream
                0x21,
                //  -11 TCP over IPv4
                0x11,
                //  -00-14 length
                0x00, 0x14,
                //  -AC-1C-00-04 src address
                0xAC, 0x1C, 0x00, 0x04,
                //  -01-02-03-04 dest address
                0x01, 0x02, 0x03, 0x04,
                //  -D7-9A src port
                0xD7, 0x9A,
                //  -13-88 dest port
                0x13, 0x88,
                //  -EE PP2_TYPE_AZURE
                0xEE,
                //  -00-05 length
                0x00, 0x05,
                //  -01 LINKID type
                0x01,
                //  -33-00-00-26 LINKID
                0x33, 0x00, 0x00, 0x26
            };
            memoryStream.Write(prefix);

            var request = $"GET / HTTP/1.1\r\nHost: {serverAddress.Host}\r\nConnection: close\r\n\r\n";
            memoryStream.Write(Encoding.ASCII.GetBytes(request));

            return memoryStream.ToArray();
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Testing.xunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.Proxy.Test
{
    public class WebSocketsTest
    {
        private static async Task<string> ReceiveTextMessage(WebSocket socket, int maxLen = 4096)
        {
            var buffer = new byte[maxLen];
            var received = 0;
            while (true)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer, received, maxLen - received), CancellationToken.None);
                Assert.Equal(WebSocketMessageType.Text, result.MessageType);
                received += result.Count;
                if (result.EndOfMessage)
                {
                    return Encoding.UTF8.GetString(buffer, 0, received);
                }
                Assert.InRange(received, 0, maxLen);
            }
        }

        [Fact]
        [OSSkipCondition(OperatingSystems.Windows, WindowsVersions.Win7, WindowsVersions.Win2008R2, SkipReason = "No WebSockets Client for this platform")]
        public async Task ProxyWebSocketsSmokeTest()
        {
            const string supportedSubProtocol = "myproto2";
            const string otherSubProtocol = "myproto1";
            const string message1Content = "TEST MESSAGE 1";
            const string message2Content = "TEST MSG 2";
            const string message3Content = "TEST MESSAGE 3";
            const string closeStatusDescription = "My Status1";

            using (var server = new WebHostBuilder()
                .UseKestrel()
                .Configure(app => app.UseWebSockets().Run(async ctx =>
                {
                    var socket = await ctx.WebSockets.AcceptWebSocketAsync(supportedSubProtocol);
                    var message1 = await ReceiveTextMessage(socket);
                    var message2 = await ReceiveTextMessage(socket);
                    Assert.Equal(message1Content, message1);
                    Assert.Equal(message2Content, message2);
                    await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message3Content)), WebSocketMessageType.Text, true, CancellationToken.None);
                    await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, closeStatusDescription, CancellationToken.None);
                })).Start("http://localhost:4001"))
            using (var proxy = new WebHostBuilder()
                .UseKestrel()
                .ConfigureServices(services => services.AddProxy())
                .Configure(app => app.UseWebSockets().RunProxy(new Uri("http://localhost:4001")))
                .Start("http://localhost:4002"))
            using (var client = new ClientWebSocket())
            {
                client.Options.AddSubProtocol(otherSubProtocol);
                client.Options.AddSubProtocol(supportedSubProtocol);
                await client.ConnectAsync(new Uri("ws://localhost:4002"), CancellationToken.None);
                Assert.Equal(supportedSubProtocol, client.SubProtocol);

                await client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message1Content)), WebSocketMessageType.Text, true, CancellationToken.None);
                await client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message2Content)), WebSocketMessageType.Text, true, CancellationToken.None);
                var message3 = await ReceiveTextMessage(client);
                Assert.Equal(message3Content, message3);

                var result = await client.ReceiveAsync(new ArraySegment<byte>(new byte[4096]), CancellationToken.None);
                Assert.Equal(WebSocketMessageType.Close, result.MessageType);
                Assert.Equal(WebSocketCloseStatus.NormalClosure, result.CloseStatus);
                Assert.Equal(closeStatusDescription, result.CloseStatusDescription);

                await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, closeStatusDescription, CancellationToken.None);
            }
        }
    }
}

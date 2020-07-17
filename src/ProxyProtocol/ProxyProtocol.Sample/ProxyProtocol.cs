using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace ProxyProtocol.Sample
{
    public static class ProxyProtocol
    {
        // The proxy protocol marker.
        private static readonly byte[] Preamble = { 0x0D, 0x0A, 0x0D, 0x0A, 0x00, 0x0D, 0x0A, 0x51, 0x55, 0x49, 0x54, 0x0A };
        public static readonly string SourceIPAddressKey = "ProxyProtocolV2SourceIPAddress";
        public static readonly string DestinationIPAddressKey = "ProxyProtocolV2DestinationIPAddress";
        public static readonly string SourcePortKey = "ProxyProtocolV2SourcePort";
        public static readonly string DestinationPortKey = "ProxyProtocolV2DestinationPort";
        public static readonly string LinkIdKey = "ProxyProtocolV2LinkId";

        /// <summary>
        /// Proxy Protocol v2: https://www.haproxy.org/download/1.8/doc/proxy-protocol.txt Section 2.2
        /// Preamble(12 bytes) : 0D-0A-0D-0A-00-0D-0A-51-55-49-54-0A
        ///  -21                        Version + stream    12
        ///  -11                        TCP over IPv4       13
        ///  -00-14                     length              14
        ///  -AC-1C-00-04               src address         16
        ///  -01-02-03-04               dest address        20
        ///  -D7-9A                     src port            24
        ///  -13-88                     dest port           26
        ///  -EE                        PP2_TYPE_AZURE      28
        ///  -00-05                     length              29
        ///  -01                        LINKID type         31
        ///  -33-00-00-26               LINKID              32.
        /// </summary>
        public static async Task ProcessAsync(ConnectionContext connectionContext, Func<Task> next, ILogger logger = null)
        {
            var input = connectionContext.Transport.Input;
            while (true)
            {
                var result = await input.ReadAsync();
                var buffer = result.Buffer;

                try
                {
                    if (result.IsCompleted)
                    {
                        return;
                    }

                    // Buffer does not have enough data to make decision
                    if (buffer.Length < 12)
                    {
                        continue;
                    }

                    var bufferArray = buffer.ToArray();
                    if (!IsProxyProtocol(bufferArray))
                    {
                        // Break if it is not PPv2.
                        break;
                    }
                    else
                    {
                        if (HasEnoughPpv2Data(bufferArray))
                        {
                            ExtractPpv2Data(ref buffer, bufferArray, connectionContext, logger);

                            // It is PPv2, and we have enough data for PPv2
                            break;
                        }

                        // It is PPv2, and we don't have enough data for PPv2
                    }
                }
                finally
                {
                    input.AdvanceTo(buffer.Start);
                }
            }

            await next();
        }

        private static bool IsProxyProtocol(byte[] buffer)
        {
            if (buffer.Length > 12)
            {
                for (var i = 0; i < 12; i++)
                {
                    if (Preamble[i] != buffer[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        private static void ExtractPpv2Data(ref ReadOnlySequence<byte> buffer, byte[] bufferArray, ConnectionContext context, ILogger logger = null)
        {
            // Probe traffic does not have valid ppv2 data.
            try
            {
                var length = (short)(bufferArray[15] | (bufferArray[14] << 8));
                var srcIpAddressArray = new byte[4];
                Array.Copy(bufferArray, 16, srcIpAddressArray, 0, 4);
                var srcAddress = new IPAddress(srcIpAddressArray);

                var destIpAddressArray = new byte[4];
                Array.Copy(bufferArray, 20, destIpAddressArray, 0, 4);
                var destAddress = new IPAddress(destIpAddressArray);

                var srcPort = (int)(bufferArray[25] | (bufferArray[24] << 8));
                var destPort = (int)(bufferArray[27] | (bufferArray[26] << 8));

                context.Items.Add(SourceIPAddressKey, srcAddress);
                context.Items.Add(DestinationIPAddressKey, destAddress);
                context.Items.Add(SourcePortKey, srcPort);
                context.Items.Add(DestinationPortKey, destPort);

                // Probe traffic does not have link ids.
                if (length > 12)
                {
                    var linkId = (long)(bufferArray[32] | (bufferArray[33] << 8) | (bufferArray[34] << 16) |
                                         (bufferArray[35] << 24));

                    context.Items.Add(LinkIdKey, linkId.ToString());
                }

                // Trim the buffer so the HTTP parser can pick up from there.
                buffer = buffer.Slice(length + 16);
            }
            catch
            {
                logger?.LogInformation($"ExtractPpv2Data error. BufferArray: {BitConverter.ToString(bufferArray)}");
                throw;
            }
        }

        private static bool HasEnoughPpv2Data(IReadOnlyList<byte> bufferArray)
        {
            if (bufferArray.Count < 16)
            {
                return false;
            }

            var length = (short)(bufferArray[15] | (bufferArray[14] << 8));
            return bufferArray.Count >= 16 + length;
        }
    }
}

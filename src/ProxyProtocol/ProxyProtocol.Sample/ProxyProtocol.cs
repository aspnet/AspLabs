// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        private static ReadOnlySpan<byte> Preamble => new byte[] { 0x0D, 0x0A, 0x0D, 0x0A, 0x00, 0x0D, 0x0A, 0x51, 0x55, 0x49, 0x54, 0x0A };

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
            // Count how many bytes we've examined so we never go backwards, Pipes don't allow that.
            var minBytesExamined = 0L;
            while (true)
            {
                var result = await input.ReadAsync();
                var buffer = result.Buffer;
                var examined = buffer.Start;

                try
                {
                    if (result.IsCompleted)
                    {
                        return;
                    }

                    if (buffer.Length == 0)
                    {
                        continue;
                    }

                    if (buffer.Length < Preamble.Length) // 12
                    {
                        // Buffer does not have enough data to make decision.
                        // Check for a partial match.
                        var partial = buffer.ToArray();
                        if (!Preamble.StartsWith(partial))
                        {
                            break;
                        }
                        minBytesExamined = buffer.Length;
                        examined = buffer.End;
                        continue;
                    }

                    var bufferArray = buffer.ToArray();
                    if (!bufferArray.AsSpan().StartsWith(Preamble))
                    {
                        // Break if it is not PPv2.
                        break;
                    }

                    if (HasEnoughPpv2Data(bufferArray))
                    {
                        // It is PPv2
                        ExtractPpv2Data(ref buffer, bufferArray, connectionContext, logger);
                        // We've consumed and sliced off the prefix.
                        minBytesExamined = 0; // Reset, we sliced off the examined bytes.
                        examined = buffer.Start;
                        break;
                    }

                    // It is PPv2, and we don't have enough data for PPv2
                    minBytesExamined = buffer.Length;
                    examined = buffer.End;
                }
                finally
                {
                    if (buffer.Slice(buffer.Start, examined).Length < minBytesExamined)
                    {
                        examined = buffer.Slice(buffer.Start, minBytesExamined).End;
                    }
                    input.AdvanceTo(buffer.Start, examined);
                }
            }

            await next();
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

                var feature = new ProxyProtocolFeature()
                {
                    SourceIp = srcAddress,
                    DestinationIp = destAddress,
                    SourcePort = srcPort,
                    DestinationPort = destPort,
                };

                // Probe traffic does not have link ids.
                if (length > 12)
                {
                    var linkId = (long)(bufferArray[32] | (bufferArray[33] << 8) | (bufferArray[34] << 16) |
                                         (bufferArray[35] << 24));

                    feature.LinkId = linkId;
                }

                // Trim the buffer so the HTTP parser can pick up from there.
                buffer = buffer.Slice(length + 16);

                context.Features.Set(feature);
            }
            catch
            {
                logger?.LogDebug($"ExtractPpv2Data error. BufferArray: {BitConverter.ToString(bufferArray)}");
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

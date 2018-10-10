// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    public class WebHookVerifySignatureFilterTests
    {
        public static TheoryData<string> InvalidData
        {
            get
            {
                return new TheoryData<string>
                {
                    { "1" },
                    { "\u0000\u0000" },
                    { "\u4F60\u597D" }, // "Hello there" in Chinese (CJK)
                    { "0\ufe0f\u20e3" },// 0 keycap (combining 3 characters into 1)
                    { "a0\ufe0f\u20e3" },// 0 keycap, offest one character
                    { "\U0001F01C" },   // Mahjong tile with four circles (aka "\ud83c\udc1c")
                    { "a\U0001F01C" },  // Mahjong tile with four circles, offset one character
                    { "\U000E0030" },   // Tag zero (aka "\udb40\udc20")
                    { "a\U000E0030" },  // Tag zero, offset one character
                    { "a\U000E0030b" }, // Tag zero, offset one character and padded to 4 chars
                };
            }
        }

        public static TheoryData<string> InvalidBase64Data
        {
            get
            {
                return new TheoryData<string>
                {
                    { "PconnnMcl8OOMBmgdd7p670K=" }, // extra padding
                    { "qiUI+5C3V6o4LtsIFcf33wzhlDxT8o+uluHcnrf2d7E===" }, // extra padding
                };
            }
        }

        public static TheoryData<string> InvalidHexData
        {
            get
            {
                return new TheoryData<string>
                {
                    { "123456789" },
                    { "github" },
                };
            }
        }

        public static TheoryData<string, byte[]> ValidBase64Data
        {
            get
            {
                return new TheoryData<string, byte[]>
                {
                    { null, Array.Empty<byte>() },
                    { string.Empty, Array.Empty<byte>() },
                    {
                        "ZRFZgf/mGj2fp0LnvRTsjmMW6aY=",
                        new byte[] { 101, 17, 89, 129, 255, 230, 26, 61, 159, 167, 66, 231, 189, 20, 236, 142, 99,
                            22, 233, 166 }
                    },
                    {
                        "ZRFZgf_mGj2fp0LnvRTsjmMW6aY", // URI safe
                        new byte[] { 101, 17, 89, 129, 255, 230, 26, 61, 159, 167, 66, 231, 189, 20, 236, 142, 99,
                            22, 233, 166 }
                    },
                    {
                        "PconnnMcl8OOMBmgdd7p670K",
                        new byte[] { 0x3d, 0xca, 0x27, 0x9e, 0x73, 0x1c, 0x97, 0xc3, 0x8e, 0x30, 0x19, 0xa0, 0x75,
                            0xde, 0xe9, 0xeb, 0xbd, 0x0a, }
                    },
                    {
                        "PconnnMcl8OOMBmgdd7p670KmQ==",
                        new byte[] { 0x3d, 0xca, 0x27, 0x9e, 0x73, 0x1c, 0x97, 0xc3, 0x8e, 0x30, 0x19, 0xa0, 0x75,
                            0xde, 0xe9, 0xeb, 0xbd, 0x0a, 0x99, }
                    },
                    {
                        "PconnnMcl8OOMBmgdd7p670KmQ", // no padding
                        new byte[] { 0x3d, 0xca, 0x27, 0x9e, 0x73, 0x1c, 0x97, 0xc3, 0x8e, 0x30, 0x19, 0xa0, 0x75,
                            0xde, 0xe9, 0xeb, 0xbd, 0x0a, 0x99, }
                    },
                    {
                        "PconnnMcl8OOMBmgdd7p670KmfA=",
                        new byte[] { 0x3d, 0xca, 0x27, 0x9e, 0x73, 0x1c, 0x97, 0xc3, 0x8e, 0x30, 0x19, 0xa0, 0x75,
                            0xde, 0xe9, 0xeb, 0xbd, 0x0a, 0x99, 0xf0, }
                    },
                    {
                        "PconnnMcl8OOMBmgdd7p670KmfD/",
                        new byte[] { 0x3d, 0xca, 0x27, 0x9e, 0x73, 0x1c, 0x97, 0xc3, 0x8e, 0x30, 0x19, 0xa0, 0x75,
                            0xde, 0xe9, 0xeb, 0xbd, 0x0a, 0x99, 0xf0, 0xff }
                    },
                    {
                        "PconnnMcl8OOMBmgdd7p670KmfD_", // URI safe
                        new byte[] { 0x3d, 0xca, 0x27, 0x9e, 0x73, 0x1c, 0x97, 0xc3, 0x8e, 0x30, 0x19, 0xa0, 0x75,
                            0xde, 0xe9, 0xeb, 0xbd, 0x0a, 0x99, 0xf0, 0xff }
                    },
                    {
                        "Urqtw9GASGPg083zL3rZcbAMGkw=",
                        new byte[] { 0x52, 0xba, 0xad, 0xc3, 0xd1, 0x80, 0x48, 0x63, 0xe0, 0xd3, 0xcd, 0xf3, 0x2f,
                            0x7a, 0xd9, 0x71, 0xb0, 0x0c, 0x1a, 0x4c, }
                    },
                    {
                        "YURTRjsganA5IGNpandlJ3IgaiBkZjtpaiAn",
                        new byte[] { 0x61, 0x44, 0x53, 0x46, 0x3b, 0x20, 0x6a, 0x70, 0x39, 0x20, 0x63, 0x69, 0x6a,
                            0x77, 0x65, 0x27, 0x72, 0x20, 0x6a, 0x20, 0x64, 0x66, 0x3b, 0x69, 0x6a, 0x20, 0x27, }
                    },
                    {
                        "qiUI+5C3V6o4LtsIFcf33wzhlDxT8o+uluHcnrf2d7E=",
                        new byte[] { 0xaa, 0x25, 0x08, 0xfb, 0x90, 0xb7, 0x57, 0xaa, 0x38, 0x2e, 0xdb, 0x08, 0x15,
                            0xc7, 0xf7, 0xdf, 0x0c, 0xe1, 0x94, 0x3c, 0x53, 0xf2, 0x8f, 0xae, 0x96, 0xe1, 0xdc, 0x9e,
                            0xb7, 0xf6, 0x77, 0xb1, }
                    },
                    {
                        "qiUI+5C3V6o4LtsIFcf33wzhlDxT8o+uluHcnrf2d7E", // no padding
                        new byte[] { 0xaa, 0x25, 0x08, 0xfb, 0x90, 0xb7, 0x57, 0xaa, 0x38, 0x2e, 0xdb, 0x08, 0x15,
                            0xc7, 0xf7, 0xdf, 0x0c, 0xe1, 0x94, 0x3c, 0x53, 0xf2, 0x8f, 0xae, 0x96, 0xe1, 0xdc, 0x9e,
                            0xb7, 0xf6, 0x77, 0xb1, }
                    },
                    {
                        "qiUI-5C3V6o4LtsIFcf33wzhlDxT8o+uluHcnrf2d7E", // URI safe
                        new byte[] { 0xaa, 0x25, 0x08, 0xfb, 0x90, 0xb7, 0x57, 0xaa, 0x38, 0x2e, 0xdb, 0x08, 0x15,
                            0xc7, 0xf7, 0xdf, 0x0c, 0xe1, 0x94, 0x3c, 0x53, 0xf2, 0x8f, 0xae, 0x96, 0xe1, 0xdc, 0x9e,
                            0xb7, 0xf6, 0x77, 0xb1, }
                    },
                };
            }
        }

        public static TheoryData<string, byte[]> ValidHexData
        {
            get
            {
                return new TheoryData<string, byte[]>
                {
                    { null, Array.Empty<byte>() },
                    { string.Empty, Array.Empty<byte>() },
                    {
                        "3dca279e731c97c38e3019a075dee9ebbd0a99f0",
                        new byte[] { 0x3d, 0xca, 0x27, 0x9e, 0x73, 0x1c, 0x97, 0xc3, 0x8e, 0x30, 0x19, 0xa0, 0x75,
                            0xde, 0xe9, 0xeb, 0xbd, 0x0a, 0x99, 0xf0, }
                    },
                    {
                        "3DCA279E731C97C38E3019A075DEE9EBBD0A99F0",
                        new byte[] { 0x3d, 0xca, 0x27, 0x9e, 0x73, 0x1c, 0x97, 0xc3, 0x8e, 0x30, 0x19, 0xa0, 0x75,
                            0xde, 0xe9, 0xeb, 0xbd, 0x0a, 0x99, 0xf0, }
                    },
                    {
                        "52baadc3d1804863e0d3cdf32f7ad971b00c1a4c",
                        new byte[] { 0x52, 0xba, 0xad, 0xc3, 0xd1, 0x80, 0x48, 0x63, 0xe0, 0xd3, 0xcd, 0xf3, 0x2f,
                            0x7a, 0xd9, 0x71, 0xb0, 0x0c, 0x1a, 0x4c, }
                    },
                    {
                        "52BAADC3D1804863E0D3CDF32F7AD971B00C1A4C",
                        new byte[] { 0x52, 0xba, 0xad, 0xc3, 0xd1, 0x80, 0x48, 0x63, 0xe0, 0xd3, 0xcd, 0xf3, 0x2f,
                            0x7a, 0xd9, 0x71, 0xb0, 0x0c, 0x1a, 0x4c, }
                    },
                    {
                        "614453463b206a70392063696a77652772206a2064663b696a2027",
                        new byte[] { 0x61, 0x44, 0x53, 0x46, 0x3b, 0x20, 0x6a, 0x70, 0x39, 0x20, 0x63, 0x69, 0x6a,
                            0x77, 0x65, 0x27, 0x72, 0x20, 0x6a, 0x20, 0x64, 0x66, 0x3b, 0x69, 0x6a, 0x20, 0x27, }
                    },
                    {
                        "614453463B206A70392063696A77652772206A2064663B696A2027",
                        new byte[] { 0x61, 0x44, 0x53, 0x46, 0x3b, 0x20, 0x6a, 0x70, 0x39, 0x20, 0x63, 0x69, 0x6a,
                            0x77, 0x65, 0x27, 0x72, 0x20, 0x6a, 0x20, 0x64, 0x66, 0x3b, 0x69, 0x6a, 0x20, 0x27, }
                    },
                    {
                        "aa2508fb90b757aa382edb0815c7f7df0ce1943c53f28fae96e1dc9eb7f677b1",
                        new byte[] { 0xaa, 0x25, 0x08, 0xfb, 0x90, 0xb7, 0x57, 0xaa, 0x38, 0x2e, 0xdb, 0x08, 0x15,
                            0xc7, 0xf7, 0xdf, 0x0c, 0xe1, 0x94, 0x3c, 0x53, 0xf2, 0x8f, 0xae, 0x96, 0xe1, 0xdc, 0x9e,
                            0xb7, 0xf6, 0x77, 0xb1, }
                    },
                    {
                        "AA2508FB90B757AA382EDB0815C7F7DF0CE1943C53F28FAE96E1DC9EB7F677B1",
                        new byte[] { 0xaa, 0x25, 0x08, 0xfb, 0x90, 0xb7, 0x57, 0xaa, 0x38, 0x2e, 0xdb, 0x08, 0x15,
                            0xc7, 0xf7, 0xdf, 0x0c, 0xe1, 0x94, 0x3c, 0x53, 0xf2, 0x8f, 0xae, 0x96, 0xe1, 0xdc, 0x9e,
                            0xb7, 0xf6, 0x77, 0xb1, }
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidData))]
        [MemberData(nameof(InvalidBase64Data))]
        public void FromBase64_ReturnsNullIfInputInvalid(string content)
        {
            // Arrange
            var filter = new TestVerifySignatureFilter();

            // Act
            var result = filter.FromBase64Public(content, "Some-Header");

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(ValidBase64Data))]
        public void FromBase64_ReturnsExpectedBytes(string content, byte[] expectedResult)
        {
            // Arrange
            var filter = new TestVerifySignatureFilter();

            // Act
            var result = filter.FromBase64Public(content, "Some-Header");

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(InvalidData))]
        [MemberData(nameof(InvalidHexData))]
        public void FromHex_ReturnsNullIfInputInvalid(string content)
        {
            // Arrange
            var filter = new TestVerifySignatureFilter();

            // Act
            var result = filter.FromHexPublic(content, "Some-Header");

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(ValidHexData))]
        public void FromHex_ReturnsExpectedBytes(string content, byte[] expectedResult)
        {
            // Arrange
            var filter = new TestVerifySignatureFilter();

            // Act
            var result = filter.FromHexPublic(content, "Some-Header");

            // Assert
            Assert.Equal(expectedResult, result);
        }

        private class TestVerifySignatureFilter : WebHookVerifySignatureFilter
        {
            public TestVerifySignatureFilter()
                : base(new ConfigurationBuilder().Build(), Mock.Of<IHostingEnvironment>(), NullLoggerFactory.Instance)
            {
            }

            public override string ReceiverName => "TestReceiver";

            public byte[] FromBase64Public(string content, string signatureHeaderName)
            {
                return FromBase64(content, signatureHeaderName);
            }

            public byte[] FromHexPublic(string content, string signatureHeaderName)
            {
                return FromHex(content, signatureHeaderName);
            }
        }
    }
}

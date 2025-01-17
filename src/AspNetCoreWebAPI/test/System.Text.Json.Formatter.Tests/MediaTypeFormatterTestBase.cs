// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting.DataSets;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;
using Xunit;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// A test class for common <see cref="MediaTypeFormatter"/> functionality across multiple implementations.
    /// </summary>
    /// <typeparam name="TFormatter">The type of formatter under test.</typeparam>
    public abstract class MediaTypeFormatterTestBase<TFormatter> where TFormatter : MediaTypeFormatter
    {
        protected MediaTypeFormatterTestBase()
        {
        }

        // Test data variations of interest in round-trip tests.
        public const TestDataVariations RoundTripDataVariations =
            TestDataVariations.All | TestDataVariations.WithNull | TestDataVariations.AsClassMember;

        public abstract IEnumerable<MediaTypeHeaderValue> ExpectedSupportedMediaTypes { get; }

        public abstract IEnumerable<Encoding> ExpectedSupportedEncodings { get; }

        /// <summary>
        /// Byte representation of an <see cref="SampleType"/> with value 42 using the default encoding
        /// for this media type formatter.
        /// </summary>
        public abstract byte[] ExpectedSampleTypeByteRepresentation { get; }

        [Fact]
        public void SupportedMediaTypes_HeaderValuesAreNotSharedBetweenInstances()
        {
            var formatter1 = CreateFormatter();
            var formatter2 = CreateFormatter();

            foreach (MediaTypeHeaderValue mediaType1 in formatter1.SupportedMediaTypes)
            {
                MediaTypeHeaderValue mediaType2 = formatter2.SupportedMediaTypes.Single(m => m.Equals(mediaType1));
                Assert.NotSame(mediaType1, mediaType2);
            }
        }

        [Fact]
        public void SupportEncodings_ValuesAreNotSharedBetweenInstances()
        {
            var formatter1 = CreateFormatter();
            var formatter2 = CreateFormatter();

            foreach (Encoding mediaType1 in formatter1.SupportedEncodings)
            {
                Encoding mediaType2 = formatter2.SupportedEncodings.Single(m => m.Equals(mediaType1));
                Assert.NotSame(mediaType1, mediaType2);
            }
        }

        [Fact]
        public void SupportMediaTypes_DefaultSupportedMediaTypes()
        {
            TFormatter formatter = CreateFormatter();
            Assert.True(ExpectedSupportedMediaTypes.SequenceEqual(formatter.SupportedMediaTypes));
        }

        [Fact]
        public void SupportEncoding_DefaultSupportedEncodings()
        {
            TFormatter formatter = CreateFormatter();
            Assert.Equal(ExpectedSupportedEncodings, formatter.SupportedEncodings);
        }

        [Fact]
        public async Task ReadFromStreamAsync_WhenContentLengthIsZero_DoesNotReadStream()
        {
            // Arrange
            TFormatter formatter = CreateFormatter();
            Mock<Stream> mockStream = new Mock<Stream>();
            IFormatterLogger mockFormatterLogger = new Mock<IFormatterLogger>().Object;
            HttpContent content = new StringContent(String.Empty);
            HttpContentHeaders contentHeaders = content.Headers;
            contentHeaders.ContentLength = 0;

            // Act
            await formatter.ReadFromStreamAsync(typeof(SampleType), mockStream.Object, content, mockFormatterLogger);

            // Assert
            mockStream.Verify(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
            mockStream.Verify(s => s.ReadByte(), Times.Never());
            mockStream.Verify(s => s.BeginRead(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()), Times.Never());
        }

        [Fact]
        public async Task ReadFromStreamAsync_WhenContentLengthIsZero_DoesNotCloseStream()
        {
            // Arrange
            TFormatter formatter = CreateFormatter();
            Mock<Stream> mockStream = new Mock<Stream>();
            IFormatterLogger mockFormatterLogger = new Mock<IFormatterLogger>().Object;
            HttpContent content = new StringContent(String.Empty);
            HttpContentHeaders contentHeaders = content.Headers;
            contentHeaders.ContentLength = 0;

            // Act
            await formatter.ReadFromStreamAsync(typeof(SampleType), mockStream.Object, content, mockFormatterLogger);

            // Assert
            mockStream.Verify(s => s.Close(), Times.Never());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(0)]
        [InlineData("")]
        public async Task ReadFromStreamAsync_WhenContentLengthIsZero_ReturnsDefaultTypeValue<T>(T value)
        {
            // Arrange
            GC.KeepAlive(value); // Mark parameter as used. See xUnit1026, [Theory] method doesn't use all parameters.
            TFormatter formatter = CreateFormatter();
            HttpContent content = new StringContent("");

            // Act
            var contentStream = await content.ReadAsStreamAsync();
            var result = await formatter.ReadFromStreamAsync(typeof(T), contentStream, content, null);

            // Assert
            Assert.Equal(default(T), (T)result);
        }

        [Fact]
        public async Task ReadFromStreamAsync_ReadsDataButDoesNotCloseStream()
        {
            // Arrange
            TFormatter formatter = CreateFormatter();
            MemoryStream memStream = new MemoryStream(ExpectedSampleTypeByteRepresentation);
            HttpContent content = new StringContent(String.Empty);
            HttpContentHeaders contentHeaders = content.Headers;
            contentHeaders.ContentLength = memStream.Length;
            contentHeaders.ContentType = CreateSupportedMediaType();

            // Act
            var result = await formatter.ReadFromStreamAsync(typeof(SampleType), memStream, content, null);

            // Assert
            Assert.True(memStream.CanRead);

            var value = Assert.IsType<SampleType>(result);
            Assert.Equal(42, value.Number);
        }

        [Fact]
        public async Task ReadFromStreamAsync_WhenContentLengthIsNull_ReadsDataButDoesNotCloseStream()
        {
            // Arrange
            TFormatter formatter = CreateFormatter();
            MemoryStream memStream = new MemoryStream(ExpectedSampleTypeByteRepresentation);
            HttpContent content = new StringContent(String.Empty);
            HttpContentHeaders contentHeaders = content.Headers;
            contentHeaders.ContentLength = null;
            contentHeaders.ContentType = CreateSupportedMediaType();

            // Act
            var result = await formatter.ReadFromStreamAsync(typeof(SampleType), memStream, content, null);

            // Assert
            Assert.True(memStream.CanRead);

            var value = Assert.IsType<SampleType>(result);
            Assert.Equal(42, value.Number);
        }

        [Fact]
        public virtual async Task WriteToStreamAsync_WhenObjectIsNull_WritesDataButDoesNotCloseStream()
        {
            // Arrange
            TFormatter formatter = CreateFormatter();
            Mock<Stream> mockStream = new Mock<Stream>();
            mockStream.Setup(s => s.CanWrite).Returns(true);
            HttpContent content = new StringContent(String.Empty);

            // Act
            await formatter.WriteToStreamAsync(typeof(SampleType), null, mockStream.Object, content, null);

            // Assert
            mockStream.Verify(s => s.Close(), Times.Never());
            mockStream.Verify(s => s.BeginWrite(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()), Times.Never());
        }

        [Fact]
        public async Task WriteToStreamAsync_WritesDataButDoesNotCloseStream()
        {
            // Arrange
            TFormatter formatter = CreateFormatter();
            SampleType sampleType = new SampleType { Number = 42 };
            MemoryStream memStream = new MemoryStream();
            HttpContent content = new StringContent(String.Empty);
            content.Headers.ContentType = CreateSupportedMediaType();

            // Act
            await formatter.WriteToStreamAsync(typeof(SampleType), sampleType, memStream, content, null);

            // Assert
            Assert.True(memStream.CanRead);

            byte[] actualSampleTypeByteRepresentation = memStream.ToArray();
            Assert.NotEmpty(actualSampleTypeByteRepresentation);
        }

        [Fact]
        public virtual async Task Overridden_WriteToStreamAsyncWithoutCancellationToken_GetsCalled()
        {
            // Arrange
            Stream stream = new MemoryStream();
            Mock<TFormatter> formatter = CreateMockFormatter();
            ObjectContent<int> content = new ObjectContent<int>(42, formatter.Object);

            formatter
                .Setup(f => f.WriteToStreamAsync(typeof(int), 42, stream, content, null /* transportContext */))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await content.CopyToAsync(stream);

            // Assert
            formatter.Verify();
        }

        [Fact]
        public virtual async Task Overridden_WriteToStreamAsyncWithCancellationToken_GetsCalled()
        {
            // Arrange
            Stream stream = new MemoryStream();
            Mock<TFormatter> formatter = CreateMockFormatter();
            ObjectContent<int> content = new ObjectContent<int>(42, formatter.Object);

            formatter
                .Setup(f => f.WriteToStreamAsync(typeof(int), 42, stream, content, null /* transportContext */, CancellationToken.None))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await content.CopyToAsync(stream);

            // Assert
            formatter.Verify();
        }

        [Fact]
        public virtual async Task Overridden_ReadFromStreamAsyncWithCancellationToken_GetsCalled()
        {
            // Arrange
            Stream stream = new MemoryStream();
            Mock<TFormatter> formatter = CreateMockFormatter();
            formatter.Object.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/test"));
            StringContent content = new StringContent(" ", Encoding.Default, "application/test");
            CancellationTokenSource cts = new CancellationTokenSource();

            formatter
                .Setup(f => f.ReadFromStreamAsync(typeof(string), It.IsAny<Stream>(), content, null /*formatterLogger */, cts.Token))
                .Returns(Task.FromResult<object>(null))
                .Verifiable();

            // Act
            await content.ReadAsAsync<string>(new[] { formatter.Object }, cts.Token);

            // Assert
            formatter.Verify();
        }

        protected virtual TFormatter CreateFormatter()
        {
            ConstructorInfo constructor = typeof(TFormatter).GetConstructor(Type.EmptyTypes);
            return (TFormatter)constructor.Invoke(null);
        }

        protected virtual Mock<TFormatter> CreateMockFormatter()
        {
            return new Mock<TFormatter>() { CallBase = true };
        }

        protected virtual MediaTypeHeaderValue CreateSupportedMediaType()
        {
            return ExpectedSupportedMediaTypes.First();
        }

        public async Task<object> ReadFromStreamAsync_RoundTripsWriteToStreamAsync_Helper(MediaTypeFormatter formatter, Type variationType, object testData)
        {
            // Arrange
            HttpContent content = new StringContent(String.Empty);
            HttpContentHeaders contentHeaders = content.Headers;
            object readObj = null;

            // Act & Assert
            using (MemoryStream stream = new MemoryStream())
            {
                await formatter.WriteToStreamAsync(variationType, testData, stream, content, transportContext: null);
                contentHeaders.ContentLength = stream.Length;

                stream.Flush();
                stream.Seek(0L, SeekOrigin.Begin);

                readObj = await formatter.ReadFromStreamAsync(variationType, stream, content, formatterLogger: null);
            }

            return readObj;
        }

        public async Task WriteAndReadAsync(Func<MemoryStream, Task> codeThatWrites, Func<MemoryStream, Task> codeThatReadsAsync)
        {
            if (codeThatWrites == null)
            {
                throw new ArgumentNullException("codeThatWrites");
            }

            if (codeThatReadsAsync == null)
            {
                throw new ArgumentNullException("codeThatReads");
            }

            using (MemoryStream stream = new MemoryStream())
            {
                await codeThatWrites(stream);

                stream.Flush();
                stream.Seek(0L, SeekOrigin.Begin);

                await codeThatReadsAsync(stream);
            }
        }

        protected async Task ReadFromStreamAsync_UsesCorrectCharacterEncodingHelper(MediaTypeFormatter formatter, string content, string formattedContent, string mediaType, string encoding, bool isDefaultEncoding)
        {
            // Arrange
            Encoding enc = null;
            if (isDefaultEncoding)
            {
                enc = formatter.SupportedEncodings.First((e) => e.WebName.Equals(encoding, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                enc = Encoding.GetEncoding(encoding);
                formatter.SupportedEncodings.Add(enc);
            }

            byte[] data = enc.GetBytes(formattedContent);
            MemoryStream memStream = new MemoryStream(data);

            StringContent dummyContent = new StringContent(string.Empty);
            HttpContentHeaders headers = dummyContent.Headers;
            headers.Clear();
            headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
            headers.ContentLength = data.Length;

            IFormatterLogger mockFormatterLogger = new Mock<IFormatterLogger>().Object;

            // Act
            string result = (await formatter.ReadFromStreamAsync(typeof(string), memStream, dummyContent, mockFormatterLogger)) as string;

            // Assert
            Assert.Equal(content, result);
        }

        protected async Task WriteToStreamAsync_UsesCorrectCharacterEncodingHelper(MediaTypeFormatter formatter, string content, string formattedContent, string mediaType, string encoding, bool isDefaultEncoding)
        {
            // Arrange
            Encoding enc = null;
            if (isDefaultEncoding)
            {
                enc = formatter.SupportedEncodings.First((e) => e.WebName.Equals(encoding, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                enc = Encoding.GetEncoding(encoding);
                formatter.SupportedEncodings.Add(enc);
            }

            byte[] preamble = enc.GetPreamble();
            byte[] data = enc.GetBytes(formattedContent);
            byte[] expectedData = new byte[preamble.Length + data.Length];
            Buffer.BlockCopy(preamble, 0, expectedData, 0, preamble.Length);
            Buffer.BlockCopy(data, 0, expectedData, preamble.Length, data.Length);

            MemoryStream memStream = new MemoryStream();

            HttpContent httpContent = new StringContent(String.Empty);
            HttpContentHeaders contentHeaders = httpContent.Headers;
            contentHeaders.Clear();
            contentHeaders.ContentType = MediaTypeHeaderValue.Parse(mediaType);
            contentHeaders.ContentLength = expectedData.Length;

            IFormatterLogger mockFormatterLogger = new Mock<IFormatterLogger>().Object;

            // Act
            await formatter.WriteToStreamAsync(typeof(string), content, memStream, httpContent, null);

            // Assert
            byte[] actualData = memStream.ToArray();
            Assert.Equal(expectedData, actualData);
        }

        public static Encoding CreateOrGetSupportedEncoding(MediaTypeFormatter formatter, string encoding, bool isDefaultEncoding)
        {
            Encoding enc = null;
            if (isDefaultEncoding)
            {
                enc = formatter.SupportedEncodings.First((e) => e.WebName.Equals(encoding, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                enc = Encoding.GetEncoding(encoding);
                formatter.SupportedEncodings.Add(enc);
            }

            return enc;
        }

        protected static Task ReadContentUsingCorrectCharacterEncodingHelperAsync(MediaTypeFormatter formatter, string content, string formattedContent, string mediaType, string encoding, bool isDefaultEncoding)
        {
            // Arrange
            Encoding enc = CreateOrGetSupportedEncoding(formatter, encoding, isDefaultEncoding);
            byte[] sourceData = enc.GetBytes(formattedContent);

            // Further Arrange, Act & Assert
            return ReadContentUsingCorrectCharacterEncodingHelperAsync(formatter, content, sourceData, mediaType);
        }

        protected static async Task ReadContentUsingCorrectCharacterEncodingHelperAsync(MediaTypeFormatter formatter, string content, byte[] sourceData, string mediaType)
        {
            // Arrange
            MemoryStream memStream = new MemoryStream(sourceData);

            StringContent dummyContent = new StringContent(string.Empty);
            HttpContentHeaders headers = dummyContent.Headers;
            headers.Clear();
            headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
            headers.ContentLength = sourceData.Length;

            IFormatterLogger mockFormatterLogger = new Mock<IFormatterLogger>().Object;

            // Act
            var result = (await formatter.ReadFromStreamAsync(typeof(string), memStream, dummyContent, mockFormatterLogger)) as string;

            // Assert
            Assert.Equal(content, result);
        }

        protected static Task WriteContentUsingCorrectCharacterEncodingHelperAsync(MediaTypeFormatter formatter, string content, string formattedContent, string mediaType, string encoding, bool isDefaultEncoding)
        {
            // Arrange
            Encoding enc = CreateOrGetSupportedEncoding(formatter, encoding, isDefaultEncoding);

            byte[] preamble = enc.GetPreamble();
            byte[] data = enc.GetBytes(formattedContent);
            byte[] expectedData = new byte[preamble.Length + data.Length];
            Buffer.BlockCopy(preamble, 0, expectedData, 0, preamble.Length);
            Buffer.BlockCopy(data, 0, expectedData, preamble.Length, data.Length);

            // Further Arrange, Act & Assert
            return WriteContentUsingCorrectCharacterEncodingHelperAsync(formatter, content, expectedData, mediaType);
        }


        protected static async Task WriteContentUsingCorrectCharacterEncodingHelperAsync(MediaTypeFormatter formatter, string content, byte[] expectedData, string mediaType)
        {
            // Arrange
            MemoryStream memStream = new MemoryStream();

            StringContent dummyContent = new StringContent(string.Empty);
            HttpContentHeaders headers = dummyContent.Headers;
            headers.Clear();
            headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
            headers.ContentLength = expectedData.Length;

            IFormatterLogger mockFormatterLogger = new Mock<IFormatterLogger>().Object;

            // Act
            await formatter.WriteToStreamAsync(typeof(string), content, memStream, dummyContent, null);

            // Assert
            byte[] actualData = memStream.ToArray();

            Assert.Equal(expectedData, actualData);
        }
    }

    [DataContract(Name = "DataContractSampleType")]
    public class SampleType
    {
        [DataMember]
        public int Number { get; set; }
    }
}

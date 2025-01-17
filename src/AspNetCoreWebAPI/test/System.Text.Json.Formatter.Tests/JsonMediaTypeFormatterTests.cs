// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting.DataSets;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace System.Net.Http.Formatting
{
    public class SystemTextJsonMediaTypeFormatterTests : MediaTypeFormatterTestBase<SystemTextJsonMediaTypeFormatter>
    {
        public static IEnumerable<TestData> ValueAndRefTypeTestDataCollectionExceptULong
        {
            get
            {
                // Include neither ISerializable data set nor unsigned longs
                return CommonUnitTestDataSets.ValueAndRefTypeTestDataCollection.Except(
                    new TestData[] { CommonUnitTestDataSets.Ulongs, CommonUnitTestDataSets.ISerializableTypes });
            }
        }

        public override IEnumerable<MediaTypeHeaderValue> ExpectedSupportedMediaTypes
        {
            get { return HttpTestData.StandardJsonMediaTypes; }
        }

        public override IEnumerable<Encoding> ExpectedSupportedEncodings
        {
            get { return HttpTestData.StandardEncodings; }
        }

        public override byte[] ExpectedSampleTypeByteRepresentation
        {
            get { return ExpectedSupportedEncodings.ElementAt(0).GetBytes("{\"Number\":42}"); }
        }

        [Theory]
        [TestDataSet(typeof(SystemTextJsonMediaTypeFormatterTests), nameof(ValueAndRefTypeTestDataCollectionExceptULong), RoundTripDataVariations)]
        public async Task ReadFromStreamAsync_RoundTripsWriteToStreamAsync(Type variationType, object testData)
        {
            // Guard
            // Arrange
            SystemTextJsonMediaTypeFormatter formatter = new SystemTextJsonMediaTypeFormatter();

            // Arrange & Act & Assert
            object readObj = await ReadFromStreamAsync_RoundTripsWriteToStreamAsync_Helper(formatter, variationType, testData);
            Assert.Equal(testData, readObj);
        }

        [Fact]
        public async Task JsonSerialization_DoesNotIndentByDefault()
        {
            SystemTextJsonMediaTypeFormatter formatter = new SystemTextJsonMediaTypeFormatter();
            MemoryStream memoryStream = new MemoryStream();
            HttpContent content = new StringContent(String.Empty);
            await formatter.WriteToStreamAsync(typeof(SampleType), new SampleType(), memoryStream, content, transportContext: null);
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Assert.DoesNotContain(Environment.NewLine, serializedString);
        }

        [Fact]
        public async Task JsonSerialization_IdentsWhenConfigured()
        {
            SystemTextJsonMediaTypeFormatter formatter = new SystemTextJsonMediaTypeFormatter
            {
                JsonSerializerOptions =
                {
                    WriteIndented = true,
                }
            };
            MemoryStream memoryStream = new MemoryStream();
            HttpContent content = new StringContent(String.Empty);
            await formatter.WriteToStreamAsync(typeof(SampleType), new SampleType(), memoryStream, content, transportContext: null);
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Assert.Contains(Environment.NewLine, serializedString);
        }

        [Theory]
        [InlineData(typeof(IQueryable<string>))]
        [InlineData(typeof(IEnumerable<string>))]
        public async Task UseJsonFormatterWithNull(Type type)
        {
            SystemTextJsonMediaTypeFormatter formatter = new SystemTextJsonMediaTypeFormatter();
            MemoryStream memoryStream = new MemoryStream();
            HttpContent content = new StringContent(String.Empty);
            await formatter.WriteToStreamAsync(type, null, memoryStream, content, transportContext: null);
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Assert.True(serializedString.Contains("null"), "Using Json formatter to serialize null should emit 'null'.");
        }
    }
}

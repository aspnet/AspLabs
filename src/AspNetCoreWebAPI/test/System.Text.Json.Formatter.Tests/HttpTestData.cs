// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting.DataSets.Types;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.TestCommon;
using Xunit;

namespace System.Net.Http.Formatting.DataSets
{
    public class HttpTestData
    {
        public static TestData<HttpMethod> AllHttpMethods
        {
            get
            {
                return new RefTypeTestData<HttpMethod>(() =>
                    StandardHttpMethods.Concat(CustomHttpMethods).ToList());
            }
        }

        public static TestData<HttpMethod> StandardHttpMethods
        {
            get
            {
                return new RefTypeTestData<HttpMethod>(() => new List<HttpMethod>()
                {
                    HttpMethod.Head,
                    HttpMethod.Get,
                    HttpMethod.Post,
                    HttpMethod.Put,
                    HttpMethod.Delete,
                    HttpMethod.Options,
                    HttpMethod.Trace,
                });
            }
        }

        public static TestData<HttpMethod> CustomHttpMethods
        {
            get
            {
                return new RefTypeTestData<HttpMethod>(() => new List<HttpMethod>()
                {
                    new HttpMethod("Custom")
                });
            }
        }

        public static TestData<HttpStatusCode> AllHttpStatusCodes
        {
            get
            {
                return new ValueTypeTestData<HttpStatusCode>(new HttpStatusCode[]
                {
                    HttpStatusCode.Accepted,
                    HttpStatusCode.Ambiguous,
                    HttpStatusCode.BadGateway,
                    HttpStatusCode.BadRequest,
                    HttpStatusCode.Conflict,
                    HttpStatusCode.Continue,
                    HttpStatusCode.Created,
                    HttpStatusCode.ExpectationFailed,
                    HttpStatusCode.Forbidden,
                    HttpStatusCode.Found,
                    HttpStatusCode.GatewayTimeout,
                    HttpStatusCode.Gone,
                    HttpStatusCode.HttpVersionNotSupported,
                    HttpStatusCode.InternalServerError,
                    HttpStatusCode.LengthRequired,
                    HttpStatusCode.MethodNotAllowed,
                    HttpStatusCode.Moved,
                    HttpStatusCode.MovedPermanently,
                    HttpStatusCode.MultipleChoices,
                    HttpStatusCode.NoContent,
                    HttpStatusCode.NonAuthoritativeInformation,
                    HttpStatusCode.NotAcceptable,
                    HttpStatusCode.NotFound,
                    HttpStatusCode.NotImplemented,
                    HttpStatusCode.NotModified,
                    HttpStatusCode.OK,
                    HttpStatusCode.PartialContent,
                    HttpStatusCode.PaymentRequired,
                    HttpStatusCode.PreconditionFailed,
                    HttpStatusCode.ProxyAuthenticationRequired,
                    HttpStatusCode.Redirect,
                    HttpStatusCode.RedirectKeepVerb,
                    HttpStatusCode.RedirectMethod,
                    HttpStatusCode.RequestedRangeNotSatisfiable,
                    HttpStatusCode.RequestEntityTooLarge,
                    HttpStatusCode.RequestTimeout,
                    HttpStatusCode.RequestUriTooLong,
                    HttpStatusCode.ResetContent,
                    HttpStatusCode.SeeOther,
                    HttpStatusCode.ServiceUnavailable,
                    HttpStatusCode.SwitchingProtocols,
                    HttpStatusCode.TemporaryRedirect,
                    HttpStatusCode.Unauthorized,
                    HttpStatusCode.UnsupportedMediaType,
                    HttpStatusCode.Unused,
                    HttpStatusCode.UseProxy
                });
            }
        }

        public static TestData<HttpStatusCode> CustomHttpStatusCodes
        {
            get
            {
                return new ValueTypeTestData<HttpStatusCode>(new HttpStatusCode[]
                {
                    (HttpStatusCode)199,
                    (HttpStatusCode)299,
                    (HttpStatusCode)399,
                    (HttpStatusCode)499,
                    (HttpStatusCode)599,
                    (HttpStatusCode)699,
                    (HttpStatusCode)799,
                    (HttpStatusCode)899,
                    (HttpStatusCode)999,
                });
            }
        }

        public static ReadOnlyCollection<TestData> ConvertablePrimitiveValueTypes
        {
            get
            {
                return new ReadOnlyCollection<TestData>(new TestData[]
                {
                    TestData.CharTestData,
                    TestData.IntTestData,
                    TestData.UintTestData,
                    TestData.ShortTestData,
                    TestData.UshortTestData,
                    TestData.LongTestData,
                    TestData.UlongTestData,
                    TestData.ByteTestData,
                    TestData.SByteTestData,
                    TestData.BoolTestData,
                    TestData.DoubleTestData,
                    TestData.FloatTestData,
                    TestData.DecimalTestData,
                    TestData.GuidTestData,
                    TestData.DateTimeTestData,
                    TestData.DateTimeOffsetTestData
                });
            }
        }

        public static ReadOnlyCollection<TestData> ConvertableEnumTypes
        {
            get
            {
                return new ReadOnlyCollection<TestData>(new TestData[]
                {
                    TestData.SimpleEnumTestData,
                    TestData.LongEnumTestData,
                    TestData.FlagsEnumTestData,
                });
            }
        }

        public static ReadOnlyCollection<TestData> ConvertableValueTypes
        {
            get
            {
                return new ReadOnlyCollection<TestData>(
                    ConvertablePrimitiveValueTypes.Concat(ConvertableEnumTypes).ToList());
            }
        }

        public static TestData<MediaTypeHeaderValue> StandardBsonMediaTypes
        {
            get
            {
                return new RefTypeTestData<MediaTypeHeaderValue>(() => new List<MediaTypeHeaderValue>()
                {
                    new MediaTypeHeaderValue("application/bson"),
                });
            }
        }

        public static TestData<MediaTypeHeaderValue> StandardJsonMediaTypes
        {
            get
            {
                return new RefTypeTestData<MediaTypeHeaderValue>(() => new List<MediaTypeHeaderValue>()
                {
                    new MediaTypeHeaderValue("application/json"),
                    new MediaTypeHeaderValue("text/json")
                });
            }
        }

        public static TestData<MediaTypeHeaderValue> StandardXmlMediaTypes
        {
            get
            {
                return new RefTypeTestData<MediaTypeHeaderValue>(() => new List<MediaTypeHeaderValue>()
                {
                    new MediaTypeHeaderValue("application/xml"),
                    new MediaTypeHeaderValue("text/xml")
                });
            }
        }

        public static TestData<MediaTypeHeaderValue> StandardODataMediaTypes
        {
            get
            {
                return new RefTypeTestData<MediaTypeHeaderValue>(() => new List<MediaTypeHeaderValue>()
                {
                    new MediaTypeHeaderValue("application/atom+xml"),
                    new MediaTypeHeaderValue("application/json"),
                });
            }
        }

        public static TestData<MediaTypeHeaderValue> StandardFormUrlEncodedMediaTypes
        {
            get
            {
                return new RefTypeTestData<MediaTypeHeaderValue>(() => new List<MediaTypeHeaderValue>()
                {
                    new MediaTypeHeaderValue("application/x-www-form-urlencoded")
                });
            }
        }

        public static TestData<string> StandardJsonMediaTypeStrings
        {
            get
            {
                return new RefTypeTestData<string>(() => new List<string>()
                {
                    "application/json",
                    "text/json"
                });
            }
        }

        public static TestData<string> StandardXmlMediaTypeStrings
        {
            get
            {
                return new RefTypeTestData<string>(() => new List<string>()
                {
                    "application/xml",
                    "text/xml"
                });
            }
        }

        public static TestData<string> LegalMediaTypeStrings
        {
            get
            {
                return new RefTypeTestData<string>(() =>
                    StandardXmlMediaTypeStrings.Concat(StandardJsonMediaTypeStrings).ToList());
            }
        }


        // Illegal media type strings.  These will cause the MediaTypeHeaderValue ctor to throw FormatException
        public static TestData<string> IllegalMediaTypeStrings
        {
            get
            {
                return new RefTypeTestData<string>(() => new List<string>()
                {
                    "\0",
                    "9\r\n"
                });
            }
        }

        public static TestData<Encoding> StandardEncodings
        {
            get
            {
                return new RefTypeTestData<Encoding>(() => new List<Encoding>()
                {
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
                    new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true),
                });
            }
        }

        public static TheoryData ReadAndWriteCorrectCharacterEncoding
        {
            get
            {
                return new TheoryData<string, string, bool>
                {
                    { "This is a test 激光這兩個字是甚麼意思 string written using utf-8", "utf-8", true },
                    { "This is a test 激光這兩個字是甚麼意思 string written using utf-16", "utf-16", true },
                    { "This is a test 激光這兩個字是甚麼意思 string written using utf-32", "utf-32", false },
#if !NETCOREAPP2_0 // shift_jis and iso-2022-kr are not supported when running on .NET Core 2.0.
                    { "This is a test 激光這兩個字是甚麼意思 string written using shift_jis", "shift_jis", false },
#endif
                    { "This is a test æøå string written using iso-8859-1", "iso-8859-1", false },
#if !NETCOREAPP2_0
                    { "This is a test 레이저 단어 뜻 string written using iso-2022-kr", "iso-2022-kr", false },
#endif
                };
            }
        }

        //// TODO: complete this list
        // Legal MediaTypeHeaderValues
        public static TestData<MediaTypeHeaderValue> LegalMediaTypeHeaderValues
        {
            get
            {
                return new RefTypeTestData<MediaTypeHeaderValue>(
                    () => LegalMediaTypeStrings.Select(mediaType => new MediaTypeHeaderValue(mediaType)).ToList());
            }
        }

        public static TestData<MediaTypeWithQualityHeaderValue> StandardMediaTypesWithQuality
        {
            get
            {
                return new RefTypeTestData<MediaTypeWithQualityHeaderValue>(() => new List<MediaTypeWithQualityHeaderValue>()
                {
                    new MediaTypeWithQualityHeaderValue("application/json", .1) { CharSet="utf-8"},
                    new MediaTypeWithQualityHeaderValue("text/json", .2) { CharSet="utf-8"},
                    new MediaTypeWithQualityHeaderValue("application/xml", .3) { CharSet="utf-8"},
                    new MediaTypeWithQualityHeaderValue("text/xml", .4) { CharSet="utf-8"},
                    new MediaTypeWithQualityHeaderValue("application/atom+xml", .5) { CharSet="utf-8"},
                });
            }
        }

        public static TestData<HttpContent> StandardHttpContents
        {
            get
            {
                return new RefTypeTestData<HttpContent>(() => new List<HttpContent>()
                {
                    new ByteArrayContent(new byte[0]),
                    new FormUrlEncodedContent(new KeyValuePair<string, string>[0]),
                    new MultipartContent(),
                    new StringContent(""),
                    new StreamContent(new MemoryStream())
                });
            }
        }

        //// TODO: make this list compose from other data?
        // Collection of legal instances of all standard MediaTypeMapping types
#if !NETFX_CORE // not present in portable library version
        public static TestData<MediaTypeMapping> StandardMediaTypeMappings
        {
            get
            {
                return new RefTypeTestData<MediaTypeMapping>(() => QueryStringMappings.Cast<MediaTypeMapping>().ToList());
            }
        }

        public static TestData<QueryStringMapping> QueryStringMappings
        {
            get
            {
                return new RefTypeTestData<QueryStringMapping>(() => new List<QueryStringMapping>()
                {
                    new QueryStringMapping("format", "json", new MediaTypeHeaderValue("application/json"))
                });
            }
        }
#endif

        public static TestData<string> LegalUriPathExtensions
        {
            get
            {
                return new RefTypeTestData<string>(() => new List<string>()
                {
                    "xml",
                    "json"
                });
            }
        }

        public static TestData<string> LegalQueryStringParameterNames
        {
            get
            {
                return new RefTypeTestData<string>(() => new List<string>()
                {
                    "format",
                    "fmt"
                });
            }
        }

        public static TestData<string> LegalHttpHeaderNames
        {
            get
            {
                return new RefTypeTestData<string>(() => new List<string>()
                {
                    "x-requested-with",
                    "some-random-name"
                });
            }
        }

        public static TestData<string> LegalHttpHeaderValues
        {
            get
            {
                return new RefTypeTestData<string>(() => new List<string>()
                {
                    "1",
                    "XMLHttpRequest",
                    "\"quoted-string\""
                });
            }
        }

        public static TestData<string> LegalQueryStringParameterValues
        {
            get
            {
                return new RefTypeTestData<string>(() => new List<string>()
                {
                    "xml",
                    "json"
                });
            }
        }

        public static TestData<string> LegalMediaRangeStrings
        {
            get
            {
                return new RefTypeTestData<string>(() => new List<string>()
                {
                    "application/*",
                    "text/*"
                });
            }
        }

        public static TestData<MediaTypeHeaderValue> LegalMediaRangeValues
        {
            get
            {
                return new RefTypeTestData<MediaTypeHeaderValue>(() =>
                    LegalMediaRangeStrings.Select(s => new MediaTypeHeaderValue(s)).ToList()
                    );
            }
        }

        public static TestData<MediaTypeWithQualityHeaderValue> MediaRangeValuesWithQuality
        {
            get
            {
                return new RefTypeTestData<MediaTypeWithQualityHeaderValue>(() => new List<MediaTypeWithQualityHeaderValue>()
                {
                    new MediaTypeWithQualityHeaderValue("application/*", .1),
                    new MediaTypeWithQualityHeaderValue("text/*", .2),
                });
            }
        }

        public static TestData<string> IllegalMediaRangeStrings
        {
            get
            {
                return new RefTypeTestData<string>(() => new List<string>()
                {
                    "application/xml",
                    "text/xml"
                });
            }
        }

        public static TestData<MediaTypeHeaderValue> IllegalMediaRangeValues
        {
            get
            {
                return new RefTypeTestData<MediaTypeHeaderValue>(() =>
                    IllegalMediaRangeStrings.Select(s => new MediaTypeHeaderValue(s)).ToList()
                    );
            }
        }

        public static TestData<MediaTypeFormatter> StandardFormatters
        {
            get
            {
                return new RefTypeTestData<MediaTypeFormatter>(() => new List<MediaTypeFormatter>()
                {
                    new XmlMediaTypeFormatter(),
                    new JsonMediaTypeFormatter(),
                    new FormUrlEncodedMediaTypeFormatter()
                });
            }
        }

        public static TestData<Type> StandardFormatterTypes
        {
            get
            {
                return new RefTypeTestData<Type>(() => StandardFormatters.Select(m => m.GetType()));
            }
        }

        public static TestData<MediaTypeFormatter> DerivedFormatters
        {
            get
            {
                return new RefTypeTestData<MediaTypeFormatter>(() => new List<MediaTypeFormatter>()
                {
                    new DerivedJsonMediaTypeFormatter(),
                });
            }
        }

        public static TestData<IEnumerable<MediaTypeFormatter>> AllFormatterCollections
        {
            get
            {
                return new RefTypeTestData<IEnumerable<MediaTypeFormatter>>(() => new List<IEnumerable<MediaTypeFormatter>>()
                {
                    new MediaTypeFormatter[0],
                    StandardFormatters,
                    DerivedFormatters,
                });
            }
        }

        public static TestData<string> LegalHttpAddresses
        {
            get
            {
                return new RefTypeTestData<string>(() => new List<string>()
                {
                    "http://somehost",
                    "https://somehost",
                });
            }
        }

        public static TestData<string> AddressesWithIllegalSchemes
        {
            get
            {
                return new RefTypeTestData<string>(() => new List<string>()
                {
                    "net.tcp://somehost",
                    "file://somehost",
                    "net.pipe://somehost",
                    "mailto:somehost",
                    "ftp://somehost",
                    "news://somehost",
                    "ws://somehost",
                    "abc://somehost"
                });
            }
        }

        /// <summary>
        /// A read-only collection of representative values and reference type test data.
        /// Uses where exhaustive coverage is not required.  It includes null values.
        /// </summary>
        public static ReadOnlyCollection<TestData> RepresentativeValueAndRefTypeTestDataCollection
        {
            get
            {
                return new ReadOnlyCollection<TestData>(new TestData[]
                {
                     TestData.ByteTestData,
                     TestData.IntTestData,
                     TestData.BoolTestData,
                     TestData.SimpleEnumTestData,
                     TestData.StringTestData,
                     TestData.DateTimeTestData,
                     TestData.DateTimeOffsetTestData,
                });
            }
        }

        public static TestData<HttpRequestMessage> NullContentHttpRequestMessages
        {
            get
            {
                return new RefTypeTestData<HttpRequestMessage>(() => new List<HttpRequestMessage>()
                {
                   new HttpRequestMessage() { Content = null },
                });
            }
        }

        public static TestData<string> LegalHttpParameterNames
        {
            get
            {
                return new RefTypeTestData<string>(() => new List<string>()
                {
                    "文",
                    "A",
                    "a",
                    "b",
                    " a",
                    "arg1",
                    "arg2",
                    "1",
                    "@",
                    "!"
                });
            }
        }

        public static TestData<Type> LegalHttpParameterTypes
        {
            get
            {
                return new RefTypeTestData<Type>(() => new List<Type>()
                {
                    typeof(string),
                    typeof(byte[]),
                    typeof(byte[][]),
                    typeof(char),
                    typeof(DateTime),
                    typeof(decimal),
                    typeof(double),
                    typeof(Guid),
                    typeof(Int16),
                    typeof(Int32),
                    typeof(object),
                    typeof(sbyte),
                    typeof(Single),
                    typeof(TimeSpan),
                    typeof(UInt16),
                    typeof(UInt32),
                    typeof(UInt64),
                    typeof(Uri),
                    typeof(Enum),
                    typeof(Collection<object>),
                    typeof(IList<object>),
                    typeof(System.Runtime.Serialization.ISerializable),
                    typeof(System.Data.DataSet),
                    typeof(System.Xml.Serialization.IXmlSerializable),
                    typeof(Nullable),
                    typeof(Nullable<DateTime>),
                    typeof(Stream),
                    typeof(HttpRequestMessage),
                    typeof(HttpResponseMessage),
                    typeof(ObjectContent),
                    typeof(ObjectContent<object>),
                    typeof(HttpContent),
                    typeof(Delegate),
                    typeof(Action),
                    typeof(System.Threading.Tasks.Task),
                });
            }
        }

        /// <summary>
        ///  Common <see cref="TestData"/> for the string form of a <see cref="Uri"/>.
        /// </summary>
        public static RefTypeTestData<string> UriTestDataStrings
        {
            get
            {
                return new RefTypeTestData<string>(() => new List<string>()
                {
                    "http://somehost",
                    "http://somehost:8080",
                    "http://somehost/",
                    "http://somehost:8080/",
                    "http://somehost/somepath",
                    "http://somehost/somepath/",
                    "http://somehost/somepath?somequery=somevalue"
                });
            }
        }

        /// <summary>
        ///  Common <see cref="TestData"/> for a <see cref="Uri"/>.
        /// </summary>
        public static RefTypeTestData<Uri> UriTestData
        {
            get
            {
                return new RefTypeTestData<Uri>(() => UriTestDataStrings.Select(s => new Uri(s)).ToList());
            }
        }
    }
}

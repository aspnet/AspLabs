// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Internal; // For DefaultApplicationModelProvider
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.WebHooks.ApplicationModels
{
    public class WebHookActionModelPropertyProviderTest
    {
        public static TheoryData<IWebHookMetadata[], Type> DuplicateMetadataData
        {
            get
            {
                var webHookBindingMetadata = new Mock<IWebHookBindingMetadata>(MockBehavior.Strict);
                webHookBindingMetadata
                    .SetupGet(m => m.ReceiverName)
                    .Returns("some name");
                var webHookBodyTypeMetadataService1 = new Mock<IWebHookBodyTypeMetadataService>(MockBehavior.Strict);
                webHookBodyTypeMetadataService1
                    .SetupGet(m => m.ReceiverName)
                    .Returns("some name");
                var webHookBodyTypeMetadataService2 = new Mock<IWebHookBodyTypeMetadataService>(MockBehavior.Strict);
                webHookBodyTypeMetadataService2
                    .SetupGet(m => m.ReceiverName)
                    .Returns("some name");
                var webHookEventFromBodyMetadata1 = new Mock<IWebHookEventFromBodyMetadata>(MockBehavior.Strict);
                webHookEventFromBodyMetadata1
                    .SetupGet(m => m.ReceiverName)
                    .Returns("some name");
                var webHookEventFromBodyMetadata2 = new Mock<IWebHookEventFromBodyMetadata>(MockBehavior.Strict);
                webHookEventFromBodyMetadata2
                    .SetupGet(m => m.ReceiverName)
                    .Returns("unique name");
                var webHookEventFromBodyMetadata3 = new Mock<IWebHookEventFromBodyMetadata>(MockBehavior.Strict);
                webHookEventFromBodyMetadata3
                    .SetupGet(m => m.ReceiverName)
                    .Returns("some name");
                var webHookEventMetadata = new Mock<IWebHookEventMetadata>(MockBehavior.Strict);
                webHookEventMetadata
                    .SetupGet(m => m.ReceiverName)
                    .Returns("some name");
                var webHookGetHeadRequestMetadata1 = new Mock<IWebHookGetHeadRequestMetadata>(MockBehavior.Strict);
                webHookGetHeadRequestMetadata1
                    .SetupGet(m => m.ReceiverName)
                    .Returns("some name");
                var webHookGetHeadRequestMetadata2 = new Mock<IWebHookGetHeadRequestMetadata>(MockBehavior.Strict);
                webHookGetHeadRequestMetadata2
                    .SetupGet(m => m.ReceiverName)
                    .Returns("some name");
                var webHookGetHeadRequestMetadata3 = new Mock<IWebHookGetHeadRequestMetadata>(MockBehavior.Strict);
                webHookGetHeadRequestMetadata3
                    .SetupGet(m => m.ReceiverName)
                    .Returns("some name");
                var webHookPingRequestMetadata = new Mock<IWebHookPingRequestMetadata>(MockBehavior.Strict);
                webHookPingRequestMetadata
                    .SetupGet(m => m.ReceiverName)
                    .Returns("some name");
                var webHookVerifyCodeMetadata = new Mock<IWebHookVerifyCodeMetadata>(MockBehavior.Strict);
                webHookVerifyCodeMetadata
                    .SetupGet(m => m.ReceiverName)
                    .Returns("some name");

                return new TheoryData<IWebHookMetadata[], Type>
                {
                    {
                        new IWebHookMetadata[]
                        {
                            // One instance, twice
                            webHookBindingMetadata.Object,
                            webHookBindingMetadata.Object,
                        },
                        typeof(IWebHookBindingMetadata)
                    },
                    {
                        new IWebHookMetadata[]
                        {
                            // Two instances, same name
                            webHookBodyTypeMetadataService1.Object,
                            webHookBodyTypeMetadataService2.Object,
                        },
                        typeof(IWebHookBodyTypeMetadataService)
                    },
                    {
                        new IWebHookMetadata[]
                        {
                            // Three instances, two with same name
                            webHookEventFromBodyMetadata1.Object,
                            webHookEventFromBodyMetadata2.Object,
                            webHookEventFromBodyMetadata3.Object,
                        },
                        typeof(IWebHookEventFromBodyMetadata)
                    },
                    {
                        new IWebHookMetadata[]
                        {
                            // One instance, thrice
                            webHookEventMetadata.Object,
                            webHookEventMetadata.Object,
                            webHookEventMetadata.Object,
                        },
                        typeof(IWebHookEventMetadata)
                    },
                    {
                        new IWebHookMetadata[]
                        {
                            // Three instances, all with same name
                            webHookGetHeadRequestMetadata1.Object,
                            webHookGetHeadRequestMetadata2.Object,
                            webHookGetHeadRequestMetadata3.Object,
                        },
                        typeof(IWebHookGetHeadRequestMetadata)
                    },
                    {
                        new IWebHookMetadata[]
                        {
                            // One instance, twice
                            webHookPingRequestMetadata.Object,
                            webHookPingRequestMetadata.Object,
                        },
                        typeof(IWebHookPingRequestMetadata)
                    },
                    {
                        new IWebHookMetadata[]
                        {
                            // One instance, twice
                            webHookVerifyCodeMetadata.Object,
                            webHookVerifyCodeMetadata.Object,
                        },
                        typeof(IWebHookVerifyCodeMetadata)
                    }
                };
            }
        }

        private static IWebHookMetadata[] ValidMetadata
        {
            get
            {
                var webHookBindingMetadata = new Mock<IWebHookBindingMetadata>(MockBehavior.Strict);
                webHookBindingMetadata
                    .SetupGet(m => m.ReceiverName)
                    .Returns("some name");
                webHookBindingMetadata
                    .Setup(m => m.IsApplicable(It.IsAny<string>()))
                    .Returns(false);
                webHookBindingMetadata
                    .Setup(m => m.IsApplicable("some name"))
                    .Returns(true);
                var webHookBodyTypeMetadataService1 = new Mock<IWebHookBodyTypeMetadataService>(MockBehavior.Strict);
                webHookBodyTypeMetadataService1
                    .SetupGet(m => m.BodyType)
                    .Returns(WebHookBodyType.Json);
                webHookBodyTypeMetadataService1
                    .SetupGet(m => m.ReceiverName)
                    .Returns("some name");
                webHookBodyTypeMetadataService1
                    .Setup(m => m.IsApplicable(It.IsAny<string>()))
                    .Returns(false);
                webHookBodyTypeMetadataService1
                    .Setup(m => m.IsApplicable("some name"))
                    .Returns(true);
                var webHookBodyTypeMetadataService2 = new Mock<IWebHookBodyTypeMetadataService>(MockBehavior.Strict);
                webHookBodyTypeMetadataService2
                    .SetupGet(m => m.BodyType)
                    .Returns(WebHookBodyType.Form);
                webHookBodyTypeMetadataService2
                    .SetupGet(m => m.ReceiverName)
                    .Returns("unique name1");
                webHookBodyTypeMetadataService2
                    .Setup(m => m.IsApplicable(It.IsAny<string>()))
                    .Returns(false);
                webHookBodyTypeMetadataService2
                    .Setup(m => m.IsApplicable("unique name1"))
                    .Returns(true);
                var webHookBodyTypeMetadataService3 = new Mock<IWebHookBodyTypeMetadataService>(MockBehavior.Strict);
                webHookBodyTypeMetadataService3
                    .SetupGet(m => m.BodyType)
                    .Returns(WebHookBodyType.Xml);
                webHookBodyTypeMetadataService3
                    .SetupGet(m => m.ReceiverName)
                    .Returns("unique name2");
                webHookBodyTypeMetadataService3
                    .Setup(m => m.IsApplicable(It.IsAny<string>()))
                    .Returns(false);
                webHookBodyTypeMetadataService3
                    .Setup(m => m.IsApplicable("unique name2"))
                    .Returns(true);
                var webHookBodyTypeMetadataService4 = new Mock<IWebHookBodyTypeMetadataService>(MockBehavior.Strict);
                webHookBodyTypeMetadataService4
                    .SetupGet(m => m.BodyType)
                    .Returns(WebHookBodyType.Json);
                webHookBodyTypeMetadataService4
                    .SetupGet(m => m.ReceiverName)
                    .Returns("unique name3");
                webHookBodyTypeMetadataService4
                    .Setup(m => m.IsApplicable(It.IsAny<string>()))
                    .Returns(false);
                webHookBodyTypeMetadataService4
                    .Setup(m => m.IsApplicable("unique name3"))
                    .Returns(true);
                var webHookEventFromBodyMetadata1 = new Mock<IWebHookEventFromBodyMetadata>(MockBehavior.Strict);
                webHookEventFromBodyMetadata1
                    .SetupGet(m => m.ReceiverName)
                    .Returns("unique name1");
                webHookEventFromBodyMetadata1
                    .Setup(m => m.IsApplicable(It.IsAny<string>()))
                    .Returns(false);
                webHookEventFromBodyMetadata1
                    .Setup(m => m.IsApplicable("unique name1"))
                    .Returns(true);
                var webHookEventFromBodyMetadata2 = new Mock<IWebHookEventFromBodyMetadata>(MockBehavior.Strict);
                webHookEventFromBodyMetadata2
                    .SetupGet(m => m.ReceiverName)
                    .Returns("unique name2");
                webHookEventFromBodyMetadata2
                    .Setup(m => m.IsApplicable(It.IsAny<string>()))
                    .Returns(false);
                webHookEventFromBodyMetadata2
                    .Setup(m => m.IsApplicable("unique name2"))
                    .Returns(true);
                var webHookEventFromBodyMetadata3 = new Mock<IWebHookEventFromBodyMetadata>(MockBehavior.Strict);
                webHookEventFromBodyMetadata3
                    .SetupGet(m => m.ReceiverName)
                    .Returns("unique name3");
                webHookEventFromBodyMetadata3
                    .Setup(m => m.IsApplicable(It.IsAny<string>()))
                    .Returns(false);
                webHookEventFromBodyMetadata3
                    .Setup(m => m.IsApplicable("unique name3"))
                    .Returns(true);
                var webHookEventMetadata = new Mock<IWebHookEventMetadata>(MockBehavior.Strict);
                webHookEventMetadata
                    .SetupGet(m => m.ReceiverName)
                    .Returns("some name");
                webHookEventMetadata
                    .Setup(m => m.IsApplicable(It.IsAny<string>()))
                    .Returns(false);
                webHookEventMetadata
                    .Setup(m => m.IsApplicable("some name"))
                    .Returns(true);
                var webHookGetHeadRequestMetadata1 = new Mock<IWebHookGetHeadRequestMetadata>(MockBehavior.Strict);
                webHookGetHeadRequestMetadata1
                    .SetupGet(m => m.ReceiverName)
                    .Returns("unique name1");
                webHookGetHeadRequestMetadata1
                    .Setup(m => m.IsApplicable(It.IsAny<string>()))
                    .Returns(false);
                webHookGetHeadRequestMetadata1
                    .Setup(m => m.IsApplicable("unique name1"))
                    .Returns(true);
                var webHookGetHeadRequestMetadata2 = new Mock<IWebHookGetHeadRequestMetadata>(MockBehavior.Strict);
                webHookGetHeadRequestMetadata2
                    .SetupGet(m => m.ReceiverName)
                    .Returns("unique name2");
                webHookGetHeadRequestMetadata2
                    .Setup(m => m.IsApplicable(It.IsAny<string>()))
                    .Returns(false);
                webHookGetHeadRequestMetadata2
                    .Setup(m => m.IsApplicable("unique name2"))
                    .Returns(true);
                var webHookGetHeadRequestMetadata3 = new Mock<IWebHookGetHeadRequestMetadata>(MockBehavior.Strict);
                webHookGetHeadRequestMetadata3
                    .SetupGet(m => m.ReceiverName)
                    .Returns("some name");
                webHookGetHeadRequestMetadata3
                    .Setup(m => m.IsApplicable(It.IsAny<string>()))
                    .Returns(false);
                webHookGetHeadRequestMetadata3
                    .Setup(m => m.IsApplicable("some name"))
                    .Returns(true);
                var webHookPingRequestMetadata = new Mock<IWebHookPingRequestMetadata>(MockBehavior.Strict);
                webHookPingRequestMetadata
                    .SetupGet(m => m.ReceiverName)
                    .Returns("some name");
                webHookPingRequestMetadata
                    .Setup(m => m.IsApplicable(It.IsAny<string>()))
                    .Returns(false);
                webHookPingRequestMetadata
                    .Setup(m => m.IsApplicable("some name"))
                    .Returns(true);
                var webHookVerifyCodeMetadata = new Mock<IWebHookVerifyCodeMetadata>(MockBehavior.Strict);
                webHookVerifyCodeMetadata
                    .SetupGet(m => m.ReceiverName)
                    .Returns("some name");
                webHookVerifyCodeMetadata
                    .Setup(m => m.IsApplicable(It.IsAny<string>()))
                    .Returns(false);
                webHookVerifyCodeMetadata
                    .Setup(m => m.IsApplicable("some name"))
                    .Returns(true);

                return new IWebHookMetadata[]
                {
                    webHookBindingMetadata.Object,
                    webHookBodyTypeMetadataService1.Object,
                    webHookBodyTypeMetadataService2.Object,
                    webHookBodyTypeMetadataService3.Object,
                    webHookBodyTypeMetadataService4.Object,
                    webHookEventFromBodyMetadata1.Object,
                    webHookEventFromBodyMetadata2.Object,
                    webHookEventFromBodyMetadata3.Object,
                    webHookEventMetadata.Object,
                    webHookGetHeadRequestMetadata1.Object,
                    webHookGetHeadRequestMetadata2.Object,
                    webHookGetHeadRequestMetadata3.Object,
                    webHookPingRequestMetadata.Object,
                    webHookVerifyCodeMetadata.Object,
                };
            }
        }

        [Fact]
        public void Constructor_SucceedsWithEmptyMetadata()
        {
            // Arrange
            var metadata = Array.Empty<IWebHookMetadata>();

            // Act & Assert (does not throw)
            new TestActionModelPropertyProvider(metadata);
        }

        [Fact]
        public void Constructor_SucceedsWithValidMetadata()
        {
            // Arrange, Act & Assert (does not throw)
            new TestActionModelPropertyProvider(ValidMetadata);
        }

        [Theory]
        [MemberData(nameof(DuplicateMetadataData))]
        public void Constructor_ThrowsWithDuplicateMetadata(IEnumerable<IWebHookMetadata> metadata, Type metadataType)
        {
            // Arrange
            var expectedMessage = "Invalid metadata services found for the 'some name' WebHook receiver. " +
                $"Receivers must not have more than one '{metadataType}' registration.";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => new TestActionModelPropertyProvider(metadata));
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void Constructor_ThrowsWithInvalidBodyTypeMetadataService()
        {
            // Arrange
            var expectedMessage = $"Enum type '{typeof(WebHookBodyType)}' has no defined '0' value.";

            var webHookBodyTypeMetadataService1 = new Mock<IWebHookBodyTypeMetadataService>(MockBehavior.Strict);
            webHookBodyTypeMetadataService1
                .SetupGet(m => m.BodyType)
                .Returns(WebHookBodyType.Json); // valid
            webHookBodyTypeMetadataService1
                .SetupGet(m => m.ReceiverName)
                .Returns("unique name1");
            var webHookBodyTypeMetadataService2 = new Mock<IWebHookBodyTypeMetadataService>(MockBehavior.Strict);
            webHookBodyTypeMetadataService2
                .SetupGet(m => m.BodyType)
                .Returns(0);
            webHookBodyTypeMetadataService2
                .SetupGet(m => m.ReceiverName)
                .Returns("unique name2");
            var metadata = new IWebHookMetadata[]
            {
                webHookBodyTypeMetadataService1.Object,
                webHookBodyTypeMetadataService2.Object,
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => new TestActionModelPropertyProvider(metadata));
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void Constructor_ThrowsWithEventAndEventFromBodyMetadata()
        {
            // Arrange
            var expectedMessage = "Invalid metadata services found for the 'some name' WebHook receiver. Receivers " +
                $"must not provide both '{typeof(IWebHookEventFromBodyMetadata)}' and " +
                $"'{typeof(IWebHookEventMetadata)}' services.";

            var webHookEventFromBodyMetadata = new Mock<IWebHookEventFromBodyMetadata>(MockBehavior.Strict);
            webHookEventFromBodyMetadata
                .SetupGet(m => m.ReceiverName)
                .Returns("some name");
            var webHookEventMetadata = new Mock<IWebHookEventMetadata>(MockBehavior.Strict);
            webHookEventMetadata
                .SetupGet(m => m.ReceiverName)
                .Returns("some name");
            webHookEventMetadata
                .Setup(m => m.IsApplicable("some name"))
                .Returns(true);
            var metadata = new IWebHookMetadata[]
            {
                webHookEventFromBodyMetadata.Object,
                webHookEventMetadata.Object,
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => new TestActionModelPropertyProvider(metadata));
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void OnProvidersExecuting_SucceedsWithGeneralAttributeAndValidMetadata()
        {
            // Arrange
            var provider = new TestActionModelPropertyProvider(ValidMetadata);
            var context = GetApplicatonModelProviderContext(typeof(GeneralController));

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            var controllerModel = Assert.Single(context.Result.Controllers);
            var actionModel = Assert.Single(controllerModel.Actions);
            Assert.Collection(actionModel.Properties.OrderBy(kvp => ((Type)kvp.Key).Name),
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookBindingMetadata), kvp.Key);
                    Assert.IsAssignableFrom<IReadOnlyList<IWebHookBindingMetadata>>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookBodyTypeMetadataService), kvp.Key);
                    Assert.IsAssignableFrom<IReadOnlyList<IWebHookBodyTypeMetadataService>>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookEventFromBodyMetadata), kvp.Key);
                    Assert.IsAssignableFrom<IReadOnlyList<IWebHookEventFromBodyMetadata>>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookEventMetadata), kvp.Key);
                    Assert.IsAssignableFrom<IReadOnlyList<IWebHookEventMetadata>>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookEventSelectorMetadata), kvp.Key);
                    Assert.IsAssignableFrom<GeneralWebHookAttribute>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookGetHeadRequestMetadata), kvp.Key);
                    Assert.IsAssignableFrom<IReadOnlyList<IWebHookGetHeadRequestMetadata>>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookPingRequestMetadata), kvp.Key);
                    Assert.IsAssignableFrom<IReadOnlyList<IWebHookPingRequestMetadata>>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookVerifyCodeMetadata), kvp.Key);
                    Assert.IsAssignableFrom<IReadOnlyList<IWebHookVerifyCodeMetadata>>(kvp.Value);
                });
        }

        [Fact]
        public void OnProvidersExecuting_SucceedsWithGeneralAttributeAndValidMetadata_IncludingBodyType()
        {
            // Arrange
            var provider = new TestActionModelPropertyProvider(ValidMetadata);
            var context = GetApplicatonModelProviderContext(typeof(JsonGeneralController));

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            var controllerModel = Assert.Single(context.Result.Controllers);
            var actionModel = Assert.Single(controllerModel.Actions);
            Assert.Collection(actionModel.Properties.OrderBy(kvp => ((Type)kvp.Key).Name),
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookBindingMetadata), kvp.Key);
                    var attribute = Assert.IsAssignableFrom<IReadOnlyList<IWebHookBindingMetadata>>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookBodyTypeMetadata), kvp.Key);
                    var attribute = Assert.IsAssignableFrom<GeneralWebHookAttribute>(kvp.Value);
                    Assert.Equal(WebHookBodyType.Json, attribute.BodyType);
                },
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookBodyTypeMetadataService), kvp.Key);
                    Assert.IsAssignableFrom<IReadOnlyList<IWebHookBodyTypeMetadataService>>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookEventFromBodyMetadata), kvp.Key);
                    Assert.IsAssignableFrom<IReadOnlyList<IWebHookEventFromBodyMetadata>>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookEventMetadata), kvp.Key);
                    Assert.IsAssignableFrom<IReadOnlyList<IWebHookEventMetadata>>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookEventSelectorMetadata), kvp.Key);
                    Assert.IsAssignableFrom<GeneralWebHookAttribute>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookGetHeadRequestMetadata), kvp.Key);
                    Assert.IsAssignableFrom<IReadOnlyList<IWebHookGetHeadRequestMetadata>>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookPingRequestMetadata), kvp.Key);
                    Assert.IsAssignableFrom<IReadOnlyList<IWebHookPingRequestMetadata>>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookVerifyCodeMetadata), kvp.Key);
                    Assert.IsAssignableFrom<IReadOnlyList<IWebHookVerifyCodeMetadata>>(kvp.Value);
                });
        }

        [Fact]
        public void OnProvidersExecuting_SucceedsWithValidAttributesAndMetadata()
        {
            // Arrange
            var provider = new TestActionModelPropertyProvider(ValidMetadata);
            var context = GetApplicatonModelProviderContext(typeof(SomeController));

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            var controllerModel = Assert.Single(context.Result.Controllers);
            var actionModel = Assert.Single(controllerModel.Actions);
            Assert.Collection(actionModel.Properties.OrderBy(kvp => ((Type)kvp.Key).Name),
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookBindingMetadata), kvp.Key);
                    Assert.IsAssignableFrom<IWebHookBindingMetadata>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookBodyTypeMetadataService), kvp.Key);
                    Assert.IsAssignableFrom<IWebHookBodyTypeMetadataService>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookEventMetadata), kvp.Key);
                    Assert.IsAssignableFrom<IWebHookEventMetadata>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookEventSelectorMetadata), kvp.Key);
                    Assert.IsAssignableFrom<SomeWebHookAttribute>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookGetHeadRequestMetadata), kvp.Key);
                    Assert.IsAssignableFrom<IWebHookGetHeadRequestMetadata>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookPingRequestMetadata), kvp.Key);
                    Assert.IsAssignableFrom<IWebHookPingRequestMetadata>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(typeof(IWebHookVerifyCodeMetadata), kvp.Key);
                    Assert.IsAssignableFrom<IWebHookVerifyCodeMetadata>(kvp.Value);
                });
        }

        private static ApplicationModelProviderContext GetApplicatonModelProviderContext(Type controllerType)
        {
            var context = new ApplicationModelProviderContext(new[] { controllerType.GetTypeInfo() });
            var defaultProvider = new DefaultApplicationModelProvider(
                Options.Create(new MvcOptions()),
                new EmptyModelMetadataProvider());

            defaultProvider.OnProvidersExecuting(context);
            return context;
        }

        private class TestActionModelPropertyProvider : WebHookActionModelPropertyProvider
        {
            public TestActionModelPropertyProvider(IEnumerable<IWebHookMetadata> metadata)
                : base(
                    metadata.OfType<IWebHookBindingMetadata>(),
                    metadata.OfType<IWebHookBodyTypeMetadataService>(),
                    metadata.OfType<IWebHookEventFromBodyMetadata>(),
                    metadata.OfType<IWebHookEventMetadata>(),
                    metadata.OfType<IWebHookFilterMetadata>(),
                    metadata.OfType<IWebHookGetHeadRequestMetadata>(),
                    metadata.OfType<IWebHookPingRequestMetadata>(),
                    metadata.OfType<IWebHookVerifyCodeMetadata>())
            {
            }
        }

        private class SomeWebHookAttribute : WebHookAttribute, IWebHookEventSelectorMetadata
        {
            public SomeWebHookAttribute()
                : base("some name")
            {
            }

            public string EventName => "non-null";
        }

        private class GeneralController : ControllerBase
        {
            [GeneralWebHook(EventName = "non-null")]
            public IActionResult MyAction()
            {
                throw new NotImplementedException();
            }
        }

        private class JsonGeneralController : ControllerBase
        {
            [GeneralWebHook(WebHookBodyType.Json, EventName = "non-null")]
            public IActionResult MyAction()
            {
                throw new NotImplementedException();
            }
        }

        private class SomeController : ControllerBase
        {
            [SomeWebHook]
            public IActionResult MyAction()
            {
                throw new NotImplementedException();
            }
        }
    }
}

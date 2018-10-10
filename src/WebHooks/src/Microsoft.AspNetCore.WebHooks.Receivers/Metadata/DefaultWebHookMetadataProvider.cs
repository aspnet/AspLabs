// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    internal class DefaultWebHookMetadataProvider : WebHookMetadataProvider
    {
        private readonly IWebHookBindingMetadata[] _bindingMetadata;
        private readonly IWebHookBodyTypeMetadataService[] _bodyTypeMetadata;
        private readonly IWebHookEventFromBodyMetadata[] _eventFromBodyMetadata;
        private readonly IWebHookEventMetadata[] _eventMetadata;
        private readonly IWebHookFilterMetadata[] _filterMetadata;
        private readonly IWebHookGetHeadRequestMetadata[] _getHeadRequestMetadata;
        private readonly IWebHookPingRequestMetadata[] _pingRequestMetadata;
        private readonly IWebHookVerifyCodeMetadata[] _verifyCodeMetadata;

        public DefaultWebHookMetadataProvider(
            IEnumerable<IWebHookBindingMetadata> bindingMetadata,
            IEnumerable<IWebHookBodyTypeMetadataService> bodyTypeMetadata,
            IEnumerable<IWebHookEventFromBodyMetadata> eventFromBodyMetadata,
            IEnumerable<IWebHookEventMetadata> eventMetadata,
            IEnumerable<IWebHookFilterMetadata> filterMetadata,
            IEnumerable<IWebHookGetHeadRequestMetadata> getHeadRequestMetadata,
            IEnumerable<IWebHookPingRequestMetadata> pingRequestMetadata,
            IEnumerable<IWebHookVerifyCodeMetadata> verifyCodeMetadata)
        {
            _bindingMetadata = bindingMetadata.ToArray();
            _bodyTypeMetadata = bodyTypeMetadata.ToArray();
            _eventFromBodyMetadata = eventFromBodyMetadata.ToArray();
            _eventMetadata = eventMetadata.ToArray();
            _filterMetadata = filterMetadata.ToArray();
            _getHeadRequestMetadata = getHeadRequestMetadata.ToArray();
            _pingRequestMetadata = pingRequestMetadata.ToArray();
            _verifyCodeMetadata = verifyCodeMetadata.ToArray();
        }

        public override IWebHookBindingMetadata GetBindingMetadata(string receiverName)
        {
            for (var i = 0; i < _bindingMetadata.Length; i++)
            {
                var metadata = _bindingMetadata[i];
                if (metadata.IsApplicable(receiverName))
                {
                    return metadata;
                }
            }

            return null;
        }

        public override IWebHookBodyTypeMetadataService GetBodyTypeMetadata(string receiverName)
        {
            for (var i = 0; i < _bodyTypeMetadata.Length; i++)
            {
                var metadata = _bodyTypeMetadata[i];
                if (metadata.IsApplicable(receiverName))
                {
                    return metadata;
                }
            }

            return null;
        }

        public override IWebHookEventFromBodyMetadata GetEventFromBodyMetadata(string receiverName)
        {
            for (var i = 0; i < _eventFromBodyMetadata.Length; i++)
            {
                var metadata = _eventFromBodyMetadata[i];
                if (metadata.IsApplicable(receiverName))
                {
                    return metadata;
                }
            }

            return null;
        }

        public override IWebHookEventMetadata GetEventMetadata(string receiverName)
        {
            for (var i = 0; i < _eventMetadata.Length; i++)
            {
                var metadata = _eventMetadata[i];
                if (metadata.IsApplicable(receiverName))
                {
                    return metadata;
                }
            }

            return null;
        }

        public override IWebHookFilterMetadata GetFilterMetadata(string receiverName)
        {
            for (var i = 0; i < _filterMetadata.Length; i++)
            {
                var metadata = _filterMetadata[i];
                if (metadata.IsApplicable(receiverName))
                {
                    return metadata;
                }
            }

            return null;
        }

        public override IWebHookGetHeadRequestMetadata GetGetHeadRequestMetadata(string receiverName)
        {
            for (var i = 0; i < _getHeadRequestMetadata.Length; i++)
            {
                var metadata = _getHeadRequestMetadata[i];
                if (metadata.IsApplicable(receiverName))
                {
                    return metadata;
                }
            }

            return null;
        }

        public override IWebHookPingRequestMetadata GetPingRequestMetadata(string receiverName)
        {
            for (var i = 0; i < _pingRequestMetadata.Length; i++)
            {
                var metadata = _pingRequestMetadata[i];
                if (metadata.IsApplicable(receiverName))
                {
                    return metadata;
                }
            }

            return null;
        }

        public override IWebHookVerifyCodeMetadata GetVerifyCodeMetadata(string receiverName)
        {
            for (var i = 0; i < _verifyCodeMetadata.Length; i++)
            {
                var metadata = _verifyCodeMetadata[i];
                if (metadata.IsApplicable(receiverName))
                {
                    return metadata;
                }
            }

            return null;
        }
    }
}

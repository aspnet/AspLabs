// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Text;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// This version of <see cref="NameValueCollection"/> creates the output supported by <see cref="SlackCommand.ParseActionWithParameters"/>
    /// </summary>
    internal class ParameterCollection : NameValueCollection
    {
        /// <inheritdoc />
        public override string ToString()
        {
            bool first = true;
            StringBuilder output = new StringBuilder();
            foreach (string key in this.AllKeys)
            {
                output.AppendFormat("{0}{1}={2}", first ? string.Empty : "; ", key, this[key]);
                first = false;
            }
            return output.ToString();
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.WebHooks
{
    internal static class EmbeddedResource
    {
        public static JObject ReadAsJObject(string name)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream content = asm.GetManifestResourceStream(name);
            using (StreamReader reader = new StreamReader(content))
            {
                string data = reader.ReadToEnd();
                return JObject.Parse(data);
            }
        }
    }
}

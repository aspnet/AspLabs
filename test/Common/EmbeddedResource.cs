// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.WebHooks
{
    internal static class EmbeddedResource
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Not called in all contexts.")]
        public static string ReadAsString(string name)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream content = asm.GetManifestResourceStream(name);
            using (StreamReader reader = new StreamReader(content))
            {
                string data = reader.ReadToEnd();
                return data;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Not called in all contexts.")]
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

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Not called in all contexts.")]
        public static JArray ReadAsJArray(string name)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream content = asm.GetManifestResourceStream(name);
            using (StreamReader reader = new StreamReader(content))
            {
                string data = reader.ReadToEnd();
                return JArray.Parse(data);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Not called in all contexts.")]
        public static XElement ReadAsJXElement(string name)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream content = asm.GetManifestResourceStream(name);
            using (StreamReader reader = new StreamReader(content))
            {
                string data = reader.ReadToEnd();
                return XElement.Parse(data);
            }
        }
    }
}

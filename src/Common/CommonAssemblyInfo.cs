// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

#if !BUILD_GENERATED_VERSION
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyCopyright("© Microsoft Corporation. All rights reserved.")]
#endif
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
#if !NOT_CLS_COMPLIANT
[assembly: CLSCompliant(true)]
#endif
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: AssemblyMetadata("Serviceable", "True")]

//// ===========================================================================
////  DO NOT EDIT OR REMOVE ANYTHING BELOW THIS COMMENT.
////  Version numbers are automatically generated based on regular expressions.
//// ===========================================================================

#if ASPNETWEBHOOKS
#if !BUILD_GENERATED_VERSION
[assembly: AssemblyVersion("1.1.0.0")]
[assembly: AssemblyFileVersion("1.1.0.0")]
#endif
[assembly: AssemblyProduct("Microsoft ASP.NET WebHooks")]
#else
#error Runtime projects must define ASPNETWEBHOOKS
#endif

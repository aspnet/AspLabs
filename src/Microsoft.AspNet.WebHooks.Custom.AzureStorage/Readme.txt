Microsoft ASP.NET Custom WebHooks Microsoft Azure Storage
---------------------------------------------------------

This package provides support for persisting your custom WebHook registrations in Microsoft Azure Table Storage.
To set up Table Storage, you must first configure a connection string with the name 'MS_AzureStoreConnectionString'.

For test and development, you can use the local Azure Storage Emulator, which comes as part of the Microsoft Azure SDK.
To download the latest Microsoft Azure SDK for Visual Studio, please see 'https://azure.microsoft.com/en-us/downloads/'.
A connection string using the local emulator looks like this:

  <add name="MS_AzureStoreConnectionString" connectionString="UseDevelopmentStorage=true;" />

For deployment, you can get a connection string through the Azure Portal at 'https://portal.azure.com'. If you don't
already have an Azure account, then you can get one at 'https://azure.com'.

For more information about Azure Table Storage, please see 'https://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-how-to-use-tables/'.

For more information about Microsoft ASP.NET WebHooks, please see 'https://go.microsoft.com/fwlink/?LinkId=690277'.

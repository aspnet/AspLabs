Microsoft ASP.NET Custom WebHooks SQL Module
--------------------------------------------

This package provides support for persisting your custom WebHook registrations in a SQL database.
To set up the SQL DB you must first configure a connection string with the name 'MS_SqlStoreConnectionString',
for example:

  <add name="MS_SqlStoreConnectionString"
    connectionString="Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=WebHooks-20151029053732;Integrated Security=True"
    providerName="System.Data.SqlClient" />

To create and initialize the DB, use code migrations as follows: Open the 'Package Manager Console' from the
'Tools\Nuget Package Manager' menu and type these three lines one after another:

  Enable-Migrations -ContextAssemblyName Microsoft.AspNet.WebHooks.Custom.SqlStorage
  Add-Migration WebHookStoreInitialDB
  Update-Database

That should create a DB (if it doesn't already exist) and update it to match the latest data model.

For more information about Entity Framework Code Migrations, please see 'https://msdn.microsoft.com/en-us/data/jj591621.aspx'.

For more information about Microsoft ASP.NET WebHooks, please see 'https://go.microsoft.com/fwlink/?LinkId=690277'.

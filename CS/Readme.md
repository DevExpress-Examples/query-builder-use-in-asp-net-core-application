# How to Use the Query Builder Control in an ASP.NET Core Application

This example leverages the ASP.NET Core Query Builder control to create a new report data source and save it for later use in the Web Report Designer.

## Overview

In this example, an authenticated user can use the Query Builder to create a new data source and save it in the database. Then, the user can invoke the Report Wizard and select the data source from the list of available data sources to create a new report. The user can edit a report in the Report Designer and save the report to the database.
The application allows the user to edit and delete the data sources and reports.

## Impementation Details 

### Authentication and Storage
The application requires authentication so that the data sources and reports are available only to the user who created them. The user data, serialized data sources and reports are stored in the EFCore application.db database. 

The application implements custom storage services to save data sources and reports. The report storage service is the ReportStorageService class (the [ReportStorageWebExtension](https://docs.devexpress.com/XtraReports/DevExpress.XtraReports.Web.Extensions.ReportStorageWebExtension) descendant).
The data source storage service is the **DataSourceStorageService** class.

Files to look at: 
- [ReportStorageService.cs](./CS/AspNetCoreQueryBuilderApp/Services/ReportStorageService.cs)
- [DataSourceStorageService.cs](./CS/AspNetCoreQueryBuilderApp/Services/DataSourceStorageService.cs)

### Connection Strings

The reports use data from the external data source - the nwind.db database populated with Northwind data. The application has no direct access to connection srings in the `appsettings.json` file and requests a connection string provider from the CustomConnectionProviderFactory service (the [IConnectionProviderFactory](https://docs.devexpress.com/CoreLibraries/DevExpress.DataAccess.Web.IConnectionProviderFactory) implementation). The connection string provider is the CustomConnectionProvider class that implements the [IConnectionProviderService](https://docs.devexpress.com/CoreLibraries/DevExpress.DataAccess.Wizard.Services.IConnectionProviderService) interface.

File to look at: 
- [CustomConnectionProvider.cs](./CS/AspNetCoreQueryBuilderApp/Services/CustomConnectionProvider.cs)

### Query Builder Control

The ASP.NET Core Query Builder control allows the user to create a query that selects and retrieves data from an external data source, edit the query and save it for later use as the report data source.

To display the Query Builder on a web page, the ASP.NET Core project requires the following modifications:

* Add scripts and styles from the `@devexpress/analytics-core` npm package.
* Add third-party dependencies. The `bundleconfig.json` and `libman.json` configuration files allow the LibMan tool to retrieve and bundle the specified dependencies from the node_modules folder, and place them in the wwwroot application directory.
* The `Startup.cs` file contains the code that registers the DevExpress services and middleware components:
```
public void ConfigureServices(IServiceCollection services) {
	// ...
	services.AddDevExpressControls();
	// ...
}
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory) {
	// ...
	app.UseDevExpressControls();
	// ...
}
```
* The `_ViewImports.cshtml` file contains the `using` directive:
```
@using DevExpress.AspNetCore;
```
* The controller includes the QueryBuilder action that generates a model required to render the Query Builder control. The model parameters are the connection name and (optionally) the [SelectQuery](https://docs.devexpress.com/CoreLibraries/DevExpress.DataAccess.Sql.SelectQuery) object. The model generator is the Query Builder native service that exposes the [IQueryBuilderClientSideModelGenerator](https://docs.devexpress.com/XtraReports/DevExpress.XtraReports.Web.QueryBuilder.Services.IQueryBuilderClientSideModelGenerator) interface. The controller obtains the generator from the built-in ASP.NET Core DI container:
```
using DevExpress.XtraReports.Web.QueryBuilder.Services;
// ...
public IActionResult QueryBuilder(
		[FromServices] IQueryBuilderClientSideModelGenerator queryBuilderClientSideModelGenerator) {
		var queryBuilderModel = queryBuilderClientSideModelGenerator.GetModel(newDataConnectionName);
		}
``` 
* The View (the Home\QueryBuilder.cshtml page) contains the Query Builder bound to the model:

```
@(Html.DevExpress()
    .QueryBuilder("webQueryBuilder")
	//...
    .Bind(Model.QueryBuilderModel))
```

Files to look at: 
- [Startup.cs](./CS/AspNetCoreQueryBuilderApp/Services/Startup.cs)
- [_ViewImports.cshtml](./CS/AspNetCoreQueryBuilderApp/Views/_ViewImports.cshtml)
- [HomeController.cs](./CS/AspNetCoreQueryBuilderApp/Controllers/HomeController.cs)

### SaveQueryRequested Event
When a user clicks the Save button in the Query Builder toolbar, the Query Builder raises the **SaveQueryRequested** client-side event. The event handler function calls the **GetSaveQueryModel** method to retrieve the model with the serialized data and uses the Ajax request to send the model to the server-side controller method:

```
<script>
    function SaveQueryRequested(sender) {
        var saveQueryModel = sender.GetSaveQueryModel();
        //...
		$.ajax({
			url: "/Home/SaveQuery",
			type: "POST",
			data: saveQueryModel,
		//...
        });
    }
</script>
```
The controller's SaveQuery method uses the [IQueryBuilderInputSerializer](https://docs.devexpress.com/CoreLibraries/DevExpress.DataAccess.Web.QueryBuilder.IQueryBuilderInputSerializer) service and the custom [SaveQueryRequest](https://docs.devexpress.com/CoreLibraries/DevExpress.DataAccess.Web.QueryBuilder.DataContracts.SaveQueryRequest) contract to deserialize the model:

```
[HttpPost]
public async Task<IActionResult> SaveQuery(
		[FromServices] IQueryBuilderInputSerializer queryBuilderInputSerializer,
		[FromServices] DataSourceStorageService dataSourceStorageService,
		[FromForm] CustomSaveQueryRequest saveQueryRequest) {
	try {
		var queryBuilderInput = queryBuilderInputSerializer.DeserializeSaveQueryRequest(saveQueryRequest);
		// ...
	}
	// ...
	}
}
```
The [SqlDataSource](https://docs.devexpress.com/CoreLibraries/DevExpress.DataAccess.Sql.SqlDataSource) object is extracted from the model, serialized and saved to the database.

Files to look at: 
- [QueryBuilder.cshtml](./CS/AspNetCoreQueryBuilderApp/Views/Home/QueryBuilder.cshtml)
- [HomeController.cs](./CS/AspNetCoreQueryBuilderApp/Controllers/HomeController.cs)
- [SerializationService.cs](./CS/AspNetCoreQueryBuilderApp/Services/SerializationService.cs)


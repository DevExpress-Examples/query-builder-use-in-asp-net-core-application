using System;
using System.Collections.Generic;
using System.Linq;
using AspNetCoreQueryBuilderApp.Reports;
using AspNetCoreQueryBuilderApp.Services;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.UI;

namespace AspNetCoreQueryBuilderApp.Data {
    public static class DbInitializer {
        public static void Initialize(ApplicationDbContext context) {
            //context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any users.
            if(context.Users.Any()) {
                return;   // DB has been seeded
            }

            var users = new ApplicationUser[] {
                new ApplicationUser { FirstMidName = "Carson", LastName = "Alexander" }
            };
            foreach(var user in users) {
                context.Users.Add(user);
                foreach(var report in PredefinedReports) {
                    var reportInstance = report.Value();
                    var reportDescription = new ReportEntity {
                        DisplayName = string.IsNullOrEmpty(reportInstance.DisplayName) ? report.Key : reportInstance.DisplayName,
                        ReportLayout = SerializationService.ReportToByteArray(reportInstance),
                        User = user
                    };
                    context.Reports.Add(reportDescription);
                }
                foreach(var dataSourceItem in PredefinedDataSources) {
                    var dataSource = dataSourceItem.Value();
                    var dataSourceEntity = new DataSourceEntity {
                        DisplayName = dataSourceItem.Key,
                        ConnectionName = dataSource.ConnectionName,
                        SerializedDataSource = SerializationService.SqlDataSourceToByteArray(dataSource),
                        User = user
                    };
                    context.DataSources.Add(dataSourceEntity);
                }
            }
            context.SaveChanges();
        }

        static Dictionary<string, Func<XtraReport>> PredefinedReports {
            get {
                return new Dictionary<string, Func<XtraReport>>() {
                    ["NorthwindBasedReport"] = () => new NWindReport()
                };
            }
        }

        static Dictionary<string, Func<SqlDataSource>> PredefinedDataSources {
            get {
                return new Dictionary<string, Func<SqlDataSource>>() {
                    ["Products"] = () => {
                        var ds = new SqlDataSource("NWindConnection");
                        SelectQuery query = SelectQueryFluentBuilder
                            .AddTable("Products")
                            .SelectColumns("ProductID", "ProductName")
                            .Build("Products");

                        query.Name = "Products";

                        ds.Queries.Add(query);
                        ds.RebuildResultSchema();
                        return ds;
                    }
                };
            }
        }
    }
}

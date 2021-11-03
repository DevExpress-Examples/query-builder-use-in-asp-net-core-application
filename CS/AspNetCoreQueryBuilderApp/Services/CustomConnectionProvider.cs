using System.Collections.Generic;
using System.Linq;
using AspNetCoreQueryBuilderApp.Models;
using DevExpress.Data.Entity;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Native;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.Web;
using DevExpress.DataAccess.Wizard.Services;
using Microsoft.Extensions.Configuration;

namespace AspNetCoreQueryBuilderApp.Services {
    public class CustomConnectionProvider : IConnectionProviderService {
        readonly IEnumerable<ConnectionStringModel> connectionStrings = new List<ConnectionStringModel>();

        public CustomConnectionProvider(IConfiguration Configuration) {
            Configuration.GetSection("QueryBuilderConnectionStrings").Bind(connectionStrings);
        }

        SqlDataConnection IConnectionProviderService.LoadConnection(string connectionName) {
            var connectionStringModel = connectionStrings.Where(x => x.Name == connectionName).FirstOrDefault();
            ConnectionStringInfo connectionStringInfo = connectionStringModel == null || string.IsNullOrEmpty(connectionStringModel.ConnectionString)
                ? null
                : new ConnectionStringInfo {
                    RunTimeConnectionString = connectionStringModel.ConnectionString,
                    Name = connectionName,
                    ProviderName = "SQLite"
                };
            DataConnectionParametersBase connectionParameters;
            if(connectionStringInfo == null
                || !AppConfigHelper.TryCreateSqlConnectionParameters(connectionStringInfo, out connectionParameters)
                || connectionParameters == null) {
                throw new KeyNotFoundException($"Connection string '{connectionName}' not found.");
            }
            return new SqlDataConnection(connectionName, connectionParameters);
        }
    }

    public class CustomConnectionProviderFactory : IConnectionProviderFactory {
        readonly IConnectionProviderService connectionProviderService;
        public CustomConnectionProviderFactory(CustomConnectionProvider connectionProviderService) {
            this.connectionProviderService = connectionProviderService;
        }
        public IConnectionProviderService Create() {
            return connectionProviderService;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreQueryBuilderApp.Data;
using DevExpress.DataAccess.Sql;

namespace AspNetCoreQueryBuilderApp.Services {
    public class DataSourceStorageService {
        readonly ApplicationDbContext dbContext;
        readonly IUserService userService;

        public DataSourceStorageService(ApplicationDbContext dbContext, IUserService userService) {
            this.dbContext = dbContext;
            this.userService = userService;
        }

        public async Task CreateOrUpdateDataSourceAsync(SelectQuery selectQuery, string dataConnectionName, int? existingDataSourceId) {
            string queryName = null;
            DataSourceEntity existingDataSource = null;
            if(existingDataSourceId.HasValue) {
                existingDataSource = dbContext.DataSources.Where(x => x.User.ID == userService.GetCurrentUserId() && x.ID == existingDataSourceId!).Single();
                queryName = existingDataSource.DisplayName;
            }

            if(!string.IsNullOrEmpty(selectQuery.Name)) {
                queryName = selectQuery.Name;
            } else if(string.IsNullOrEmpty(queryName)) {
                queryName = selectQuery.Tables.FirstOrDefault().Name;
                selectQuery.Name = queryName;
            }
            SqlDataSource ds = new SqlDataSource(dataConnectionName);
            ds.Queries.Add(selectQuery);
            ds.RebuildResultSchema();
            var serializedDataSource = SerializationService.SqlDataSourceToByteArray(ds);
            if(existingDataSource == null) {
                var newUserDataSource = new DataSourceEntity {
                    SerializedDataSource = serializedDataSource,
                    DisplayName = queryName,
                    ConnectionName = dataConnectionName,
                    User = dbContext.Users.Where(a => a.ID == userService.GetCurrentUserId()).Single()
                };
                dbContext.DataSources.Add(newUserDataSource);
            } else {
                existingDataSource.DisplayName = queryName;
                existingDataSource.SerializedDataSource = serializedDataSource;
                dbContext.DataSources.Update(existingDataSource);
            }
            await dbContext.SaveChangesAsync();
        }
        public Dictionary<string, object> GetAvailableSqlDataSources() {
            return dbContext.DataSources
                .Where(x => x.User.ID == userService.GetCurrentUserId())
                .ToDictionary(
                    x => x.DisplayName,
                    x => (object)SerializationService.SqlDataSourceFromByteArray(x.SerializedDataSource)
                );
        }
    }
}

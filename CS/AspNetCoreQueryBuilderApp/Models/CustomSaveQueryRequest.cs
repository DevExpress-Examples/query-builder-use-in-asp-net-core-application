using DevExpress.DataAccess.Web.QueryBuilder.DataContracts;

namespace AspNetCoreQueryBuilderApp.Models {
    public class CustomSaveQueryRequest : SaveQueryRequest {
        public int? ExistingDataSourceId { get; set; }
        public string ConnectionName { get; set; }
    }
}

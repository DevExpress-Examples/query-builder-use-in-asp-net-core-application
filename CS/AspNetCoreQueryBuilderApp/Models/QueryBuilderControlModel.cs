using DevExpress.XtraReports.Web.QueryBuilder;

namespace AspNetCoreQueryBuilderApp.Models {
    public class QueryBuilderControlModel {
        public QueryBuilderModel QueryBuilderModel { get; set; }
        public DataSourceModel Query { get; set; }
    }
}

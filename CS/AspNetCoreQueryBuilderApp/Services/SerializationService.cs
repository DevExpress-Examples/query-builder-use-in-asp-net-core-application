using System.IO;
using System.Text;
using System.Xml.Linq;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.UI;

namespace AspNetCoreQueryBuilderApp.Services {
    public class SerializationService {
        public static byte[] SqlDataSourceToByteArray(SqlDataSource sqlDataSource) {
            XElement dataSourceXElement = sqlDataSource.SaveToXml();
            var dataSourceXmlString = dataSourceXElement.ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
            return Encoding.UTF8.GetBytes(dataSourceXmlString);
        }

        public static SqlDataSource SqlDataSourceFromByteArray(byte[] serializedSqlDataSource) {
            string dataSourceXmlString = Encoding.UTF8.GetString(serializedSqlDataSource);
            var dataSourceXElement = XElement.Parse(dataSourceXmlString);
            var sqlDataSource = new SqlDataSource();
            sqlDataSource.LoadFromXml(dataSourceXElement);
            return sqlDataSource;
        }

        public static byte[] ReportToByteArray(XtraReport report) {
            using(var memoryStream = new MemoryStream()) {
                report.SaveLayoutToXml(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}

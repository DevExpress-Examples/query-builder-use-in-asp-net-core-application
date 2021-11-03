using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetCoreQueryBuilderApp.Data {
    public class DataSourceEntity {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public byte[] SerializedDataSource { get; set; }
        public string ConnectionName { get; set; }
        public string DisplayName { get; set; }
        public ApplicationUser User { get; set; }
    }
}

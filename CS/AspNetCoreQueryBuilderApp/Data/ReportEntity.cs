using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetCoreQueryBuilderApp.Data {
    public class ReportEntity {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public byte[] ReportLayout { get; set; }
        public string DisplayName { get; set; }
        public ApplicationUser User { get; set; }
    }
}

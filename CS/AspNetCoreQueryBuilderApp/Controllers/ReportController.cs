using System.Linq;
using System.Threading.Tasks;
using AspNetCoreQueryBuilderApp.Data;
using AspNetCoreQueryBuilderApp.Models;
using AspNetCoreQueryBuilderApp.Services;
using DevExpress.AspNetCore.Reporting.QueryBuilder;
using DevExpress.AspNetCore.Reporting.ReportDesigner;
using DevExpress.AspNetCore.Reporting.WebDocumentViewer;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.ReportDesigner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreQueryBuilderApp.Controllers {
    [Authorize]
    public class ReportController : Controller {
        public async Task<IActionResult> ReportList(
                [FromServices] IUserService userService, 
                [FromServices] ApplicationDbContext dBContext) {
            var reportEntity = dBContext.Reports
                .Where(a => a.User.ID == userService.GetCurrentUserId())
                .Select(a => new ReportModel {
                    Id = a.ID.ToString(),
                    Title = string.IsNullOrEmpty(a.DisplayName) ? "Noname Report" : a.DisplayName
                });
            return View(await reportEntity.ToListAsync());
        }

        public IActionResult Design(
                [FromServices] DataSourceStorageService dataSourceStorageService,
                [FromServices] IReportDesignerClientSideModelGenerator reportDesignerClientSideModelGenerator,
                ReportModel model) {
            var availableDataSources = dataSourceStorageService.GetAvailableSqlDataSources();
            var reportDesignerModel = model?.Id == null ?
                reportDesignerClientSideModelGenerator
                    .GetModel(new XtraReport(), availableDataSources, ReportDesignerController.DefaultUri, WebDocumentViewerController.DefaultUri, QueryBuilderController.DefaultUri)
                : reportDesignerClientSideModelGenerator
                    .GetModel(model.Id, availableDataSources, ReportDesignerController.DefaultUri, WebDocumentViewerController.DefaultUri, QueryBuilderController.DefaultUri);
            return View(reportDesignerModel);
        }

        [HttpPost]
        public async Task<IActionResult> Remove(
                [FromServices] IUserService userService, 
                [FromServices] ApplicationDbContext dBContext,
                [FromServices] DataSourceStorageService dataSourceStorageService,
                int reportId) {
            var reportEntity = await dBContext.Reports.Where(a => a.ID == reportId && a.User.ID == userService.GetCurrentUserId()).FirstOrDefaultAsync();
            if(reportEntity != null) {
                dBContext.Reports.Remove(reportEntity);
                await dBContext.SaveChangesAsync();
            }
            var availableDataSources = dataSourceStorageService.GetAvailableSqlDataSources();
            if(availableDataSources.Count > 0) {
                return RedirectToAction(nameof(ReportList));
            } else { 
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}

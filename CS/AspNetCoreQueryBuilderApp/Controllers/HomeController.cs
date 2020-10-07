using System;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreQueryBuilderApp.Data;
using AspNetCoreQueryBuilderApp.Models;
using AspNetCoreQueryBuilderApp.Services;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.Web.QueryBuilder;
using DevExpress.XtraReports.Web.QueryBuilder.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreQueryBuilderApp.Controllers {
    [Authorize]
    public class HomeController : Controller {
        [HttpGet]
        public async Task<IActionResult> Index(
                [FromServices] IUserService userService,
                [FromServices] ApplicationDbContext dBContext) {
            var dataSources = dBContext.DataSources
                .Where(a => a.User.ID == userService.GetCurrentUserId())
                .Select(a => new DataSourceModel {
                    DataSourceId = a.ID,
                    DataConnectionName = a.ConnectionName,
                    Title = string.IsNullOrEmpty(a.DisplayName) ? "Noname Data Source" : a.DisplayName
                });
            return View(await dataSources.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> SaveQuery(
                [FromServices] IQueryBuilderInputSerializer queryBuilderInputSerializer,
                [FromServices] DataSourceStorageService dataSourceStorageService,
                [FromForm] CustomSaveQueryRequest saveQueryRequest) {
            try {
                var queryBuilderInput = queryBuilderInputSerializer.DeserializeSaveQueryRequest(saveQueryRequest);
                await dataSourceStorageService.CreateOrUpdateDataSourceAsync(queryBuilderInput.ResultQuery, saveQueryRequest.ConnectionName, saveQueryRequest.ExistingDataSourceId);
                return new RedirectToActionResult("Index", "Home", null);
            } catch(Exception ex) {
                var validationException = ex.InnerException as ValidationException;
                return Problem(validationException?.Message ?? ex.Message);
            }
        }

        public IActionResult QueryBuilder(
                [FromServices] IUserService userService,
                [FromServices] ApplicationDbContext dbContext,
                [FromServices] IQueryBuilderClientSideModelGenerator queryBuilderClientSideModelGenerator,
                DataSourceModel queryModel) {
            QueryBuilderControlModel queryBuilderControlModel;
            if(queryModel?.DataSourceId.HasValue ?? false) {
                queryBuilderControlModel = GetExistingQueryModel(dbContext, queryBuilderClientSideModelGenerator, userService.GetCurrentUserId(), queryModel.DataSourceId.Value);
            } else {
                queryBuilderControlModel = CreateNewQueryBuilderModel(queryBuilderClientSideModelGenerator);
            }
            return View(queryBuilderControlModel);
        }

        QueryBuilderControlModel GetExistingQueryModel(
                ApplicationDbContext dbContext, 
                IQueryBuilderClientSideModelGenerator queryBuilderClientSideModelGenerator, 
                int userId, 
                int dataSourceId) {
            var existingDataSource = dbContext.DataSources.Where(x => x.ID == dataSourceId && x.User.ID == userId).Single();
            var dataSource = SerializationService.SqlDataSourceFromByteArray(existingDataSource.SerializedDataSource);
            var queryBuilderModel = queryBuilderClientSideModelGenerator.GetModel(existingDataSource.ConnectionName, (SelectQuery)dataSource.Queries[0]);
            return new QueryBuilderControlModel {
                Query = new DataSourceModel {
                    DataSourceId = existingDataSource.ID,
                    Title = existingDataSource.DisplayName,
                    DataConnectionName = existingDataSource.ConnectionName
                },
                QueryBuilderModel = queryBuilderModel
            };
        }

        QueryBuilderControlModel CreateNewQueryBuilderModel(IQueryBuilderClientSideModelGenerator queryBuilderClientSideModelGenerator) {
            var newDataConnectionName = "NWindConnection";
            var queryBuilderModel = queryBuilderClientSideModelGenerator.GetModel(newDataConnectionName);
            return new QueryBuilderControlModel {
                Query = new DataSourceModel {
                    Title = "New Query",
                    DataConnectionName = newDataConnectionName
                },
                QueryBuilderModel = queryBuilderModel
            };
        }

        [HttpPost]
        public async Task<IActionResult> RemoveDataSource(
                [FromServices] IUserService userService, 
                [FromServices] ApplicationDbContext dBContext, 
                int dataSourceId) {
            var dataSourceEntity = await dBContext.DataSources
                .Where(a => a.ID == dataSourceId && a.User.ID == userService.GetCurrentUserId())
                .FirstOrDefaultAsync();
            if(dataSourceEntity != null) {
                dBContext.DataSources.Remove(dataSourceEntity);
                await dBContext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

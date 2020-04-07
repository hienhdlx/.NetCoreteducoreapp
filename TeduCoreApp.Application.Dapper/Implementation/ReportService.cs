using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using TeduCoreApp.Application.Dapper.Interfaces;
using TeduCoreApp.Application.Dapper.ViewModels;

namespace TeduCoreApp.Application.Dapper.Implementation
{
    public class ReportService : IReportService
    {
        private readonly IConfiguration _configuration;

        public ReportService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IEnumerable<RevenueReportViewModel>> GetReportAsync(string fromDate, string toDate)
        {
            using (var sqlConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await sqlConnection.OpenAsync();
                var dynamicParameters = new DynamicParameters();
                var now = DateTime.Now;

                var firtDayofMonth = new DateTime(now.Year, now.Month, 1);
                var lastDayofMonth = firtDayofMonth.AddMonths(1).AddDays(-1);

                dynamicParameters.Add("@fromDate", string.IsNullOrEmpty(fromDate) ? firtDayofMonth.ToString("MM/dd/yyyy") : fromDate);
                dynamicParameters.Add("@toDate", string.IsNullOrEmpty(toDate) ? lastDayofMonth.ToString("MM/dd/yyyy") : toDate);

                try
                {
                    return await sqlConnection.QueryAsync<RevenueReportViewModel>(
                        "GetRevenueDaily", dynamicParameters, commandType: CommandType.StoredProcedure);
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }
    }
}

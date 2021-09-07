using Deepwell.Common.Extensions;
using Deepwell.Data.Repository;
using Deepwell.Front.CustomFilters;
using Deepwell.Front.Models.Constants;
using Deepwell.Front.Models.Log;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Deepwell.Front.Controllers
{
    [Authorize, CustomerAuthorizationFilter]
    public class LogController : Controller
    {
        private LogRepository _logRepository;

        public LogController()
        {
            if (_logRepository.IsNull())
            {
                _logRepository = new LogRepository();
            }
        }

        // GET: Log
        public ActionResult Index(int page = 1)
        {
            var response = this.GetLogs().ToPagedList(page, DeepwellConstants.PAGESIZE_FOR_LOGS);

            return View("Index", response);
        }

        public FileContentResult Download()
        {
            string csv = this.GetCsvString(this.GetLogs());
            string fileName = $"Log_{DateTime.Now.ToString("ddMMMy_HHmmss")}.csv";
            return File(new UTF8Encoding().GetBytes(csv), "text/csv", fileName);
        }

        private string GetCsvString(IEnumerable<ExceptionLogModel> logs)
        {
            var csv = new StringBuilder();

            csv.AppendLine("ExceptionDate,Message,Level,Logger");

            foreach (var log in logs)
            {
                csv.Append(log.ExceptionDate.ToString("dd MMM yyyy HH:mm:ss tt") + ",");
                csv.Append(Regex.Replace(log.Message.Replace(",", ""), @"\t|\n|\r", "") + ",");
                csv.Append(log.Level + ",");
                csv.Append(log.Logger + ",");
                csv.AppendLine();
            }

            return csv.ToString();
        }

        private IEnumerable<ExceptionLogModel> GetLogs()
        {
            var logs = _logRepository.GetLogs().ToList();

            return logs.Select(l =>
                new ExceptionLogModel
                {
                    ExceptionDate = l.Date,
                    Level = l.Level,
                    Logger = l.Logger,
                    Message = l.Message,
                }
            );
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using WorkloadWebApp.Models;

namespace WorkloadWebApp.Controllers
{
    public class DatabaseController : Controller
    {
        // GET: Database
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DataManagement()
        {
            return View();
        }

        public ActionResult CommitData()
        {
            string currentUser = User.Identity.GetUserName();
            if (currentUser == "")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        public ActionResult UpdateCommitData(FormCollection form)
        {
            string currentUser = User.Identity.GetUserName();
            if (currentUser == "")
            {
                return RedirectToAction("Login", "Account");
            }

            var entities = new WorkloadDBEntities();

            string Year = form["Year"];
            string Month = form["Month"];
            string Commit = form["Commit"];

            int intYear = Convert.ToInt16(Year);
            int intMonth = Convert.ToInt16(Month);
            int intCommit = Convert.ToInt16(Commit);
            Boolean _Commit = Convert.ToBoolean(intCommit);

            entities = new WorkloadDBEntities();
            var WeekNo = (from tb in entities.RecordMonthTBs
                          where tb.year == intYear & tb.month == intMonth
                          select tb).ToList();

            var intFirstWeek = Convert.ToInt16(WeekNo[0].firstWeek);
            var intLastWeek = Convert.ToInt16(WeekNo[0].lastWeek);

            entities = new WorkloadDBEntities();
            var ListWorkload = new List<WorkloadRecordTB>();
            ListWorkload = (from list in entities.WorkloadRecordTBs
                            where list.Year == intYear & list.Week >= intFirstWeek & list.Week <= intLastWeek
                            select list).ToList();
            foreach(var i in ListWorkload)
            {
                i.CommitData = _Commit;
            }
            entities.SaveChanges();

            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PopupCommitData(FormCollection form)
        {
            string currentUser = User.Identity.GetUserName();
            if (currentUser == "")
            {
                return RedirectToAction("Login", "Account");
            }

            var entities = new WorkloadDBEntities();

            string Year = form["Year"];
            string Month = form["Month"];

            int intYear = Convert.ToInt16(Year);
            int intMonth = Convert.ToInt16(Month);

            var WeekNo = (from tb in entities.RecordMonthTBs
                          where tb.year == intYear & tb.month == intMonth
                          select tb).ToList();

            var intFirstWeek = Convert.ToInt16(WeekNo[0].firstWeek);
            var intLastWeek = Convert.ToInt16(WeekNo[0].lastWeek);

            entities = new WorkloadDBEntities();
            var dateList = ((from tb in entities.WeekTBs
                             where tb.week >= intFirstWeek & tb.week <= intLastWeek
                             select tb).ToList());
            string FirstDate = Convert.ToDateTime(dateList[0].firstDate).ToString("d MMM yyyy");
            string LastDate = Convert.ToDateTime(dateList[dateList.Count-1].lastDate).ToString("d MMM yyyy");

            return Json(new { FirstDate, LastDate }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProjectDatabase()
        {
            string currentUser = User.Identity.GetUserName();
            if (currentUser == "")
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

    }
}
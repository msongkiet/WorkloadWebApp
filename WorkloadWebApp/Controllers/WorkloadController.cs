using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WorkloadWebApp.Models;
using Microsoft.AspNet.Identity;
using System.Text.RegularExpressions;
using System.Globalization;

namespace WorkloadWebApp.Controllers
{
    public class WorkloadController : Controller
    {
        #region Workload Record
        public ActionResult Index()
        {
            string currentUser = User.Identity.GetUserName();
            if (currentUser == "")
            {
                return RedirectToAction("Login", "Account");
            }

            WorkloadDBEntities entities = new WorkloadDBEntities();
            List<int?> listID = new List<int?>();
            var projectName = new List<ProjectTB>();

            listID = (from ProjectSelect in entities.ProjectSelectionTBs
                      where ProjectSelect.Username == currentUser
                      select ProjectSelect.ProjectID).ToList();

            foreach(var item in listID)
            {
                ProjectTB obj = new ProjectTB();
                var name = (from Project in entities.ProjectTBs
                           where Project.ProjectID == item 
                           select Project).SingleOrDefault();
                obj.ProjectID = name.ProjectID;
                obj.ProjectName = name.ProjectName;
                projectName.Add(obj);
            }

            return View(projectName);
       }

        public ActionResult SaveWorkloadDB(FormCollection form)
        {
            string currentUser = User.Identity.GetUserName();
            var entities = new WorkloadDBEntities();
            var ListWorkload = new List<WorkloadRecordTB>();

            var SplitPattern = ",";            // Split on Comma
            string[] ProjectID = Regex.Split(form["ProjectID"], SplitPattern);
            string[] Workload = null;
            string[] Remark = Regex.Split(form["Remark"], SplitPattern);

            //string Month = form["Month"];
            string Year = form["Year"];
            string totalWeek = form["totalWeek"];
            string FirstWeek = form["FirstWeek"];
            string LastWeek = form["LastWeek"];

            //int intMonth = Convert.ToInt16(Month);
            int intYear = Convert.ToInt16(Year);
            int intTotalWeek = Convert.ToInt16(totalWeek);
            int intFirstWeek = Convert.ToInt16(FirstWeek);
            int intLastWeek = Convert.ToInt16(LastWeek);

            entities.WorkloadRecordTBs.RemoveRange(entities.WorkloadRecordTBs.Where(c => c.Username == currentUser && c.Year == intYear && c.Week >= intFirstWeek && c.Week <= intLastWeek && c.CommitData == false));
            entities.SaveChanges();

            for (var i = 0; i < ProjectID.Count();i++)
            {
                for (var j = intFirstWeek; j<= intLastWeek; j++)
                {
                    string WorkloadRow = "WorkloadWeek" + j.ToString();
                    Workload = null;
                    Workload = Regex.Split(form[WorkloadRow], SplitPattern);
                    WorkloadRecordTB obj = new WorkloadRecordTB();
                    obj.Username = currentUser;
                    obj.Year = Convert.ToInt16(intYear);
                    obj.Week = j;
                    obj.ProjectID = Convert.ToInt16(ProjectID[i]);
                    if(Workload[i] != "")
                    {
                        Single.TryParse(Workload[i], out var x);
                        obj.Workload = Math.Round(x, 1);
                    }
                    else
                    {
                        obj.Workload = null;
                    }
                    obj.Remark = Remark[i];
                    obj.CommitData = Convert.ToBoolean(0);
                    ListWorkload.Add(obj);
                }
            }
           
            var entitiesRecord = entities.WorkloadRecordTBs.Where(c => c.Username == currentUser && c.Year == intYear && c.Week >= intFirstWeek && c.Week <= intLastWeek);
            string status;

            if (entitiesRecord.Count() == 0)
            {
                entities = new WorkloadDBEntities();
                entities.WorkloadRecordTBs.AddRange(ListWorkload);
                entities.SaveChanges();
                status = "Success";
            }
            else
            {
                status = "Save Fail";
            }

            return Json(new { name = status }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateTable(FormCollection form)
        {
            string currentUser = User.Identity.GetUserName();
            if (currentUser == "Admin@sa.com")
            {
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }

            var entities = new WorkloadDBEntities();
            var workingDay = (from tb in entities.EmployeeTBs
                             where tb.Username == currentUser
                             select tb.WorkingDay).ToList();
            int intWorkingDay = (int)(workingDay[0]);

            var monthNo = form["Month"];
            var yearNo = form["Year"];
            int intMonth = Convert.ToInt16(monthNo);
            int intYear = Convert.ToInt16(yearNo);

            entities = new WorkloadDBEntities();
            var WeekNo = (from tb in entities.RecordMonthTBs
                             where tb.year == intYear & tb.month == intMonth
                             select tb).ToList();
            var intFirstWeek = Convert.ToInt16(WeekNo[0].firstWeek);
            var intLastWeek = Convert.ToInt16(WeekNo[0].lastWeek);
            var totalWeeks = intLastWeek - intFirstWeek + 1;

            entities = new WorkloadDBEntities();
            var dateList = ((from tb in entities.WeekTBs
                            where tb.week >= intFirstWeek & tb.week <= intLastWeek
                            select tb).ToList());
            List<string> FirstDateList = new List<string>();
            List<string> LastDateList = new List<string>();
            for (var i = 0; i< dateList.Count; i++)
            {
                FirstDateList.Add(Convert.ToDateTime(dateList[i].firstDate).ToString("d MMM yyyy"));
                LastDateList.Add(Convert.ToDateTime(dateList[i].lastDate).ToString("d MMM yyyy"));
            }

            List<int> stdHour = new List<int>();
            if (intWorkingDay == 5)
            {
                for (var i = 0; i < dateList.Count; i++)
                {
                    var intHoliday = 0;
                    var firstDate = dateList[i].firstDate;
                    var lastDate = dateList[i].lastDate;
                    var dateCount = (int)(Convert.ToDateTime(lastDate).DayOfWeek - Convert.ToDateTime(firstDate).DayOfWeek) + 1;
                    entities = new WorkloadDBEntities();
                    var holiday = (from tb in entities.C5dayHolidayTB
                                   where tb.date >= firstDate & tb.date <= lastDate
                                   select tb).ToList();
                    if (Convert.ToDateTime(firstDate).DayOfWeek == DayOfWeek.Saturday || Convert.ToDateTime(firstDate).DayOfWeek == DayOfWeek.Sunday)
                    {
                        intHoliday = intHoliday + 1;
                    }
                    if (Convert.ToDateTime(firstDate) != Convert.ToDateTime(lastDate))
                    {
                        if (Convert.ToDateTime(lastDate).DayOfWeek == DayOfWeek.Saturday || Convert.ToDateTime(lastDate).DayOfWeek == DayOfWeek.Sunday)
                        {
                            intHoliday = intHoliday + 1;
                        }
                    }
                    intHoliday = intHoliday + holiday.Count;
                    dateCount = dateCount - intHoliday;
                    stdHour.Add(dateCount * 8);
                }
            }
            else if(intWorkingDay == 6)
            {
                for (var i = 0; i < dateList.Count; i++)
                {
                    var intHoliday = 0;
                    var firstDate = dateList[i].firstDate;
                    var lastDate = dateList[i].lastDate;
                    var dateCount = (int)(Convert.ToDateTime(lastDate).DayOfWeek - Convert.ToDateTime(firstDate).DayOfWeek) + 1;
                    entities = new WorkloadDBEntities();
                    var holiday = (from tb in entities.C6dayHolidayTB
                                   where tb.date >= firstDate & tb.date <= lastDate
                                   select tb).ToList();
                    if (Convert.ToDateTime(firstDate).DayOfWeek == DayOfWeek.Sunday)
                    {
                        intHoliday = intHoliday + 1;
                    }
                    if (Convert.ToDateTime(firstDate) != Convert.ToDateTime(lastDate))
                    {
                        if (Convert.ToDateTime(lastDate).DayOfWeek == DayOfWeek.Sunday)
                        {
                            intHoliday = intHoliday + 1;
                        }
                    }
                    intHoliday = intHoliday + holiday.Count;
                    dateCount = dateCount - intHoliday;
                    stdHour.Add(dateCount * 8);
                }
            }
            return Json(new { FirstDateList, LastDateList, totalWeeks, intFirstWeek, intLastWeek, stdHour }, JsonRequestBehavior.AllowGet);           
        }

        public ActionResult LoadLastData(FormCollection form)
        {
            string currentUser = User.Identity.GetUserName();
            if (currentUser == "Admin@sa.com")
            {
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }

            var entities = new WorkloadDBEntities();
            var ListWorkload = new List<WorkloadRecordTB>();

            string Year = form["Year"];
            string FirstWeek = form["FirstWeek"];
            string LastWeek = form["LastWeek"];

            int intYear = Convert.ToInt16(Year);
            int intFirstWeek = Convert.ToInt16(FirstWeek);
            int intLastWeek = Convert.ToInt16(LastWeek);

            ListWorkload = (from list in entities.WorkloadRecordTBs
                            where list.Username == currentUser & list.Year == intYear & list.Week >= intFirstWeek & list.Week <= intLastWeek
                            select list).ToList();
            int countList = ListWorkload.Count();

            return Json(new { list = ListWorkload, count = countList, intFirstWeek, intLastWeek }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Project Selection
        public ActionResult QueryProjectList()
        {
            string currentUser = User.Identity.GetUserName();
            WorkloadDBEntities entities = new WorkloadDBEntities();
            List<int?> listID = new List<int?>();
            listID = (from ProjectSelect in entities.ProjectSelectionTBs
                      where ProjectSelect.Username == currentUser
                      select ProjectSelect.ProjectID).ToList();
            return Json(new { name = listID, count = listID.Count }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProjectSelection()
        {
            string currentUser = User.Identity.GetUserName();
            if (currentUser == "")
            {
                return RedirectToAction("Login", "Account");
            }
            var entities = new WorkloadDBEntities();
            return View(entities.ProjectTBs.ToList());
        }

        public ActionResult PopupProjectSelected(FormCollection form)
        {
            string currentUser = User.Identity.GetUserName();
            var entities = new WorkloadDBEntities();
            List<string> listName = new List<string>();

            for (var i = 0; i < form.Keys.Count; i++)
            {
                string itemName = form.AllKeys[i];

                if (itemName != "" && itemName != null)
                {
                    listName.Add(itemName);
                }
            }

            return Json(new { list = listName, count = listName.Count }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveProjectSelectionDB(FormCollection form)
        {
            string currentUser = User.Identity.GetUserName();
            var entities = new WorkloadDBEntities();
            var ListProject = new List<ProjectSelectionTB>();
            List<int> listID = new List<int>();

            //Delete project selection entities 
            entities.ProjectSelectionTBs.RemoveRange(entities.ProjectSelectionTBs.Where(c => c.Username == currentUser));
            entities.SaveChanges();
            ///////////////////////////////////            

            foreach (var item in form.Keys)
            {
                string itemValue = form[item.ToString()];

                if (itemValue != "")
                {
                    var value = Convert.ToInt16(itemValue);
                    listID.Add(value);
                }
            }

            foreach (var item in listID)
            {
                ProjectSelectionTB obj = new ProjectSelectionTB();
                obj.Username = currentUser;
                obj.ProjectID = item;
                ListProject.Add(obj);
            }

            // add Leave to obj (Leave ID = 999)
            ProjectSelectionTB obj1 = new ProjectSelectionTB();
            obj1.Username = currentUser;
            obj1.ProjectID = 999;
            ListProject.Add(obj1);

            entities.ProjectSelectionTBs.AddRange(ListProject);
            entities.SaveChanges();

            return Json(new { }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}
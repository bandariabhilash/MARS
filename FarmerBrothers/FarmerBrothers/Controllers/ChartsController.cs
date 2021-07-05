using FarmerBrothers.Data;
using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FarmerBrothers.Controllers
{
    public class ChartsController : BaseController
    {
        // GET: Charts
        public ActionResult AllCharts(int? isBack)
        {
            Models.Charts RedirectCallModel = new Models.Charts();

            if (TempData["SearchCriteria"] != null && isBack == 1)
            {
                RedirectCallModel = TempData["SearchCriteria"] as Models.Charts;
                TempData["SearchCriteria"] = RedirectCallModel;
            }
            else
            {
                RedirectCallModel = new Models.Charts();
                TempData["SearchCriteria"] = null;
            }

            IEnumerable<TechHierarchyView> Techlist = Utility.GetAllTechDataByBranchType(FarmerBrothersEntitites);

            List<TECH_HIERARCHY> newTechlistCollection = new List<TECH_HIERARCHY>();
            TECH_HIERARCHY techhierarchy;
            foreach (var item in Techlist.ToList())
            {
                techhierarchy = new TECH_HIERARCHY();
                techhierarchy.DealerId = Convert.ToInt32(item.TechID);
                techhierarchy.CompanyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.PreferredProvider.ToLower());
                newTechlistCollection.Add(techhierarchy);
            }

            TECH_HIERARCHY techhierarchy1 = new TECH_HIERARCHY()
            {
                DealerId = -1,
                CompanyName = "Please select Technician"
            };
            newTechlistCollection.Insert(0, techhierarchy1);

            RedirectCallModel.Technicianlist = newTechlistCollection;

            System.Data.DataTable dt = Security.GetFamilyAff();
            List<Technician> TechnicianAffs = new List<Technician>();
            foreach (DataRow dr in dt.Rows)
            {
                Technician tech = new Technician();
                tech.TechID = dr[0].ToString();
                if (dr[0].ToString() == "SPD")
                {
                    tech.TechName = "Internal";
                    TechnicianAffs.Add(tech);
                }
                if (dr[0].ToString() == "SPT")
                {
                    tech.TechName = "3rd Party";
                    TechnicianAffs.Add(tech);
                }
            }

            Technician tech1 = new Technician();
            tech1.TechID = "All";
            tech1.TechName = "All";
            TechnicianAffs.Insert(0, tech1);

            RedirectCallModel.FamilyAffs = TechnicianAffs;

            RedirectCallModel.SearchResults = new List<Models.ChartData>();


            List<Models.ChartData> chartData = new List<Models.ChartData>
            {
                new Models.ChartData { xValue = "Labour", yValue = 18, text = "18%"},
                new Models.ChartData { xValue = "Legal", yValue = 8 , text = "8% "},
                new Models.ChartData { xValue = "Production", yValue = 15, text = "15%"},
                new Models.ChartData { xValue = "License", yValue = 11, text = "11%"},
                new Models.ChartData { xValue = "Facilities", yValue = 18, text = "18%"},
                new Models.ChartData { xValue = "Taxes", yValue = 14, text = "14%"},
                new Models.ChartData { xValue = "Insurance", yValue = 16, text = "16%"},
            };
            //ViewBag.dataSource = chartData;
            RedirectCallModel.SearchResults = chartData;
            return View(RedirectCallModel);
        }
    }
}
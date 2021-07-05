using FarmerBrothers.Data;
using FarmerBrothers.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class FBReportModel
    {
        public FBReportModel()
        {

        }
        public FBReportModel(FBReport report)
        {
            this.report_id = report.report_id;
            this.report_name = report.report_name;
            //this.report_filename = report.report_filename;
            //this.report_category = report.report_category;
            //this.DownloadName = report.DownloadName;
            //this.DisplayFile = report.DisplayFile;
            //this.DescriptionFile = report.DescriptionFile;
            //this.DownloadFile = report.DownloadFile;
            //this.Active = report.Active;
            //this.ReportType = report.ReportType;
        }
        public FBReportModel(FBUserReport report)
        {
            this.report_id = report.report_id;
            this.report_name = report.FBReport.report_name;
        }

        public int report_id { get; set; }
        public string report_name { get; set; }
        //public string report_filename { get; set; }
        //public string report_category { get; set; }
        //public string DownloadName { get; set; }
        //public string DisplayFile { get; set; }
        //public string DescriptionFile { get; set; }
        //public string DownloadFile { get; set; }
        //public Nullable<short> Active { get; set; }
        //public string ReportType { get; set; }
    }
}
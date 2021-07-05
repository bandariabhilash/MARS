using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Web;
using System.Configuration;
using System.IO;
using System.Data;

namespace FarmerBrothers.Models
{
    class ESMCCMRSMModel
    {
        public decimal PostalCode { get; set; }
        public string Country { get; set; }

        public int ESMId { get; set; }
        public string ESMName { get; set; }
        public string ESMEmail { get; set; }
        public string ESMPhone { get; set; }

        public int CCMId { get; set; }
        public string CCMName { get; set; }
        public string CCMEmail { get; set; }
        public string CCMPhone { get; set; }

        public int RSMId { get; set; }
        public string RSMName { get; set; }
        public string RSMEmail { get; set; }
        public string RSMPhone { get; set; }


        public static void InsertData(List<ESMCCMRSMModel> esmList, FarmerBrothersEntities fileUploadEntity)
        {
            DateTime currentDate = DateTime.Now;

            foreach (ESMCCMRSMModel esmItem in esmList)
            {
                ESMCCMRSMEscalation esmccmrsmEscalation = new ESMCCMRSMEscalation();

                ESMCCMRSMEscalation esmEscalationItem = fileUploadEntity.ESMCCMRSMEscalations.Where(ecr => ecr.ZIPCode == esmItem.PostalCode.ToString()).FirstOrDefault();
                
                if (esmEscalationItem != null)
                {
                    esmEscalationItem.EDSMID = esmItem.ESMId;
                    esmEscalationItem.ESMName = esmItem.ESMName;
                    esmEscalationItem.ESMEmail = esmItem.ESMEmail;
                    esmEscalationItem.ESMPhone = esmItem.ESMPhone;

                    esmEscalationItem.CCMID = esmItem.CCMId;
                    esmEscalationItem.CCMName = esmItem.CCMName;
                    esmEscalationItem.CCMEmail = esmItem.CCMEmail;
                    esmEscalationItem.CCMPhone = esmItem.CCMPhone;

                    esmEscalationItem.RSMID = esmItem.RSMId;
                    esmEscalationItem.RSM = esmItem.RSMName;
                    esmEscalationItem.RSMEmail = esmItem.RSMEmail;
                    esmEscalationItem.RSMPhone = esmItem.RSMPhone;

                    esmEscalationItem.ModifiedDate = currentDate;
                }
                else
                {
                    ESMCCMRSMEscalation ee = new ESMCCMRSMEscalation();
                    ee.ZIPCode = esmItem.PostalCode.ToString();
                    ee.Country = esmItem.Country;

                    ee.EDSMID = esmItem.ESMId;
                    ee.ESMName = esmItem.ESMName;
                    ee.ESMEmail = esmItem.ESMEmail;
                    ee.ESMPhone = esmItem.ESMPhone;

                    ee.CCMID = esmItem.CCMId;
                    ee.CCMName = esmItem.CCMName;
                    ee.CCMEmail = esmItem.CCMEmail;
                    ee.CCMPhone = esmItem.CCMPhone;

                    ee.RSMID = esmItem.RSMId;
                    ee.RSM = esmItem.RSMName;
                    ee.RSMEmail = esmItem.RSMEmail;
                    ee.RSMPhone = esmItem.RSMPhone;

                    ee.ModifiedDate = currentDate;

                    fileUploadEntity.ESMCCMRSMEscalations.Add(ee);
                }
            }

            fileUploadEntity.SaveChanges();
        }

        public static void UpdateContacts(FarmerBrothersEntities fileUploadEntity)
        {
            DateTime CurrentDate = DateTime.Now;
            List<ESMCCMRSMEscalation> esmList = fileUploadEntity.ESMCCMRSMEscalations.Where(e => e.ModifiedDate.Value.Year == CurrentDate.Year && e.ModifiedDate.Value.Month == CurrentDate.Month
            && e.ModifiedDate.Value.Day == CurrentDate.Day).ToList();

            foreach (ESMCCMRSMEscalation esmItem in esmList)
            {
                List<Contact> contactsList = fileUploadEntity.Contacts.Where(c => c.PostalCode == esmItem.ZIPCode.ToString()).ToList();
                foreach (Contact con in contactsList)
                {
                    con.ESMName = esmItem.ESMName;
                    con.ESMPhone = esmItem.ESMPhone;
                    con.ESMEmail = esmItem.ESMEmail;

                    con.RSMName = esmItem.RSM;
                    con.RSMPhone = esmItem.RSMPhone;
                    con.RSMEmail = esmItem.RSMEmail;

                    con.CCMName = esmItem.CCMName;
                    con.CCMPhone = esmItem.CCMPhone;
                    con.CCMEmail = esmItem.CCMEmail;
                }
            }
            fileUploadEntity.SaveChanges();
        }

    }

    class FBCustomerServiceDistributionModel
    {
        public string Route { get; set; }
        public string Branch { get; set; }
       
        public string RSRName { get; set; }
        public string RSREmail { get; set; }
        public string RSRPhone { get; set; }
        
        public string SalesManagerName { get; set; }
        public string SalesManagerEmail { get; set; }
        public string SalesManagerPhone { get; set; }
        
        public string RegionalsName { get; set; }
        public string RegionalsEmail { get; set; }
        public string RegionalsPhone { get; set; }

        public static void InsertData(List<FBCustomerServiceDistributionModel> escalationList, FarmerBrothersEntities fileUploadEntity)
        {
            var maxValue = fileUploadEntity.FBCustomerServiceDistributions.Max(x => x.ServiceId);
            int tmpServiceId = maxValue + 1;
            foreach (FBCustomerServiceDistributionModel escalationItem in escalationList)
            {
                FBCustomerServiceDistribution esmccmrsmEscalation = new FBCustomerServiceDistribution();

                FBCustomerServiceDistribution esmEscalationItem = fileUploadEntity.FBCustomerServiceDistributions.Where(csd => csd.Route == escalationItem.Route.ToString()).FirstOrDefault();
                if (esmEscalationItem != null)
                {
                    esmEscalationItem.Route = escalationItem.Route;
                    esmEscalationItem.Branch = escalationItem.Branch;

                    esmEscalationItem.RSRName = escalationItem.RSRName;
                    esmEscalationItem.RSREmail = escalationItem.RSREmail;
                    esmEscalationItem.RSRPhone = escalationItem.RSRPhone;
                   
                    esmEscalationItem.SalesManagerName = escalationItem.SalesManagerName;
                    esmEscalationItem.SalesMmanagerEmail = escalationItem.SalesManagerEmail;
                    esmEscalationItem.SalesManagerPhone = escalationItem.SalesManagerPhone;
                    
                    esmEscalationItem.RegionalsName = escalationItem.RegionalsName;
                    esmEscalationItem.RegionalsEmail = escalationItem.RegionalsEmail;
                    esmEscalationItem.RegonalsPhone = escalationItem.RegionalsPhone;
                }
                else
                {
                    FBCustomerServiceDistribution fbcsd = new FBCustomerServiceDistribution();
                    fbcsd.ServiceId = tmpServiceId;
                    fbcsd.Route = escalationItem.Route;
                    fbcsd.Branch = escalationItem.Branch;
                    
                    fbcsd.RSRName = escalationItem.RSRName;
                    fbcsd.RSREmail = escalationItem.RSREmail;
                    fbcsd.RSRPhone = escalationItem.RSRPhone;
                   
                    fbcsd.SalesManagerName = escalationItem.SalesManagerName;
                    fbcsd.SalesMmanagerEmail = escalationItem.SalesManagerEmail;
                    fbcsd.SalesManagerPhone = escalationItem.SalesManagerPhone;
                   
                    fbcsd.RegionalsName = escalationItem.RegionalsName;
                    fbcsd.RegionalsEmail = escalationItem.RegionalsEmail;
                    fbcsd.RegonalsPhone = escalationItem.RegionalsPhone;

                    fileUploadEntity.FBCustomerServiceDistributions.Add(fbcsd);

                    tmpServiceId = tmpServiceId + 1;
                }

            }

            fileUploadEntity.SaveChanges();
        }
    }

    class FileReading
    {
        public string FileName { get; set; }
        public string SubmittedBy { get; set; }
        public string SubmittedByEmail { get; set; }
        public string ErrorMsg { get; set; }
        public bool IsValid { get; set; }
        public List<ESMCCMRSMModel> EsmDataList { get; set; }
        public List<FBCustomerServiceDistributionModel> CustomerServiceEscalationDataList { get; set; }
        public List<CustomerModel> CustomerDataList { get; set; }

        public static FileReading ReadCSVFile(string inputFilePath, string FileName)
        {            
            try
            {
                List<ESMCCMRSMModel> esmList = new List<ESMCCMRSMModel>();
                string _path = Path.Combine(inputFilePath, FileName);

                var contents = File.ReadAllText(_path).Split('\n');
                FileReading fileDataObj = new FileReading();
                fileDataObj.IsValid = true;
                fileDataObj.FileName = FileName;
                int i = 0;
                foreach (string line in contents)
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    string lineVal = line.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ').Replace('\\', ' ').Replace("\"", "");
                    if (string.IsNullOrEmpty(lineVal) || lineVal == " ") continue;

                    if (i == 0)
                    {
                        fileDataObj = IsValidCSVFile(lineVal);

                        if (!fileDataObj.IsValid)
                        {
                            return fileDataObj;
                        }
                    }

                    string[] lineValues = lineVal.Split(',');
                    if (i != 0)
                    {
                        ESMCCMRSMModel esmItem = new ESMCCMRSMModel();
                        for (int ind = 0; ind <= lineValues.Count() - 1; ind++)
                        {
                            string str = lineValues[ind].Trim();
                            switch (ind)
                            {
                                case 0:
                                    esmItem.PostalCode = string.IsNullOrEmpty(str) ? 0 : Convert.ToDecimal(str);
                                    break;
                                case 1:
                                    esmItem.Country = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 2:
                                    esmItem.ESMId = string.IsNullOrEmpty(str) ? 0 : Convert.ToInt32(str);
                                    break;
                                case 3:
                                    esmItem.ESMName = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 4:
                                    esmItem.ESMEmail = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 5:
                                    esmItem.ESMPhone = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 6:
                                    esmItem.CCMId = string.IsNullOrEmpty(str) ? 0 : Convert.ToInt32(str);
                                    break;
                                case 7:
                                    esmItem.CCMName = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 8:
                                    esmItem.CCMEmail = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 9:
                                    esmItem.CCMPhone = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 10:
                                    esmItem.RSMId = string.IsNullOrEmpty(str) ? 0 : Convert.ToInt32(str);
                                    break;
                                case 11:
                                    esmItem.RSMName = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 12:
                                    esmItem.RSMEmail = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 13:
                                    esmItem.RSMPhone = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                            }
                        }
                        esmList.Add(esmItem);                        
                    }
                    i++;
                }

                fileDataObj.EsmDataList = esmList;
                return fileDataObj;
            }
            catch (Exception ex)
            {
                FileReading fileDataObj = new FileReading();
                fileDataObj.FileName = FileName;
                fileDataObj.IsValid = false;
                fileDataObj.ErrorMsg = "Exception : " + ex;
                return fileDataObj;
            }
        }

        private static FileReading IsValidCSVFile(string HeaderRow)
        {
            FileReading fr = new FileReading();
            fr.ErrorMsg = "";
            fr.IsValid = true;
            
            string[] headerValues = HeaderRow.Split(',');

            for(var index = 0; index<= headerValues.Count()-1; index++)
            {
                string hdrValue = headerValues[index].ToLower().Trim();

                switch (index)
                {
                    case 0:
                        if (hdrValue != "postalcode")
                        {
                            fr.ErrorMsg += "\n Postal Code Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 1:
                        if (hdrValue != "country")
                        {
                            fr.ErrorMsg += "\n Country Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 2:
                        if (hdrValue != "esmid")
                        {
                            fr.ErrorMsg += "\n ESMID Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 3:
                        if (hdrValue != "esmname")
                        {
                            fr.ErrorMsg += "\n ESM Name Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 4:
                        if (hdrValue != "esmemail")
                        {
                            fr.ErrorMsg += "\n ESM Email Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 5:
                        if (hdrValue != "esmphone")
                        {
                            fr.ErrorMsg += "\n ESM Phone Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 6:
                        if (hdrValue != "ccmid")
                        {
                            fr.ErrorMsg += "\n CCMID Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 7:
                        if (hdrValue != "ccmname")
                        {
                            fr.ErrorMsg += "\n CCM NAme Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 8:
                        if (hdrValue != "ccmemail")
                        {
                            fr.ErrorMsg += "\n CCM Email Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 9:
                        if (hdrValue != "ccmphone")
                        {
                            fr.ErrorMsg += "\n CCM Phone Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 10:
                        if (hdrValue != "rsmid")
                        {
                            fr.ErrorMsg += "\n RSMID Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 11:
                        if (hdrValue != "rsmname")
                        {
                            fr.ErrorMsg += "\n RSM Name Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 12:
                        if (hdrValue != "rsmemail")
                        {
                            fr.ErrorMsg += "\n RSM Email Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 13:
                        if (hdrValue.Trim() != "rsmphone")
                        {
                            fr.ErrorMsg += "\n RSM Phone Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                }
            }
            
            return fr;
        }


        public static FileReading ReadEscalationCSVFile(string inputFilePath, string FileName)
        {
            try
            {
                List<FBCustomerServiceDistributionModel> esmList = new List<FBCustomerServiceDistributionModel>();
                string _path = Path.Combine(inputFilePath, FileName);

                var contents = File.ReadAllText(_path).Split('\n');
                FileReading fileDataObj = new FileReading();
                fileDataObj.IsValid = true;
                fileDataObj.FileName = FileName;
                int i = 0;
                foreach (string line in contents)
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    string lineVal = line.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ').Replace('\\', ' ').Replace("\"", "");
                    if (string.IsNullOrEmpty(lineVal) || lineVal == " ") continue;

                    if (i == 0)
                    {
                        fileDataObj = IsValidEscalationCSVFile(lineVal);

                        if (!fileDataObj.IsValid)
                        {
                            return fileDataObj;
                        }
                    }

                    string[] lineValues = lineVal.Split(',');
                    if (i != 0)
                    {
                        FBCustomerServiceDistributionModel esmItem = new FBCustomerServiceDistributionModel();
                        for (int ind = 0; ind <= lineValues.Count() - 1; ind++)
                        {
                            string str = lineValues[ind].Trim();
                            switch (ind)
                            {
                                case 0:
                                    string route = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    if(!string.IsNullOrEmpty(route))
                                    {
                                       switch(route.Length)
                                        {
                                            case 1:
                                                route = "00" + route;
                                                break;
                                            case 2:
                                                route = "0" + route;
                                                break;
                                        }
                                    }
                                    esmItem.Route = route;
                                    break;
                                case 1:
                                    string branch = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    if (!string.IsNullOrEmpty(branch))
                                    {
                                        switch (branch.Length)
                                        {
                                            case 1:
                                                branch = "00" + branch;
                                                break;
                                            case 2:
                                                branch = "0" + branch;
                                                break;
                                        }
                                    }
                                    esmItem.Branch = branch;
                                    break;
                                case 2:
                                    esmItem.RSRName = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 3:
                                    esmItem.RSRPhone = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 4:
                                    esmItem.RSREmail = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;                                
                                case 5:
                                    esmItem.SalesManagerName = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 6:
                                    esmItem.SalesManagerPhone = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 7:
                                    esmItem.SalesManagerEmail = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;                               
                                case 8:
                                    esmItem.RegionalsName = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 9:
                                    esmItem.RegionalsPhone = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 10:
                                    esmItem.RegionalsEmail = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                
                            }
                        }
                        esmList.Add(esmItem);
                    }
                    i++;
                }

                fileDataObj.CustomerServiceEscalationDataList = esmList;
                return fileDataObj;
            }
            catch (Exception ex)
            {
                FileReading fileDataObj = new FileReading();
                fileDataObj.FileName = FileName;
                fileDataObj.IsValid = false;
                fileDataObj.ErrorMsg = "Exception : " + ex;
                return fileDataObj;
            }
        }

        private static FileReading IsValidEscalationCSVFile(string HeaderRow)
        {
            FileReading fr = new FileReading();
            fr.ErrorMsg = "";
            fr.IsValid = true;

            string[] headerValues = HeaderRow.Split(',');

            for (var index = 0; index <= headerValues.Count() - 1; index++)
            {
                string hdrValue = headerValues[index].ToLower().Trim();

                switch (index)
                {
                    case 0:
                        if (hdrValue != "route")
                        {
                            fr.ErrorMsg += "\n Route Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 1:
                        if (hdrValue != "branch")
                        {
                            fr.ErrorMsg += "\n Branch Column Missing";
                            fr.IsValid = false;
                        }
                        break;                       
                    case 2:
                        if (hdrValue != "rsrname")
                        {
                            fr.ErrorMsg += "\n RSR Name Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 3:
                        if (hdrValue != "rsrphone")
                        {
                            fr.ErrorMsg += "\n RSR Phone Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 4:
                        if (hdrValue != "rsremail")
                        {
                            fr.ErrorMsg += "\n RSR Email Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 5:
                        if (hdrValue != "salesmanagername")
                        {
                            fr.ErrorMsg += "\n Sales Manager Name Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 6:
                        if (hdrValue != "salesmanagerphone")
                        {
                            fr.ErrorMsg += "\n Sales Manager Phone Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 7:
                        if (hdrValue != "salesmanageremail")
                        {
                            fr.ErrorMsg += "\n Sales Manager Email Column Missing";
                            fr.IsValid = false;
                        }
                        break; 
                    case 8:
                        if (hdrValue != "regionalsname")
                        {
                            fr.ErrorMsg += "\n Regionals Name Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 9:
                        if (hdrValue.Trim() != "regionalsphone")
                        {
                            fr.ErrorMsg += "\n Regionals Phone Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 10:
                        if (hdrValue != "regionalsemail")
                        {
                            fr.ErrorMsg += "\n Regionals Email Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                }
            }

            return fr;
        }

        public static FileReading ReadExcel(string inputFilePath, string FileName)
        {           
                List<ESMCCMRSMModel> esmList = new List<ESMCCMRSMModel>();
                //string inputFilePath = ConfigurationManager.AppSettings["InputFilePath"];

                Excel.Application xlApp;
                Excel.Workbook xlWorkBook;
                Excel.Worksheet xlWorkSheet;
                Excel.Range range;

                //var str;
                int rCnt;
                int cCnt;
                int rw = 1;
                int cl = 0;
            
                string _path = Path.Combine(inputFilePath, FileName);

                xlApp = new Excel.Application();
                xlWorkBook = xlApp.Workbooks.Open(_path, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            try
            {
                range = xlWorkSheet.UsedRange;
                rw = range.Rows.Count;
                cl = range.Columns.Count;

                FileReading fileDataObj = isValidExcel(range, rw, cl);
                if (!fileDataObj.IsValid)
                {
                    fileDataObj.FileName = FileName;

                    xlWorkBook.Close(true, null, null);
                    if (xlWorkSheet != null) Marshal.ReleaseComObject(xlWorkSheet);
                    if (xlWorkBook != null) Marshal.ReleaseComObject(xlWorkBook);
                   
                    xlApp.Quit();
                    Marshal.ReleaseComObject(xlApp);

                    return fileDataObj;
                }

                //for (rCnt = 1; rCnt <= 2; rCnt++)
                //{
                //    for (cCnt = 1; cCnt <= 2; cCnt++)
                //    {
                //        var str = (range.Cells[rCnt, cCnt] as Excel.Range).Value2;

                //        if (cCnt == 2)
                //        {
                //            if (rCnt == 1)
                //            {
                //                fileDataObj.SubmittedBy = str;
                //            }
                //            if (rCnt == 2)
                //            {
                //                fileDataObj.SubmittedByEmail = str;
                //            }
                //        }
                //    }
                //}

                for (rCnt = 2; rCnt <= rw; rCnt++)
                {
                    ESMCCMRSMModel esmItem = new ESMCCMRSMModel();
                    for (cCnt = 1; cCnt <= cl; cCnt++)
                    {
                        var str = (range.Cells[rCnt, cCnt] as Excel.Range).Value2;
                        switch (cCnt)
                        {
                            case 1:
                                esmItem.PostalCode = Convert.ToDecimal(str);
                                break;
                            case 2:
                                esmItem.Country = str.ToString();
                                break;
                            case 3:
                                esmItem.ESMId = Convert.ToInt32(str);
                                break;
                            case 4:
                                esmItem.ESMName = str.ToString();
                                break;
                            case 5:
                                esmItem.ESMEmail = str.ToString();
                                break;
                            case 6:
                                esmItem.ESMPhone = str.ToString();
                                break;
                            case 7:
                                esmItem.CCMId = Convert.ToInt32(str);
                                break;
                            case 8:
                                esmItem.CCMName = str.ToString();
                                break;
                            case 9:
                                esmItem.CCMEmail = str.ToString();
                                break;
                            case 10:
                                esmItem.CCMPhone = str.ToString();
                                break;
                            case 11:
                                esmItem.RSMId = Convert.ToInt32(str);
                                break;
                            case 12:
                                esmItem.RSMName = str.ToString();
                                break;
                            case 13:
                                esmItem.RSMEmail = str.ToString();
                                break;
                            case 14:
                                esmItem.RSMPhone = str.ToString();
                                break;
                        }
                    }
                    esmList.Add(esmItem);
                }

                xlWorkBook.Close(true, null, null);
                if (xlWorkSheet != null) Marshal.ReleaseComObject(xlWorkSheet);
                if (xlWorkBook != null) Marshal.ReleaseComObject(xlWorkBook);

                xlApp.Quit();
                Marshal.ReleaseComObject(xlApp);

                fileDataObj.EsmDataList = esmList;

                return fileDataObj;
            }
            catch (Exception ex)
            {
                xlWorkBook.Close(true, null, null);
                if (xlWorkSheet != null) Marshal.ReleaseComObject(xlWorkSheet);
                if (xlWorkBook != null) Marshal.ReleaseComObject(xlWorkBook);

                xlApp.Quit();
                Marshal.ReleaseComObject(xlApp);

                FileReading fileDataObj = new FileReading();
                fileDataObj.FileName = FileName;
                fileDataObj.IsValid = false;
                fileDataObj.ErrorMsg = "Exception : " + ex;
                return fileDataObj;
            }
            finally
            {
                System.Diagnostics.Process[] process = System.Diagnostics.Process.GetProcessesByName("Excel");
                foreach (System.Diagnostics.Process p in process)
                {
                    //Picks processes with Name Excel from last 60 sec
                    if (!string.IsNullOrEmpty(p.ProcessName) && p.StartTime.AddSeconds(+60) > DateTime.Now)
                    {
                        try
                        {
                            p.Kill();
                        }
                        catch { }
                    }
                }
            }
        }

        private static FileReading isValidExcel(Excel.Range range, int rw, int cl)
        {
            FileReading fr = new FileReading();
            fr.ErrorMsg = "";
            fr.IsValid = true;
            for (int rCnt = 1; rCnt <= 1; rCnt++)
            {
                ESMCCMRSMModel esmItem = new ESMCCMRSMModel();
                for (int cCnt = 1; cCnt <= cl; cCnt++)
                {
                    var str = (range.Cells[rCnt, cCnt] as Excel.Range).Value2;
                    string strVal = str== null ? "" :  str.ToString();
                    switch (cCnt)
                    {
                        case 1:
                            if (strVal.ToLower() != "postalcode")
                            {
                                fr.ErrorMsg += "\n Postal Code Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 2:
                            if (strVal.ToLower() != "country")
                            {
                                fr.ErrorMsg += "\n Country Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 3:
                            if (strVal.ToLower() != "esmid")
                            {
                                fr.ErrorMsg += "\n ESMID Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 4:
                            if (strVal.ToLower() != "esmname")
                            {
                                fr.ErrorMsg += "\n ESM Name Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 5:
                            if (strVal.ToLower() != "esmemail")
                            {
                                fr.ErrorMsg += "\n ESM Email Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 6:
                            if (strVal.ToLower() != "esmphone")
                            {
                                fr.ErrorMsg += "\n ESM Phone Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 7:
                            if (strVal.ToLower() != "ccmid")
                            {
                                fr.ErrorMsg += "\n CCMID Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 8:
                            if (strVal.ToLower() != "ccmname")
                            {
                                fr.ErrorMsg += "\n CCM NAme Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 9:
                            if (strVal.ToLower() != "ccmemail")
                            {
                                fr.ErrorMsg += "\n CCM Email Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 10:
                            if (strVal.ToLower() != "ccmphone")
                            {
                                fr.ErrorMsg += "\n CCM Phone Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 11:
                            if (strVal.ToLower() != "rsmid")
                            {
                                fr.ErrorMsg += "\n RSMID Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 12:
                            if (strVal.ToLower() != "rsmname")
                            {
                                fr.ErrorMsg += "\n RSM Name Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 13:
                            if (strVal.ToLower() != "rsmemail")
                            {
                                fr.ErrorMsg += "\n RSM Email Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 14:
                            if (strVal.ToLower() != "rsmphone")
                            {
                                fr.ErrorMsg += "\n RSM Phone Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                    }
                }
            }

            return fr;
        }


        public static FileReading ReadCustomerCSVFile(string inputFilePath, string FileName)
        {
            try
            {
                List<CustomerModel> customerList = new List<CustomerModel>();
                string _path = Path.Combine(inputFilePath, FileName);

                var contents = File.ReadAllText(_path).Split('\n');
                FileReading fileDataObj = new FileReading();
                fileDataObj.IsValid = true;
                fileDataObj.FileName = FileName;
                int i = 0;
                foreach (string line in contents)
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    string lineVal = line.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ').Replace('\\', ' ').Replace("\"", "");
                    if (string.IsNullOrEmpty(lineVal) || lineVal == " ") continue;

                    if (i == 0)
                    {
                        fileDataObj = IsValidCustomerCSVFile(lineVal);

                        if (!fileDataObj.IsValid)
                        {
                            return fileDataObj;
                        }
                    }

                    string[] lineValues = lineVal.Split(',');
                    if (i != 0)
                    {
                        CustomerModel customerRec = new CustomerModel();
                        for (int ind = 0; ind <= lineValues.Count() - 1; ind++)
                        {
                            string str = lineValues[ind].Trim();
                            switch (ind)
                            {
                                case 0:
                                    customerRec.CustomerId = string.IsNullOrEmpty(str) ? "" : str;
                                    break;
                                case 1:
                                    customerRec.CustomerName = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 2:
                                    customerRec.Address = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 3:
                                    customerRec.Address2 = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 4:
                                    customerRec.Address3 = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 5:
                                    customerRec.City = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 6:
                                    customerRec.State = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 7:
                                    customerRec.ZipCode = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 8:
                                    customerRec.PhoneNumber = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 9:
                                    customerRec.Route = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 10:
                                    customerRec.Branch = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                case 11:
                                    customerRec.RouteCode = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                    break;
                                    /*case 9:
                                       customerRec.BusinessUnit = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        break;
                                    case 10:
                                        customerRec.PricingParentId = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        break;
                                    case 11:
                                        customerRec.PricingParentDesc = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        break;
                                    case 12:
                                        customerRec.LastSaleDate = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        break;
                                    case 13:
                                        customerRec.Route = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        break;
                                    case 14:
                                        customerRec.Branch = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        break;
                                    case 15:
                                        customerRec.RouteCode = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        break;
                                    case 16:
                                        customerRec.ZoneNumber = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        break;
                                    case 17:
                                        customerRec.Division = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        break;
                                    case 18:
                                        customerRec.District = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        break;
                                    case 19:
                                        customerRec.CustomerRegion = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        break;
                                    case 20:
                                        customerRec.CustomerBranch = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        break;
                                    case 21:
                                        customerRec.BillingCode = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        break;*/
                            }
                        }
                        customerList.Add(customerRec);
                    }
                    i++;
                }

                fileDataObj.CustomerDataList = customerList;
                return fileDataObj;
            }
            catch (Exception ex)
            {
                FileReading fileDataObj = new FileReading();
                fileDataObj.FileName = FileName;
                fileDataObj.IsValid = false;
                fileDataObj.ErrorMsg = "Exception : " + ex;
                return fileDataObj;
            }
        }

        private static FileReading IsValidCustomerCSVFile(string HeaderRow)
        {
            FileReading fr = new FileReading();
            fr.ErrorMsg = "";
            fr.IsValid = true;

            string[] headerValues = HeaderRow.Split(',');

            for (var index = 0; index <= headerValues.Count() - 1; index++)
            {
                string hdrValue = headerValues[index].ToLower().Trim();

                switch (index)
                {
                    case 0:
                        if (hdrValue != "contactid")
                        {
                            fr.ErrorMsg += "\n ContactId Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 1:
                        if (hdrValue != "company")
                        {
                            fr.ErrorMsg += "\n Company Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 2:
                        if (hdrValue != "address1")
                        {
                            fr.ErrorMsg += "\n Address1 Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 3:
                        if (hdrValue != "address2")
                        {
                            fr.ErrorMsg += "\n Address2 Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 4:
                        if (hdrValue != "address3")
                        {
                            fr.ErrorMsg += "\n Address3 Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 5:
                        if (hdrValue != "city")
                        {
                            fr.ErrorMsg += "\n City Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 6:
                        if (hdrValue != "state")
                        {
                            fr.ErrorMsg += "\n State Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 7:
                        if (hdrValue != "postalcode")
                        {
                            fr.ErrorMsg += "\n PostalCode Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 8:
                        if (hdrValue != "phone")
                        {
                            fr.ErrorMsg += "\n Phone Number Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 9:
                        if (hdrValue.Trim() != "route")
                        {
                            fr.ErrorMsg += "\n Route Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 10:
                        if (hdrValue.Trim() != "branch")
                        {
                            fr.ErrorMsg += "\n Branch Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 11:
                        if (hdrValue.Trim() != "route code")
                        {
                            fr.ErrorMsg += "\n Route Code Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                        /*case 9:
                            if (hdrValue != "business unit")
                            {
                                fr.ErrorMsg += "\n Business Unit Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 10:
                            if (hdrValue != "pricingparentid")
                            {
                                fr.ErrorMsg += "\n PricingParentId Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 11:
                            if (hdrValue != "pricingparent description")
                            {
                                fr.ErrorMsg += "\n Pricingparent Description Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 12:
                            if (hdrValue != "lastsaledate")
                            {
                                fr.ErrorMsg += "\n LastSaleDate Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 13:
                            if (hdrValue.Trim() != "route")
                            {
                                fr.ErrorMsg += "\n Route Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 14:
                            if (hdrValue.Trim() != "branch")
                            {
                                fr.ErrorMsg += "\n Branch Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 15:
                            if (hdrValue.Trim() != "route code")
                            {
                                fr.ErrorMsg += "\n Route Code Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 16:
                            if (hdrValue.Trim() != "zone number")
                            {
                                fr.ErrorMsg += "\n Zone Number Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 17:
                            if (hdrValue.Trim() != "division")
                            {
                                fr.ErrorMsg += "\n Division Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 18:
                            if (hdrValue.Trim() != "district")
                            {
                                fr.ErrorMsg += "\n District Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 19:
                            if (hdrValue.Trim() != "customer region")
                            {
                                fr.ErrorMsg += "\n Customer Region Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 20:
                            if (hdrValue.Trim() != "customer branch")
                            {
                                fr.ErrorMsg += "\n Customer Branch Column Missing";
                                fr.IsValid = false;
                            }
                            break;
                        case 21:
                            if (hdrValue.Trim() != "billing code")
                            {
                                fr.ErrorMsg += "\n Billing code  Column Missing";
                                fr.IsValid = false;
                            }
                            break;*/
                }
            }

            return fr;
        }
    }
}
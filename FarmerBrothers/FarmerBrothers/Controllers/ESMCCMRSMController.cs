using FarmerBrothers.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace FarmerBrothers.Controllers
{
    public class ESMCCMRSMController : BaseController
    {
        // GET: ESMCCMRSM
        public ActionResult ESMCCMRSMMaintenance()
        {
            return View();
        }

        public ActionResult CustomerServiceEscalationMaintenance()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            //ESMCCMRSMModel.UpdateContacts(FarmerBrothersEntitites);
            //return null;

            try
            {
                if (file == null)
                {
                    ViewBag.Message = "No File Selected ";
                    ViewBag.isSuccess = false;
                    ViewBag.dataSource = new List<ESMCCMRSMModel>();
                    return View("ESMCCMRSMMaintenance");
                }

                else if (Path.GetExtension(file.FileName).ToLower() != ".csv")
                {
                    ViewBag.Message = "Selected file is not CSV file ";
                    ViewBag.isSuccess = false;
                    ViewBag.dataSource = new List<ESMCCMRSMModel>();
                    return View("ESMCCMRSMMaintenance");
                }

                if (file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string DirPath = Server.MapPath("~/UploadedFiles/ESMCCMRSM");

                    if (!Directory.Exists(DirPath))
                    {
                        Directory.CreateDirectory(DirPath);
                    }
                    string _inputPath = Path.Combine(DirPath, _FileName);
                    file.SaveAs(_inputPath);

                    //Thread.Sleep(5000);

                    //FileReading fileData = FileReading.ReadExcel(DirPath, _FileName);      
                    FileReading fileData = FileReading.ReadCSVFile(DirPath, _FileName);

                    if (fileData != null && fileData.IsValid)
                    {
                        ESMCCMRSMModel.InsertData(fileData.EsmDataList, FarmerBrothersEntitites);
                        ESMCCMRSMModel.UpdateContacts(FarmerBrothersEntitites);
                        string CompletedFilePath = Path.Combine(DirPath, "Completed");
                        if (!Directory.Exists(CompletedFilePath))
                        {
                            Directory.CreateDirectory(CompletedFilePath);
                        }

                        string _completedPath = Path.Combine(CompletedFilePath, _FileName);
                        if (System.IO.File.Exists(_completedPath))
                        {
                            System.IO.File.Delete(_completedPath);
                        }
                        System.IO.File.Move(_inputPath, _completedPath);

                        ViewBag.Message = "File Uploaded Successfully!!";
                        ViewBag.isSuccess = true;
                        ViewBag.dataSource = fileData.EsmDataList;
                    }
                    else
                    {
                        //sendEmail(esmData);
                        ViewBag.Message = "File upload failed!! " + "\n" + fileData.ErrorMsg;
                        ViewBag.isSuccess = false;
                        ViewBag.dataSource = new List<ESMCCMRSMModel>();
                    }
                }
                
                return View("ESMCCMRSMMaintenance");
            }
            catch(Exception ex)
            {
                ViewBag.Message = "File upload failed!! " + ex;
                ViewBag.isSuccess = false;
                ViewBag.dataSource = new List<ESMCCMRSMModel>();
                return View("ESMCCMRSMMaintenance");
            }
        }

        [HttpPost]
        public ActionResult CustomerServiceEscalationUploadFile(HttpPostedFileBase file)
        {
            try
            {
                if (file == null)
                {
                    ViewBag.Message = "No File Selected ";
                    ViewBag.isSuccess = false;
                    ViewBag.dataSource = new List<FBCustomerServiceDistributionModel>();
                    return View("CustomerServiceEscalationMaintenance");
                }

                else if (Path.GetExtension(file.FileName).ToLower() != ".csv")
                {
                    ViewBag.Message = "Selected file is not CSV file ";
                    ViewBag.isSuccess = false;
                    ViewBag.dataSource = new List<FBCustomerServiceDistributionModel>();
                    return View("CustomerServiceEscalationMaintenance");
                }

                if (file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string DirPath = Server.MapPath("~/UploadedFiles/CustomerServiceEscalation");

                    if (!Directory.Exists(DirPath))
                    {
                        Directory.CreateDirectory(DirPath);
                    }
                    string _inputPath = Path.Combine(DirPath, _FileName);
                    file.SaveAs(_inputPath);
  
                    FileReading fileData = FileReading.ReadEscalationCSVFile(DirPath, _FileName);

                    if (fileData != null && fileData.IsValid)
                    {
                        FBCustomerServiceDistributionModel.InsertData(fileData.CustomerServiceEscalationDataList, FarmerBrothersEntitites);
                        string CompletedFilePath = Path.Combine(DirPath, "Completed");
                        if (!Directory.Exists(CompletedFilePath))
                        {
                            Directory.CreateDirectory(CompletedFilePath);
                        }

                        string _completedPath = Path.Combine(CompletedFilePath, _FileName);
                        if (System.IO.File.Exists(_completedPath))
                        {
                            System.IO.File.Delete(_completedPath);
                        }
                        System.IO.File.Move(_inputPath, _completedPath);

                        ViewBag.Message = "File Uploaded Successfully!!";
                        ViewBag.isSuccess = true;
                        ViewBag.dataSource = fileData.CustomerServiceEscalationDataList;
                    }
                    else
                    {
                        //sendEmail(esmData);
                        ViewBag.Message = "File upload failed!! " + "\n" + fileData.ErrorMsg;
                        ViewBag.isSuccess = false;
                        ViewBag.dataSource = new List<FBCustomerServiceDistributionModel>();
                    }
                }

                return View("CustomerServiceEscalationMaintenance");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "File upload failed!! " + ex;
                ViewBag.isSuccess = false;
                ViewBag.dataSource = new List<FBCustomerServiceDistributionModel>();
                return View("CustomerServiceEscalationMaintenance");
            }
        }

    }

}
using FarmerBrothers.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FarmerBrothers.Data;
using System.Configuration;



namespace FarmerBrothers.Controllers
{
    public class AssetImageController : BaseController
    {
        public ActionResult AssetImage(string serialNumber, int assetId)
        {
            AssetImageModel imageModel = new AssetImageModel();

            IEnumerable<WorkorderEquipment> equipments = FarmerBrothersEntitites.WorkorderEquipments.Where(e => e.Assetid == assetId);
            foreach(WorkorderEquipment equipment in equipments)
            {
                string imageUrl = ConfigurationManager.AppSettings["ImageLocationUrl"].ToString() + equipment.WorkorderID + "/Equipment/SerialNumber/" + assetId + ".jpg";
                imageModel.ImageUrls.Add(imageUrl);
            }

            return View("AssetImage", "_Layout_WithOutMenu", imageModel);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FarmerBrothers
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "CustomerSearch", action = "CustomerSearch", id = UrlParameter.Optional }
            );

                    routes.MapRoute(
              "ClosestTechLookup",
              "{controller}/{action}",
              new { controller = "ClosestTechLookup", action = "ClosestTechLookup" });

                 

            routes.MapRoute(
         "WorkOrderInvoiceUpdate",
         "{controller}/{action}",
         new { controller = "WorkOrderInvoiceUpdate", action = "WorkOrderInvoiceUpdate" });

            routes.MapRoute(
         "GetWorkOrderInvoice",
         "{controller}/{action}/{workOrderId}",
         new { controller = "WorkOrderInvoiceUpdate", action = "GetWorkOrderInvoice",  workOrderId = -1 });

            routes.MapRoute(
         "SaveWorkOrderInvoice",
         "{controller}/{action}/{workOrderId}/{invoiceNumber}",
         new { controller = "WorkOrderInvoiceUpdate", action = "SaveWorkOrderInvoice",  workOrderId = -1, invoiceNumber = -1 });

            routes.MapRoute(
           "UnknownCustomerWorkOrder",
           "{controller}/{action}/{customerId}/{workOrderId}/{unknownCustomerId}",
           new { controller = "Workorder", action = "WorkorderManagement", customerId = -1, workOrderId = -1, unknownCustomerId = -1 });


            routes.MapRoute(
            "WorkOrderManagement",
            "{controller}/{action}/{customerId}/{workOrderId}/{isNewPartsOrder}/{showAllTechs}/{isCustomerDashboard}",
            new { controller = "Workorder", action = "WorkorderManagement", customerId = -1, workOrderId = -1, isNewPartsOrder= UrlParameter.Optional, showAllTechs =UrlParameter.Optional, isCustomerDashboard=UrlParameter.Optional });

            routes.MapRoute(
           "LoadWorkOrderManagement",
           "{controller}/{action}/{customerId}/{workOrderId}",
           new { controller = "Workorder", action = "WorkorderManagement", customerId = -1, workOrderId = -1, isNewPartsOrder = UrlParameter.Optional, showAllTechs = UrlParameter.Optional, isCustomerDashboard = UrlParameter.Optional });


            routes.MapRoute(
           "NonServiceEventManagement",
           "{controller}/{action}/{CustomerID}/{workOrderId}",
           new { controller = "NonServiceEvent", action = "NonServiceEventCall", customerId = UrlParameter.Optional, workOrderId = UrlParameter.Optional });

            routes.MapRoute(
             "AutoCallGenerate",
             "{controller}",
             new { controller = "AutoCallGenerate", action = "Index" });

            routes.MapRoute(
               name: "UserRole",
               url: "{controller}/{action}/{roleId}",
               defaults: new { controller = "Admin", action = "UserRoles", roleId = UrlParameter.Optional }
           );

        }
    }
}

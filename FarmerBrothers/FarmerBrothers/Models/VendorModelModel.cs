using FarmerBrothers.Data;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FarmerBrothers.Models
{
    public class VendorModelModel
    {
        public VendorModelModel(string model)
        {
            Model = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.ToUpper().Trim().ToLower()); 
        }
        public string Model { get; set; }
    }

    public class ExpendableProduct
    {
        public ExpendableProduct(string product)
        {
            Product = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(product.ToUpper().Trim().ToLower()); 
        }
        public string Product { get; set; }

    }
    public class EquipmentProduct
    {
        public EquipmentProduct(string product)
        {
            Product = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(product.ToUpper().Trim().ToLower()); 
        }
        public string Product { get; set; }
    }
    
    public class TransactionTypeModel
    {
        public TransactionTypeModel(string model)
        {
            Model = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.ToUpper().Trim().ToLower()); 
        }
        public string Model { get; set; }
        
    }

    public class EquipmentTypeModel
    {
        public EquipmentTypeModel(string model)
        {
            Model = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.ToUpper().Trim().ToLower());
        }
        public string Model { get; set; }
    }

    public class SubstituionModel
    {
        public SubstituionModel(string model)
        {
            Model = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.ToUpper().Trim().ToLower());
        }
        public string Model { get; set; }
    }

    public class ErfEqpViewModel
    {
        public ErfEqpViewModel(int modelId, string modelName)
        {
            ModelName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(modelName.ToUpper().Trim().ToLower());
            ModelId = modelId;
        }
        public string ModelName { get; set; }
        public int ModelId { get; set; }
    }

    public class UserRole
    {
        public UserRole(string htmlrole,string roleId)
        {
            Role = roleId;
            RolesInHtml = htmlrole;
            using (FarmerBrothersEntities entities = new FarmerBrothersEntities())
            {
                Roles = entities.FbRoles.ToList();
            }
        }
        public string Role { get; set; }
        public IList<FbRole> Roles { get; set; }
        public string RolesInHtml { get; set; }
    }


    public class ERFEqpModel
    { 
        public IList<ContingentDetail> ContingentDetails { get; set; }
        public int id { get; set; }
        public int ContingentId { get; set; }
        public decimal LaidInCost { get; set; }
        public decimal CashSale { get; set; }
        public decimal Rental { get; set; }
    }
}
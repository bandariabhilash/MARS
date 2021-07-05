using FarmerBrothers.Data;

namespace FarmerBrothers.Models
{
    public class SkuModel
    {
        public SkuModel(Sku sku)
        {
            Skuid = sku.Skuid;
            Sku = sku.Sku1;
        }

        public int Skuid { get; set; }
        public string Sku { get; set; }
    }
}
namespace Motorbike.ViewModels
{
    public class ProductDetailViewModel
    {
        public Models.Motorbike Motorbike { get; set; }
        public List<Models.Motorbike> RelatedProducts { get; set; } = new List<Models.Motorbike>();
    }
}
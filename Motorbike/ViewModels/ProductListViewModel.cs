// ProductListViewModel.cs
namespace Motorbike.ViewModels
{
    public class ProductListViewModel
    {
        public List<Models.Motorbike> Motorbikes { get; set; } = new List<Models.Motorbike>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int BrandId { get; set; }
        public string SortOrder { get; set; } = "";
        public string SearchTerm { get; set; } = ""; // Thêm thuộc tính để lưu từ khóa tìm kiếm
    }

}
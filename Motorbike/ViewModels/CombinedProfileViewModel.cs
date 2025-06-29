using System.ComponentModel.DataAnnotations;

namespace Motorbike.ViewModels
{
    public class CombinedProfileViewModel
    {
        public ProfileViewModel ProfileInfo { get; set; }
        public ChangePasswordViewModel Password { get; set; }
    }
}
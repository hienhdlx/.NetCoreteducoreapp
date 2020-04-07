using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeduCoreApp.Application.ViewModels.Common;
using TeduCoreApp.Application.ViewModels.Systems;
using TeduCoreApp.Data.Enums;
using TeduCoreApp.Utilities.Extention;

namespace TeduCoreApp.Models
{
    public class CheckoutViewModel : BillViewModel
    {
        public List<ShoppingCartViewModel> Carts { get; set; }

        public List<EnumModel> PaymentMethods
        {
            get
            {
                return ((PaymentMethod[]) Enum.GetValues(typeof(PaymentMethod))).Select(x => new EnumModel
                {
                    Value = (int) x,
                    Name = x.GetDescription()
                }).ToList();
            }
        }
    }
}

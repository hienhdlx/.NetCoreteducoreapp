using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeduCoreApp.Application.ViewModels.Common;

namespace TeduCoreApp.Models
{
    public class ContactPageViewModel
    {
        public ContactViewModel Contact { get; set; }
        public FeedbackViewModel Feedback { get; set; }
    }
}

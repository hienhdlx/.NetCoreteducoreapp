using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.Common;
using TeduCoreApp.Models;
using TeduCoreApp.Services;
using TeduCoreApp.Utilities.Constants;

namespace TeduCoreApp.Controllers
{
    public class ContactController : Controller
    {
        private readonly IContactService _contactService;
        private readonly IFeedbackService _feedbackService;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly IViewRenderService _viewRenderService;

        public ContactController(IContactService contactService, IFeedbackService feedbackService,
            IEmailSender emailSender, IConfiguration configuration, IViewRenderService viewRenderService)
        {
            _configuration = configuration;
            _contactService = contactService;
            _emailSender = emailSender;
            _viewRenderService = viewRenderService;
            _feedbackService = feedbackService;
        }


        [Route("contact.html")]
        [HttpGet]
        public IActionResult Index()
        {
            var contact = _contactService.GetById(CommonConstants.DefaultContactId);
            var mode = new ContactPageViewModel {Contact = contact};
            return View(mode);
        }

        [Route("contact.html")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Index(ContactPageViewModel model)
        {
            if (ModelState.IsValid)
            {
                _feedbackService.Add(model.Feedback);
                _feedbackService.SaveChanges();
                //var content = await _viewRenderService.RenderToStringAsync("Contact/_ContactMail", model.Feedback);
                //await _emailSender.SendEmailAsync(_configuration["MailSettings:AdminMail"], "Have new contact feedback", content);
                ViewData["Success"] = true;
            }

            model.Contact = _contactService.GetById(model.Contact.Id);
            return View("Index", model);
        }
    }
}
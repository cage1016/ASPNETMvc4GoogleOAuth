using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ASPNETMvc4GoogleOAuth.Models;
using ASPNETMvc4GoogleOAuth.Services;

namespace ASPNETMvc4GoogleOAuth.Controllers
{
    public class EventController : Controller
    {
        private IGEventService service;

        public EventController()
        {
            service = new GEventService(this.ModelState);
        }

        //
        // GET: /Event/

        public ActionResult Index()
        {
            if (Session["googletoken"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }


        // AJAX
        // POST:/Event/GetAll
        [HttpPost]
        public JsonResult GetAll()
        {
            var gevents = service.GetAll();

            return Json(gevents);
        }

         // AJAX
        // POST /Event/FetchGoogleCalendar
        [HttpPost]
        public JsonResult FetchGoogleCalendar()
        {
            string token = Session["googletoken"] as string;
            var calendarList = service.GetAllGoogleCalendar(token);
            return Json(calendarList);
        }

        // AJAX
        // POST /Event/Create
        [HttpPost]
        public JsonResult Create(string calendarId, string requestBody)
        {
            string token = Session["googletoken"] as string;
            var e = service.Insert(token, calendarId, requestBody);

            return Json(e);
        }

        // AJAX
        // POST:/Event/GetEvent
        [HttpPost]
        public ActionResult GetEventByID(string id)
        {
            Guid _id;
            Guid.TryParse(id, out _id);
            if (_id == default(Guid))
                return Redirect("Index");

            var devent = service.FindAll().Where(x => x.guid == _id).SingleOrDefault();

            return Json(devent);
        }

        // AJAX
        // POST:/Event/UpdateEventByID
        [HttpPost]
        public JsonResult UpdateEventByID(string guid, string calendarId, string id, string requestBody)
        {
            string token = Session["googletoken"] as string;
            var updatedGvent = service.Update(token, guid, calendarId, id, requestBody);

            return Json(updatedGvent);
        }

        // AJAX
        // POST:/Event/DeleteEventByID
        [HttpPost]
        public JsonResult DeleteEventByID(string guid, string calendarId, string id)
        {
            string token = Session["googletoken"] as string;

            var b = service.Delete(token, guid, calendarId, id);

            return Json(b);
        }
    }
}
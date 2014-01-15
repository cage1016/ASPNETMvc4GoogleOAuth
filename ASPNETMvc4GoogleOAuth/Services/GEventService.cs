using ASPNETMvc4GoogleOAuth.Models;
using Google.Apis.Calendar.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ASPNETMvc4GoogleOAuth.Services
{
    public interface IGEventService
    {
        IEnumerable<GEvent> GetAll();
        IEnumerable<CalendarListEntry> GetAllGoogleCalendar(string token);

        bool Insert(string calendarId, string requestBody);
        bool Update(GEvent gEvent);
        bool Delete(Guid guid);
    }

    public class GEventService : IGEventService
    {
        private GEventContext db;
        private ModelStateDictionary _modelState;

        public GEventService(ModelStateDictionary modelState)
        {
            _modelState = modelState;
            db = new GEventContext();
        }

        protected void Dispose(bool disposing)
        {
            db.Dispose();
            //base.Dispose(disposing);
        }

        public IEnumerable<GEvent> GetAll()
        {
            return db.GEvents.ToList();
        }

        private Event CreateGoogleEvent(string token, string calendarId, string requestBody)
        {
            var jsonSerializer = new JavaScriptSerializer();
            var request = WebRequest.Create("https://www.googleapis.com/calendar/v3/calendars/" + calendarId + "/events") as HttpWebRequest;
            request.KeepAlive = true;
            request.ContentType = "application/json";
            request.Method = "POST";
            request.Headers.Add("Authorization", "Bearer " + token);

            Stream ws = request.GetRequestStream();
            using (var streamWriter = new StreamWriter(ws, new UTF8Encoding(false)))
            {
                streamWriter.Write(requestBody);
            }

            var response = request.GetResponse();
            var stream = new StreamReader(response.GetResponseStream());

            var googleEvent = Newtonsoft.Json.JsonConvert.DeserializeObject<Event>(stream.ReadToEnd().Trim());

            return googleEvent;
        }

        public IEnumerable<CalendarListEntry> GetAllGoogleCalendar(string token)
        {
            var jsonSerializer = new JavaScriptSerializer();
            var request = WebRequest.Create("https://www.googleapis.com/calendar/v3/users/me/calendarList") as HttpWebRequest;
            request.Headers.Add("Accept-Charset", "utf-8");
            request.KeepAlive = true;
            request.Method = "GET";
            request.Headers.Add("Authorization", "Bearer " + token);

            // fetch all calendars
            try
            {
                WebResponse response = request.GetResponse();
                var stream = new StreamReader(response.GetResponseStream());
                var calendar = Newtonsoft.Json.JsonConvert.DeserializeObject<CalendarList>(stream.ReadToEnd().Trim());

                return calendar.Items.Where(x => x.AccessRole == "owner");
            }
            catch (Exception ex)
            {
                return null;
            }            
        }

        public bool Insert(string calendarId, string requestBody)
        {
            throw new NotImplementedException();
        }

        public bool Update(GEvent gEvent)
        {
            var dGEvent = db.GEvents.Where(x => x.guid == gEvent.guid).SingleOrDefault();
            if (dGEvent != null)
            {
                dGEvent.summary = gEvent.summary;
                dGEvent.description = gEvent.description;
                dGEvent.start = gEvent.start;
                dGEvent.startDateTime = gEvent.startDateTime;
                dGEvent.end = gEvent.end;
                dGEvent.endDateTime = gEvent.endDateTime;
                    
                db.SaveChanges();
            }

            return true;
        }

        public bool Delete(Guid guid)
        {
            var dGEvent = db.GEvents.Where(x => x.guid == guid).SingleOrDefault();
            if (dGEvent != null)
            {
                db.GEvents.Remove(dGEvent);
                db.SaveChanges();
            }

            return true;
        }
    }
}
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
        IQueryable<GEvent> FindAll();

        IEnumerable<GEvent> GetAll();
        IEnumerable<CalendarListEntry> GetAllGoogleCalendar(string token);

        GEvent Insert(string token, string calendarId, string requestBody);
        GEvent Update(string token, string guid, string calendarId, string id, string requestBody);
        bool Delete(string token, string guid, string calendarId, string id);
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

        public IQueryable<GEvent> FindAll()
        {
            return db.GEvents;
        }

        public IEnumerable<GEvent> GetAll()
        {
            return db.GEvents.ToList();
        }

        private Event GoogleEventHandle(string token, string method, string requestURL, string requestBody = null)
        {
            var jsonSerializer = new JavaScriptSerializer();
            var request = WebRequest.Create(requestURL) as HttpWebRequest;
            request.KeepAlive = true;
            request.ContentType = "application/json";
            request.Method = method;
            request.Headers.Add("Authorization", "Bearer " + token);

            if(requestBody != null)
            {
                Stream ws = request.GetRequestStream();
                using (var streamWriter = new StreamWriter(ws, new UTF8Encoding(false)))
                {
                    streamWriter.Write(requestBody);
                }
            }

            var response = request.GetResponse();
            var stream = new StreamReader(response.GetResponseStream());

            var googleEvent = Newtonsoft.Json.JsonConvert.DeserializeObject<Event>(stream.ReadToEnd().Trim());

            return googleEvent;  
        }

        private Event CreateGoogleEvent(string token, string calendarId, string requestBody)
        {
            var requestURL = string.Format("https://www.googleapis.com/calendar/v3/calendars/{0}/events", calendarId);
            return GoogleEventHandle(token, "POST", requestURL, requestBody);              
        }

        private Event UpdateGoogleEvent(string token, string guid, string calendarId, string id, string requestBody)
        {
            var requestURL = string.Format("https://www.googleapis.com/calendar/v3/calendars/{0}/events/{1}", calendarId, id);
            return GoogleEventHandle(token, "PUT", requestURL, requestBody);
        }

        private Event DeleteGoogleEvent(string token, string calendarId, string id ) {
            var requestURL = string.Format("https://www.googleapis.com/calendar/v3/calendars/{0}/events/{1}", calendarId, id);
            return GoogleEventHandle(token, "DELETE", requestURL);
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

        public GEvent Insert(string token, string calendarId, string requestBody)
        {
            var e = CreateGoogleEvent(token, calendarId, requestBody);

            GEvent newGevent = new GEvent
            {
                guid = Guid.NewGuid(),
                calendarId = calendarId,
                Id = e.Id,
                summary = e.Summary,
                description = e.Description,
                start = e.Start.Date,
                startDateTime = e.Start.DateTime,
                end = e.End.Date,
                endDateTime = e.End.DateTime
            };

            db.GEvents.Add(newGevent);
            db.SaveChanges();

            return newGevent;
        }

        public GEvent Update(string token, string guid, string calendarId, string id, string requestBody)
        {
            // 1. update google calendar event via API
            var e = UpdateGoogleEvent(token, guid, calendarId, id, requestBody);

            Guid _guid;
            Guid.TryParse(guid, out _guid);

            var dGEvent = db.GEvents.Where(x => x.guid == _guid).SingleOrDefault();
            if (dGEvent != null)
            {
                dGEvent.summary = e.Summary;
                dGEvent.description = e.Description;
                dGEvent.start = e.Start.Date;
                dGEvent.startDateTime = e.Start.DateTime;
                dGEvent.end = e.End.Date;
                dGEvent.endDateTime = e.End.DateTime;
                    
                db.SaveChanges();
            }

            return dGEvent;
        }

        public bool Delete(string token, string guid, string calendarId, string id)
        {
            var e = DeleteGoogleEvent(token, calendarId, id);

            if (e == null)
            {
                Guid _guid;
                Guid.TryParse(guid, out _guid);
                var dGEvent = db.GEvents.Where(x => x.guid == _guid).SingleOrDefault();
                if (dGEvent != null)
                {
                    db.GEvents.Remove(dGEvent);
                    db.SaveChanges();
                }
            }
            return true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ASPNETMvc4GoogleOAuth.Models
{
    public class GEvent
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public Guid guid { get; set; }
        public string Id { get; set; }
        public string calendarId { get; set; }
        public string start { get; set; }
        public Nullable<DateTime> startDateTime { get; set; }
        public string end { get; set; }
        public Nullable<DateTime> endDateTime { get; set; }
        public string summary { get; set; }
        public string description { get; set; }
       
    }
}
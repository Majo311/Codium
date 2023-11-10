using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Documents;

namespace Codium.Models
{
    public class Event
    {
        public int Id { get; set; }
        public int ProviderEventID { get; set; }
        public string EventName { get; set; }
        public string EventDate { get; set; }
        public List<Odd> OddsList { get; set; }
        public Event(int ProviderEventID, string EventName, string EventDate)
        {
            this.ProviderEventID = ProviderEventID;
            this.EventName = EventName;
            this.EventDate = EventDate;
            //this.OddsList = OddsList;
        }
    }
}

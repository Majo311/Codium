using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace Codium.Models
{
    public class Event
    {
        public int ProviderEventID { get; set; }
        public string EventName { get; set; }
        public string EventDate { get; set; }
        public List<Odd> OddsList { get; set; }
    }
}

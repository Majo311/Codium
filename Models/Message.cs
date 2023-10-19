using System;


namespace Codium.Models
{
    public class Message
    {
        public string MessageID { get; set; }
        public string GeneratedDate { get; set; }
        public Event Event { get; set; }
        public Message(string messageID,string GenerateData,Event Event)
        {
            this.MessageID = messageID;
            this.GeneratedDate = GenerateData;
            this.Event = Event;
        }
    }
   
}

using Codium.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Codium
{
    public static  class Extensions
    {
        public static Event? GetEvent(this JToken JToken)
        {
            Event result = new Event((int)JToken["ProviderEventID"], JToken["EventName"].ToString(), JToken["EventDate"].ToString());
            JArray array = (JArray)JToken["OddsList"];
            List<Odd> oddList = array.Select(x => new Odd((int)x["ProviderOddsID"],
                                                               x["OddsName"].ToString(),
                                                               (float)x["OddsRate"],
                                                               x["Status"].ToString())).ToList();
            result.OddsList = new System.Collections.Concurrent.ConcurrentQueue<Odd>(oddList);
            return result;
        }
    }
}

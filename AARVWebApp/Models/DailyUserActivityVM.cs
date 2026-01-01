using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AARVWebApp.Models
{
    public class DailyUserActivityVM
    {

            public string Username { get; set; }
            public DateTime LoginTime { get; set; }
            public string IP { get; set; }
            public string Browser { get; set; }
            public int TotalLogins { get; set; }
        
    }


}
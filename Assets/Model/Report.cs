using System;

namespace Assets.Model
{
    public class Report
    {
        public DateTime Time { get; set; }
        public string PatientId { get; set; }
        public Session SessionWithoutDisturbances { get; set; }
        public Session SessionWithDisturbances { get; set; }
    }
}

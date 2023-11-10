using System;


namespace Codium.Models
{
    public enum Status
    {
        active,
        suspended
    }
    public class Odd
    {
        public int Id { get; set; }
        public int ProviderOddsID { get; set; }
        public string MessageID { get; set; }
        public string OddsName {  get; set; }
        public float OddsRate {  get; set; }
        public string Status { get; set; }
        public Odd(int ProviderOddsID, string OddsName, float OddsRate, string Status) 
        { 
            this.ProviderOddsID = ProviderOddsID;
            this.OddsName = OddsName;
            this.OddsRate = OddsRate;
            this.Status = Status;
        }
    }
}

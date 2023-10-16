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
        public int ProviderOddsID { get; set; }
        public int ProviderEventID { get; set; }
        public string Name {  get; set; }
        public float Rate {  get; set; }
        public Status Status { get; set; }
    }
}

namespace UKMCAB.Core.Models
{
    public class CABData
    {
        public string CABId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Phone { get; set; }
        public string Country { get; set; }
        public string BodyType { get; set; }
        public List<string> Regulation { get; set; }
    }
}

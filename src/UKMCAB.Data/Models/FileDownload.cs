namespace UKMCAB.Data.Models
{
    public class FileDownload
    {
        public string ContentType { get; set; }
        public string ContentDisposition { get; set; }
        public Stream FileStream { get; set; }
    }
}

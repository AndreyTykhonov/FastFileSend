namespace FastFileSend.Main
{
    public class FileItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public int CRC32 { get; set; }
        public System.DateTime CreationDate { get; set; }
        public string Url { get; set; }
    }
}

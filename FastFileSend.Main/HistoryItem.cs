namespace FastFileSend.Main
{
    public class HistoryItem
    {
        public int Id { get; set; }
        public int Receiver { get; set; }
        public int Sender { get; set; }
        public int Status { get; set; }
        public FileItem File { get; set; }
    }
}

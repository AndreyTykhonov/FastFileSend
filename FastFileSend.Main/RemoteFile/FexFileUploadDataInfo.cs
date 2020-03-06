namespace FastFileSend.Main.RemoteFile
{
    /// <summary>
    /// Used by FileUploader. Contains Fex.net information.
    /// </summary>
    #pragma warning disable CA1812 // used by newtonsoft
    class FexFileUploadDataInfo
    {
        public string anon_upload_link { get; set; }
        public long anon_upload_root_id { get; set; }
        public string location { get; set; }

        public override string ToString()
        {
            return $"{anon_upload_link}:{anon_upload_root_id}:{location}";
        }
    }
}

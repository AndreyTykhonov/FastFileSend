using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FastFileSend.Main
{
    public class ProgressableFile
    {
        protected HttpClient HttpClient { get; set; } = new HttpClient();

        private long position;

        protected DateTime? DownloadStartedTime { get; set; } = null;
        protected long Size { get; set; }
        protected long Position
        {
            get { return position; }
            set
            {
                position = value;
                Report(position);
            }
        }

        public event Action<double, double> OnProgress = delegate { };

        void Report(long downloaded)
        {
            if (!DownloadStartedTime.HasValue)
            {
                DownloadStartedTime = DateTime.Now;
            }

            TimeSpan elapsedTime = DateTime.Now.Subtract((DateTime)DownloadStartedTime);
            double speedMB = downloaded / Math.Max(elapsedTime.TotalSeconds, 1);
            OnProgress((double)downloaded / Size, speedMB);
        }
    }
}

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Windows.Forms;

namespace AutoJunk
{
    public partial class update : Form
    {
        public update()
        {
            InitializeComponent();
            label1.Text = GetUpdate("http://mangekyoukraken.mygamesonline.org/version/new/updateInfo.html");
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var linkOr = GetLink("http://mangekyoukraken.mygamesonline.org/link");
            System.Diagnostics.Process.Start(linkOr); //redirect to download page (github) after clicking.
        }
        private static string GetUpdate(string url)
        {
            using (var client = new WebClient())
            using (var stream = client.OpenRead(url))
            using (var textReader = new StreamReader(stream, Encoding.UTF8, true))
            {
                var versionUpdate = HttpUtility.HtmlDecode(textReader.ReadToEnd());
                return versionUpdate;
            }
        }
        private static string GetLink(string url)
        {
            using (var client = new WebClient())
            using (var stream = client.OpenRead(url))
            using (var textReader = new StreamReader(stream, Encoding.UTF8, true))
            {
                var versionUpdate = HttpUtility.HtmlDecode(textReader.ReadToEnd());
                return versionUpdate;
            }
        }
    }
}

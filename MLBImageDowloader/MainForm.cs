using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace MLBImageDowloader
{
    public partial class MainForm : Form
    {

        #region Variables
        string Path = AppDomain.CurrentDomain.BaseDirectory;
        private string Id = "";
        #endregion

        public MainForm()
        {
            InitializeComponent();
        }


        private void reset()
        {
            foreach (Control c in this.Controls)
            {
                if (c.GetType() == typeof(PictureBox))
                {
                    PictureBox pb = c as PictureBox;
                    if (pb.Tag.ToString().Contains("selected"))
                    {
                        pb.Image = null;
                        pb.Tag = "";
                    }
                }

            }
            Id = "";
            txtUrl.Text = "";
            this.Invalidate();
        }

        /// <summary>
        /// Devolve a resposta da URL em formato JSON
        /// *nao requer token*
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        public static JObject getJsonResultFromUrl(string URL)
        {
            string html = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.Method = WebRequestMethods.Http.Get;
            request.Accept = "application/json";
            request.Headers.Add("x-format-new", "true");
            request.AutomaticDecompression = DecompressionMethods.GZip;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return JObject.Parse(reader.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public void parseData(string url)
        {
            Match match = Regex.Match(url, @"[a-zA-Z]{3}\-\d{9}");
            if (match.Success)
            {
                Id = match.Captures[0].Value.Replace("-", "");
                var json = getJsonResultFromUrl("https://api.mercadolibre.com/items/"+Id);
                JArray pics =JArray.Parse( json["pictures"].ToString());
                List<string> imgs = new List<string>();

                foreach (var item in pics)
                {
                    imgs.Add((string)item["secure_url"]);
                    PictureBox pic = (PictureBox)Controls["pbImg"+imgs.Count];
                    pic.Load((string)item["secure_url"]);
                    pic.Tag = "selected";
                }

                lbLog.Items.Add(imgs.Count + " Imagens Encontradas");
            }

           
  
            //[a-zA-Z]{3}\-\d{9}
        }

        #region Events
        private void txtUrl_TextChanged(object sender, EventArgs e)
        {
            if (Uri.IsWellFormedUriString(this.txtUrl.Text, UriKind.Absolute))
            {
                parseData(this.txtUrl.Text);
            }
        }
        private void pbImg_Paint(object sender, PaintEventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            if (pb.Tag.ToString().Contains("selected"))
            {
                ControlPaint.DrawBorder(e.Graphics, pb.ClientRectangle, Color.Red, ButtonBorderStyle.Solid);
            }
        }

        private void pbImg_Click(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            if (pb.Tag.ToString().Contains("selected"))
            {
                pb.Tag = "";
            }
            else
            {
                pb.Tag = "selected";
            }
            pb.Invalidate();
        }

        private void btnSaveImages_Click(object sender, EventArgs e)
        {
            lbLog.Items.Add("Salvando imagens em :");
            lbLog.Items.Add(Path+Id);

            bool exists = System.IO.Directory.Exists(Path+Id);

            if (!exists)
                System.IO.Directory.CreateDirectory(Path + Id);

            int i = 1;
            foreach (Control c in this.Controls)
            {
                if (c.GetType() == typeof(PictureBox))
                {
                    PictureBox pb = c as PictureBox;
                    if (pb.Tag.ToString().Contains("selected"))
                    {
                        pb.Image.Save(Path + Id +"/"+ i+".jpeg", ImageFormat.Jpeg);
                        i++;
                    }
                }
              
            }
            lbLog.Items.Add(i+" Imagens foram salvas");
            reset();
        }
        #endregion


    }
}

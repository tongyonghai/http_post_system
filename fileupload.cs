using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace http_post_files
{
    public partial class fileupload : Form
    {
        public fileupload()
        {
            InitializeComponent();
            m_address = "http://192.168.7.8:9999/yin/solidworks/Upload/";
        }
        string m_address = null;
        void FileUpload(string m_fileNamePath)
        {
            DateTime start = DateTime.Now;

            // 要上传的文件
            FileStream oFileStream = new FileStream(m_fileNamePath, FileMode.Open, FileAccess.Read);
            BinaryReader oBinaryReader = new BinaryReader(oFileStream);

            // 根据uri创建HttpWebRequest对象
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(new Uri(m_address));
            httpReq.Method = "POST";
            httpReq.Credentials = new NetworkCredential(username, password);

            //对发送的数据不使用缓存
            httpReq.AllowWriteStreamBuffering = false;

            //设置获得响应的超时时间（半小时）
            httpReq.Timeout = 300000;
            //httpReq.Timeout = 5000;
            // httpReq.ReadWriteTimeout = 150000;
            httpReq.KeepAlive = false;
            httpReq.ProtocolVersion = HttpVersion.Version11;
            httpReq.AllowAutoRedirect = true;
            httpReq.CookieContainer = new CookieContainer();
            //ServicePointManager.Expect100Continue = true;
            //ServicePointManager.MaxServicePointIdleTime = 150000;
            httpReq.ContentType = "text/plain";
            long filelength = oFileStream.Length;
            httpReq.SendChunked = true;
            //在将 AllowWriteStreamBuffering 设置为 false 的情况下执行写操作时，必须将 ContentLength 设置为非负数，或者将 SendChunked 设置为 true。
            
            //每次上传4k
            int bufferLength = 4 * 1024;
            byte[] buffer = new byte[bufferLength];

            //已上传的字节数
            long offset = 0;

            //开始上传时间
            DateTime startTime = DateTime.Now;
            int size = oBinaryReader.Read(buffer, 0, bufferLength);
            Stream postStream = httpReq.GetRequestStream();

            while (size > 0)
            {
                postStream.Write(buffer, 0, size);
                offset += size;
                size = oBinaryReader.Read(buffer, 0, bufferLength);
                //Console.Write(".");
            }
            //Console.WriteLine(".");
            postStream.Flush();
            postStream.Close();

            //获取服务器端的响应
            WebResponse webRespon = httpReq.GetResponse();
            Stream s = webRespon.GetResponseStream();
            StreamReader sr = new StreamReader(s);
            DateTime end = DateTime.Now;
            TimeSpan ts = end - start;
            //读取服务器端返回的消息
            String sReturnString = sr.ReadLine();
            string  info="retcode=" + sReturnString + " 花费时间=" + ts.TotalSeconds.ToString();
            richTextBox1.Text = info;
            s.Close();
            sr.Close();
        }
        static private string path = @"ftp://" + "192.168.7.8:9999" + "/yin/solidworks/Upload/";    //目标路径192.168.7.8
        static private string ftpip = "192.168.7.8";    //ftp IP地址
        static private string username = @"YINGROUP\wangzhiming";   //ftp用户名
        static private string password = "a123456";


        public void Upload(string filename)
        {
            FileInfo fileInf = new FileInfo(filename);
            string uri = path + fileInf.Name;
            FtpWebRequest reqFTP;

            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
            reqFTP.Credentials = new NetworkCredential(username, password);
            reqFTP.KeepAlive = true;
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
            reqFTP.UseBinary = true;
            reqFTP.UsePassive = true;
            reqFTP.ContentLength = fileInf.Length;
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            FileStream fs = fileInf.OpenRead();
            //string[] infos;
            //infos = reqFTP.GetResponse().Headers.AllKeys;
            //MessageBox.Show(infos[0]);
            try
            {
                Stream strm = reqFTP.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }
               // FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                strm.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
               
                throw new Exception("Ftphelper Upload Error --> " + ex.Message);
            }
            
        }


        private void Button1_Click(object sender, EventArgs e)
        {
            FileUpload(@"C:\Users\NancyMa\Desktop\21A-01000HS20.001.SLDASM");
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Upload(@"C:\Users\NancyMa\Desktop\21A-01000HS20.001.SLDASM");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SWFExtractorNS;
using Microsoft.Win32;
using System.Diagnostics;


namespace SWFExtratorClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Media _currentMedia = null;

        public MainWindow()
        {
            InitializeComponent();
           // test();
            
        }


        static string RunCmd(string command)
        {
            //例Process
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";           //确定程序名
            p.StartInfo.Arguments = "/c " + command;    //确定程式命令行
            p.StartInfo.UseShellExecute = false;        //Shell的使用
            p.StartInfo.RedirectStandardInput = true;   //重定向输入
            p.StartInfo.RedirectStandardOutput = true; //重定向输出
            p.StartInfo.RedirectStandardError = true;   //重定向输出错误
            p.StartInfo.CreateNoWindow = true;          //设置置不显示示窗口
            p.Start();
            return p.StandardOutput.ReadToEnd();        //输出出流取得命令行结果果
        }


        static void test()
        {
            Console.WriteLine("-------------------------------------------------------------");


            //string abc = "abcdefghijklmnopqrstuvwxyz";
            //byte[] bytes = Encoding.UTF8.GetBytes(abc);
            //for (int i = 0; i < bytes.Length; i++)
            //    Console.WriteLine(bytes[i]);
            //SWFExtractorNS.Extractors.LeExtractor.decode(bytes);

                //Console.WriteLine(SWFExtractorNS.Extractors.LeExtractor.decode("012345678 012345678 012345678"));

            //byte[] bytes = SWFExtractorNS.Extractors.LeExtractor.getBytesFromFile();
            //bytes = SWFExtractorNS.Extractors.LeExtractor.decode(bytes);
            //string info = SWFExtractorNS.Extractors.LeExtractor.getInfo(bytes);
            //Console.WriteLine(info);
           // SWFExtractorNS.Extractors.LeExtractor.video_info("27160609");



           // string str = "01234567";

//            Console.WriteLine(str.Substring(3,str.Length-3));

           // Console.WriteLine(RunCmd("you-get -i http://www.le.com/ptv/vplay/25892509.html"));
    
            Console.WriteLine("-------------------------------------------------------------");
        }


        private void btnExtract_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Media media = SWFExtractor.Extract(tbURL.Text);
                if (media == null || media.Streams == null)
                {
                    MessageBox.Show("解析失败!");
                    return;
                }

                IList<StreamAddress> addresses = new List<StreamAddress>();
                int streamIndex = 1;
                foreach (Stream stream in media.Streams)
                {
                    int addressIndex = 1;
                    foreach (String address in stream.Addresses)
                    {
                        StreamAddress addr = new StreamAddress();
                        addr.StreamIndex = streamIndex;
                        addr.StreamType = stream.Type;
                        addr.AddressIndex = addressIndex;
                        addr.Address = address;
                        addr.SourceStreamIndex = addresses.Count;
                        addresses.Add(addr);
                        addressIndex++;
                    }
                    streamIndex++;
                }
                lvStreamAddress.ItemsSource = addresses;
                _currentMedia = media;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int streamIndex = (int)btn.Tag;
            StreamAddress streamAddress = (StreamAddress)(lvStreamAddress.ItemsSource as List<StreamAddress>)[streamIndex];
            if (streamAddress.StreamType != StreamType.M3U8)
            {
                MessageBox.Show("不是M3U8，下载不支持");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "m3u8文件|*.m3u8";
            if (sfd.ShowDialog() == true)
            {
                String localPath = sfd.FileName;
                try
                {
                    if (SWFExtractor.DownloadM3U8(streamAddress.Address, sfd.FileName))
                    {
                        MessageBox.Show("下载成功");
                    }
                    else
                    {
                        MessageBox.Show("下载失败!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("下载失败!" + ex.ToString());
                }
            }
            else
            {
                MessageBox.Show("取消保存");
            }
        }
    }
}

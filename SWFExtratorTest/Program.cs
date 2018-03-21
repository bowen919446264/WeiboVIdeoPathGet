using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SWFExtractorNS;

namespace SWFExtratorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                VideoURL videoURL = SWFExtractor.Extract("http://www.hunantv.com/v/1/1/f/1827705.html");
                System.Console.WriteLine("Normal URL: " + videoURL.NormalURL);
                foreach (KeyValuePair<String, String> pair in videoURL.URLs)
                {
                    System.Console.WriteLine(pair.Key + ": " + pair.Value);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            
        }
    }
}

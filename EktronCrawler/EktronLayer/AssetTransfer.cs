using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EktronCrawler.EktronLayer
{
    public class AssetTransfer
    {
        public static byte[] GetAsset(string location)
        {
             var client = new EktronCrawler.AssetTransferServiceReference.AssetTransferServerClient();

            var lastWriteDate = new DateTime(2000, 1, 1);
            
            long size=0;
            string status = null;
            bool success;
            Stream data = null;

            client.GetAsset(lastWriteDate, location, out size, out status, out success, out data);

            using (var stream = new MemoryStream())
            {
                byte[] buffer = new byte[2048]; 
                int bytesRead;
                
                while ((bytesRead = data.Read(buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, bytesRead);
                }
                byte[] result = stream.ToArray();

                return result;
            }
                        
        }
    }
}

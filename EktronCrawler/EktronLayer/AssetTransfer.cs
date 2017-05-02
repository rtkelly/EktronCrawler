using EktronCrawler.AssetTransferServiceReference;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace EktronCrawler.EktronLayer
{
    public class Asset
    {
        public bool Success { get; set; }
        public long Size { get; set; }
        public string Status { get; set; }
        public byte[] AssetData { get; set; }
    }

    public class AssetTransfer
    {
        AssetTransferServerClient AssetServerClient;

        public AssetTransfer(string serviceEndPoint)
        {
            AssetServerClient = new AssetTransferServerClient();
            AssetServerClient.Endpoint.Address = new EndpointAddress(serviceEndPoint);
        }

        public  Asset GetAsset(string location)
        {
            
            var lastWriteDate = new DateTime(2000, 1, 1);
            
            long size=0;
            string status = null;
            bool success;
            Stream data = null;

            AssetServerClient.GetAsset(lastWriteDate, location, out size, out status, out success, out data);
            
            using (var stream = new MemoryStream())
            {
                byte[] buffer = new byte[2048]; 
                int bytesRead;
                
                while ((bytesRead = data.Read(buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, bytesRead);
                }
                
                var asset = new Asset()
                {
                    AssetData = stream.ToArray(),
                    Size = size,
                    Status = status,
                    Success = success,
                };

                return asset;
            }
                        
        }
    }
}

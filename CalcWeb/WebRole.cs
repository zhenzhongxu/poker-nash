using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameTreeDraft.Equity;
using Microsoft.Ajax.Utilities;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Web;

namespace CalcWeb
{
    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
             DownloadBlob();
             //LocalResource lr = RoleEnvironment.GetLocalResource("TempDatStorage");
             //PreComputedEquityCalculator.DefaultPath = lr.RootPath;
             //IEquityCalculator cac = PreComputedEquityCalculator.Instance;


            return base.OnStart();
        }

        public void DownloadBlob()
        {
           //  Retrieve storage account from connection string.
            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("popokrStorage"));


            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("equity");

            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob1 = container.GetBlockBlobReference("169HandOutcomes2Way.dat");
            
            CloudBlockBlob blockBlob3 = container.GetBlockBlobReference("169LinearRangeOutcome2Way.dat");
            CloudBlockBlob blockBlob4 = container.GetBlockBlobReference("169LinearRangeOutcome3Way.dat");

            CloudBlockBlob blockBlob5 = container.GetBlockBlobReference("169HandOutcomes3Way_LoopA.dat");
            CloudBlockBlob blockBlob6 = container.GetBlockBlobReference("169HandOutcomes3Way_LoopB.dat");
            CloudBlockBlob blockBlob7 = container.GetBlockBlobReference("169HandOutcomes3Way_LoopC.dat");

            LocalResource lr = RoleEnvironment.GetLocalResource("TempDatStorage");
            FileInfo fi1 = new FileInfo(Path.Combine(lr.RootPath, "169HandOutcomes2Way.dat"));
            
            FileInfo fi3 = new FileInfo(Path.Combine(lr.RootPath, "169LinearRangeOutcome2Way.dat"));
            FileInfo fi4 = new FileInfo(Path.Combine(lr.RootPath, "169LinearRangeOutcome3Way.dat"));

            FileInfo fi5 = new FileInfo(Path.Combine(lr.RootPath, "169HandOutcomes3Way_LoopA.dat"));
            FileInfo fi6 = new FileInfo(Path.Combine(lr.RootPath, "169HandOutcomes3Way_LoopB.dat"));
            FileInfo fi7 = new FileInfo(Path.Combine(lr.RootPath, "169HandOutcomes3Way_LoopC.dat"));
            if (fi1.Exists)
            {
                fi1.Delete();
            }
           
            if (fi3.Exists)
            {
                fi3.Delete();
            }
            if (fi4.Exists)
            {
                fi4.Delete();
            }
            if (fi5.Exists)
            {
                fi5.Delete();
            }
            if (fi6.Exists)
            {
                fi6.Delete();
            }
            if (fi7.Exists)
            {
                fi7.Delete();
            }
            blockBlob1.DownloadToFile(fi1.FullName, FileMode.CreateNew);
            
            blockBlob3.DownloadToFile(fi3.FullName, FileMode.CreateNew);
            blockBlob4.DownloadToFile(fi4.FullName, FileMode.CreateNew);

            blockBlob5.DownloadToFile(fi5.FullName, FileMode.CreateNew);
            blockBlob6.DownloadToFile(fi6.FullName, FileMode.CreateNew);
            blockBlob7.DownloadToFile(fi7.FullName, FileMode.CreateNew);
            
        }
    }
}

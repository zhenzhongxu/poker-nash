using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using GameTreeDraft.Equity;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace EquityCalc
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {

            try
            {
                int count = 0;
                while (count <= 10)
                {
                    using (var db = new WorkloadDataContext())
                    {
                        var result = db.p_GetNextWorkload().FirstOrDefault();
                        if (result != null)
                        {
                            FileInfo fi = RunCacluation(result.StartIndex, result.EndIndex);

                            var workload = db.Workloads.FirstOrDefault(t => t.Id == result.Id);
                            workload.Status = 2;
                            workload.EndTime = DateTime.UtcNow;
                            workload.OutputLocation = fi.Name;

                            db.SubmitChanges();

                        }
                        else
                        {
                            Thread.Sleep(30000);
                            count++;
                        }
                    }
                }

            }
            catch (System.Exception ex)
            {
                using (var db = new WorkloadDataContext())
                {
                    Exception exp = new Exception();
                    exp.Exception1 = ex.ToString();
                    exp.OccuredTime = DateTime.UtcNow;
                    db.Exceptions.InsertOnSubmit(exp);
                    db.SubmitChanges();
                }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("EquityCalc has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("EquityCalc is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("EquityCalc has stopped");
        }



        private FileInfo RunCacluation(int startIndex, int endIndex)
        {

            LocalResource lr = RoleEnvironment.GetLocalResource("TempDatStorage");
            FileInfo fi = new FileInfo(Path.Combine(lr.Name, String.Format("{0}_{1}_{2}.dat", "3WayFullOutcomes", startIndex.ToString("00000000"), endIndex.ToString("00000000"))));

            DirectoryInfo di = new DirectoryInfo(lr.Name);
            if (!di.Exists)
            {
                di.Create();
            }

            if (fi.Exists)
            {
                fi.Delete();
            }


            using (FileStream fs = fi.OpenWrite())
            {
                EquityGenerator gen = new EquityGenerator();

                gen.GenerateThreeWayOutcomes(fs, startIndex, endIndex);
            }
            UploadBlob(fi);
            return fi;
        }


        private void UploadBlob(FileInfo fi)
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("BlobStorage"));


            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("workload");

            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fi.Name);
            if (blockBlob.Exists())
            {
                blockBlob.Delete();
            }

            // Create or overwrite the "myblob" blob with contents from a local file.
            using (var fileStream = fi.OpenRead())
            {
                blockBlob.UploadFromStream(fileStream);
            }
        }


        //using (var db = new WorkloadDataContext())
        //       {
        //           int total = 818805;

        //           int iterations = total / 300;
        //           int startIndex = 0;
        //           int endIndex = startIndex + 299;
        //           for (int i = 1; i <= iterations; i++)
        //           {
        //               Workload wl = new Workload()
        //               {
        //                   WorkloadId = i ,
        //                   StartIndex = startIndex,
        //                   EndIndex = endIndex,
        //                   StartTime = null,
        //                   EndTime = null,
        //                   OutputLocation = null,
        //                   Status = 0
        //               };
        //               db.Workloads.InsertOnSubmit(wl);
        //               startIndex = endIndex + 1;
        //               endIndex = startIndex + 299;
        //           }

        //           Workload wl1 = new Workload()
        //           {
        //               WorkloadId = iterations + 1,
        //               StartIndex = startIndex,
        //               EndIndex = 818804,
        //               StartTime = null,
        //               EndTime = null,
        //               OutputLocation = null,
        //               Status = 1
        //           };
        //           db.Workloads.InsertOnSubmit(wl1);
        //           db.SubmitChanges();


    }
}

using System.IO;
using GameTreeDraft.Equity;
using GameTreeDraft.GameTree;
using GameTreeDraft.Hands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GameTreeDraft.ICM;
using Microsoft.WindowsAzure.ServiceRuntime;
using GameTreeDraft;

namespace CalcWeb
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            Globals.DefaultPath = RoleEnvironment.GetLocalResource("TempDatStorage").RootPath;
            Globals.HandRankingPath = HttpContext.Current.Server.MapPath("~/bin");
            Globals.TotalMissesTolerated = 40;
            PreComputedOutcomeCaculator2Way.DefaultPath = RoleEnvironment.GetLocalResource("TempDatStorage").RootPath;
           // PreComputedOutcomeCalculator3Way.DefaultPath = RoleEnvironment.GetLocalResource("TempDatStorage").RootPath;

        }

        protected void btnCalc_Click(object sender, EventArgs e)
        {
            litResult.Text = "Running...";
            string[] stacksStr = txtStacks.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] blindsStr = txtBlinds.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] payoutsStr = txtPayouts.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string anteStr = txtAnte.Text;


            DateTime now = DateTime.UtcNow;
            double[] stack = Array.ConvertAll(stacksStr, double.Parse);
            double[] payouts = Array.ConvertAll(payoutsStr, double.Parse);
            int sb = int.Parse(blindsStr[0]);
            int bb = int.Parse(blindsStr[1]);
            double ante = Convert.ToDouble(anteStr); 

            GameTreeNode.GlobalId = 0;
            var root = GameTreeFactory.Create(new GameInfo() { Sb = sb, Bb = bb, Payouts = payouts, Stacks = stack, Ante = ante });
            
            root.PopulatePossibleEndScenarios();

            NashOptimizer nashCalc = new NashOptimizer(root);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            bool linearConvered = false;
            for (int i = 0; i < 300; i++)
            {
                if (!linearConvered)
                {
                    linearConvered = nashCalc.CalculateOptimalLinearRange(false);
                }


                if (linearConvered)
                {
                    if (cbUnrestricted.Checked)
                    {
                        bool unstrictedConverged = false;
                        unstrictedConverged = nashCalc.CalculateOptimalUnrestrictedRange(false);
                        if (unstrictedConverged)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }           

            stopwatch.Stop();
            var result = nashCalc.GetCurrentEquity();
            StringBuilder strb = new StringBuilder();

            for (int i = 0; i < result.Length; i++)
            {
                var equity = Icm.GetEquity(result, payouts, i);
                strb.AppendFormat("Player {0} Equity: {1}  ({2})", i + 1, equity, equity / payouts.Sum());
                strb.AppendLine();
            }

            strb.AppendFormat("Elapsed: {0}ms", stopwatch.ElapsedMilliseconds);
            strb.AppendLine();

            strb.Append(root.PrintEquity());

            litResult.Text = strb.Replace("\n", "</br>").ToString() ;
        }


    }
}
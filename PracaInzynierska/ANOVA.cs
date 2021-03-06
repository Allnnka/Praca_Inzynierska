﻿using PracaInzynierska.DescriptiveStatistics;
using PracaInzynierska.Distribution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PracaInzynierska.Statystyka.Statystyki;

namespace PracaInzynierska
{
    public static class ANOVA
    {
        public struct AnovaResult
        {
            public double TestValue;
            public double Dfwg;
            public double Dfbg;

            public double MsWG;
            public double MsBG; 

            public double SsWG;
            public double SsBG;

            public double PValue;
        }
        public static AnovaResult OneWayAnalysisOfVariance(params IEnumerable<double>[] args)
        {
            if (args.Length == 1) throw new ArgumentException("Need at least two collection");
            foreach (IEnumerable<double> item in args)
            {
                if (item.Count()==0) throw new EmptyCollectionException();
            }
            int r = args.Length;
            int N=0;
            
            List<double> meanInEachGroup = new List<double>();
            double ssWG = 0;
            int nmean = 0;
            foreach (IEnumerable<double> list in args)
            {
                meanInEachGroup.Add(list.Average());
                for (int i = 0; i < list.Count(); i++)
                {
                    ssWG += (list.ElementAt(i) - meanInEachGroup.ElementAt(nmean))* (list.ElementAt(i) - meanInEachGroup.ElementAt(nmean));
                }
                nmean++;
                N += list.Count();
            }
            double overallMean = meanInEachGroup.Sum() / meanInEachGroup.Count();
            double ssBG = 0;
            for (int i = 0; i < meanInEachGroup.Count(); i++)
            {
                ssBG += (double)args[i].Count() * (meanInEachGroup.ElementAt(i) - overallMean) * (meanInEachGroup.ElementAt(i) - overallMean);
            }
            double dfBG = r - 1;
            double msBG = ssBG / dfBG;
            double dfWG = N-1-dfBG;
            double msWG = ssWG / dfWG;
            double statistic = msBG / msWG;
            double p = ContinuousDistribution.FCdf(statistic, (int)dfBG, (int)dfWG);

            return new AnovaResult
            {
                TestValue = Math.Round(statistic,3),
                Dfbg=dfBG,
                Dfwg=dfWG,
                MsBG= Math.Round(msBG,3),
                MsWG= Math.Round(msWG,3),
                SsBG= Math.Round(ssBG,3),
                SsWG= Math.Round(ssWG,3),
                PValue=Math.Round(p,4)
            };
        }
        //public static AnovaResult OneWayAnalysisOfVariance(params IEnumerable<double>[] args)
        //{


        //    return new AnovaResult
        //    {
        //        TestValue = Math.Round(statistic, 3),
        //        Dfbg = dfBG,
        //        Dfwg = dfWG,
        //        MsBG = msBG,
        //        MsWG = msWG,
        //        SsBG = ssBG,
        //        SsWG = ssWG,
        //        PValue = p
        //    };
        //}

        public static TestResult AnovaKruskalWalisTest(params IEnumerable<double>[] args)
        {
            if (args.Length == 1) throw new ArgumentException("Need at least two collection");
            int n = 0;
            List<double> list = new List<double>();
            foreach(IEnumerable<double> el in args)
            {
                n += el.Count();
                list=list.Concat(el).ToList();
            }
            list = list.OrderBy(x => Math.Abs(x)).ToList();
            
            Dictionary<double, double> dictOfPairs = Ranks.CalculateRanks(list);

            double sumRij = 0;
            double totalSum = 0;
            foreach (IEnumerable<double> el in args)
            {
               for(int i=0;i<el.Count();i++)
                {
                    sumRij+= dictOfPairs[Math.Abs(el.ElementAt(i))];
                }
                totalSum += (sumRij * sumRij) / el.Count();
                sumRij = 0;
            }
            double correctionForTied = 1.0 - (Ranks.SumOfTiedPairs(list) / (n * n * n - n));

            double kwScore = ((12.0 * totalSum) / (n * (n + 1.0)) - 3.0 * (n + 1.0));
            kwScore = kwScore / correctionForTied;
            int df = args.Length - 1;
            double pVal = 1.0 - ContinuousDistribution.ChiSquareCdf(kwScore, df);
            return new TestResult
            {
                TestValue = Math.Round(kwScore,4),
                PValue = Math.Round(pVal, 6),
                DegreesOfFreedom = df
            };
        }

        public static TestResult AnovaFriedmanaTest(params IEnumerable<double>[] args)
        {

            int n = args.FirstOrDefault().Count();
            int p= args.Length;
            if (args.Length == 1) throw new ArgumentException("Need at least two collection");
            foreach (IEnumerable<double> item in args)
            {
                if (n != item.Count()) throw new SizeOutOfRangeException();
            }
            List<double> list = new List<double>();
            List<double> sumRj = Enumerable.Repeat<double>(0, p).ToList();
            Dictionary<double, double> dictOfPairs= new Dictionary<double, double>();

            double sumCorrectionForTiedRanks = 0;
            for (int i = 0; i < n; i++)
            {
                foreach(IEnumerable<double> el in args)
                {
                    list.Add(el.ElementAt(i));
                }
                sumCorrectionForTiedRanks += Ranks.SumOfTiedPairs(list);
                dictOfPairs =Ranks.CalculateRanks(list);
                for(int j = 0; j < p; j++)
                {
                    sumRj[j]+= dictOfPairs[Math.Abs(list.ElementAt(j))];
                }
                list.Clear();
            }
            double totalSum = 0;
            foreach(double element in sumRj)
            {
                totalSum += element * element;
            }
            double kwScore = ((12.0 * totalSum) / (n*p * (p + 1.0)) - 3.0 * n*(p + 1.0));
            int df = (int)p- 1;
            double correctionForTied = 1.0 - sumCorrectionForTiedRanks / (n *( p * p*p - p));
            double statistics = kwScore / correctionForTied;
            double pVal = 1.0 - ContinuousDistribution.ChiSquareCdf(statistics, df);
            return new TestResult
            {
                TestValue = Math.Round(statistics, 4),
                PValue = Math.Round(pVal, 5),
                DegreesOfFreedom = df
            };
        }
    }
}

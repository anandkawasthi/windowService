using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace FileReader
{
    public static class TextReaderHelper
    {
        public static List<KeyValuePair<int, List<string>>> FirstLabelParsing(string path)
        {
            List<string> headerEob = new List<string>();

            List<string> OrginalDataToPrase = new List<string>();

            var DataBloks = new List<KeyValuePair<int, List<string>>>();

            string[] lines = File.ReadAllLines(path);

            bool readFlagforHeaderEob = false;

            foreach (string line in lines)
            {
                if (line.ToLower().Contains(FirstLabelRules.startToken1))
                {
                    FirstLabelRules.readLineStart = true;
                    readFlagforHeaderEob = false;
                }
                else
                {
                    if (line.ToLower().Contains(FirstLabelRules.startToken2) 
                        && FirstLabelRules.readLineStart == false)
                    {
                        readFlagforHeaderEob = true;
                    }
                    if (readFlagforHeaderEob)
                    {
                        headerEob.Add(line);
                    }
                }

                if (FirstLabelRules.readLineStart)
                {
                    OrginalDataToPrase.Add(line);
                }

            }

            FirstLabelRules.readLineStart = false;
            int firstCount = 0;
            var block = new List<string>();
            int blockCount = 1;

            DataBloks.Add(new KeyValuePair<int, List<string>>(blockCount, headerEob));


            foreach (string line in OrginalDataToPrase)
            {

                if (line.ToLower().Contains(FirstLabelRules.startToken2))
                {
                    FirstLabelRules.readLineStart = true;
                }
                if (firstCount > 1 & line.ToLower().Contains(FirstLabelRules.endToken))
                {
                    FirstLabelRules.readLineEnd = true;
                }
                if (FirstLabelRules.readLineStart && !FirstLabelRules.readLineEnd)
                {
                    firstCount += 1;
                    block.Add(line);
                }
                if (FirstLabelRules.readLineStart && FirstLabelRules.readLineEnd)
                {
                    blockCount += 1;
                    DataBloks.Add(new KeyValuePair<int, List<string>>(blockCount, block));
                    FirstLabelRules.readLineStart = false;
                    FirstLabelRules.readLineEnd = false;
                    firstCount = 0;
                    block = new List<string>();


                }
            }

            return DataBloks;

        }

        public static List<Eob> SecondLabelParsing(List<KeyValuePair<int, List<string>>> keyValuePairs)
        {
            Regex regex = new Regex("[ ]{2,}", RegexOptions.None);

            var FinalEobList = new List<Eob>();
            keyValuePairs.ForEach(x =>
            {
                List<KeyValuePair<int, List<string>>> LineItems = new List<KeyValuePair<int, List<string>>>();                // Header Data Parsing Start

                var blockData = x.Value;
                blockData.RemoveAt(0);
                var HeaderDataBlock = blockData[0];
                var objHeader = new Eob();
                var FirstLineData = regex.Replace(HeaderDataBlock, "*");
                var HeaderData = FirstLineData.Split(new char[] { '*' });
                ParseHeaderData(objHeader, HeaderData);

                //Header Data Parsing End

                //Line Item start  
                List<string> LineItem = new List<string>();
                int LineItemblockCount = 0;
                blockData.ForEach(p =>
                {
                    if (p.Contains(SecondLabelRules.startToken))
                    {

                        SecondLabelRules.LineItemStart = true;
                    }

                    if (SecondLabelRules.LineItemStart && !SecondLabelRules.LineItemEnd)
                    {
                        LineItem.Add(p);
                    }
                    if (p.Contains(SecondLabelRules.endToken))
                    {

                        SecondLabelRules.LineItemEnd = true;
                    }
                    if (SecondLabelRules.LineItemStart && SecondLabelRules.LineItemEnd)
                    {
                        LineItemblockCount = LineItemblockCount + 1;
                        LineItems.Add(new KeyValuePair<int, List<string>>(LineItemblockCount, LineItem));
                        SecondLabelRules.LineItemStart = false;
                        SecondLabelRules.LineItemEnd = false;
                        LineItem = new List<string>();
                    }

                });
                ParseLineData(objHeader, LineItems);
                FinalEobList.Add(objHeader);

            });

            return FinalEobList;
        }

        private static void ParseHeaderData(Eob obj, string[] HeaderData)
        {
            obj.LineItems = new List<LineItem>();
            if (HeaderData.Length == SecondLabelRules.headerColumnCount)
            {
                obj.CheckNo = HeaderData[0];
                obj.PaitentId = HeaderData[1];
                obj.Name = HeaderData[2];
                obj.ChargeAmt = HeaderData[3];
                obj.PaymentAmt = HeaderData[4];
                obj.AccountNumber = HeaderData[5];
                obj.Status = HeaderData[6];
                obj.Payer = HeaderData[7];
            }
            else
            {
                var Data = HeaderData[2].Split(new char[] { ' ' }); ;
                obj.CheckNo = HeaderData[0];
                obj.PaitentId = HeaderData[1];
                obj.Name = Data[0] + ' ' + Data[1];
                obj.ChargeAmt = Data[2];
                obj.PaymentAmt = HeaderData[3];
                obj.AccountNumber = HeaderData[4];
                obj.Status = HeaderData[5];
                obj.Payer = HeaderData[6];


            }
        }

        private static void ParseLineData(Eob obj, List<KeyValuePair<int, List<string>>> keyValuePairs)
        {
            Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
            keyValuePairs.ForEach(x =>
            {
                var blockData = x.Value;
                blockData.RemoveAt(0);
                var LineDataBlock = regex.Replace(blockData[0], "*");
                var LineData = LineDataBlock.Split(new char[] { '*' });
                var objLineItem = new LineItem();

                if (LineData.Length > SecondLabelRules.LineColumnCount)
                {
                    var Data = LineData[1].Split(new char[] { ' ' }); ;

                    objLineItem.CheckNo = obj.CheckNo;
                    objLineItem.Name = obj.Name;
                    objLineItem.Status = obj.Status;
                    objLineItem.Payer = obj.Payer;

                    objLineItem.SvcDate = Data[0];
                    objLineItem.CPT = Data[1];
                    objLineItem.ChargeAmt = LineData[2];
                    objLineItem.PaymentAmt = LineData[3];
                    objLineItem.TotalAdjAmt = LineData[4];
                    objLineItem.Remarks = LineData[5];
                    obj.LineItems.Add(objLineItem);

                }

                blockData.RemoveRange(0, 2);
                List<string> description = new List<string>();
                decimal Cobligatition = 0;
                decimal PatientResposbility = 0;

                foreach (string p in blockData)
                {
                    var data = p.TrimStart().TrimEnd();

                    if (data.Contains(SecondLabelRules.midToken))
                    {
                        var data1 = regex.Replace(data, "*");
                        var Data = data1.Split(new char[] { '*' });
                        if (Data.Length > 0)
                        {
                            Cobligatition += Convert.ToDecimal(Data[1]);
                        }
                    }
                    if (data.Contains(SecondLabelRules.endToken))
                    {
                        var data1 = regex.Replace(data, "*");
                        var Data = data1.Split(new char[] { '*' });
                        if (Data.Length > 0)
                        {
                            PatientResposbility += Convert.ToDecimal(Data[1]);
                        }
                    }
                    description.Add(data);
                }

                var tempAllowedAmt = Cobligatition + PatientResposbility + Convert.ToDecimal(objLineItem.PaymentAmt);
                objLineItem.AllowedAmount = tempAllowedAmt.ToString();
                objLineItem.PatientRsponsibility = PatientResposbility.ToString();

                objLineItem.AdjustmentDescription = string.Join(System.Environment.NewLine, description);

            });
        }

        public static List<ParentEob> ParseParentEob(KeyValuePair<int, List<string>> keyValuePairs)
        {
            Regex regex = new Regex("[ ]{2,}", RegexOptions.None);

            var FinalEobList = new List<ParentEob>();
            var blockData = keyValuePairs.Value;
            var HeaderDataBlock = blockData[2];
            var obj = new ParentEob();
            var FirstLineData = regex.Replace(HeaderDataBlock, "*");
            var HeaderData = FirstLineData.Split(new char[] { '*' });
            obj.CheckNo = HeaderData[0];
            obj.Amount = HeaderData[1];
            obj.Claims = HeaderData[2];
            obj.TaxId = HeaderData[3];
            obj.Payee = HeaderData[4];
            obj.Date = HeaderData[5];
            FinalEobList.Add(obj);
            return FinalEobList;
        }
    }
}

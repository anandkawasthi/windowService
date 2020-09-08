using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{

    public class ParentEob
    {
        public string CheckNo { get; set; }
        public string Amount { get; set; }
        public string Claims { get; set; }
        public string TaxId { get; set; }
        public string Date { get; set; }
        public string Payee { get; set; }
        public string Payer { get; set; }

    }

    public class Eob
    {
        public string CheckNo { get; set; }
        public string PaitentId { get; set; }
        public string Name { get; set; }
        //public string LastName { get; set; }
        public string ChargeAmt { get; set; }
        public string PaymentAmt { get; set; }
        public string AccountNumber { get; set; }
        public string Status { get; set; }
        public string Payer { get; set; }
        public List<LineItem> LineItems { get; set; }
        
    }
    public class LineItem
    {
        public string CheckNo { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Payer { get; set; }
        public string SvcDate { get; set; }
        public string CPT { get; set; }
        public string ChargeAmt { get; set; }
        public string PaymentAmt { get; set; }
        public string TotalAdjAmt { get; set; }
        public string Remarks { get; set; }
        public string AdjustmentDescription { get; set; }
        public string AllowedAmount { get; set; }
        public string PatientRsponsibility { get; set; }


    }
}

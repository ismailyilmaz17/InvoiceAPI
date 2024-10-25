﻿namespace InvoiceAPI.Models
{
    using System.Collections.Generic;

    public class InvoiceData
    {
        
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public CustomerInfo CustomerInfo { get; set; }
        public List<InvoiceItem> Items { get; set; }
        public InvoiceDetail InvoiceDetails { get; set; }        
        public IndexInfo IndexInfo { get; set; }
    }

}

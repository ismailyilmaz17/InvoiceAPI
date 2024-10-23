namespace InvoiceAPI.Models
{
    public class InvoiceDetail
    {
        public string CustomizationNumber { get; set; }  // Özelleştirme No
        public string Scenario { get; set; }             // Senaryo (Örn: Temel Fatura)
        public string InvoiceType { get; set; }          // Fatura Tipi (Örn: SATIŞ)
        public DateTime InvoiceDate { get; set; }        // Fatura Tarihi
        public DateTime EditDate { get; set; }           // Düzenleme Tarihi
    }

}

namespace InvoiceAPI.Models
{
    public class CustomerInfo
    {
        public string CustomerName { get; set; }  // Müşteri Adı
        public string TaxOffice { get; set; }     // Vergi Dairesi
        public string TCKN { get; set; }          // T.C. Kimlik No
        public string Email { get; set; }         // E-posta
    }
}

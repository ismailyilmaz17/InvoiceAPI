namespace InvoiceAPI.Models
{
    public class InvoiceItem
    {
        public string Description { get; set; }    // Mal/Hizmet Cinsi
        public int Quantity { get; set; }          // Miktar
        public decimal UnitPrice { get; set; }     // Birim Fiyat
        public decimal TaxRate { get; set; }       // Vergi Tutarı
    }

}

namespace apbd_cw7_s30522.Models;

public class ClientTripRegistration
{
    public int IdClient { get; set; }
    public int IdTrip { get; set; }
    public int RegisteredAt { get; set; }
    public int? PaymentDate { get; set; }

}
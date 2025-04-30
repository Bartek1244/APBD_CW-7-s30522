namespace apbd_cw7_s30522.Models.DTOs;

public class ClientTripGetDTO : TripGetDTO
{
    public int RegisteredAt { get; set; }
    public int? PaymentDate { get; set; }
}
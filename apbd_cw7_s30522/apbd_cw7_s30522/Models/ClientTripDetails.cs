﻿namespace apbd_cw7_s30522.Models;

public class ClientTripDetails : Trip
{
    public int RegisteredAt { get; set; }
    public int? PaymentDate { get; set; }
}
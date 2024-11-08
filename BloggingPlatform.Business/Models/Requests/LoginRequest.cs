namespace BloggingPlatform.Business.Models.Requests;

public abstract record LoginRequest(string PhoneNumber, string Password);
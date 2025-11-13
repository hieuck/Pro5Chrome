
using System;
using OtpNet;

public static class OtpGenerator
{
    // Generates a Time-based One-Time Password (TOTP) from a secret key.
    public static string GenerateTotp(string secretKey)
    {
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new ArgumentException("OTP secret key cannot be null or empty.", nameof(secretKey));
        }

        try
        {
            // The secret key is usually in Base32 format.
            var secretBytes = Base32Encoding.ToBytes(secretKey.Trim());
            var totp = new Totp(secretBytes);
            return totp.ComputeTotp();
        }
        catch (Exception ex)
        {
            // This can happen if the secret key is not valid Base32.
            throw new InvalidOperationException("Failed to generate OTP. Ensure the secret key is a valid Base32 string.", ex);
        }
    }
}

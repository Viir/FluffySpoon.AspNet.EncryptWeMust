using FluffySpoon.AspNet.EncryptWeMust.Certes;
using FluffySpoon.AspNet.EncryptWeMust.Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluffySpoon.AspNet.EncryptWeMust
{
    internal class KestrelOptionsSetup : IConfigureOptions<KestrelServerOptions>
    {
        readonly ILogger<KestrelOptionsSetup> _logger;

        public KestrelOptionsSetup(ILogger<KestrelOptionsSetup> logger)
        {
            _logger = logger;
        }

        public void Configure(KestrelServerOptions options)
        {
            if (LetsEncryptRenewalService.Certificate is not { } certificate)
            {
                _logger.LogWarning("No certificate found, Kestrel will not be configured to use HTTPS.");
                return;
            }

            _logger.LogInformation("Found certificate. Thumbprint: {Thumbprint}", certificate.Thumbprint);

            if (certificate is LetsEncryptX509Certificate x509Certificate)
            {
                _logger.LogInformation("Configuring Kestrel to use HTTPS with the certificate.");

                options.ConfigureHttpsDefaults(o =>
                {
                    o.ServerCertificateSelector = (_a, _b) => x509Certificate.GetCertificate();
                });
            }
            else
            {
                _logger.LogError("This certificate type cannot be used with Kestrel: {type}", certificate.GetType().FullName);
            }
        }
    }
}

using System;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace SignedCmsIncompatibility
{
#if NET6_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    internal class Program
    {
        public const string CodeSigningOid = "1.3.6.1.5.5.7.3.3";

        public const string PkcsObjectIdentifiersData = "1.2.840.113549.1.7.1";

        private const string CertificatePath = @".\code-signing.pfx";

        private const string SignaturePath = @".\signature.p7s";

        private const string Password = @"our secret";

        private static readonly byte[] InputBytes = { 1, 2, 3 };

        static void Main(string[] args)
        {
            if (args[0].Equals("CreateECCertificate"))
            {
                CreateCertificates(false);
            }
            else if (args[0].Equals("CreateRSACertificate"))
            {
                CreateCertificates(true);
            }
            else if (args[0].Equals("CreateSignature"))
            {
                CreateSignature();
            }
            else if (args[0].Equals("VerifySignature"))
            {
                VerifySignature();
            }
            else
            {
                Console.WriteLine("Wrong arg");
            }
        }

        public static void CreateCertificates(bool rsa)
        {
            File.Delete(CertificatePath);
            var rootSubjectName = new X500DistinguishedName("CN=my root");
            var notBefore = DateTimeOffset.Now.AddDays(-1);
            var notAfter = DateTimeOffset.Now.AddDays(1);
            var rootCertWithPrivateKey = CreateRootCert(rootSubjectName, notBefore, notAfter, rsa);

            var codeSigningSubjectName = new X500DistinguishedName("CN=my signer");
            var codeSigningCert = CreateCodeSigningCert(
                codeSigningSubjectName,
                rootCertWithPrivateKey,
                1,
                notBefore,
                notAfter,
                rsa);

            File.WriteAllBytes(CertificatePath, codeSigningCert.Export(X509ContentType.Pfx, Password));
        }

        public static void CreateSignature()
        {
            File.Delete(SignaturePath);
            using (var certificate = new X509Certificate2(CertificatePath, Password))
            {
                var cmsSigner = new CmsSigner(certificate);
                cmsSigner.IncludeOption = X509IncludeOption.EndCertOnly;
                var contentInfo = new ContentInfo(new Oid(PkcsObjectIdentifiersData), InputBytes);
                var signedCms = new SignedCms(SubjectIdentifierType.Unknown, contentInfo, true);
                signedCms.ComputeSignature(cmsSigner);
                File.WriteAllBytes(SignaturePath, signedCms.Encode());
            }
        }

        public static void VerifySignature()
        {
            try
            {
                var signatureBytes = File.ReadAllBytes(SignaturePath);

                var contentInfo = new ContentInfo(InputBytes);
                var signedData = new SignedCms(contentInfo, true);
                signedData.Decode(signatureBytes);
                var signerInfos = signedData.SignerInfos;
                foreach (var signerInfo in signerInfos)
                {
                    // If you set a break point here you can see that the signatureAlgorithmOid is different for signatures
                    // created by .Net Framework and .Net Core.
                    var signatureAlgorithm = signerInfo.SignatureAlgorithm;
                }

                signedData.CheckSignature(true);
                Console.WriteLine(" Verification ok");
            }
            catch (Exception e)
            {
                Console.WriteLine(" Verification failed");
                Console.WriteLine(" Exception: " + e.Message);
            }
        }

        private static X509Certificate2 CreateRootCert(X500DistinguishedName subjectName, DateTimeOffset notBefore,
            DateTimeOffset notAfter, bool useRsa)
        {
            void AddExtensions(CertificateRequest request)
            {
                request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
            }

            if (useRsa)
            {
                using (var rsa = new RSACng())
                {
                    var request = new CertificateRequest(
                        subjectName,
                        rsa,
                        HashAlgorithmName.SHA256,
                        RSASignaturePadding.Pkcs1);
                    
                    AddExtensions(request);

                    return request.CreateSelfSigned(notBefore, notAfter);
                }
            }

            using (var ecdsa = new ECDsaCng())
            {
                var request = new CertificateRequest(
                    subjectName,
                    ecdsa,
                    HashAlgorithmName.SHA256);

                AddExtensions(request);

                return request.CreateSelfSigned(notBefore, notAfter);
            }
        }

        private static X509Certificate2 CreateCodeSigningCert(
            X500DistinguishedName subjectName,
            X509Certificate2 rootCertWithPrivateKey,
            int serialNumber,
            DateTimeOffset notBefore,
            DateTimeOffset notAfter,
            bool useRsa)
        {
            void AddExtensions(CertificateRequest request)
            {
                var extension = new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false);
                request.CertificateExtensions.Add(extension);

                request.CertificateExtensions.Add(
                    new X509EnhancedKeyUsageExtension(new OidCollection { new Oid(CodeSigningOid) }, true));
            }

            if (useRsa)
            {
                using (var rsa = new RSACng())
                {
                    var request = new CertificateRequest(
                        subjectName,
                        rsa,
                        HashAlgorithmName.SHA256,
                        RSASignaturePadding.Pkcs1);

                    AddExtensions(request);

                    var serialNumberArray = new byte[1] { (byte)serialNumber };

                    var codeSigningCert = request.Create(
                        rootCertWithPrivateKey,
                        notBefore,
                        notAfter,
                        serialNumberArray);

                    return codeSigningCert.CopyWithPrivateKey(rsa);
                }
            }

            using (var ecdsa = new ECDsaCng())
            {
                var request1 = new CertificateRequest(
                    subjectName,
                    ecdsa,
                    HashAlgorithmName.SHA256);

                AddExtensions(request1);

                var serialNumberArray1 = new byte[1] { (byte)serialNumber };

                var codeSigningCert1 = request1.Create(
                    rootCertWithPrivateKey,
                    notBefore,
                    notAfter,
                    serialNumberArray1);

                return codeSigningCert1.CopyWithPrivateKey(ecdsa);
            }
        }
    }
}
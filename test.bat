@echo off

echo Test create signature in .Net Framework. Verify in .Net Framework using elliptic curve.
.\SignedCmsIncompatibility\SignedCmsIncompatibility\bin\Debug\net48\SignedCmsIncompatibility.exe CreateECCertificate
.\SignedCmsIncompatibility\SignedCmsIncompatibility\bin\Debug\net48\SignedCmsIncompatibility.exe CreateSignature
.\SignedCmsIncompatibility\SignedCmsIncompatibility\bin\Debug\net48\SignedCmsIncompatibility.exe VerifySignature

echo Test create signature in .Net Core. Verify in .Net Core using elliptic curve.
.\SignedCmsIncompatibility\SignedCmsIncompatibility\bin\Debug\net6.0\SignedCmsIncompatibility.exe CreateECCertificate
.\SignedCmsIncompatibility\SignedCmsIncompatibility\bin\Debug\net6.0\SignedCmsIncompatibility.exe CreateSignature
.\SignedCmsIncompatibility\SignedCmsIncompatibility\bin\Debug\net6.0\SignedCmsIncompatibility.exe VerifySignature

echo Test create signature in .Net Framework. Verify in .Net Core using RSA.
.\SignedCmsIncompatibility\SignedCmsIncompatibility\bin\Debug\net48\SignedCmsIncompatibility.exe CreateRSACertificate
.\SignedCmsIncompatibility\SignedCmsIncompatibility\bin\Debug\net48\SignedCmsIncompatibility.exe CreateSignature
.\SignedCmsIncompatibility\SignedCmsIncompatibility\bin\Debug\net6.0\SignedCmsIncompatibility.exe VerifySignature

echo Test create signature in .Net Framework. Verify in .Net Core using elliptic curve.
.\SignedCmsIncompatibility\SignedCmsIncompatibility\bin\Debug\net48\SignedCmsIncompatibility.exe CreateECCertificate
.\SignedCmsIncompatibility\SignedCmsIncompatibility\bin\Debug\net48\SignedCmsIncompatibility.exe CreateSignature
.\SignedCmsIncompatibility\SignedCmsIncompatibility\bin\Debug\net6.0\SignedCmsIncompatibility.exe VerifySignature


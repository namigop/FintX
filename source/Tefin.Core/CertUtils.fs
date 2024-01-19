module Tefin.Core.CertUtils

open System.Security.Cryptography.X509Certificates

let getCertificates (location: StoreLocation) : X509Certificate2Collection =
    // Get the certificate store for the current user.
    let store = new X509Store(StoreName.My, location)

    try
        store.Open(OpenFlags.ReadOnly)
        store.Certificates
    finally
        store.Close()

let findBySubjectDistinguishedName (certName: string) (location: StoreLocation) : X509Certificate2 =
    let certCollection = getCertificates (location)

    // If using a certificate with a trusted root you do not need to FindByTimeValid, instead:
    // currentCerts.Find(X509FindType.FindBySubjectDistinguishedName, certName, true);
    // X509Certificate2Collection currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
    let signingCert =
        certCollection.Find(X509FindType.FindBySubjectDistinguishedName, certName, false)

    if (signingCert.Count = 0) then
        Unchecked.defaultof<X509Certificate2>
    else
        signingCert[0]

let findByThumbprint (thumbprint: string) (location: StoreLocation) : X509Certificate2 =
    let certCollection = getCertificates (location)

    let signingCert =
        certCollection.Find(X509FindType.FindByThumbprint, thumbprint, false)

    if (signingCert.Count = 0) then
        Unchecked.defaultof<X509Certificate2>
    else
        signingCert[0]

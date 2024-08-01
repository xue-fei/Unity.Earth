using UnityEngine.Networking;

/// <summary>
/// 跳过Web请求证书避免出现 报错：【SSL CA certificate error】 与 【Curl error 60: Cert verify failed: UNITYTLS_X509VERIFY_FLAG_USER_ERROR1】
/// </summary>
public class WebRequestSkipCertificate : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}
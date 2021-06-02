﻿//******************************************************************************************************************************************************************************************//
// Copyright (c) 2021 abergs (https://github.com/abergs/fido2-net-lib)                                                                                                                      //                        
//                                                                                                                                                                                          //
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),                                       //
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,   //
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:                                                                                   //
//                                                                                                                                                                                          //
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.                                                           //
//                                                                                                                                                                                          //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,                                      //
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,                            //
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                               //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
// Copyright (c) 2021 @redhook62 (adfsmfa@gmail.com)                                                                                                                                    //                        
//                                                                                                                                                                                          //
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),                                       //
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,   //
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:                                                                                   //
//                                                                                                                                                                                          //
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.                                                           //
//                                                                                                                                                                                          //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,                                      //
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,                            //
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                               //
//                                                                                                                                                                                          //
//                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Neos.IdentityServer.MultiFactor.WebAuthN.Library.ASN;
using Neos.IdentityServer.MultiFactor.WebAuthN.Objects;


namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    public static class CryptoUtils
    {
        public static HashAlgorithm GetHasher(HashAlgorithmName hashName)
        {
            switch (hashName.Name)
            {
                case "SHA1":
                    return SHA1.Create();
                case "SHA256":
                case "HS256":
                case "RS256":
                case "ES256":
                case "PS256":
                    return SHA256.Create();
                case "SHA384":
                case "HS384":
                case "RS384":
                case "ES384":
                case "PS384":
                    return SHA384.Create();
                case "SHA512":
                case "HS512":
                case "RS512":
                case "ES512":
                case "PS512":
                    return SHA512.Create();
                default:
                    throw new ArgumentOutOfRangeException(nameof(hashName));
            }
        }

		public static HashAlgorithmName HashAlgFromCOSEAlg(int alg)
        {
            switch ((COSE.Algorithm)alg)
            {
                case COSE.Algorithm.RS1: return HashAlgorithmName.SHA1;
                case COSE.Algorithm.ES256: return HashAlgorithmName.SHA256;
                case COSE.Algorithm.ES384: return HashAlgorithmName.SHA384;
                case COSE.Algorithm.ES512: return HashAlgorithmName.SHA512;
                case COSE.Algorithm.PS256: return HashAlgorithmName.SHA256;
                case COSE.Algorithm.PS384: return HashAlgorithmName.SHA384;
                case COSE.Algorithm.PS512: return HashAlgorithmName.SHA512;
                case COSE.Algorithm.RS256: return HashAlgorithmName.SHA256;
                case COSE.Algorithm.RS384: return HashAlgorithmName.SHA384;
                case COSE.Algorithm.RS512: return HashAlgorithmName.SHA512;
                case (COSE.Algorithm)4: return HashAlgorithmName.SHA1;
                case (COSE.Algorithm)11: return HashAlgorithmName.SHA256;
                case (COSE.Algorithm)12: return HashAlgorithmName.SHA384;
                case (COSE.Algorithm)13: return HashAlgorithmName.SHA512;
                case COSE.Algorithm.EdDSA: return HashAlgorithmName.SHA512;
                default:
                    throw new VerificationException("Unrecognized COSE alg value");
            };
        }        

        public static byte[] SigFromEcDsaSig(byte[] ecDsaSig, int keySize)
        {
            var decoded = AsnElt.Decode(ecDsaSig);
            var r = decoded.Sub[0].GetOctetString();
            var s = decoded.Sub[1].GetOctetString();

            // .NET requires IEEE P-1363 fixed size unsigned big endian values for R and S
            // ASN.1 requires storing positive integer values with any leading 0s removed
            // Convert ASN.1 format to IEEE P-1363 format 
            // determine coefficient size 
            var coefficientSize = (int)Math.Ceiling((decimal)keySize / 8);

            // Create byte array to copy R into 
            var P1363R = new byte[coefficientSize];

            if (0x0 == r[0] && (r[1] & (1 << 7)) != 0)
            {
                r.Skip(1).ToArray().CopyTo(P1363R, coefficientSize - r.Length + 1);
            }
            else
            {
                r.CopyTo(P1363R, coefficientSize - r.Length);
            }

            // Create byte array to copy S into 
            var P1363S = new byte[coefficientSize];

            if (0x0 == s[0] && (s[1] & (1 << 7)) != 0)
            {
                s.Skip(1).ToArray().CopyTo(P1363S, coefficientSize - s.Length + 1);
            }
            else
            {
                s.CopyTo(P1363S, coefficientSize - s.Length);
            }

            // Concatenate R + S coordinates and return the raw signature
            return P1363R.Concat(P1363S).ToArray();
        }

        /// <summary>
        /// Convert PEM formated string into byte array.
        /// </summary>
        /// <param name="pemStr">source string.</param>
        /// <returns>output byte array.</returns>
        public static byte[] PemToBytes(string pemStr)
        {
            const string PemStartStr = "-----BEGIN";
            const string PemEndStr = "-----END";
            byte[] retval = null;
            var lines = pemStr.Split('\n');
            var base64Str = "";
            bool started = false, ended = false;
            var cline = "";
            for (var i = 0; i < lines.Length; i++)
            {
                cline = lines[i].ToUpper();
                if (cline == "")
                    continue;
                if (cline.Length > PemStartStr.Length)
                {
                    if (!started && cline.Substring(0, PemStartStr.Length) == PemStartStr)
                    {
                        started = true;
                        continue;
                    }
                }
                if (cline.Length > PemEndStr.Length)
                {
                    if (cline.Substring(0, PemEndStr.Length) == PemEndStr)
                    {
                        ended = true;
                        break;
                    }
                }
                if (started)
                {
                    base64Str += lines[i];
                }
            }
            if (!(started && ended))
            {
                throw new Exception("'BEGIN'/'END' line is missing.");
            }
            base64Str = base64Str.Replace("\r", "");
            base64Str = base64Str.Replace("\n", "");
            base64Str = base64Str.Replace("\n", " ");
            retval = Convert.FromBase64String(base64Str);
            return retval;
        }

        public static string CDPFromCertificateExts(X509ExtensionCollection exts)
        {
            var cdp = "";
            foreach (var ext in exts)
            {
                if (ext.Oid.Value.Equals("2.5.29.31")) // id-ce-CRLDistributionPoints
                {
                    var asnData = AsnElt.Decode(ext.RawData);
                    cdp = System.Text.Encoding.ASCII.GetString(asnData.Sub[0].Sub[0].Sub[0].Sub[0].GetOctetString());
                }
            }
            return cdp;
        }
        public static bool IsCertInCRL(byte[] crl, X509Certificate2 cert)
        {
            var pemCRL = System.Text.Encoding.ASCII.GetString(crl);
            var crlBytes = PemToBytes(pemCRL);
            var asnData = AsnElt.Decode(crlBytes);
            if (7 > asnData.Sub[0].Sub.Length)
                return false; // empty CRL

            var revokedCertificates = asnData.Sub[0].Sub[5].Sub;
            var revoked = new List<long>();

            foreach (AsnElt s in revokedCertificates)
            {
                revoked.Add(BitConverter.ToInt64(s.Sub[0].GetOctetString().Reverse().ToArray(), 0));
            }

            return revoked.Contains(BitConverter.ToInt64(cert.GetSerialNumber(), 0));
        }
    }
}

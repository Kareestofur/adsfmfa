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
using Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Objects
{
    /// <summary>
    /// <see cref="https://www.w3.org/TR/webauthn/#extensions"/>
    /// </summary>
    public class Extensions
    {
        private readonly byte[] _extensionBytes;
        public Extensions(byte[] extensions)
        {
            _extensionBytes = extensions;
        }

        public int Length => _extensionBytes.Length;

        public override string ToString()
        {
            return $"Extensions: {CBORObject.DecodeFromBytes(_extensionBytes)}";
        }

        public byte[] GetBytes()
        {
            return _extensionBytes;
        }
    }
}


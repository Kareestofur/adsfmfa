﻿//******************************************************************************************************************************************************************************************//
// Copyright (c) 2022 @redhook62 (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Neos.IdentityServer.MultiFactor.Administration;
using Neos.IdentityServer.MultiFactor;
using Microsoft.ManagementConsole.Advanced;
using Neos.IdentityServer.Console.Resources;

namespace Neos.IdentityServer.Console
{
    public partial class UserPropertiesKeysControl : UserControl, IUserPropertiesDataObject
    {
        private UserPropertyPage userPropertyPage;
        private string _secretkey = string.Empty;
        private string _upn = string.Empty;
        private bool _emailnotset = false;
        private string _email;
        
        /// <summary>
        /// UserPropertiesKeysControl constructor
        /// </summary>
        public UserPropertiesKeysControl(UserPropertyPage parentPropertyPage)
        {
            InitializeComponent();
            userPropertyPage = parentPropertyPage;
        }

        #region IUserPropertiesDataObject
        /// <summary>
        /// SyncDisabled property implmentation
        /// </summary>
        public bool SyncDisabled { get; set; } = false;


        /// <summary>
        /// GetUserControlData method implmentation
        /// </summary>
        public MFAUserList GetUserControlData(MFAUserList lst)
        {
            MFAUser obj = lst[0];
            return lst;
        }

        /// <summary>
        /// SetUserControlData method implmentation
        /// </summary>
        public void SetUserControlData(MFAUserList lst, bool disablesync)
        {
            SyncDisabled = disablesync;
            try
            {
                MFAUser obj = lst[0];
                _upn = obj.UPN;
                _email = obj.MailAddress;
                _secretkey = MMCService.GetEncodedUserKey(obj.UPN);

                if (string.IsNullOrEmpty(_email))
                {
                    this.EmailPrompt.Text = "Email : ";
                    _emailnotset = true;
                }
                else
                {
                    this.EmailPrompt.Text = string.Format("Email : {0}", _email);
                    _emailnotset = false;
                }
                if (!string.IsNullOrEmpty(_secretkey))
                {
                    this.DisplayKey.Text =_secretkey;
                    if (!string.IsNullOrEmpty(_upn))
                        this.qrCodeGraphic.Text = MMCService.GetQRCodeValue(_upn, this.DisplayKey.Text);
                    else
                        this.qrCodeGraphic.Text = string.Empty;
                }
                else
                    userPropertyPage.Dirty = true;
                UpdateControlsEnabled();
            }
            catch (Exception)
            {
                this.DisplayKey.Text = string.Empty;
                this.qrCodeGraphic.Text = string.Empty;
            }
            finally
            {
                SyncDisabled = false;
            }
        }
        #endregion

        /// <summary>
        /// newkeyBtn_Click event
        /// </summary>
        private void newkeyBtn_Click(object sender, EventArgs e)
        {
            Cursor crs = this.Cursor;
            try
            {

                MMCService.NewUserKey(_upn);
                _secretkey = MMCService.GetEncodedUserKey(_upn);
                this.DisplayKey.Text = _secretkey;
                this.qrCodeGraphic.Text = MMCService.GetQRCodeValue(_upn, this.DisplayKey.Text);
                if (!SyncDisabled)
                    userPropertyPage.SyncSharedUserData(this, true);
            }
            catch (Exception ex)
            {
                this.Cursor = crs;
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                userPropertyPage.ParentSheet.ShowDialog(messageBoxParameters);
            }
            finally
            {
                this.Cursor = crs;
            }
        }

        /// <summary>
        /// clearkeyBtn_Click event
        /// </summary>
        private void clearkeyBtn_Click(object sender, EventArgs e)
        {
            Cursor crs = this.Cursor;
            try
            {
                _secretkey = string.Empty;
                this.DisplayKey.Text = string.Empty;
                this.qrCodeGraphic.Text = string.Empty;
                MMCService.RemoveUserKey(_upn);
                if (!SyncDisabled)
                    userPropertyPage.SyncSharedUserData(this, true);
            }
            catch (Exception ex)
            {
                this.Cursor = crs;
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                userPropertyPage.ParentSheet.ShowDialog(messageBoxParameters);
            }
            finally
            {
                this.Cursor = crs;
            }
        }

        /// <summary>
        /// BTNSendByMail_Click event
        /// </summary>
        private void BTNSendByMail_Click(object sender, EventArgs e)
        {
            Cursor crs = this.Cursor;
            try
            {
                this.Cursor = Cursors.WaitCursor;
                 MMCService.SendKeyByEmail(_email, _upn, this.DisplayKey.Text);
            }
            catch (Exception ex)
            {
                this.Cursor = crs;
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                userPropertyPage.ParentSheet.ShowDialog(messageBoxParameters);
            }
            finally
            {
                this.Cursor = crs;
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = string.Format(errors_strings.InfoSendingMailToUser, _email),
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Information
                };
                userPropertyPage.ParentSheet.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// EmailPrompt_TextChanged event
        /// </summary>
        private void EmailPrompt_TextChanged(object sender, EventArgs e)
        {
            if (!SyncDisabled)
                userPropertyPage.SyncSharedUserData(this, true);
            UpdateControlsEnabled();
        }

        /// <summary>
        /// UpdateControlsEnabled method implentation
        /// </summary>
        private void UpdateControlsEnabled()
        {
            if ((_emailnotset) || string.IsNullOrEmpty(_upn) || (string.IsNullOrEmpty(_secretkey)))
                BTNSendByMail.Enabled = false;
            else
                BTNSendByMail.Enabled = true;
        }
    }
}

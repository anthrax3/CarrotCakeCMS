﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Carrotware.CMS.Core;
using Carrotware.CMS.UI.Base;
using Carrotware.Web.UI.Controls;
/*
* CarrotCake CMS
* http://www.carrotware.com/
*
* Copyright 2011, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: October 2011
*/

namespace Carrotware.CMS.UI.Admin.c3_admin.MasterPages {
	public partial class Main : AdminBaseMasterPage {

		public string userName = String.Empty;

		protected void Page_Load(object sender, EventArgs e) {

			if (!Page.User.Identity.IsAuthenticated) {
				FormsAuthentication.SignOut();
				Response.Redirect(SiteFilename.LogonURL);
			}

			if (SecurityData.CurrentUser != null) {
				userName = SecurityData.CurrentUser.UserName;
			}

			if (!SecurityData.IsAdmin) {
				tabUserSecurity.Visible = false;
			}

			tabUserAdmin.Visible = tabUserSecurity.Visible;
			tabGroupAdmin.Visible = tabUserSecurity.Visible;
			tabSites.Visible = tabUserSecurity.Visible;

			if (SiteData.CurretSiteExists) {
				litServerTime.Text = SiteData.CurrentSite.Now.ToString() + " " + SiteData.CurrentSite.TimeZoneIdentifier;
				litSiteIdent.Text = SiteData.CurrentSite.SiteName;
				litTag.Text = SiteData.CurrentSite.SiteTagline;

				if (!string.IsNullOrEmpty(SiteData.CurrentSite.SiteName) && !string.IsNullOrEmpty(SiteData.CurrentSite.SiteTagline)) {
					litSiteIdent.Text = SiteData.CurrentSite.SiteName.Trim() + ":   ";
				}
			} else {
				litServerTime.Text = DateTime.UtcNow.ToString() + " UTC";
			}

			LoadFooterCtrl(plcFooter, ControlLocation.MainFooter);

			litCMSBuildInfo.Text = SiteData.CarrotCakeCMSVersion;
			litVersion.Text = SiteData.CarrotCakeCMSVersionMM;

			tabModules.Visible = CMSConfigHelper.HasAdminModules();

			HideWhenNoSiteProfileExists();
		}

		public void HideWhenNoSiteProfileExists() {

			bool bShowTop = SiteData.CurretSiteExists;

			tabContentTop.Visible = bShowTop;
			tabExportSite.Visible = bShowTop;
			tabTxtWidgets.Visible = bShowTop;
			tabSnippets.Visible = bShowTop;
			tabBlogTop.Visible = bShowTop;
			tabContent.Visible = bShowTop;
			tabModules.Visible = bShowTop;
			tabMainTemplate.Visible = bShowTop;
			tabContentSiteMap.Visible = bShowTop;
			tabImportContent.Visible = bShowTop;
			tabStatusChange.Visible = bShowTop;
			//tabDashboard.Visible = bShowTop;
			tabExtensions.Visible = bShowTop;
			tabHistory.Visible = bShowTop;
		}


		protected void btnLogout_Click(object sender, EventArgs e) {
			FormsAuthentication.SignOut();
			Response.Redirect(SiteFilename.LogonURL);
		}


		public void ActivateTab(SectionID sectionID) {
			string sCSSTop = "current sub";
			string sCSSSecondary = "current";

			switch (sectionID) {

				case SectionID.SiteDashboard:
					tabMainTop.Attributes["class"] = sCSSTop;
					//tabDashboard.Attributes["class"] = sCSSSecondary;
					break;
				case SectionID.SiteInfo:
					tabMainTop.Attributes["class"] = sCSSTop;
					tabMain.Attributes["class"] = sCSSSecondary;
					break;
				case SectionID.ContentHistory:
					tabMainTop.Attributes["class"] = sCSSTop;
					tabHistory.Attributes["class"] = sCSSSecondary;
					break;
				case SectionID.SiteTemplate:
					tabMainTop.Attributes["class"] = sCSSTop;
					tabMainTemplate.Attributes["class"] = sCSSSecondary;
					break;
				case SectionID.ContentSkinEdit:
					tabMainTop.Attributes["class"] = sCSSTop;
					tabContentSkin.Attributes["class"] = sCSSSecondary;
					break;
				case SectionID.DataImport:
					tabMainTop.Attributes["class"] = sCSSTop;
					tabImportContent.Attributes["class"] = sCSSSecondary;
					break;
				case SectionID.SiteExport:
					tabMainTop.Attributes["class"] = sCSSTop;
					tabExportSite.Attributes["class"] = sCSSSecondary;
					break;
				case SectionID.StatusChange:
					tabMainTop.Attributes["class"] = sCSSTop;
					tabStatusChange.Attributes["class"] = sCSSSecondary;
					break;

				case SectionID.ContentAdd:
					tabContentTop.Attributes["class"] = sCSSTop;
					tabContent.Attributes["class"] = sCSSSecondary;
					break;
				case SectionID.ContentTemplate:
					tabContentTop.Attributes["class"] = sCSSTop;
					tabContentTemplate.Attributes["class"] = sCSSSecondary;
					break;
				case SectionID.PageComment:
					tabContentTop.Attributes["class"] = sCSSTop;
					tabContentCommentIndex.Attributes["class"] = sCSSSecondary;
					break;
				case SectionID.ContentSiteMap:
					tabContentTop.Attributes["class"] = sCSSTop;
					tabContentSiteMap.Attributes["class"] = sCSSSecondary;
					break;

				case SectionID.Modules:
					tabExtensions.Attributes["class"] = sCSSTop;
					tabModules.Attributes["class"] = sCSSSecondary;
					break;
				case SectionID.TextWidget:
					tabExtensions.Attributes["class"] = sCSSTop;
					tabTxtWidgets.Attributes["class"] = sCSSSecondary;
					break;
				case SectionID.ContentSnippet:
					tabExtensions.Attributes["class"] = sCSSTop;
					tabSnippets.Attributes["class"] = sCSSSecondary;
					break;

				case SectionID.UserAdmin:
					tabUserSecurity.Attributes["class"] = sCSSTop;
					tabUserAdmin.Attributes["class"] = sCSSSecondary;
					break;
				case SectionID.GroupAdmin:
					tabUserSecurity.Attributes["class"] = sCSSTop;
					tabGroupAdmin.Attributes["class"] = sCSSSecondary;
					break;
				case SectionID.SiteIndex:
					tabUserSecurity.Attributes["class"] = sCSSTop;
					tabSites.Attributes["class"] = sCSSSecondary;
					break;

				case SectionID.BlogContentAdd:
					tabBlogTop.Attributes["class"] = sCSSTop;
					tabBlogContent.Attributes["class"] = sCSSSecondary;
					break;
				case SectionID.BlogIndex:
					tabBlogTop.Attributes["class"] = sCSSTop;
					tabBlogContent.Attributes["class"] = sCSSSecondary;
					break;
				case SectionID.BlogCategory:
					tabBlogTop.Attributes["class"] = sCSSTop;
					tabBlogCategoryIndex.Attributes["class"] = sCSSSecondary;
					break;
				case SectionID.BlogTag:
					tabBlogTop.Attributes["class"] = sCSSTop;
					tabBlogTagIndex.Attributes["class"] = sCSSSecondary;
					break;
				case SectionID.BlogTemplate:
					tabBlogTop.Attributes["class"] = sCSSTop;
					tabBlogTemplate.Attributes["class"] = sCSSSecondary;
					break;
				case SectionID.BlogComment:
					tabBlogTop.Attributes["class"] = sCSSTop;
					tabBlogCommentIndex.Attributes["class"] = sCSSSecondary;
					break;
			}

		}

		protected void ScriptManager1_AsyncPostBackError(object sender, AsyncPostBackErrorEventArgs e) {
			string sError = String.Empty;

			if (e.Exception != null) {
				Exception objErr = e.Exception;
				sError = objErr.Message;
				if (objErr.StackTrace != null) {
					sError += "\r\n<hr />\r\n" + objErr.StackTrace;
				}

				if (objErr.InnerException != null) {
					sError += "\r\n<hr />\r\n" + objErr.InnerException;
				}

				SiteData.WriteDebugException("main master - AsyncPostBackError", objErr);
			} else {
				sError = " An error occurred. (Generic Main) ";
			}

			ScriptManager1.AsyncPostBackErrorMessage = sError;
		}

	}
}

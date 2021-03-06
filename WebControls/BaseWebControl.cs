﻿using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
/*
* CarrotCake CMS
* http://www.carrotware.com/
*
* Copyright 2011, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: October 2011
*/

namespace Carrotware.Web.UI.Controls {

	public abstract class BaseWebControl : WebControl {

		private static Page CachedPage {
			get {
				if (_CachedPage == null) {
					_CachedPage = new Page();
					_CachedPage.AppRelativeVirtualPath = "~/";
				}
				return _CachedPage;
			}
		}
		private static Page _CachedPage;

		public static string GetWebResourceUrl(Type type, string resource) {
			string sPath = "";

			try {
				sPath = CachedPage.ClientScript.GetWebResourceUrl(type, resource);
				sPath = HttpUtility.HtmlEncode(sPath);
			} catch { }

			return sPath;
		}

		public static string GetWebResourceUrl(Page page, Type type, string resource) {
			string sPath = "";
			if (page != null) {
				try {
					sPath = page.ClientScript.GetWebResourceUrl(type, resource);
					sPath = HttpUtility.HtmlEncode(sPath);
				} catch { }
			} else {
				sPath = GetWebResourceUrl(type, resource);
			}

			return sPath;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsPostBack {
			get {
				string sReq = "GET";
				try { sReq = HttpContext.Current.Request.ServerVariables["REQUEST_METHOD"].ToString().ToUpper(); } catch { }
				return sReq != "GET" ? true : false;
			}
		}

		[Browsable(true)]
		public bool IsWebView {
			get { return (HttpContext.Current != null); }
		}

		protected string CurrentScriptName {
			get { return HttpContext.Current.Request.ServerVariables["script_name"].ToString(); }
		}


		protected override void Render(HtmlTextWriter writer) {
			RenderContents(writer);
		}

		protected override void RenderContents(HtmlTextWriter output) {

		}


		protected void BaseRender(HtmlTextWriter writer) {
			base.Render(writer);
		}

		protected void BaseRenderContents(HtmlTextWriter output) {
			base.RenderContents(output);
		}


	}
}

﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Xml.Serialization;
using Carrotware.CMS.Data;
using Carrotware.CMS.Interface;

/*
* CarrotCake CMS
* http://carrotware.com/
*
* Copyright 2011, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: October 2011
*/

namespace Carrotware.CMS.Core {




	public class CMSConfigHelper : IDisposable {

		public CMSConfigHelper() {

		}


		private CarrotCMSDataContext db = new CarrotCMSDataContext();

		#region IDisposable Members

		public void Dispose() {
			if (db != null) {
				db.Dispose();
			}
		}

		#endregion

		private static string keyAdminMenuModules = "cms_AdminMenuModules";

		private static string keyAdminToolboxModules = "cms_AdminToolboxModules";

		private static string keyDynamicSite = "cms_DynamicSite";

		private static string keyTemplates = "cms_Templates";

		private static string keyDynSite = "cms_DynSite_";

		public static string keyAdminContent = "cmsAdminContent";

		public static string keyAdminWidget = "cmsAdminWidget";


		public void ResetConfigs() {

			HttpContext.Current.Cache.Remove(keyAdminMenuModules);

			HttpContext.Current.Cache.Remove(keyAdminToolboxModules);

			HttpContext.Current.Cache.Remove(keyDynamicSite);

			HttpContext.Current.Cache.Remove(keyTemplates);

			string ModuleKey = keyDynSite + DomainName;
			HttpContext.Current.Cache.Remove(ModuleKey);

			System.Web.HttpRuntime.UnloadAppDomain();
		}


		public string DomainName {
			get {
				var domName = HttpContext.Current.Request.ServerVariables["HTTP_HOST"];
				if (domName.IndexOf(":") > 0) {
					domName = domName.Substring(0, domName.IndexOf(":"));
				}

				return domName.ToLower();
			}
		}

		private enum CMSConfigFileType {
			AdminModules,
			PublicControls,
			Skin,
			SiteSkins,
			SiteMapping
		}

		private DataSet ReadDataSetConfig(CMSConfigFileType cfg, string sPath) {
			string sPlugCfg = "default.config";
			string sRealPath = HttpContext.Current.Server.MapPath(sPath);

			int iExpectedTblCount = 1;

			switch (cfg) {
				case CMSConfigFileType.AdminModules:
					sPlugCfg = sRealPath + "AdminModules.config";
					iExpectedTblCount = 2;
					break;
				case CMSConfigFileType.PublicControls:
					sPlugCfg = sRealPath + "PublicControls.config";
					break;
				case CMSConfigFileType.Skin:
					sPlugCfg = sRealPath + "Skin.config";
					break;
				case CMSConfigFileType.SiteSkins:
					sPlugCfg = sRealPath + "SiteSkins.config";
					break;
				case CMSConfigFileType.SiteMapping:
					sPlugCfg = sRealPath + "SiteMapping.config";
					break;
				default:
					sPlugCfg = sRealPath + "default.config";
					iExpectedTblCount = -1;
					break;
			}

			DataSet ds = new DataSet();
			if (File.Exists(sPlugCfg) && iExpectedTblCount > 0) {
				ds.ReadXml(sPlugCfg);
			}

			if (ds == null) {
				ds = new DataSet();
			}

			int iTblCount = ds.Tables.Count;

			// if dataset has wrong # of tables, build out more tables
			if (iTblCount < iExpectedTblCount) {
				for (int t = iTblCount; t <= iExpectedTblCount; t++) {
					ds.Tables.Add(new DataTable());
					ds.AcceptChanges();
				}
			}

			if (iExpectedTblCount > 0) {
				iTblCount = ds.Tables.Count;

				List<string> reqCols0 = new List<string>();
				List<string> reqCols1 = new List<string>();

				switch (cfg) {
					case CMSConfigFileType.AdminModules:
						reqCols0.Add("caption");
						reqCols0.Add("pluginid");

						reqCols1.Add("pluginlabel");
						reqCols1.Add("menuorder");
						reqCols1.Add("parm");
						reqCols1.Add("plugincontrol");
						reqCols1.Add("useajax");
						reqCols1.Add("usepopup");
						reqCols1.Add("visible");
						reqCols1.Add("pluginid");
						break;
					case CMSConfigFileType.PublicControls:
						reqCols0.Add("filepath");
						reqCols0.Add("crtldesc");
						break;
					case CMSConfigFileType.Skin:
					case CMSConfigFileType.SiteSkins:
						reqCols0.Add("templatefile");
						reqCols0.Add("filedesc");
						break;
					case CMSConfigFileType.SiteMapping:
						reqCols0.Add("domname");
						reqCols0.Add("siteid");
						break;
					default:
						reqCols0.Add("caption");
						reqCols0.Add("pluginid");
						break;
				}

				//validate that the dataset has the right table configuration
				DataTable dt0 = ds.Tables[0];
				foreach (string c in reqCols0) {
					if (!dt0.Columns.Contains(c)) {
						DataColumn dc = new DataColumn(c);
						dc.DataType = System.Type.GetType("System.String"); // add if not found

						dt0.Columns.Add(dc);
						dt0.AcceptChanges();
					}
				}

				for (int iTbl = 1; iTbl < iTblCount; iTbl++) {
					DataTable dt = ds.Tables[iTbl];
					foreach (string c in reqCols1) {
						if (!dt.Columns.Contains(c)) {
							DataColumn dc = new DataColumn(c);
							dc.DataType = System.Type.GetType("System.String"); // add if not found

							dt.Columns.Add(dc);
							dt.AcceptChanges();
						}
					}
				}

			}

			return ds;
		}


		public List<CMSAdminModule> AdminModules {
			get {

				var _modules = new List<CMSAdminModule>();

				bool bCached = false;

				try {
					_modules = (List<CMSAdminModule>)HttpContext.Current.Cache[keyAdminMenuModules];
					if (_modules != null) {
						bCached = true;
					}
				} catch {
					bCached = false;
				}

				if (!bCached) {
					DataSet ds = ReadDataSetConfig(CMSConfigFileType.AdminModules, "~/");

					_modules = (from d in ds.Tables[0].AsEnumerable()
								select new CMSAdminModule {
									PluginName = d.Field<string>("caption"),
									PluginID = new Guid(d.Field<string>("pluginid"))
								}).ToList();

					foreach (var p in _modules) {
						p.PluginMenus = (from d in ds.Tables["ID_" + p.PluginID.ToString()].AsEnumerable()
										 select new CMSAdminModuleMenu {
											 Caption = d.Field<string>("pluginlabel"),
											 SortOrder = string.IsNullOrEmpty(d.Field<string>("menuorder")) ? -1 : int.Parse(d.Field<string>("menuorder")),
											 PluginParm = d.Field<string>("parm"),
											 ControlFile = d.Field<string>("plugincontrol"),
											 UsePopup = string.IsNullOrEmpty(d.Field<string>("usepopup")) ? false : Convert.ToBoolean(d.Field<string>("usepopup")),
											 UseAjax = string.IsNullOrEmpty(d.Field<string>("useajax")) ? false: Convert.ToBoolean(d.Field<string>("useajax")),
											 IsVisible = string.IsNullOrEmpty(d.Field<string>("visible")) ? false : Convert.ToBoolean(d.Field<string>("visible")),
											 PluginID = string.IsNullOrEmpty(d.Field<string>("pluginid")) ? Guid.Empty : new Guid(d.Field<string>("pluginid"))
										 }).ToList();
					}
					HttpContext.Current.Cache.Insert(keyAdminMenuModules, _modules, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration);
				}
				return _modules;
			}
		}

		public List<CMSPlugin> ToolboxPlugins {
			get {

				var _plugins = new List<CMSPlugin>();

				bool bCached = false;

				try {
					_plugins = (List<CMSPlugin>)HttpContext.Current.Cache[keyAdminToolboxModules];
					if (_plugins != null) {
						bCached = true;
					}
				} catch {
					bCached = false;
				}

				if (!bCached) {
					DataSet ds = ReadDataSetConfig(CMSConfigFileType.PublicControls, "~/");

					List<CMSPlugin> _p1 = new List<CMSPlugin>();

					_p1.Add(new CMSPlugin { Caption = "Generic HTML &#0134;", FilePath = "~/Manage/ucGenericContent.ascx" });
					_p1.Add(new CMSPlugin { Caption = "Plain Text &#0134;", FilePath = "~/Manage/ucTextContent.ascx" });
					_p1.Add(new CMSPlugin { Caption = "Top Level Navigation &#0134;", FilePath = "CLASS:Carrotware.CMS.UI.Controls.TopLevelNavigation, Carrotware.CMS.UI.Controls" });
					_p1.Add(new CMSPlugin { Caption = "Two Level Navigation &#0134;", FilePath = "CLASS:Carrotware.CMS.UI.Controls.TwoLevelNavigation, Carrotware.CMS.UI.Controls" });
					_p1.Add(new CMSPlugin { Caption = "Child Navigation &#0134;", FilePath = "CLASS:Carrotware.CMS.UI.Controls.ChildNavigation, Carrotware.CMS.UI.Controls" });
					_p1.Add(new CMSPlugin { Caption = "Sibling Navigation &#0134;", FilePath = "CLASS:Carrotware.CMS.UI.Controls.SiblingNavigation, Carrotware.CMS.UI.Controls" });

					List<CMSPlugin> _p2 = (from d in ds.Tables[0].AsEnumerable()
										   select new CMSPlugin {
											   FilePath = d.Field<string>("filepath"),
											   Caption = d.Field<string>("crtldesc")
										   }).ToList();

					_plugins = _p1.Union(_p2).ToList();

					HttpContext.Current.Cache.Insert(keyAdminToolboxModules, _plugins, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration);
				}
				return _plugins;
			}
		}


		private List<CMSTemplate> GetTemplatesByDirectory() {
			var _plugins = new List<CMSTemplate>();

			var wwwpath = HttpContext.Current.Server.MapPath("~/");
			string sPlugCfg = HttpContext.Current.Server.MapPath("~/cmsTemplates/");

			if (Directory.Exists(sPlugCfg)) {

				string[] subdirs;
				try {
					subdirs = Directory.GetDirectories(sPlugCfg);
				} catch {
					subdirs = null;
				}


				if (subdirs != null) {
					foreach (string theDir in subdirs) {

						string sTplDef = theDir + @"\Skin.config";

						if (File.Exists(sTplDef)) {
							string sPathPrefix = theDir.Replace(wwwpath, @"\").Replace(@"\", @"/") + "/";
							DataSet ds = ReadDataSetConfig(CMSConfigFileType.Skin, sPathPrefix);

							var _p2 = (from d in ds.Tables[0].AsEnumerable()
									   select new CMSTemplate {
										   TemplatePath = (sPathPrefix + d.Field<string>("templatefile").ToLower()).ToLower(),
										   EncodedPath = EncodeBase64((sPathPrefix + d.Field<string>("templatefile").ToLower()).ToLower()),
										   Caption = d.Field<string>("filedesc")
									   }).ToList();

							_plugins = _plugins.Union(_p2).ToList();

						}
					}
				}
			}

			return _plugins;
		}


		public List<CMSTemplate> Templates {
			get {

				List<CMSTemplate> _plugins = null;
				string sDefTemplate = SiteData.DefaultTemplateFilename.ToLower();

				bool bCached = false;

				try {
					_plugins = (List<CMSTemplate>)HttpContext.Current.Cache[keyTemplates];
					if (_plugins != null) {
						bCached = true;
					}
				} catch {
					bCached = false;
				}

				if (!bCached) {
					_plugins = new List<CMSTemplate>();
					CMSTemplate t = new CMSTemplate();
					t.TemplatePath = sDefTemplate;
					t.EncodedPath = EncodeBase64(sDefTemplate);
					t.Caption = "  Black 'n White - Plain L-R-C Content [*]  ";
					_plugins.Add(t);
				}

				if (!bCached) {
					DataSet ds = ReadDataSetConfig(CMSConfigFileType.SiteSkins, "~/");

					var _p1 = (from d in ds.Tables[0].AsEnumerable()
							   select new CMSTemplate {
								   TemplatePath = d.Field<string>("templatefile").ToLower(),
								   EncodedPath = EncodeBase64(d.Field<string>("templatefile").ToLower()),
								   Caption = d.Field<string>("filedesc").Trim()
							   }).ToList();

					var _p2 = GetTemplatesByDirectory();

					_plugins = _plugins.Union(_p1.Where(t => t.TemplatePath.ToLower() != sDefTemplate)).ToList().
						Union(_p2.Where(t => t.TemplatePath.ToLower() != sDefTemplate)).ToList();

					HttpContext.Current.Cache.Insert(keyTemplates, _plugins, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration);
				}

				return _plugins.OrderBy(t => t.Caption).ToList();
			}
		}

		public List<DynamicSite> SiteList {
			get {

				var _plugins = new List<DynamicSite>();

				bool bCached = false;

				try {
					_plugins = (List<DynamicSite>)HttpContext.Current.Cache[keyDynamicSite];
					if (_plugins != null) {
						bCached = true;
					}
				} catch {
					bCached = false;
				}

				if (!bCached) {
					DataSet ds = ReadDataSetConfig(CMSConfigFileType.SiteMapping, "~/");

					_plugins = (from d in ds.Tables[0].AsEnumerable()
								select new DynamicSite {
									DomainName = d.Field<string>("domname"),
									SiteID = new Guid(d.Field<string>("siteid"))
								}).ToList();

					HttpContext.Current.Cache.Insert(keyDynamicSite, _plugins, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration);
				}
				return _plugins;
			}
		}


		public DynamicSite DynSite {
			get {
				var _plugins = new DynamicSite();

				string ModuleKey = keyDynSite + DomainName;
				bool bCached = false;

				try {
					_plugins = (DynamicSite)HttpContext.Current.Cache[ModuleKey];
					if (_plugins != null) {
						bCached = true;
					}
				} catch {
					bCached = false;
				}

				if ((SiteList.Count > 0) && !bCached) {

					_plugins = (from ss in SiteList
								where ss.DomainName == DomainName
								select ss).FirstOrDefault();

					HttpContext.Current.Cache.Insert(ModuleKey, _plugins, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration);
				}
				return _plugins;
			}
		}


		public static string DecodeBase64(string ValIn) {
			string val = "";
			if (!string.IsNullOrEmpty(ValIn)) {
				ASCIIEncoding encoding = new ASCIIEncoding();
				val = encoding.GetString(Convert.FromBase64String(ValIn));
			}
			return val;
		}

		public static string EncodeBase64(string ValIn) {
			string val = "";
			if (!string.IsNullOrEmpty(ValIn)) {
				ASCIIEncoding encoding = new ASCIIEncoding();
				byte[] toEncodeAsBytes = ASCIIEncoding.ASCII.GetBytes(ValIn);
				val = System.Convert.ToBase64String(toEncodeAsBytes);
			}
			return val;
		}


		public static List<ObjectProperty> GetProperties(Object theObject) {
			PropertyInfo[] info = theObject.GetType().GetProperties();

			List<ObjectProperty> props = (from i in info.AsEnumerable()
										  select GetCustProps(theObject, i)).ToList();

			return props;
		}


		public static ObjectProperty GetCustProps(Object obj, PropertyInfo prop) {

			ObjectProperty objprop = new ObjectProperty {
				Name = prop.Name,
				DefValue = obj.GetType().GetProperty(prop.Name).GetValue(obj, null),
				PropertyType = prop.PropertyType,
				CanRead = prop.CanRead,
				CanWrite = prop.CanWrite,
				Props = prop,
				CompanionSourceFieldName = "",
				FieldMode = (prop.PropertyType.ToString().ToLower() == "system.boolean") ?
						WidgetAttribute.FieldMode.CheckBox : WidgetAttribute.FieldMode.TextBox
			};

			foreach (Attribute attr in objprop.Props.GetCustomAttributes(true)) {
				if (attr is WidgetAttribute) {
					var widgetAttrib = attr as WidgetAttribute;
					if (null != widgetAttrib) {
						try { objprop.CompanionSourceFieldName = widgetAttrib.SelectFieldSource; } catch { objprop.CompanionSourceFieldName = ""; }
						try { objprop.FieldMode = widgetAttrib.Mode; } catch { objprop.FieldMode = WidgetAttribute.FieldMode.Unknown; }
					}
				}
			}

			return objprop;
		}


		public static List<ObjectProperty> GetTypeProperties(Type theType) {
			PropertyInfo[] info = theType.GetProperties();

			List<ObjectProperty> props = (from i in info.AsEnumerable()
										  select new ObjectProperty {
											  Name = i.Name,
											  PropertyType = i.PropertyType,
											  CanRead = i.CanRead,
											  CanWrite = i.CanWrite
										  }).ToList();
			return props;
		}

		public void OverrideKey(Guid guidContentID) {
			filePage = null;
			using (ContentPageHelper pageHelper = new ContentPageHelper()) {
				filePage = pageHelper.GetLatestContent(SiteData.CurrentSiteID, guidContentID);
			}
		}

		public void OverrideKey(string sPageName) {
			filePage = null;
			using (ContentPageHelper pageHelper = new ContentPageHelper()) {
				filePage = pageHelper.GetLatestContent(SiteData.CurrentSiteID, null, sPageName.ToLower());
			}
		}

		protected ContentPage filePage = null;

		protected void LoadGuids() {
			if (filePage == null) {
				using (ContentPageHelper pageHelper = new ContentPageHelper()) {
					if (SiteData.CurrentScriptName.ToString().ToLower().StartsWith("/manage/")) {
						Guid guidPage = Guid.Empty;
						if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["pageid"])) {
							guidPage = new Guid(HttpContext.Current.Request.QueryString["pageid"].ToString());
						}
						filePage = pageHelper.GetLatestContent(SiteData.CurrentSiteID, guidPage);
					} else {
						filePage = pageHelper.GetLatestContent(SiteData.CurrentSiteID, null, SiteData.CurrentScriptName.ToString().ToLower());
					}
				}
			}
		}

		public ContentPage cmsAdminContent {
			get {
				ContentPage c = null;
				try {
					var sXML = GetSerialized(keyAdminContent);
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(ContentPage));
					Object genpref = null;
					using (StringReader stringReader = new StringReader(sXML)) {
						genpref = xmlSerializer.Deserialize(stringReader);
					}
					c = genpref as ContentPage;
				} catch (Exception ex) { }
				return c;
			}
			set {
				if (value == null) {
					ClearSerialized(keyAdminContent);
				} else {
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(ContentPage));
					string sXML = "";
					using (StringWriter stringWriter = new StringWriter()) {
						xmlSerializer.Serialize(stringWriter, value);
						sXML = stringWriter.ToString();
					}
					SaveSerialized(keyAdminContent, sXML);
				}
			}
		}

		public List<PageWidget> cmsAdminWidget {
			get {
				List<PageWidget> c = null;
				var sXML = GetSerialized(keyAdminWidget);
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<PageWidget>));
				Object genpref = null;
				using (StringReader stringReader = new StringReader(sXML)) {
					genpref = xmlSerializer.Deserialize(stringReader);
				}
				c = genpref as List<PageWidget>;
				return c;
			}
			set {
				if (value == null) {
					ClearSerialized(keyAdminWidget);
				} else {
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<PageWidget>));
					string sXML = "";
					using (StringWriter stringWriter = new StringWriter()) {
						xmlSerializer.Serialize(stringWriter, value);
						sXML = stringWriter.ToString();
					}
					SaveSerialized(keyAdminWidget, sXML);
				}
			}
		}


		public static void SaveSerialized(Guid itemID, string sKey, string sData) {

			using (CarrotCMSDataContext _db = new CarrotCMSDataContext()) {
				bool bAdd = false;

				var itm = (from c in _db.carrot_SerialCaches
						   where c.ItemID == itemID
						   && c.EditUserId == SiteData.CurrentUserGuid
						   && c.KeyType == sKey
						   && c.SiteID == SiteData.CurrentSiteID
						   select c).FirstOrDefault();

				if (itm == null) {
					bAdd = true;
					itm = new carrot_SerialCache();
					itm.SerialCacheID = Guid.NewGuid();
					itm.SiteID = SiteData.CurrentSiteID;
					itm.ItemID = itemID;
					itm.EditUserId = SiteData.CurrentUserGuid;
					itm.KeyType = sKey;
				}

				itm.SerializedData = sData;
				itm.EditDate = DateTime.Now;

				if (bAdd) {
					_db.carrot_SerialCaches.InsertOnSubmit(itm);
				}
				_db.SubmitChanges();
			}
		}


		public static string GetSerialized(Guid itemID, string sKey) {
			string sData = "";
			using (CarrotCMSDataContext _db = new CarrotCMSDataContext()) {

				var itm = (from c in _db.carrot_SerialCaches
						   where c.ItemID == itemID
						   && c.EditUserId == SiteData.CurrentUserGuid
						   && c.KeyType == sKey
						   && c.SiteID == SiteData.CurrentSiteID
						   select c).FirstOrDefault();

				if (itm != null) {
					sData = itm.SerializedData;
				}
			}
			return sData;
		}


		public static bool ClearSerialized(Guid itemID, string sKey) {
			bool bRet = false;
			using (CarrotCMSDataContext _db = new CarrotCMSDataContext()) {
				var itm = (from c in _db.carrot_SerialCaches
						   where c.ItemID == itemID
						   && c.EditUserId == SiteData.CurrentUserGuid
						   && c.KeyType == sKey
						   && c.SiteID == SiteData.CurrentSiteID
						   select c).FirstOrDefault();

				if (itm != null) {
					_db.carrot_SerialCaches.DeleteOnSubmit(itm);
					_db.SubmitChanges();
					bRet = true;
				}
			}
			return bRet;
		}



		private void SaveSerialized(string sKey, string sData) {
			LoadGuids();

			CMSConfigHelper.SaveSerialized(filePage.Root_ContentID, sKey, sData);
		}


		private string GetSerialized(string sKey) {
			string sData = "";
			LoadGuids();

			sData = CMSConfigHelper.GetSerialized(filePage.Root_ContentID, sKey);

			return sData;
		}


		private bool ClearSerialized(string sKey) {
			LoadGuids();

			return CMSConfigHelper.ClearSerialized(filePage.Root_ContentID, sKey); ;
		}

	}




	public class ObjectProperty {
		public ObjectProperty() { }
		public string Name { get; set; }
		public bool CanWrite { get; set; }
		public bool CanRead { get; set; }
		public Type PropertyType { get; set; }

		public Object DefValue { get; set; }

		public PropertyInfo Props { get; set; }

		public string CompanionSourceFieldName { get; set; }

		public WidgetAttribute.FieldMode FieldMode { get; set; }


		public override bool Equals(Object obj) {
			//Check for null and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) return false;
			if (obj is ObjectProperty) {
				ObjectProperty p = (ObjectProperty)obj;
				return (Name == p.Name) && (PropertyType == p.PropertyType);
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return Name.GetHashCode() ^ PropertyType.ToString().GetHashCode();
		}

	}


	public class CMSAdminModule {
		public CMSAdminModule() {
			PluginMenus = new List<CMSAdminModuleMenu>();
		}
		public Guid PluginID { get; set; }
		public string PluginName { get; set; }
		public List<CMSAdminModuleMenu> PluginMenus { get; set; }
	}

	public class CMSAdminModuleMenu {
		public Guid PluginID { get; set; }
		public int SortOrder { get; set; }
		public string Caption { get; set; }
		public string PluginParm { get; set; }
		public string ControlFile { get; set; }
		public bool UseAjax { get; set; }
		public bool UsePopup { get; set; }
		public bool IsVisible { get; set; }

	}

	public class CMSPlugin {
		public string FilePath { get; set; }
		public string Caption { get; set; }
	}

	public class CMSTemplate {
		public string TemplatePath { get; set; }
		public string Caption { get; set; }
		public string EncodedPath { get; set; }
	}

	public class DynamicSite {
		public Guid SiteID { get; set; }
		public string DomainName { get; set; }

	}

}
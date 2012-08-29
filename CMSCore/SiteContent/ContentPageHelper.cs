﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Carrotware.CMS.Data;
using System.Reflection;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
/*
* CarrotCake CMS
* http://www.carrotware.com/
*
* Copyright 2011, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: October 2011
*/


namespace Carrotware.CMS.Core {

	public class ContentPageHelper : IDisposable {


		protected CarrotCMSDataContext db = new CarrotCMSDataContext();

		public ContentPageHelper() {

		}

		public static string ScrubFilename(Guid rootContentID, string FileName) {
			string newFileName = FileName;

			if (string.IsNullOrEmpty(newFileName)) {
				newFileName = rootContentID.ToString();
			}

			newFileName = newFileName.Replace(" ", "-");
			newFileName = newFileName.Replace("'", "-");
			newFileName = newFileName.Replace("\"", "-");
			newFileName = newFileName.Replace(@"\", @"/");
			newFileName = newFileName.Replace("*", "-star-");
			newFileName = newFileName.Replace("%", "-percent-");
			newFileName = newFileName.Replace("&", "-n-");
			newFileName = newFileName.Replace("--", "-").Replace("--", "-");
			newFileName = newFileName.Replace(@"//", @"/").Replace(@"//", @"/");
			newFileName = newFileName.Trim();

			newFileName = Regex.Replace(newFileName, "[:\"*?<>|]+", "-");
			newFileName = Regex.Replace(newFileName, @"[^0-9a-zA-Z.-/_]+", "-");

			newFileName = newFileName.Replace("--", "-").Replace("--", "-");

			if (newFileName.ToLower().EndsWith(".htm")) {
				newFileName = newFileName.Substring(0, newFileName.Length - 4) + ".aspx";
			}
			if (newFileName.ToLower().EndsWith(".html")) {
				newFileName = newFileName.Substring(0, newFileName.Length - 5) + ".aspx";
			}

			if (!newFileName.ToLower().EndsWith(".aspx")) {
				newFileName = newFileName + ".aspx";
			}
			if (newFileName.ToLower().IndexOf(@"/") < 0) {
				newFileName = "/" + newFileName;
			}

			return newFileName;
		}

		public List<ContentPage> GetLatestContentList(Guid siteID) {
			List<ContentPage> lstContent = (from ct in db.carrot_Contents
											join r in db.carrot_RootContents on ct.Root_ContentID equals r.Root_ContentID
											orderby ct.NavOrder, ct.NavMenuText
											where r.SiteID == siteID
											 && ct.IsLatestVersion == true
											select new ContentPage(r, ct)).ToList();
			return lstContent;
		}

		public int GetContentPagedListCount(Guid siteID, bool bActiveOnly) {
			int iCount = (from ct in db.carrot_Contents
						  join r in db.carrot_RootContents on ct.Root_ContentID equals r.Root_ContentID
						  where r.SiteID == siteID
							  && ct.IsLatestVersion == true
							  && (r.PageActive == bActiveOnly || bActiveOnly == false)
						  select r).Count();
			return iCount;
		}


		public List<ContentPage> GetLatestContentPagedList(Guid siteID, bool bActiveOnly, int pageNumber, string sortField, string sortDir) {
			return GetLatestContentPagedList(siteID, bActiveOnly, 10, pageNumber, sortField, sortDir);
		}

		public List<ContentPage> GetLatestContentPagedList(Guid siteID, bool bActiveOnly, int pageNumber) {
			return GetLatestContentPagedList(siteID, bActiveOnly, 10, pageNumber, "", "");
		}

		public List<ContentPage> GetLatestContentPagedList(Guid siteID, bool bActiveOnly, int pageSize, int pageNumber) {
			return GetLatestContentPagedList(siteID, bActiveOnly, pageSize, pageNumber, "", "");
		}

		public void ResetHeartbeatLock(Guid rootContentID, Guid siteID) {

			carrot_RootContent rc = (from r in db.carrot_RootContents
									 where r.Root_ContentID == rootContentID
									   && r.SiteID == siteID
									 select r).FirstOrDefault();

			rc.EditHeartbeat = DateTime.Now.AddHours(-2);
			rc.Heartbeat_UserId = null;
			db.SubmitChanges();
		}

		public bool RecordHeartbeatLock(Guid rootContentID, Guid siteID, Guid currentUserID) {

			carrot_RootContent rc = (from r in db.carrot_RootContents
									 where r.Root_ContentID == rootContentID
									 && r.SiteID == siteID
									 select r).FirstOrDefault();

			if (rc != null) {
				rc.Heartbeat_UserId = currentUserID;
				rc.EditHeartbeat = DateTime.Now;
				db.SubmitChanges();
				return true;
			}

			return false;
		}

		public bool IsPageLocked(Guid rootContentID) {

			carrot_RootContent rc = (from r in db.carrot_RootContents
									 where r.Root_ContentID == rootContentID
									 && r.SiteID == SiteData.CurrentSiteID
									 select r).FirstOrDefault();

			bool bLock = false;
			if (rc.Heartbeat_UserId != null) {
				if (rc.Heartbeat_UserId != SecurityData.CurrentUserGuid
						&& rc.EditHeartbeat.Value > DateTime.Now.AddMinutes(-2)) {
					bLock = true;
				}
				if (rc.Heartbeat_UserId == SecurityData.CurrentUserGuid
					|| rc.Heartbeat_UserId == null) {
					bLock = false;
				}
			}
			return bLock;
		}

		public bool IsPageLocked(Guid rootContentID, Guid siteID, Guid currentUserID) {

			carrot_RootContent rc = (from r in db.carrot_RootContents
									 where r.Root_ContentID == rootContentID
									 && r.SiteID == siteID
									 select r).FirstOrDefault();

			bool bLock = false;
			if (rc.Heartbeat_UserId != null) {
				if (rc.Heartbeat_UserId != currentUserID
						&& rc.EditHeartbeat.Value > DateTime.Now.AddMinutes(-2)) {
					bLock = true;
				}
				if (rc.Heartbeat_UserId == currentUserID
					|| rc.Heartbeat_UserId == null) {
					bLock = false;
				}
			}
			return bLock;
		}

		public bool IsPageLocked(Guid rootContentID, Guid siteID) {

			carrot_RootContent rc = (from r in db.carrot_RootContents
									 where r.Root_ContentID == rootContentID
									 && r.SiteID == siteID
									 select r).FirstOrDefault();

			bool bLock = false;
			if (rc.Heartbeat_UserId != null) {
				if (rc.Heartbeat_UserId != SecurityData.CurrentUserGuid
						&& rc.EditHeartbeat.Value > DateTime.Now.AddMinutes(-2)) {
					bLock = true;
				}
				if (rc.Heartbeat_UserId == SecurityData.CurrentUserGuid
					|| rc.Heartbeat_UserId == null) {
					bLock = false;
				}
			}
			return bLock;
		}

		public bool IsPageLocked(ContentPage cp) {

			bool bLock = false;
			if (cp.Heartbeat_UserId != null) {
				if (cp.Heartbeat_UserId != SecurityData.CurrentUserGuid
						&& cp.EditHeartbeat.Value > DateTime.Now.AddMinutes(-2)) {
					bLock = true;
				}
				if (cp.Heartbeat_UserId == SecurityData.CurrentUserGuid
					|| cp.Heartbeat_UserId == null) {
					bLock = false;
				}
			}
			return bLock;
		}

		public Guid GetCurrentEditUser(Guid rootContentID, Guid siteID) {

			carrot_RootContent rc = (from r in db.carrot_RootContents
									 where r.Root_ContentID == rootContentID
									 && r.SiteID == siteID
									 select r).FirstOrDefault();

			if (rc != null) {
				return (Guid)rc.Heartbeat_UserId;
			} else {
				return Guid.Empty;
			}
		}


		public List<ContentPage> GetLatestContentPagedList(Guid siteID, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			int startRec = pageNumber * pageSize;

			if (pageSize < 0 || pageSize > 200) {
				pageSize = 25;
			}

			if (pageNumber < 0 || pageNumber > 10000) {
				pageNumber = 0;
			}

			if (string.IsNullOrEmpty(sortField)) {
				sortField = "CreateDate";
			}

			if (string.IsNullOrEmpty(sortDir)) {
				sortDir = "DESC";
			}

			IEnumerable<ContentPage> query = new List<ContentPage>();

			bool bIsContent = TestIfPropExists(new carrot_Content(), sortField);
			bool bIsRootContent = TestIfPropExists(new carrot_RootContent(), sortField);

			if (bIsRootContent) {
				if (sortDir.ToUpper().Trim().IndexOf("ASC") < 0) {
					query = (from enu in
								 (from ct in db.carrot_Contents.AsEnumerable()
								  join r in db.carrot_RootContents.AsEnumerable() on ct.Root_ContentID equals r.Root_ContentID
								  orderby GetPropertyValue(r, sortField) descending
								  where r.SiteID == siteID
									 && ct.IsLatestVersion == true
									 && (r.PageActive == bActiveOnly || bActiveOnly == false)
								  select new ContentPage(r, ct)).AsQueryable()
							 select enu).AsQueryable().Skip(startRec).Take(pageSize);
				} else {
					query = (from enu in
								 (from ct in db.carrot_Contents.AsEnumerable()
								  join r in db.carrot_RootContents.AsEnumerable() on ct.Root_ContentID equals r.Root_ContentID
								  orderby GetPropertyValue(r, sortField) ascending
								  where r.SiteID == siteID
									 && ct.IsLatestVersion == true
									 && (r.PageActive == bActiveOnly || bActiveOnly == false)
								  select new ContentPage(r, ct)).AsQueryable()
							 select enu).AsQueryable().Skip(startRec).Take(pageSize);
				}
			}

			if (bIsContent && !bIsRootContent) {
				if (sortDir.ToUpper().Trim().IndexOf("ASC") < 0) {
					query = (from enu in
								 (from ct in db.carrot_Contents.AsEnumerable()
								  join r in db.carrot_RootContents.AsEnumerable() on ct.Root_ContentID equals r.Root_ContentID
								  orderby GetPropertyValue(ct, sortField) descending
								  where r.SiteID == siteID
									 && ct.IsLatestVersion == true
									 && (r.PageActive == bActiveOnly || bActiveOnly == false)
								  select new ContentPage(r, ct)).AsQueryable()
							 select enu).AsQueryable().Skip(startRec).Take(pageSize);
				} else {
					query = (from enu in
								 (from ct in db.carrot_Contents.AsEnumerable()
								  join r in db.carrot_RootContents.AsEnumerable() on ct.Root_ContentID equals r.Root_ContentID
								  orderby GetPropertyValue(ct, sortField) ascending
								  where r.SiteID == siteID
									 && ct.IsLatestVersion == true
									 && (r.PageActive == bActiveOnly || bActiveOnly == false)
								  select new ContentPage(r, ct)).AsQueryable()
							 select enu).AsQueryable().Skip(startRec).Take(pageSize);
				}
			}

			if (!bIsContent && !bIsRootContent) {
				query = (from enu in
							 (from ct in db.carrot_Contents
							  join r in db.carrot_RootContents on ct.Root_ContentID equals r.Root_ContentID
							  orderby r.CreateDate descending
							  where r.SiteID == siteID
								 && ct.IsLatestVersion == true
								 && (r.PageActive == bActiveOnly || bActiveOnly == false)
							  select new ContentPage(r, ct)).AsQueryable()
						 select enu).AsQueryable().Skip(startRec).Take(pageSize);
			}

			return query.ToList();
		}


		private object GetPropertyValue(object obj, string property) {
			PropertyInfo propertyInfo = obj.GetType().GetProperty(property);
			return propertyInfo.GetValue(obj, null);
		}

		private bool TestIfPropExists(object obj, string property) {
			PropertyInfo propertyInfo = obj.GetType().GetProperty(property);
			return propertyInfo == null ? false : true;
		}

		public ContentPage CopyContentPageToNew(ContentPage pageSource) {
			ContentPage pageNew = new ContentPage();
			pageNew.ContentID = Guid.NewGuid();
			pageNew.SiteID = pageSource.SiteID;
			pageNew.Parent_ContentID = pageSource.Parent_ContentID;
			pageNew.Root_ContentID = pageSource.Root_ContentID;

			pageNew.PageText = pageSource.PageText;
			pageNew.LeftPageText = pageSource.LeftPageText;
			pageNew.RightPageText = pageSource.RightPageText;

			pageNew.IsLatestVersion = true;
			pageNew.FileName = pageSource.FileName;
			pageNew.TitleBar = pageSource.TitleBar;
			pageNew.NavMenuText = pageSource.NavMenuText;
			pageNew.NavOrder = pageSource.NavOrder;
			pageNew.PageHead = pageSource.PageHead;
			pageNew.PageActive = pageSource.PageActive;
			pageNew.EditUserId = SecurityData.CurrentUserGuid;
			pageNew.EditDate = DateTime.Now;
			pageNew.CreateDate = pageSource.CreateDate;

			pageNew.TemplateFile = pageSource.TemplateFile;
			pageNew.MetaDescription = pageSource.MetaDescription;
			pageNew.MetaKeyword = pageSource.MetaKeyword;

			return pageNew;
		}


		public ContentPage GetSamplerView() {

			string sFile1 = "";
			string sFile2 = "";

			Assembly _assembly = Assembly.GetExecutingAssembly();

			using (StreamReader oTextStream = new StreamReader(_assembly.GetManifestResourceStream("Carrotware.CMS.Core.SiteContent.SampleContent1.txt"))) {
				sFile1 = oTextStream.ReadToEnd();
			}
			using (StreamReader oTextStream = new StreamReader(_assembly.GetManifestResourceStream("Carrotware.CMS.Core.SiteContent.SampleContent2.txt"))) {
				sFile2 = oTextStream.ReadToEnd();
			}

			ContentPage pageNew = new ContentPage();
			pageNew.Root_ContentID = Guid.NewGuid();
			pageNew.ContentID = pageNew.Root_ContentID;
			pageNew.SiteID = SiteData.CurrentSiteID;
			pageNew.Parent_ContentID = null;

			pageNew.PageText = "<h2>CENTER</h2>\r\n" + sFile1;
			pageNew.LeftPageText = "<h2>LEFT</h2>\r\n" + sFile2;
			pageNew.RightPageText = "<h2>RIGHT</h2>\r\n" + sFile2;

			pageNew.IsLatestVersion = true;
			pageNew.TitleBar = "Template Preview - TITLE";
			pageNew.NavMenuText = "Template Preview - NAV";
			pageNew.NavOrder = -1;
			pageNew.PageHead = "Template Preview - HEAD";
			pageNew.PageActive = true;
			pageNew.EditUserId = SecurityData.CurrentUserGuid;
			pageNew.EditDate = DateTime.Now.AddMinutes(-30);
			pageNew.CreateDate = DateTime.Today.AddDays(-1);

			pageNew.TemplateFile = SiteData.PreviewTemplateFile;
			pageNew.FileName = SiteData.VirtualCMSEditPrefix + "TemplatePreviw.aspx";
			pageNew.NavFileName = pageNew.FileName;
			pageNew.MetaDescription = "Meta Description";
			pageNew.MetaKeyword = "Meta Keyword";

			return pageNew;
		}



		public List<ContentPage> GetVersionHistory(Guid siteID, Guid rootContentID) {
			List<ContentPage> content = (from ct in db.carrot_Contents
										 join r in db.carrot_RootContents on ct.Root_ContentID equals r.Root_ContentID
										 orderby ct.EditDate descending
										 where r.SiteID == siteID
										  && r.Root_ContentID == rootContentID
										 select new ContentPage(r, ct)).ToList();
			return content;
		}

		public ContentPage GetVersion(Guid siteID, Guid contentID) {
			ContentPage content = (from ct in db.carrot_Contents
								   join r in db.carrot_RootContents on ct.Root_ContentID equals r.Root_ContentID
								   orderby ct.EditDate descending
								   where r.SiteID == siteID
									&& ct.ContentID == contentID
								   select new ContentPage(r, ct)).FirstOrDefault();
			return content;
		}


		public List<ContentPage> GetLatestContentList(Guid siteID, bool? active) {
			List<ContentPage> lstContent = (from ct in db.carrot_Contents
											join r in db.carrot_RootContents on ct.Root_ContentID equals r.Root_ContentID
											orderby ct.NavOrder, ct.NavMenuText
											where r.SiteID == siteID
											 && ct.IsLatestVersion == true
											 && (r.PageActive == active || active == null)
											select new ContentPage(r, ct)).ToList();

			return lstContent;
		}


		public void RemoveVersions(Guid siteID, List<Guid> lstDel) {

			List<carrot_Content> lstContent = (from ct in db.carrot_Contents
											   join r in db.carrot_RootContents on ct.Root_ContentID equals r.Root_ContentID
											   orderby ct.EditDate descending
											   where r.SiteID == siteID
												&& lstDel.Contains(ct.ContentID)
												&& ct.IsLatestVersion != true
											   select ct).ToList();

			if (lstContent.Count > 0) {
				foreach (carrot_Content c in lstContent) {
					db.carrot_Contents.DeleteOnSubmit(c);
				}
				db.SubmitChanges();
			}
		}


		public void BulkUpdateTemplate(Guid siteID, List<Guid> lstUpd, string sTemplateFile) {

			List<carrot_Content> lstContent = (from ct in db.carrot_Contents
											   join r in db.carrot_RootContents on ct.Root_ContentID equals r.Root_ContentID
											   where r.SiteID == siteID
												&& lstUpd.Contains(r.Root_ContentID)
												&& ct.IsLatestVersion == true
											   select ct).ToList();

			if (lstContent.Count > 0) {
				foreach (carrot_Content c in lstContent) {
					c.TemplateFile = sTemplateFile;
					//c.EditDate = DateTime.Now;
				}
				db.SubmitChanges();
			}
		}



		public ContentPage GetLatestContent(Guid siteID, Guid rootContentID) {
			ContentPage content = (from ct in db.carrot_Contents
								   join r in db.carrot_RootContents on ct.Root_ContentID equals r.Root_ContentID
								   where r.SiteID == siteID
									   && r.Root_ContentID == rootContentID
									   && ct.IsLatestVersion == true
								   select new ContentPage(r, ct)).FirstOrDefault();

			return content;
		}


		public ContentPage GetLatestContent(Guid siteID, bool? active, string sPage) {
			ContentPage content = (from ct in db.carrot_Contents
								   join r in db.carrot_RootContents on ct.Root_ContentID equals r.Root_ContentID
								   where r.SiteID == siteID
									   && (r.PageActive == active || active == null)
									   && r.FileName.ToLower() == sPage.ToLower()
									   && ct.IsLatestVersion == true
								   select new ContentPage(r, ct)).FirstOrDefault();

			return content;
		}



		public ContentPage FindHome(Guid siteID) {
			ContentPage content = (from ct in db.carrot_Contents
								   join r in db.carrot_RootContents on ct.Root_ContentID equals r.Root_ContentID
								   orderby ct.NavOrder ascending
								   where r.SiteID == siteID
									   && r.PageActive == true
									   && ct.NavOrder < 1
									   && ct.IsLatestVersion == true
								   select new ContentPage(r, ct)).FirstOrDefault();

			return content;
		}


		public int GetSitePageCount(Guid siteID) {
			int content = (from r in db.carrot_RootContents
						   where r.SiteID == siteID
						   select r).Count();
			return content;
		}

		public ContentPage FindByFilename(Guid siteID, string urlFileName) {
			ContentPage content = (from ct in db.carrot_Contents
								   join r in db.carrot_RootContents on ct.Root_ContentID equals r.Root_ContentID
								   where r.SiteID == siteID
									   && ct.IsLatestVersion == true
									   && r.FileName.ToLower() == urlFileName.ToLower()
								   select new ContentPage(r, ct)).FirstOrDefault();

			return content;
		}

		public ContentPage FindHome(Guid siteID, bool? active) {
			ContentPage content = (from ct in db.carrot_Contents
								   join r in db.carrot_RootContents on ct.Root_ContentID equals r.Root_ContentID
								   orderby ct.NavOrder ascending
								   where r.SiteID == siteID
									   && (r.PageActive == active || active == null)
									   && ct.NavOrder < 1
									   && ct.IsLatestVersion == true
								   select new ContentPage(r, ct)).FirstOrDefault();

			return content;
		}



		#region IDisposable Members

		public void Dispose() {
			if (db != null) {
				db.Dispose();
			}
		}

		#endregion
	}

}
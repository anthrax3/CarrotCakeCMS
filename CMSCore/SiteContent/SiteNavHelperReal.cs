﻿using System;
using System.Collections.Generic;
using System.Linq;
using Carrotware.CMS.Data;
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

	public class SiteNavHelperReal : IDisposable, ISiteNavHelper {
		protected CarrotCMSDataContext db = new CarrotCMSDataContext();

		public SiteNavHelperReal() { }

		private SiteNav MakeSiteNav(carrot_RootContent rc, carrot_Content c) {
			var nav = new SiteNav();

			if (rc == null) {
				rc = new carrot_RootContent();
				rc.Root_ContentID = Guid.NewGuid();
				rc.PageActive = true;
			}
			if (c == null) {
				c = new carrot_Content();
				c.ContentID = rc.Root_ContentID;
				c.Root_ContentID = rc.Root_ContentID;
			}

			if (c.Root_ContentID == rc.Root_ContentID) {

				nav.Root_ContentID = rc.Root_ContentID;
				nav.SiteID = rc.SiteID;
				nav.FileName = rc.FileName;
				nav.PageActive = Convert.ToBoolean(rc.PageActive);
				nav.CreateDate = rc.CreateDate;

				nav.ContentID = c.ContentID;
				nav.Parent_ContentID = c.Parent_ContentID;
				nav.TitleBar = c.TitleBar;
				nav.NavMenuText = c.NavMenuText;
				nav.PageHead = c.PageHead;
				nav.PageText = c.PageText;
				nav.NavOrder = c.NavOrder;
				nav.EditDate = c.EditDate;
				nav.TemplateFile = c.TemplateFile;
			}

			return nav;
		}

		private SiteNav MakeSiteNav(vw_carrot_Content c) {
			var nav = new SiteNav();

			if (c == null) {
				c = new vw_carrot_Content();
				c.Root_ContentID = Guid.NewGuid();
				c.ContentID = c.Root_ContentID;
				c.PageActive = true;
			}

			nav.Root_ContentID = c.Root_ContentID;
			nav.SiteID = c.SiteID;
			nav.FileName = c.FileName;
			nav.PageActive = Convert.ToBoolean(c.PageActive);
			nav.CreateDate = c.CreateDate;

			nav.ContentID = c.ContentID;
			nav.Parent_ContentID = c.Parent_ContentID;
			nav.TitleBar = c.TitleBar;
			nav.NavMenuText = c.NavMenuText;
			nav.PageHead = c.PageHead;
			nav.PageText = c.PageText;
			nav.NavOrder = c.NavOrder;
			nav.EditDate = c.EditDate;
			nav.TemplateFile = c.TemplateFile;

			return nav;
		}

		public List<SiteNav> GetMasterNavigation(Guid siteID, bool bActiveOnly) {
			List<SiteNav> lstContent = (from ct in db.vw_carrot_Contents
										orderby ct.NavOrder, ct.NavMenuText
										where ct.SiteID == siteID
											&& (ct.PageActive == bActiveOnly || bActiveOnly == false)
											&& ct.IsLatestVersion == true
										select MakeSiteNav(ct)).ToList();

			return lstContent;
		}



		public List<SiteNav> GetTwoLevelNavigation(Guid siteID, bool bActiveOnly) {
			List<SiteNav> lstContent = null;

			List<Guid> lstTop = (from ct in db.vw_carrot_Contents
								 where ct.SiteID == siteID
									 && ct.Parent_ContentID == null
									 && ct.IsLatestVersion == true
								 select ct.Root_ContentID).ToList();

			lstContent = (from ct in db.vw_carrot_Contents
						  orderby ct.NavOrder, ct.NavMenuText
						  where ct.SiteID == siteID
								&& (ct.PageActive == bActiveOnly || bActiveOnly == false)
								&& ct.IsLatestVersion == true
								&& (lstTop.Contains(ct.Root_ContentID) || lstTop.Contains(ct.Parent_ContentID.Value))
						  select MakeSiteNav(ct)).ToList();

			return lstContent;
		}


		public List<SiteNav> GetTopNavigation(Guid siteID, bool bActiveOnly) {
			List<SiteNav> lstContent = null;

			lstContent = (from ct in db.vw_carrot_Contents
						  orderby ct.NavOrder, ct.NavMenuText
						  where ct.SiteID == siteID
							  && ct.Parent_ContentID == null
							  && (ct.PageActive == bActiveOnly || bActiveOnly == false)
							  && ct.IsLatestVersion == true
						  select MakeSiteNav(ct)).ToList();

			return lstContent;
		}

		private SiteNav GetPageNavigation(Guid siteID, Guid? rootContentID, bool bActiveOnly) {

			SiteNav content = null;

			content = (from ct in db.vw_carrot_Contents
					   where ct.SiteID == siteID
						   && ct.Root_ContentID == rootContentID
						   && ct.IsLatestVersion == true
						   && (ct.PageActive == bActiveOnly || bActiveOnly == false)
					   select MakeSiteNav(ct)).FirstOrDefault();

			return content;
		}


		public List<SiteNav> GetPathNavigation(Guid siteID, Guid rootContentID, bool bActiveOnly) {

			List<SiteNav> lstContent = new List<SiteNav>();

			int iOrder = 1000000;

			if (rootContentID != Guid.Empty) {
				Guid? gLast = rootContentID;

				while (gLast != null) {
					SiteNav nav = GetPageNavigation(siteID, gLast, false);
					gLast = null;

					if (nav != null) {
						nav.NavOrder = iOrder;
						lstContent.Add(nav);
						iOrder--;

						gLast = nav.Parent_ContentID;
					}
				}

				SiteNav home = FindHome(siteID, null);
				home.NavOrder = 0;

				if (lstContent.Where(x => x.Root_ContentID == home.Root_ContentID).Count() < 1) {
					lstContent.Add(home);
				}

			}

			return lstContent.OrderBy(x => x.NavOrder).ToList();

		}

		public List<SiteNav> GetPathNavigation(Guid siteID, string sPage, bool bActiveOnly) {

			vw_carrot_Content c = (from ct in db.vw_carrot_Contents
								   where ct.SiteID == siteID
										  && ct.FileName.ToLower() == sPage.ToLower()
										  && ct.IsLatestVersion == true
								   select ct).FirstOrDefault();

			return GetPathNavigation(siteID, c.Root_ContentID, bActiveOnly);

		}

		public List<SiteNav> GetChildNavigation(Guid siteID, string sParentID, bool bActiveOnly) {

			carrot_RootContent c = (from r in db.carrot_RootContents
									where r.SiteID == siteID
										   && r.FileName.ToLower() == sParentID.ToLower()
									select r).FirstOrDefault();

			return GetChildNavigation(siteID, c.Root_ContentID, bActiveOnly);
		}

		public List<SiteNav> GetChildNavigation(Guid siteID, Guid ParentID, bool bActiveOnly) {
			List<SiteNav> lstContent = null;

			lstContent = (from ct in db.vw_carrot_Contents
						  orderby ct.NavOrder, ct.NavMenuText
						  where ct.SiteID == siteID
							  && ct.Parent_ContentID == ParentID
							  && (ct.PageActive == bActiveOnly || bActiveOnly == false)
							  && ct.IsLatestVersion == true
						  select MakeSiteNav(ct)).ToList();

			return lstContent;
		}

		public SiteNav GetPageCrumbNavigation(Guid siteID, string sPage) {

			SiteNav content = null;

			content = (from ct in db.vw_carrot_Contents
					   where ct.SiteID == siteID
						   && ct.FileName.ToLower() == sPage.ToLower()
						   && ct.IsLatestVersion == true
					   select MakeSiteNav(ct)).FirstOrDefault();

			return content;
		}

		public SiteNav GetPageCrumbNavigation(Guid siteID, Guid rootContentID) {

			SiteNav content = null;

			content = (from ct in db.vw_carrot_Contents
					   where ct.SiteID == siteID
						   && ct.Root_ContentID == rootContentID
						   && ct.IsLatestVersion == true
					   select MakeSiteNav(ct)).FirstOrDefault();

			return content;
		}


		public SiteNav GetParentPageNavigation(Guid siteID, string sPage) {
			SiteNav nav1 = GetPageCrumbNavigation(siteID, sPage);

			return GetParentPageNavigation(siteID, nav1.Root_ContentID);
		}


		public SiteNav GetParentPageNavigation(Guid siteID, Guid rootContentID) {
			SiteNav nav1 = GetPageCrumbNavigation(siteID, rootContentID);

			SiteNav content = null;
			if (nav1 != null) {
				content = (from ct in db.vw_carrot_Contents
						   where ct.SiteID == siteID
							   && ct.Root_ContentID == nav1.Parent_ContentID
							   && ct.IsLatestVersion == true
						   select MakeSiteNav(ct)).FirstOrDefault();
			}

			return content;
		}


		public List<SiteNav> GetSiblingNavigation(Guid siteID, Guid PageID, bool bActiveOnly) {

			carrot_Content c = (from ct in db.carrot_Contents
								where ct.Root_ContentID == PageID
								   && ct.IsLatestVersion == true
								select ct).FirstOrDefault();

			List<SiteNav> lstContent = new List<SiteNav>();

			if (c != null) {
				lstContent = (from ct in db.vw_carrot_Contents
							  orderby ct.NavOrder, ct.NavMenuText
							  where ct.SiteID == siteID
								  && ct.Parent_ContentID == c.Parent_ContentID
								  && (ct.PageActive == bActiveOnly || bActiveOnly == false)
								  && ct.IsLatestVersion == true
							  select MakeSiteNav(ct)).ToList();
			}

			return lstContent;
		}

		public List<SiteNav> GetSiblingNavigation(Guid siteID, string sPage, bool bActiveOnly) {

			vw_carrot_Content c = (from ct in db.vw_carrot_Contents
								   where ct.SiteID == siteID
										  && ct.FileName.ToLower() == sPage.ToLower()
										  && ct.IsLatestVersion == true
								   select ct).FirstOrDefault();

			return GetSiblingNavigation(siteID, c.Root_ContentID, bActiveOnly);
		}

		public List<SiteNav> GetLatest(Guid siteID, int iUpdates, bool bActiveOnly) {

			List<SiteNav> lstContent = null;

			lstContent = (from ct in db.vw_carrot_Contents
						  orderby ct.EditDate descending
						  where ct.SiteID == siteID
							  && ct.IsLatestVersion == true
							  && (ct.PageActive == bActiveOnly || bActiveOnly == false)
						  select MakeSiteNav(ct)).Take(iUpdates).ToList();

			return lstContent;
		}


		public SiteNav GetLatestVersion(Guid siteID, Guid rootContentID) {
			SiteNav content = (from ct in db.vw_carrot_Contents
							   where ct.SiteID == siteID
								   && ct.Root_ContentID == rootContentID
								   && ct.IsLatestVersion == true
							   select MakeSiteNav(ct)).FirstOrDefault();

			return content;
		}

		public SiteNav GetLatestVersion(Guid siteID, bool? active, string sPage) {
			SiteNav content = (from ct in db.vw_carrot_Contents
							   where ct.SiteID == siteID
								   && (ct.PageActive == active || active == null)
								   && ct.FileName.ToLower() == sPage.ToLower()
								   && ct.IsLatestVersion == true
							   select MakeSiteNav(ct)).FirstOrDefault();

			return content;
		}

		public SiteNav FindHome(Guid siteID) {
			SiteNav content = (from ct in db.vw_carrot_Contents
							   orderby ct.NavOrder ascending
							   where ct.SiteID == siteID
								   && ct.PageActive == true
								   && ct.NavOrder < 1
								   && ct.IsLatestVersion == true
							   select MakeSiteNav(ct)).FirstOrDefault();

			return content;
		}

		public SiteNav FindByFilename(Guid siteID, string urlFileName) {
			SiteNav content = (from ct in db.vw_carrot_Contents
							   where ct.SiteID == siteID
								   && ct.IsLatestVersion == true
								   && ct.FileName.ToLower() == urlFileName.ToLower()
							   select MakeSiteNav(ct)).FirstOrDefault();

			return content;
		}

		public SiteNav FindHome(Guid siteID, bool? active) {
			SiteNav content = (from ct in db.vw_carrot_Contents
							   orderby ct.NavOrder ascending
							   where ct.SiteID == siteID
								   && (ct.PageActive == active || active == null)
								   && ct.NavOrder < 1
								   && ct.IsLatestVersion == true
							   select MakeSiteNav(ct)).FirstOrDefault();

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

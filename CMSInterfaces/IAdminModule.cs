﻿using System;
using System.Collections.Generic;
using System.Linq;
/*
* CarrotCake CMS
* http://www.carrotware.com/
*
* Copyright 2011, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: October 2011
*/

namespace Carrotware.CMS.Interface {

	public interface IAdminModule {

		Guid SiteID { get; set; }

        Guid ModuleID { get; set; }

		string ModuleName { get; set; }

        string QueryStringFragment { get; set; }

        string QueryStringPattern { get; set; }

	}
}

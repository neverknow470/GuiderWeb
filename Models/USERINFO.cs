﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GuiderWeb.Models
{
    public class USERINFO
    {
        public string USERNAME { get; set; }
        public string ID { get; set; }
        public string PASSWORD { get; set; }
        public string BIRTHDAY { get; set; }
        public string GENDER { get; set; }
        public string USERID { get; set; }
        public string PHONE { get; set; }
        public IEnumerable<SelectListItem> LEVEL { get; set; }
        public IEnumerable<SelectListItem> STATUS { get; set; }
        public IEnumerable<SelectListItem> AREAID { get; set; }
    }
}
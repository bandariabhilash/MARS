﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FBCall.Models
{
    public class NewNotesModel
    {
        [AllowHtml]
        public string Text { get; set; }
        public string Value { get; set; }
    }
}
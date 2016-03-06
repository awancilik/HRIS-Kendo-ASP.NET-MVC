using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;

namespace CVScreeningWeb.ViewModels.Shared
{
    public class RadioButtonViewModel
    {
        /// <summary>
        /// Values to provide in the radio button list
        /// </summary>
        public List<RadioButtonData> Data { set; get; }

        /// <summary>
        /// Value returns in the post form
        /// </summary>
        public string SelectedValue { get; set; }

    }


    public class RadioButtonData
    {
        public int Id { set; get; }
        public string Text { set; get; }
        public bool Checked { set; get; }
    }   
}
using System;
using System.Collections.Generic;

namespace CVScreeningWeb.ViewModels.Report
{
    [Serializable]
    public class VerificationViewModel
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
    }

    [Serializable]
    public class TypeOfCheckVerificationViewModel
    {
        public string Name { get; set; }
        public IEnumerable<VerificationViewModel> Verifications { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.UI.WebControls;
using AutoMapper;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.Settings;
using CVScreeningService.Filters;
using CVScreeningService.Services.SystemTime;
using Nalysa.Common.Log;

namespace CVScreeningService.Services.UserManagement
{
    [Logging(Order = 1), ExceptionHandling(Order = 2)]
    public class JobWebSecurity : IWebSecurity
    {
        public JobWebSecurity()
        {

        }

        public string GetCurrentUserName()
        {
            return "job@admin.com";
        }
    }
}
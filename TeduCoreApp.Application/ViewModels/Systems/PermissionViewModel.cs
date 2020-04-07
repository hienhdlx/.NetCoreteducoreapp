using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using TeduCoreApp.Data.Entities;

namespace TeduCoreApp.Application.ViewModels.Systems
{
    public class PermissionViewModel
    {
        public int Id { get; set; }
        public Guid RoleId { get; set; }
        public string FunctionId { get; set; }
        public bool CanCreate { set; get; }
        public bool CanRead { set; get; }
        public bool CanUpdate { set; get; }
        public bool CanDelete { set; get; }
        public AppRoleViewModel AppRole { get; set; }
        public FunctionViewModel Function { get; set; }
    }
}

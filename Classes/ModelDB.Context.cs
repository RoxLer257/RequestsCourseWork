﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RequestsCourse.Classes
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class WebAgencyRequestsEntities : DbContext
    {

        private static WebAgencyRequestsEntities _context;
        public static WebAgencyRequestsEntities GetContext()
        {
            if (_context == null)
                _context = new WebAgencyRequestsEntities();
            return _context;
        }
        public WebAgencyRequestsEntities()
            : base("name=WebAgencyRequestsEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Clients> Clients { get; set; }
        public virtual DbSet<Employees> Employees { get; set; }
        public virtual DbSet<Requests> Requests { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<Statuses> Statuses { get; set; }
    }
}

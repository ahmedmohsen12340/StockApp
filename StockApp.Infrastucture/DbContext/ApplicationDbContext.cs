﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockApp.Core.Domain.Identity_Entities;

namespace Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser,ApplicationRole,Guid>
    {
        public ApplicationDbContext(DbContextOptions options):base(options) 
        {

        }

        //we put virtual so we can override it when using mocking
        public virtual DbSet<BuyOrder> BuyOrders { get; set; }
        public virtual DbSet<SellOrder> SellOrders { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BuyOrder>().ToTable(nameof(BuyOrders));
            modelBuilder.Entity<SellOrder>().ToTable(nameof(SellOrders));
            base.OnModelCreating(modelBuilder);
        }
    }
}

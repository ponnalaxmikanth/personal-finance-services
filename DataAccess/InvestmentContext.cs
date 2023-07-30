using System;
using System.Collections.Generic;
using System.Configuration;
using Entities;
using Entities.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class InvestmentContext : DbContext
    {
        public DbSet<FundsNav> FundsNav { get; set; }

        public InvestmentContext()
        {
        }

        public InvestmentContext(DbContextOptions<InvestmentContext> options) : base(options)
        {

        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseMySQL(Configuration.GetConnectionString("investments"));
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FundsNav>(entity =>
            {
                entity.HasKey(e => e.SchemaCode);
                //entity.Property(e => e.Name).IsRequired();
            });
        }

        //public ServiceResponse<int> SaveFundsNAV(List<NAVData> latestNavData) {
        //    //for (int i = 0; i < latestNavData.Count; i++) {
        //    NAVData.AddRange(latestNavData);
        //    //}
        //    context.SaveChanges();
        //}
    }
}

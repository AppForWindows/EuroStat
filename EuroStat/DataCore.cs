using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EuroStat {
    public class DataContext : DbContext {
        #region Объявление таблиц
        public DbSet<ApiBaseURI> ApiBaseURIes { get; set; }
        public DbSet<CategoryScheme> CategorySchemes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Categorisation> Categorisations { get; set; }
        public DbSet<Dataflow> Dataflows { get; set; }
        #endregion

        public DataContext() : base() {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AssetData");
            if (!Directory.Exists(dbPath))
                Directory.CreateDirectory(dbPath);
            dbPath = Path.Combine(dbPath, "EuroStat.db");
            optionsBuilder
                .UseLazyLoadingProxies()
                .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.DetachedLazyLoadingWarning))
                .UseSqlite("Data Source=" + dbPath);
            SQLitePCL.Batteries.Init();
            //Database.EnsureCreated();
            //SQLitePCL.Batteries_V2.Init();
            //SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_dynamic_cdecl());
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<ApiBaseURI>().HasData( Dictionary.ApiBaseEmpty);

            //modelBuilder.Entity<ApiBaseURI>()
            //    .HasMany(a => a.CategorySchemeList)
            //    .WithOne(c => c.ApiBase)
            //    .HasForeignKey(c => c.ApiBaseID);

            //modelBuilder.Entity<ApiBaseURI>()
            //    .HasMany(a => a.CategoryList)
            //    .WithOne(c => c.ApiBase)
            //    .HasForeignKey(c => c.ApiBaseID);

            //modelBuilder.Entity<ApiBaseURI>()
            //    .HasMany(a => a.CategorisationList)
            //    .WithOne(c => c.ApiBase)
            //    .HasForeignKey(c => c.ApiBaseID);

            //modelBuilder.Entity<ApiBaseURI>()
            //    .HasMany(a => a.DataflowList)
            //    .WithOne(c => c.ApiBase)
            //    .HasForeignKey(c => c.ApiBaseID);

            //modelBuilder.Entity<CategoryScheme>()
            //    .HasMany(a => a.CategoryList)
            //    .WithOne(c => c.CategoryScheme)
            //    .HasForeignKey(c => c.CategorySchemeID);

            base.OnModelCreating(modelBuilder);
        }
    }
}

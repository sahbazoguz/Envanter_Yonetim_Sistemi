using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TKBYSApp.Web.Models;
using TKBYSApp.Web.Models.Enums;

namespace TKBYSApp.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<Personel>(options)
{
    public DbSet<Malzeme> Malzemeler => Set<Malzeme>();
    public DbSet<Depo> Depolar => Set<Depo>();
    public DbSet<Zimmet> Zimmetler => Set<Zimmet>();
    public DbSet<SatinAlmaTalep> SatinAlmaTalepleri => Set<SatinAlmaTalep>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Personel>(entity =>
        {
            entity.Property(e => e.AdSoyad).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SicilNo).HasMaxLength(50).IsRequired();
        });

        builder.Entity<Depo>(entity =>
        {
            entity.Property(e => e.Ad).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Aciklama).HasMaxLength(500);
        });

        builder.Entity<Malzeme>(entity =>
        {
            entity.Property(e => e.Ad).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Ozellikler).HasMaxLength(500);
            entity.Property(e => e.Aciklama).HasMaxLength(500);
            entity.Property(e => e.TKBYSNo).HasMaxLength(100);
            entity.Property(e => e.Durum).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.KayitTarihi).HasColumnType("datetime2");
            entity.HasOne(e => e.Depo)
                  .WithMany(d => d.Malzemeler)
                  .HasForeignKey(e => e.DepoId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.TKBYSNo)
                  .IsUnique()
                  .HasFilter("[TKBYSNo] IS NOT NULL");
        });

        builder.Entity<Zimmet>(entity =>
        {
            entity.Property(e => e.Durum).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.ZimmetTarihi).HasColumnType("datetime2");
            entity.Property(e => e.IadeTarihi).HasColumnType("datetime2");
            entity.HasOne(e => e.Malzeme)
                  .WithMany(m => m.Zimmetler)
                  .HasForeignKey(e => e.MalzemeId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Personel)
                  .WithMany(p => p.Zimmetler)
                  .HasForeignKey(e => e.PersonelId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.OnaylayanYetkili)
                  .WithMany()
                  .HasForeignKey(e => e.OnaylayanYetkiliId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SatinAlmaTalep>(entity =>
        {
            entity.Property(e => e.MalzemeAdi).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Gerekce).HasMaxLength(1000);
            entity.Property(e => e.Durum).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.TalepTarihi).HasColumnType("datetime2");
            entity.Property(e => e.BelgeYolu).HasMaxLength(500);
            entity.HasOne(e => e.TalepEdenPersonel)
                  .WithMany(p => p.SatinAlmaTalepleri)
                  .HasForeignKey(e => e.TalepEdenPersonelId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

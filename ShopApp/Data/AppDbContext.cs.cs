using Microsoft.EntityFrameworkCore;
using QuadroApp.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuadroApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<TypeLijst> TypeLijsten => Set<TypeLijst>();
        public DbSet<AfwerkingsGroep> AfwerkingsGroepen => Set<AfwerkingsGroep>();
        public DbSet<AfwerkingsOptie> AfwerkingsOpties => Set<AfwerkingsOptie>();
        public DbSet<Lijst> Lijsten => Set<Lijst>();
        public DbSet<Offerte> Offertes => Set<Offerte>();
        public DbSet<WerkBon> WerkBonnen => Set<WerkBon>();
        public DbSet<WerkTaak> WerkTaken => Set<WerkTaak>();
        public DbSet<OfferteRegel> OfferteRegels { get; set; } = default!;

        public DbSet<Instelling> Instellingen => Set<Instelling>();
        public DbSet<Leverancier> Leveranciers => Set<Leverancier>();

        public DbSet<Klant> Klanten => Set<Klant>();


        public AppDbContext(DbContextOptions<AppDbContext> opties) : base(opties) { }

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // TypeLijst entity
            b.Entity<TypeLijst>(entity =>
            {
                entity.Property(x => x.Artikelnummer).HasMaxLength(20);
                entity.Property(x => x.LeverancierCode).HasMaxLength(3);
                entity.Property(x => x.PrijsPerMeter).HasPrecision(10, 2);
                entity.Property(x => x.WinstMargeFactor).HasPrecision(6, 3);
                entity.Property(x => x.AfvalPercentage).HasPrecision(5, 2);
                entity.Property(x => x.VasteKost).HasPrecision(10, 2);
                entity.Property(x => x.VoorraadMeter).HasPrecision(10, 2);
                entity.Property(x => x.InventarisKost).HasPrecision(10, 2);
                entity.Property(x => x.MinimumVoorraad).HasPrecision(10, 2);

                entity.HasOne(x => x.Leverancier)
                      .WithMany(l => l.TypeLijsten)
                      .HasForeignKey(x => x.LeverancierId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Lijst entity
            b.Entity<Lijst>(entity =>
            {
                entity.Property(x => x.LengteMeter).HasPrecision(10, 2);
                entity.HasOne(x => x.TypeLijst)
                      .WithMany(t => t.Lijsten)
                      .HasForeignKey(x => x.TypeLijstId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // AfwerkingsGroep entity
            b.Entity<AfwerkingsGroep>(entity =>
            {
                entity.Property(x => x.Code).HasMaxLength(1);
                entity.Property(x => x.Naam).HasMaxLength(50);
            });

            // AfwerkingsOptie entity
            b.Entity<AfwerkingsOptie>(entity =>
            {
                entity.Property(x => x.KostprijsPerM2).HasPrecision(10, 2);
                entity.Property(x => x.WinstMarge).HasPrecision(6, 3);
                entity.Property(x => x.AfvalPercentage).HasPrecision(5, 2);
                entity.Property(x => x.VasteKost).HasPrecision(10, 2);
                entity.HasIndex(x => new { x.AfwerkingsGroepId, x.Volgnummer }).IsUnique();
                entity.HasOne(x => x.AfwerkingsGroep)
                      .WithMany(g => g.Opties)
                      .HasForeignKey(x => x.AfwerkingsGroepId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Leverancier)
                      .WithMany(l => l.AfwerkingsOpties)
                      .HasForeignKey(x => x.LeverancierId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Offerte entity
            b.Entity<Offerte>(entity =>
            {

                entity.Property(x => x.SubtotaalExBtw).HasPrecision(10, 2);
                entity.Property(x => x.BtwBedrag).HasPrecision(10, 2);
                entity.Property(x => x.TotaalInclBtw).HasPrecision(10, 2);

                entity.HasOne(o => o.Klant)
                      .WithMany(k => k.Offertes)
                      .HasForeignKey(o => o.KlantId)
                      .OnDelete(DeleteBehavior.SetNull);


            });
            // OfferteRegel entity
            b.Entity<OfferteRegel>(entity =>
            {
                entity.Property(x => x.BreedteCm).HasPrecision(18, 2);
                entity.Property(x => x.HoogteCm).HasPrecision(18, 2);
                entity.Property(x => x.ExtraPrijs).HasPrecision(18, 2);
                entity.Property(x => x.Korting).HasPrecision(18, 2);
                entity.Property(x => x.SubtotaalExBtw).HasPrecision(18, 2);
                entity.Property(x => x.BtwBedrag).HasPrecision(18, 2);
                entity.Property(x => x.TotaalInclBtw).HasPrecision(18, 2);

                // Parent
                entity.HasOne(r => r.Offerte)
                      .WithMany(o => o.Regels)
                      .HasForeignKey(r => r.OfferteId)
                      .OnDelete(DeleteBehavior.Cascade);

                // TypeLijst (optioneel, geen cascade)
                entity.HasOne(r => r.TypeLijst)
                      .WithMany()
                      .HasForeignKey(r => r.TypeLijstId)
                      .OnDelete(DeleteBehavior.NoAction);

                // 6× AfwerkingsOptie (allemaal NO ACTION om multiple cascade paths te vermijden)
                entity.HasOne(r => r.Glas)
                      .WithMany()
                      .HasForeignKey(r => r.GlasId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(r => r.PassePartout1)
                      .WithMany()
                      .HasForeignKey(r => r.PassePartout1Id)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(r => r.PassePartout2)
                      .WithMany()
                      .HasForeignKey(r => r.PassePartout2Id)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(r => r.DiepteKern)
                      .WithMany()
                      .HasForeignKey(r => r.DiepteKernId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(r => r.Opkleven)
                      .WithMany()
                      .HasForeignKey(r => r.OpklevenId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(r => r.Rug)
                      .WithMany()
                      .HasForeignKey(r => r.RugId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // ---- WerkTaak constraints
            b.Entity<WerkTaak>(et =>
            {
                // Tot moet ná Van liggen
                et.ToTable(t => t.HasCheckConstraint(
                    "CK_WerkTaak_Tot_After_Van", "[GeplandTot] > [GeplandVan]"));

                // Duur positief (minstens 1 minuut)
                et.ToTable(t => t.HasCheckConstraint(
                    "CK_WerkTaak_Duur_Positive", "[DuurMinuten] >= 1"));
            });

            // ---- WerkBon audit
            b.Entity<WerkBon>()
             .Property(x => x.BijgewerktOp)
             .ValueGeneratedOnAddOrUpdate();

            // Instelling entity
            b.Entity<Instelling>(entity =>
            {
                entity.HasKey(x => x.Sleutel);
                entity.Property(x => x.Sleutel).HasMaxLength(100);
                entity.Property(x => x.Waarde).HasMaxLength(255);
            });
        }
        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            foreach (var e in ChangeTracker.Entries<WerkTaak>())
            {
                if (e.State is EntityState.Added or EntityState.Modified)
                {
                    // Houd model consistent
                    var start = e.Entity.GeplandVan;
                    e.Entity.GeplandTot = start.AddMinutes(Math.Max(1, e.Entity.DuurMinuten));
                }
            }

            foreach (var e in ChangeTracker.Entries<WerkBon>())
            {
                if (e.State == EntityState.Modified)
                    e.Entity.BijgewerktOp = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(ct);
        }


    }
}

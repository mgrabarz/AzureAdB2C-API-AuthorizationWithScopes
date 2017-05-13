using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using EternalSolutions.Samples.B2C.Common.Contracts;
using Microsoft.EntityFrameworkCore;

namespace EternalSolutions.Samples.B2C.Api.NotesService.Notes
{
    public class NotesContext : DbContext
    {
        public NotesContext(DbContextOptions<NotesContext> options)
            : base(options)
        { }

        public DbSet<Note> Notes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Note>()
                .Property(note => note.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();
            builder.Entity<Note>()
                .HasKey(note => note.Id);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            SaveChangesInternal();

            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            SaveChangesInternal();

            return base.SaveChanges();
        }

        private void SaveChangesInternal()
        {
            this.ChangeTracker.DetectChanges();

            var notes = this.ChangeTracker.Entries<Note>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var note in notes)
            {
                note.Property(nameof(Note.CreatedAt)).CurrentValue = DateTime.UtcNow;
                note.Property(nameof(Note.CreatedBy)).CurrentValue = ClaimsPrincipal.Current?.Claims.FirstOrDefault(claim => claim.Type == "Name")?.Value;
            }
        }
    }
}

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

        public async Task<int> SaveChangesAsync(ClaimsPrincipal principal)
        {
            SaveChangesInternal(principal);

            return await base.SaveChangesAsync();
        }

        private void SaveChangesInternal(ClaimsPrincipal principal)
        {
            this.ChangeTracker.DetectChanges();

            var notes = this.ChangeTracker.Entries<Note>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var note in notes)
            {
                note.Property(nameof(Note.CreatedAt)).CurrentValue = DateTime.UtcNow;
                note.Property(nameof(Note.CreatedBy)).CurrentValue = principal?.Claims.FirstOrDefault(claim => claim.Type == "name")?.Value;
            }
        }
    }
}

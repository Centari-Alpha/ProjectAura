namespace Aura.Core.Data;

using Aura.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Collections.Generic;
using System;

public class AuraDbContext : DbContext
{
    public DbSet<ThoughtNode> ThoughtNodes { get; set; }
    public DbSet<Relationship> Relationships { get; set; }
    public DbSet<Constellation> Constellations { get; set; }

    // If you ever want to inject options from ASP.NET or an app builder, you'd add a constructor.
    // For now we will just hardcode the SQLite connection directly in OnConfiguring.

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // This will create a file named "aura.db" wherever the application is executed
        optionsBuilder.UseSqlite("Data Source=aura.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Configure the Self-Referencing Many-to-Many Connections
        modelBuilder.Entity<Relationship>()
            .HasKey(r => new { r.SourceNodeId, r.TargetNodeId }); // Composite Key prevents duplicate exact connections

        modelBuilder.Entity<Relationship>()
            .HasOne(r => r.SourceNode)
            .WithMany(n => n.Connections)
            .HasForeignKey(r => r.SourceNodeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Relationship>()
            .HasOne(r => r.TargetNode)
            .WithMany() // Target nodes don't necessarily have a backward list unless we want bidirectionality
            .HasForeignKey(r => r.TargetNodeId)
            .OnDelete(DeleteBehavior.Restrict);

        // 2. Map AuraVector3 flat into columns (e.g. DailyHomePosition_X, DailyHomePosition_Y)
        modelBuilder.Entity<ThoughtNode>().OwnsOne(t => t.DailyHomePosition);
        modelBuilder.Entity<Constellation>().OwnsOne(c => c.CenterOfMass);

        // 3. Map lists to JSON columns to keep the database flat and simple for now!
        // We do this instead of creating a huge web of extra tables for tags and IDs.
        modelBuilder.Entity<ThoughtNode>()
            .Property(t => t.Attachments)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<MediaTag>>(v, (JsonSerializerOptions)null));

        modelBuilder.Entity<Constellation>()
            .Property(c => c.NodeIds)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions)null));
    }
}

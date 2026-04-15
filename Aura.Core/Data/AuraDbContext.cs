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

    // For now we will just hardcode the SQLite connection directly in OnConfiguring.

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // This will create a file named "aura.db" wherever the application is executed
        optionsBuilder.UseSqlite("Data Source=aura.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Relationship>()
            .HasKey(r => new { r.SourceNodeId, r.TargetNodeId });

        modelBuilder.Entity<Relationship>()
            .HasOne(r => r.SourceNode)
            .WithMany(n => n.Connections)
            .HasForeignKey(r => r.SourceNodeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Relationship>()
            .HasOne(r => r.TargetNode)
            .WithMany()
            .HasForeignKey(r => r.TargetNodeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ThoughtNode>().OwnsOne(t => t.DailyHomePosition);
        modelBuilder.Entity<Constellation>().OwnsOne(c => c.CenterOfMass);

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

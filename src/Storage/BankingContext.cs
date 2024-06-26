﻿using System.Text.Json;

using Banking.Abstractions.Entities;

using Microsoft.EntityFrameworkCore;

namespace Banking.Storage;

public class BankingContext : DbContext
{
    public BankingContext(DbContextOptions<BankingContext> options)
        : base(options)
    {

    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Source> Sources => Set<Source>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(b =>
        {
            b.Property(a => a.ExtraInfo).IsNullableJson();
        });

        modelBuilder.Entity<Transaction>(b =>
        {
            b.HasIndex(t => t.TransactionId).IsUnique(false);

            b.Property(t => t.ExtraInfo).IsNullableJson();

            b.Property(t => t.Tags)
                .HasConversion(
                    d => JsonSerializer.Serialize(d, (JsonSerializerOptions?)null),
                    s => s != null ? JsonSerializer.Deserialize<string[]>(s, (JsonSerializerOptions?)null) : null
                );
        });

        modelBuilder.Entity<ApiKey>(b =>
        {
            b.Property(a => a.Scopes)
                .HasConversion(
                    d => JsonSerializer.Serialize(d, (JsonSerializerOptions?)null),
                    s => s != null ? JsonSerializer.Deserialize<string[]>(s, (JsonSerializerOptions?)null)! : Array.Empty<string>()
                );
        });

        modelBuilder.Entity<Source>(b =>
        {
            b.Property(s => s.Config).IsJson();
        });

        base.OnModelCreating(modelBuilder);
    }
}

using System;
using System.Collections.Generic;
using LTEEquipmentManager.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LTEEquipmentManager.Data;

public partial class LTEEquipmentDbContext : DbContext
{
    public LTEEquipmentDbContext(DbContextOptions<LTEEquipmentDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AntennaType> AntennaType { get; set; }

    public virtual DbSet<AntennaTypePort> AntennaTypePort { get; set; }

    public virtual DbSet<AntennaTypePortStandart> AntennaTypePortStandart { get; set; }

    public virtual DbSet<PlacementType> PlacementType { get; set; }

    public virtual DbSet<PortType> PortType { get; set; }

    public virtual DbSet<RruType> RruType { get; set; }

    public virtual DbSet<RruTypePort> RruTypePort { get; set; }

    public virtual DbSet<RruTypePortStandart> RruTypePortStandart { get; set; }

    public virtual DbSet<Standart> Standart { get; set; }

    public virtual DbSet<Vendor> Vendor { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AntennaType>(entity =>
        {
            entity.HasKey(e => e.Id).HasFillFactor(90);

            entity.HasIndex(e => e.AntennaName, "IX_AntennaType")
                .IsUnique()
                .HasFillFactor(90);

            entity.Property(e => e.AntennaFamily)
                .HasMaxLength(10)
                .HasColumnName("Antenna_Family");
            entity.Property(e => e.AntennaName)
                .HasMaxLength(100)
                .HasColumnName("Antenna_Name");
            entity.Property(e => e.Manufacturer).HasMaxLength(10);
            entity.Property(e => e.Polarization).HasMaxLength(10);
        });

        modelBuilder.Entity<AntennaTypePort>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("trg_AntennaTypePort_SyncTilt"));

            entity.HasIndex(e => new { e.AntennaTypeId, e.PortName }, "UQ_AntennaTypePort_Name").IsUnique();

            entity.HasIndex(e => new { e.AntennaTypeId, e.PortNumber }, "UQ_AntennaTypePort_Number").IsUnique();

            entity.Property(e => e.MaxTilt).HasColumnType("decimal(4, 1)");
            entity.Property(e => e.MinTilt).HasColumnType("decimal(4, 1)");
            entity.Property(e => e.PortName).HasMaxLength(30);
            entity.Property(e => e.TiltStep).HasColumnType("decimal(3, 1)");

            entity.HasOne(d => d.AntennaType).WithMany(p => p.AntennaTypePort)
                .HasForeignKey(d => d.AntennaTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AntennaTypePort_AntennaType");

            entity.HasOne(d => d.PortType).WithMany(p => p.AntennaTypePort)
                .HasForeignKey(d => d.PortTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AntennaTypePort_PortType");
        });

        modelBuilder.Entity<AntennaTypePortStandart>(entity =>
        {
            entity.HasIndex(e => new { e.AntennaTypePortId, e.StandartId }, "UQ_AntennaTypePortStandart").IsUnique();

            entity.Property(e => e.Gain).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Hhpbw)
                .HasColumnType("decimal(4, 1)")
                .HasColumnName("HHPBW");
            entity.Property(e => e.Vhpbw)
                .HasColumnType("decimal(4, 1)")
                .HasColumnName("VHPBW");

            entity.HasOne(d => d.AntennaTypePort).WithMany(p => p.AntennaTypePortStandart)
                .HasForeignKey(d => d.AntennaTypePortId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AntennaTypePortStandart_AntennaTypePort");

            entity.HasOne(d => d.Standart).WithMany(p => p.AntennaTypePortStandart)
                .HasForeignKey(d => d.StandartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AntennaTypePortStandart_Standart");
        });

        modelBuilder.Entity<PlacementType>(entity =>
        {
            entity.HasKey(e => e.Id).HasFillFactor(90);

            entity.Property(e => e.Name).HasMaxLength(30);
        });

        modelBuilder.Entity<PortType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PortType__3214EC0753F35200");

            entity.HasIndex(e => e.Name, "UQ__PortType__737584F668AB15AD").IsUnique();

            entity.Property(e => e.Category)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Channels).HasDefaultValue((byte)1);
            entity.Property(e => e.Connector).HasMaxLength(50);
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<RruType>(entity =>
        {
            entity.HasKey(e => e.Id).HasFillFactor(90);

          

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Comment).HasMaxLength(250);
            entity.Property(e => e.CuCount).HasColumnName("CU_Count");
            entity.Property(e => e.IdPlacementType).HasColumnName("Id_PlacementType");
            entity.Property(e => e.IdVendor).HasColumnName("Id_Vendor");
            entity.Property(e => e.MaxPowerWatt).HasColumnName("Max_Power_Watt");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.TrxCount).HasColumnName("TRx_Count");
            entity.Property(e => e.Type).HasMaxLength(50);
        });

        modelBuilder.Entity<RruTypePort>(entity =>
        {
            entity.HasIndex(e => new { e.RruTypeId, e.PortName }, "UQ_RruTypePort_Name").IsUnique();

            entity.HasIndex(e => new { e.RruTypeId, e.PortNumber }, "UQ_RruTypePort_Number").IsUnique();

            entity.Property(e => e.PortName).HasMaxLength(30);

            entity.HasOne(d => d.PortType).WithMany(p => p.RruTypePort)
                .HasForeignKey(d => d.PortTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RruTypePort_PortType");

            entity.HasOne(d => d.RruType).WithMany(p => p.RruTypePort)
                .HasForeignKey(d => d.RruTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RruTypePort_RruType");
        });

        modelBuilder.Entity<RruTypePortStandart>(entity =>
        {
            entity.HasIndex(e => new { e.RruTypePortId, e.StandartId }, "UQ_RruTypePortStandart").IsUnique();

            entity.HasOne(d => d.RruTypePort).WithMany(p => p.RruTypePortStandart)
                .HasForeignKey(d => d.RruTypePortId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RruTypePortStandart_RruTypePort");

            entity.HasOne(d => d.Standart).WithMany(p => p.RruTypePortStandart)
                .HasForeignKey(d => d.StandartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RruTypePortStandart_Standart");
        });

        modelBuilder.Entity<Standart>(entity =>
        {
            entity.HasKey(e => e.Id).HasFillFactor(90);

            entity.Property(e => e.StandartName).HasMaxLength(15);
            entity.Property(e => e.StandartName2)
                .HasMaxLength(10)
                .IsFixedLength();
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.HasKey(e => e.Id).HasFillFactor(90);

            entity.Property(e => e.VendorName)
                .HasMaxLength(30)
                .HasColumnName("Vendor_Name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

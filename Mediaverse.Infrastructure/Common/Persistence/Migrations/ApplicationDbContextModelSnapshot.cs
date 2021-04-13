﻿// <auto-generated />
using System;
using Mediaverse.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Mediaverse.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.3")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Mediaverse.Domain.Authentication.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("LastActive")
                        .HasColumnType("datetime2");

                    b.Property<string>("Nickname")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Mediaverse.Infrastructure.JointContentConsumption.Repositories.Dtos.RoomDto", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ActivePlaylistId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("Host")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("MaxViewersQuantity")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Rooms");
                });

            modelBuilder.Entity("Mediaverse.Infrastructure.JointContentConsumption.Repositories.Dtos.ViewerDto", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("RoomDtoId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("RoomDtoId");

                    b.ToTable("ViewerDto");
                });

            modelBuilder.Entity("Mediaverse.Infrastructure.JointContentConsumption.Repositories.Dtos.ViewerDto", b =>
                {
                    b.HasOne("Mediaverse.Infrastructure.JointContentConsumption.Repositories.Dtos.RoomDto", null)
                        .WithMany("Viewers")
                        .HasForeignKey("RoomDtoId");
                });

            modelBuilder.Entity("Mediaverse.Infrastructure.JointContentConsumption.Repositories.Dtos.RoomDto", b =>
                {
                    b.Navigation("Viewers");
                });
#pragma warning restore 612, 618
        }
    }
}

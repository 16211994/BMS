using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BuildingManagment.Models;

public partial class City
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    [InverseProperty("City")]
    public virtual ICollection<Building> Buildings { get; set; } = new List<Building>();

    [InverseProperty("City")]
    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();

    [InverseProperty("City")]
    public virtual ICollection<ShopLocation> ShopLocations { get; set; } = new List<ShopLocation>();
}

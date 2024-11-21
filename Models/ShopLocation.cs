using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BuildingManagment.Models;

public partial class ShopLocation
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    public int? LocationId { get; set; }

    public int CityId { get; set; }

    public int BuildingId { get; set; }

    public int RoomId { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("BuildingId")]
    [InverseProperty("ShopLocations")]
    public virtual Building Building { get; set; }

    [ForeignKey("CityId")]
    [InverseProperty("ShopLocations")]
    public virtual City City { get; set; }

    [ForeignKey("LocationId")]
    [InverseProperty("ShopLocations")]
    public virtual Location Location { get; set; }

    [ForeignKey("RoomId")]
    [InverseProperty("ShopLocations")]
    public virtual Room Room { get; set; }
}

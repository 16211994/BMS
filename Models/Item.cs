using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BuildingManagment.Models;

public partial class Item
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    public int ItemCategoryId { get; set; }

    public bool IsAvailable { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("ItemCategoryId")]
    [InverseProperty("Items")]
    public virtual ItemCategory ItemCategory { get; set; }

    [InverseProperty("Item")]
    public virtual ICollection<ItemImage> ItemImages { get; set; } = new List<ItemImage>();

    [InverseProperty("Item")]
    public virtual ICollection<ItemPrice> ItemPrices { get; set; } = new List<ItemPrice>();

    [InverseProperty("Item")]
    public virtual ICollection<Shop> Shops { get; set; } = new List<Shop>();
}

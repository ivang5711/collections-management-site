﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Collections.Models;

public class Item
{
    public int Id { get; set; }

    [Column(TypeName = "varchar(100)")]
    public string Name { get; set; } = string.Empty;

    public List<Tag> Tags { get; } = [];

    public List<NumericalField> NumericalFields { get; } = [];

    public List<StringField> StringFields { get; } = [];

    public List<TextField> TextFields { get; } = [];

    public List<LogicalField> LogicalFields { get; } = [];

    public List<DateField> DateFields { get; } = [];

    public Collection Collection { get; set; }

    public int CollectionId { get; set; }

    public List<Like> Likes { get; set; } = [];

    public List<Comment> Comments { get; set; } = [];

    [Column(TypeName = "varchar(200)")]

    public string? ImageLink { get; set; } = null;

    public DateTime CreationDateTime { get; set; }
}
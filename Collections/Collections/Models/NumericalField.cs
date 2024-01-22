﻿namespace Collections.Models
{
    public class NumericalField : IFieldType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public List<Item> Items { get; set; } = [];
    }
}
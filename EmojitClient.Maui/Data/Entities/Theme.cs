using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmojitClient.Maui.Data.Entities;

/// <summary>
/// Represents a game theme containing related symbols.
/// </summary>
public class Theme
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
}
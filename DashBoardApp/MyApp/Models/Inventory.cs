using System;
using System.Collections.Generic;
using System.Text;

namespace DashBoardApp.Models;

public class HardwareAsset
{
    public string AssetId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public Employee AssignedTo { get; set; }
}


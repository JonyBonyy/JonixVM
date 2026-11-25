using System;

namespace Client.Models
{ 
    public class VmConfig
    {
    
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string IsoOrImage { get; set; } = string.Empty;
        public int RamMb { get; set; } = 2048;
        public int Cores { get; set; } = 2;
        public int DiskGb { get; set; } = 20;
        public string NetworkMode { get; set; } = "NAT";
        public bool EnableHardwareAccel { get; set; } = true;
        public bool IsCloud { get; set; }
        public string? LocalDiskPath { get; set; }
    }
}
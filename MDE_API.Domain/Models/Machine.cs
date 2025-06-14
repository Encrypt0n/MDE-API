﻿namespace MDE_API.Domain.Models
{
    public class Machine
    {
        public int MachineID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string IP { get; set; }

        public string DashboardUrl { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDE_API.Application.Interfaces
{
    public interface IVPNRepository
    {
        int SaveClientConnection(string clientName, string description, int userId, string assignedIp, List<string> uibuilderUrls);
    }

}

using System;
using System.Collections.Generic;
using System.Text;
using LTEEquipmentManager.Data;
using Microsoft.EntityFrameworkCore;

namespace LTEEquipmentManager.Services
{
    public class LTEEquipmentDbContextFactory
    {
        private readonly DbContextOptions<LTEEquipmentDbContext> _options;

        public LTEEquipmentDbContextFactory(string connectionString)
        {
            _options = new DbContextOptionsBuilder<LTEEquipmentDbContext>().UseSqlServer(connectionString).Options;
        }

        public LTEEquipmentDbContext CreateDbContext()
        {
            return new LTEEquipmentDbContext(_options);
        }
    }
}

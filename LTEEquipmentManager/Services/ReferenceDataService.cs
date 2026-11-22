using LTEEquipmentManager.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LTEEquipmentManager.Services
{
    //Сервисы для выгрузки справочников для DataGridComboBoxColumn (параметров портов, стандартов)
    public class ReferenceDataService
    {
        private readonly LTEEquipmentDbContextFactory _dbContextFactory;

        public ReferenceDataService(LTEEquipmentDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<PortType>> GetPortTypesAsync()
        {
            await using var db = _dbContextFactory.CreateDbContext();

            return await db.PortType
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<List<Standart>> GetStandartsAsync()
        {
            await using var db = _dbContextFactory.CreateDbContext();

            return await db.Standart
                .AsNoTracking()
                .OrderBy(s => s.Id)
                .ToListAsync();
        }
    }
}

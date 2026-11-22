using LTEEquipmentManager.Data;
using LTEEquipmentManager.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.Collections.ObjectModel;


namespace LTEEquipmentManager.Services
{
   
    public class RruTypeService
    {
        // сохраняем фабрику, через которую будем работать с базой
        private readonly LTEEquipmentDbContextFactory _dbContextFactory;
        // тут храним порты выбранной RRU
        private readonly ObservableCollection<RruTypePort> _rruPorts = new();

        // получаем фабрику при создании нашего сервиса


        public async Task UpdateRruPortStandartAsync(RruTypePortStandart standard)
        {
            await using var db = _dbContextFactory.CreateDbContext();

            // Ищем существующую запись по её ID
            var existingStandard = await db.RruTypePortStandart
                .FirstOrDefaultAsync(s => s.Id == standard.Id);

            if (existingStandard == null)
            {
                throw new Exception("Стандарт порта не найден в базе данных!");
            }

            // Меняем только стандарт, связь с портом сохраняем
            existingStandard.StandartId = standard.StandartId;

            await db.SaveChangesAsync();
        }

        public async Task DeleteRruPortStandartAsync(int id)
        {
            // Создаём контекст базы данных
            await using var db = _dbContextFactory.CreateDbContext();

            // Находим запись по ID
            var portStandart = await db.RruTypePortStandart
                .FirstOrDefaultAsync(s => s.Id == id);

            // Проверяем, существует ли запись
            if (portStandart == null)
            {
                throw new Exception("Стандарт порта не найден в базе данных!");
            }

            // Удаляем связь порта со стандартом
            db.RruTypePortStandart.Remove(portStandart);

            // Сохраняем удаление в SQL Server
            await db.SaveChangesAsync();
        }

        public async Task<List<Standart>> GetStandartsAsync()
        {
            // Создаём контекст базы данных
            await using var db = _dbContextFactory.CreateDbContext();

            // Загружаем справочник стандартов
            return await db.Standart
                .AsNoTracking()
                .OrderBy(s => s.StandartName)
                .ToListAsync();
        }


        public async Task<List<RruTypePortStandart>> GetRruPortStandartsAsync(int portId)
        {
            // Создаём контекст базы данных
            await using var db = _dbContextFactory.CreateDbContext();

            // Получаем стандарты только выбранного порта
            return await db.RruTypePortStandart
                .AsNoTracking()
                .Where(s => s.RruTypePortId == portId)
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        public RruTypeService(LTEEquipmentDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        // этот метод получает список всех RRU из базы данных
        public async Task<List<RruType>> GetRruTypesAsync()
        {
            // создаём контекст для одного запроса к базе
            await using var db = _dbContextFactory.CreateDbContext();

            // получаем список RRU и сортируем его по названию
            return await db.RruType
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        // получаем порты только той RRU, которую выбрали в первой таблице
        public async Task<List<RruTypePort>> GetRruPortsAsync(byte rruTypeId)
        {
            // создаём контекст для запроса к базе
            await using var db = _dbContextFactory.CreateDbContext();

            // ищем порты по номеру выбранной RRU
            return await db.RruTypePort
                .AsNoTracking()
                .Where(p => p.RruTypeId == rruTypeId)
                .OrderBy(p => p.PortNumber)
                .ToListAsync();
        }

        public async Task DeleteRruPortAsync(int portId)
        {
            // Создаём контекст базы данных
            await using var db = _dbContextFactory.CreateDbContext();

            // Ищем порт по его ID
            var port = await db.RruTypePort
                .FirstOrDefaultAsync(p => p.Id == portId);

            // Если порт не найден, сообщаем об ошибке
            if (port == null)
            {
                throw new Exception("Порт не найден в базе данных!");
            }

            // Помечаем порт для удаления
            db.RruTypePort.Remove(port);

            // Выполняем DELETE в SQL Server
            await db.SaveChangesAsync();
        }
        // этот метод сохраняет новый порт RRU в базу данных
        public async Task AddRruPortAsync(RruTypePort port)
        {
            // создаём контекст для работы с базой
            await using var db = _dbContextFactory.CreateDbContext();

            // говорим базе, что хотим добавить новый порт
            db.RruTypePort.Add(port);

            // сохраняем изменения в SQL Server
            await db.SaveChangesAsync();
        }


        public async Task UpdateRruPortAsync(RruTypePort port)
        {
            // Создаём контекст для работы с базой
            await using var db = _dbContextFactory.CreateDbContext();

            // Находим существующий порт по его ID
            var existingPort = await db.RruTypePort
                .FirstOrDefaultAsync(p => p.Id == port.Id);

            // Если порт не найден
            if (existingPort == null)
            {
                throw new Exception("Порт не найден в базе данных!");
            }

            // Номер порта
            existingPort.PortNumber = port.PortNumber;

            // Название порта
            existingPort.PortName = port.PortName;

            // Тип порта
            existingPort.PortTypeId = port.PortTypeId;

            // Возможность передачи
            existingPort.Tx = port.Tx;

            // Возможность приёма
            existingPort.Rx = port.Rx;

            // Максимальная мощность
            existingPort.MaxPowerWatt = port.MaxPowerWatt;

            // Сохраняем изменения в SQL Server
            await db.SaveChangesAsync();
        }

        public async Task AddRruPortStandartAsync(RruTypePortStandart portStandart)
        {
            // Создаём подключение к базе данных
            await using var db = _dbContextFactory.CreateDbContext();

            // Проверяем, существует ли уже такой стандарт у этого порта
            bool alreadyExists = await db.RruTypePortStandart
                .AnyAsync(s =>
                    s.RruTypePortId == portStandart.RruTypePortId &&
                    s.StandartId == portStandart.StandartId);

            // Запрещаем добавлять одинаковый стандарт дважды
            if (alreadyExists)
            {
                throw new Exception(
                    "Этот стандарт уже добавлен к выбранному порту!");
            }

            // Добавляем связь между портом и стандартом
            db.RruTypePortStandart.Add(portStandart);

            // Сохраняем новую запись в SQL Server
            await db.SaveChangesAsync();
        }


    }
}
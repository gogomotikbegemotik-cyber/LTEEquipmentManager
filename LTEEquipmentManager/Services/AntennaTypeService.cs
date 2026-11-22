using LTEEquipmentManager.Data.Entities;
using LTEEquipmentManager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LTEEquipmentManager.Services
{
    public class AntennaTypeService
    {
        private readonly LTEEquipmentDbContextFactory _dbContextFactory;

        public AntennaTypeService(LTEEquipmentDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        //выгрузка списка антенн из БД
        public async Task<List<AntennaType>> GetAntennasAsync()
        {
            await using var db = _dbContextFactory.CreateDbContext();

            // Получаем список антенн
            List<AntennaType> antennas = await db.AntennaType.AsNoTracking().OrderBy(a => a.AntennaName).ToListAsync();

            // Возвращаем список антенн
            return antennas;

        }

        //выгрузка списка антенных портов из БД
        public async Task<List<AntennaTypePort>> GetAntennaPortsAsync(short antennaTypeId)
        {
            await using var db = _dbContextFactory.CreateDbContext();

            // Получаем список портов
            List<AntennaTypePort> antennaPorts = await db.AntennaTypePort.AsNoTracking().Where(p => p.AntennaTypeId == antennaTypeId).OrderBy(p => p.PortNumber).ToListAsync();

            // Возвращаем список портов
            return antennaPorts;
        }

        //выгрузка списка поддерживаемых портами стандартов из БД
        public async Task<List<AntennaTypePortStandart>> GetAntennaPortStandartsAsync(int antennaTypePortId)
        {
            await using var db = _dbContextFactory.CreateDbContext();

            // Получаем список поддерживаемых Стандартов
            List<AntennaTypePortStandart> antennaPortStandarts = await db.AntennaTypePortStandart.AsNoTracking().Where(p => p.AntennaTypePortId == antennaTypePortId).OrderBy(p => p.StandartId).ToListAsync();

            // Возвращаем список поддерживаемых Стандартов
            return antennaPortStandarts;
        }
        //добавление антенны
        public async Task AddAntennaAsync(AntennaType antenna)
        {
            await using var db = _dbContextFactory.CreateDbContext();

            db.AntennaType.Add(antenna);

            await db.SaveChangesAsync();
        }

        //добавление порта
        public async Task AddAntennaPortAsync(AntennaTypePort antennaPort)
        {
            await using var db = _dbContextFactory.CreateDbContext();

            db.AntennaTypePort.Add(antennaPort);

            await db.SaveChangesAsync();
        }

        //добавление стандарта
        public async Task AddAntennaPortStandartAsync(AntennaTypePortStandart antennaPortStandart)
        {
            await using var db = _dbContextFactory.CreateDbContext();

            db.AntennaTypePortStandart.Add(antennaPortStandart);

            await db.SaveChangesAsync();
        }

        //кол-во антенн
        public async Task<int> GetAntennaCountAsync()
        {
            
            await using var db = _dbContextFactory.CreateDbContext();

            // Получаем количество антенн
            int count = await db.AntennaType.CountAsync();

            // Возвращаем количество антенн
            return count;
        }

        //обновление антенны
        public async Task UpdateAntennaAsync(AntennaType antenna)
        {
            await using var db = _dbContextFactory.CreateDbContext();

            AntennaType? dbAntenna = await db.AntennaType.FindAsync(antenna.Id);

            if (dbAntenna == null)
            {
                throw new InvalidOperationException("Антенна не найдена в базе данных.");
            }

            dbAntenna.AntennaName = antenna.AntennaName;
            dbAntenna.Manufacturer = antenna.Manufacturer;
            dbAntenna.Polarization = antenna.Polarization;
            dbAntenna.AntennaFamily = antenna.AntennaFamily;
            dbAntenna.Height = antenna.Height;

            await db.SaveChangesAsync();
        }
        //обновление порта
        public async Task UpdateAntennaPortAsync(AntennaTypePort antennaPort)
        {
            await using var db = _dbContextFactory.CreateDbContext();

            AntennaTypePort? dbAntennaPort = await db.AntennaTypePort.FindAsync(antennaPort.Id);

            if (dbAntennaPort == null)
            {
                throw new InvalidOperationException("Порт не найден в базе данных.");
            }

            dbAntennaPort.PortNumber = antennaPort.PortNumber;
            dbAntennaPort.PortName = antennaPort.PortName;
            dbAntennaPort.PortTypeId = antennaPort.PortTypeId;
            dbAntennaPort.TiltGroup = antennaPort.TiltGroup;
            dbAntennaPort.MinTilt = antennaPort.MinTilt;
            dbAntennaPort.MaxTilt = antennaPort.MaxTilt;
            dbAntennaPort.TiltStep = antennaPort.TiltStep;
            dbAntennaPort.Polarization = antennaPort.Polarization;
            dbAntennaPort.AzimuthOffsetDeg = antennaPort.AzimuthOffsetDeg;
            dbAntennaPort.Combiner = antennaPort.Combiner;

            await db.SaveChangesAsync();
        }

        //обновление стандарта
        public async Task UpdateAntennaPortStandartAsync(AntennaTypePortStandart antennaPortStandart)
        {
            await using var db = _dbContextFactory.CreateDbContext();

            AntennaTypePortStandart? dbAntennaPortStandart = await db.AntennaTypePortStandart.FindAsync(antennaPortStandart.Id);

            if (dbAntennaPortStandart == null)
            {
                throw new InvalidOperationException("Стандарт не найден в базе данных.");
            }

            dbAntennaPortStandart.StandartId = antennaPortStandart.StandartId;
            dbAntennaPortStandart.Gain = antennaPortStandart.Gain;
            dbAntennaPortStandart.Vhpbw = antennaPortStandart.Vhpbw;
            dbAntennaPortStandart.Hhpbw = antennaPortStandart.Hhpbw;
            
            await db.SaveChangesAsync();
        }

        // удаление антенны
        public async Task DeleteAntennaAsync(short antennaTypeId)
        {
            await using var db = _dbContextFactory.CreateDbContext();

            AntennaType? antenna = await db.AntennaType.FindAsync(antennaTypeId);

            if (antenna == null)
            {
                throw new InvalidOperationException("Антенна не найдена в базе данных.");
            }

            bool hasPorts = await db.AntennaTypePort.AnyAsync(p => p.AntennaTypeId == antennaTypeId);

            if (hasPorts)
            {
                throw new InvalidOperationException("Невозможно удалить антенну, поскольку у антенны имеются порты.");
            }

            db.AntennaType.Remove(antenna);

            await db.SaveChangesAsync();
        }
        //удаление порта
        public async Task DeleteAntennaPortAsync(int antennaTypePortId)
        {
            await using var db = _dbContextFactory.CreateDbContext();

            AntennaTypePort? dbAntennaPort = await db.AntennaTypePort.FindAsync(antennaTypePortId);

            if (dbAntennaPort == null)
            {
                throw new InvalidOperationException("Порт не найден в базе данных.");
            }

            bool hasStandarts = await db.AntennaTypePortStandart.AnyAsync(p => p.AntennaTypePortId == antennaTypePortId);

            if (hasStandarts)
            {
                throw new InvalidOperationException("Невозможно удалить порт, поскольку у порта имеются стандарты.");
            }

            db.AntennaTypePort.Remove(dbAntennaPort);

            await db.SaveChangesAsync();
        }

        //удаление стандарта
        public async Task DeleteAntennaPortStandartAsync(int antennaTypePortStandartId)
        {
            await using var db = _dbContextFactory.CreateDbContext();

            AntennaTypePortStandart? dbAntennaPortStandart = await db.AntennaTypePortStandart.FindAsync(antennaTypePortStandartId);

            if (dbAntennaPortStandart == null)
            {
                throw new InvalidOperationException("Стандарт не найден в базе данных.");
            }

            db.AntennaTypePortStandart.Remove(dbAntennaPortStandart);

            await db.SaveChangesAsync();
        }
    }
}

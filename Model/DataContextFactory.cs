using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Model
{
    public class DataContextFactory : IDesignTimeDbContextFactory<NamikoDbContext>
    {
        public NamikoDbContext CreateDbContext(string[] args)
        {
            return new NamikoDbContext();
        }
    }
}
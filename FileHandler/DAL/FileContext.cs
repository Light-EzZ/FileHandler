using FileHandler.Models; 
using System.Data.Entity; 

namespace FileHandler.DAL
{
   
    public class FileContext : DbContext
    {
       
        public FileContext() : base("name=FileHandlerDb")
        {
        }

       
        public DbSet<StoredFile> StoredFiles { get; set; }
    }
}
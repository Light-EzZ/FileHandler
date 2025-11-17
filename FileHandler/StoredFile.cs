using System.ComponentModel.DataAnnotations; 

namespace FileHandler.Models
{
   
    public class StoredFile
    {
        public int Id { get; set; }

        [StringLength(255)] 
        public string Name { get; set; }

       
        public byte[] Data { get; set; }
    }
}